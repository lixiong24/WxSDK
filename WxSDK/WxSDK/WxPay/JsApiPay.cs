using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using WxSDK.Common;
using System.Web;
using WxSDK.Config;
using LitJson;
using System.Web.Security;
using System.Collections;

namespace WxSDK.WxPay
{
	/// <summary>
	/// 公众号支付
	/// </summary>
	public class JsApiPay
	{
		/// <summary>
		/// 保存页面对象，因为要在类的方法中使用Page的Request对象
		/// </summary>
		private Page page { get; set; }

		/// <summary>
		/// openid用于调用统一下单接口
		/// </summary>
		public string openid { get; set; }

		/// <summary>
		/// access_token用于获取收货地址js函数入口参数
		/// </summary>
		public string access_token { get; set; }

		/// <summary>
		/// 商品金额，用于统一下单
		/// </summary>
		public int total_fee { get; set; }

		/// <summary>
		/// 商品描述，用于统一下单
		/// </summary>
		public string body { get; set; }

		/// <summary>
		/// 附加数据，原样返回，用于统一下单
		/// </summary>
		public string attach { get; set; }

		/// <summary>
		/// 商户订单号，用于统一下单
		/// </summary>
		public string out_trade_no { get; set; }

		/// <summary>
		/// 商品标记，用于统一下单。该字段不能随便填，不使用请填空，使用说明详见企业红包接口。
		/// </summary>
		public string goods_tag { get; set; }

		/// <summary>
		/// 异步通知url，用于统一下单
		/// </summary>
		public string notify_url { get; set; }

		/// <summary>
		/// 统一下单接口返回结果
		/// </summary>
		public WxData unifiedOrderResult { get; set; }

		/// <summary>
		/// 通过OpenID和AccessToken拉取用户信息的返回数据
		/// </summary>
		public WxData UserInfoResult { get; set; }

		public JsApiPay(Page page)
		{
			this.page = page;
		}

		/// <summary>
		/// 网页授权获取用户基本信息的全部过程。采用“snsapi_userinfo”方式。
		/// 第一步：利用url跳转获取code。
		/// 第二步：利用code去获取openid和access_token。
		/// </summary>
		public void GetOpenidAndAccessToken()
		{
			GetOpenidAndAccessToken(0);
		}
		/// <summary>
		/// 网页授权获取用户基本信息的全部过程。
		/// 详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html。
		/// 第一步：利用url跳转获取code。
		/// 第二步：利用code去获取openid和access_token。
		/// 第三步：拉取用户信息（scopeType＝1时）
		/// </summary>
		/// <param name="scopeType">0：snsapi_base；1：snsapi_userinfo；</param>
		public void GetOpenidAndAccessToken(int scopeType)
		{
			if (!string.IsNullOrEmpty(page.Request.QueryString["code"]))
			{
				//获取code码，以获取openid和access_token
				string code = page.Request.QueryString["code"];
				Log.Debug(this.GetType().ToString(), "网页授权获取用户基本信息Get code : " + code);
				GetOpenidAndAccessTokenFromCode(code);
				if (scopeType == 1)
				{
					GetUserInfoFromOpenidAndAccessToken(openid, access_token);
				}
			}
			else
			{
				//构造网页授权获取code的URL
				//取提交页面的完整URL，包括请求参数
				string redirect_uri = HttpUtility.UrlEncode(page.Request.Url.ToString());
				WxData data = new WxData();
				data.SetValue("appid", WxConfig.APPID);
				data.SetValue("redirect_uri", redirect_uri);
				data.SetValue("response_type", "code");
				data.SetValue("scope", (scopeType == 0) ? "snsapi_base" : "snsapi_userinfo");
				data.SetValue("state", "STATE" + "#wechat_redirect");
				string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
				Log.Debug(this.GetType().ToString(), "网页授权获取用户基本信息跳转到URL : " + url);
				try
				{
					//触发微信返回code码
					//Redirect函数会抛出ThreadAbortException异常，不用处理这个异常
					page.Response.Redirect(url);
				}
				catch (System.Threading.ThreadAbortException ex)
				{
				}
			}
		}

		/// <summary>
		/// 通过code换取网页授权access_token和openid的返回数据，正确时返回的JSON数据包如下：
		/// {
		/// "access_token":"ACCESS_TOKEN",
		/// "expires_in":7200,
		/// "refresh_token":"REFRESH_TOKEN",
		/// "openid":"OPENID",
		/// "scope":"SCOPE",
		/// "unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
		/// }
		/// 其中access_token可用于获取共享收货地址。
		/// openid是微信支付jsapi支付接口统一下单时必须的参数。
		/// 更详细的说明请参考网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
		/// </summary>
		/// <param name="code"></param>
		public void GetOpenidAndAccessTokenFromCode(string code)
		{
			try
			{
				//构造获取openid及access_token的url
				WxData data = new WxData();
				data.SetValue("appid", WxConfig.APPID);
				data.SetValue("secret", WxConfig.APPSECRET);
				data.SetValue("code", code);
				data.SetValue("grant_type", "authorization_code");
				string url = "https://api.weixin.qq.com/sns/oauth2/access_token?" + data.ToUrl();

				//请求url以获取数据
				string result = HttpService.Get(url);

				Log.Debug(this.GetType().ToString(), "通过code换取网页授权(GetOpenidAndAccessTokenFromCode) response数据 : " + result);

				JsonData jd = JsonMapper.ToObject(result);
				if (!string.IsNullOrEmpty((string)jd["access_token"]))
				{
					//保存access_token，用于收货地址获取
					access_token = (string)jd["access_token"];
					//获取用户openid
					openid = (string)jd["openid"];

					Log.Debug(this.GetType().ToString(), "Get openid : " + openid);
					Log.Debug(this.GetType().ToString(), "Get access_token : " + access_token);
				}
				else
				{
					Log.Error(this.GetType().ToString(), "通过code换取网页授权失败 errcode : " + (string)jd["errcode"]);
					Log.Error(this.GetType().ToString(), "通过code换取网页授权失败 errmsg : " + (string)jd["errmsg"]);
				}
			}
			catch (Exception ex)
			{
				Log.Error(this.GetType().ToString(), ex.ToString());
				throw new WxException(ex.ToString());
			}
		}

		/// <summary>
		/// 通过OpenID和AccessToken拉取用户信息的返回数据，正确时返回的JSON数据包如下：
		/// {
		///  "openid":" OPENID",		//用户的唯一标识 
		///  "nickname": NICKNAME,	//用户昵称
		///  "sex":"1",//用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
		///  "province":"PROVINCE"//用户个人资料填写的省份
		///  "city":"CITY",//普通用户个人资料填写的城市
		///  "country":"COUNTRY",//国家，如中国为CN 
		///  "headimgurl": "http://wx.qlogo.cn/mmopen/g3MonUZtNHkdmzicIlibx6iaFqAc56vxLSUfpb6n5WKSYVY0ChQKkiaJSgQ1dZuTOgvLLrhJbERQQ4eMsv84eavHiaiceqxibJxCfHe/46", //用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像），用户没有头像时该项为空。若用户更换头像，原有头像URL将失效。
		///  "privilege":["PRIVILEGE1","PRIVILEGE2"],//用户特权信息，json 数组，如微信沃卡用户为（chinaunicom） 
		///  "unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"//只有在用户将公众号绑定到微信开放平台帐号后，才会出现该字段。
		/// }
		/// 更详细的说明请参考：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
		/// </summary>
		/// <param name="OpenID"></param>
		/// <param name="AccessToken"></param>
		public void GetUserInfoFromOpenidAndAccessToken(string OpenID, string AccessToken)
		{
			try
			{
				//构造拉取用户信息的url
				WxData data = new WxData();
				data.SetValue("access_token", AccessToken);
				data.SetValue("openid", OpenID);
				data.SetValue("lang", "zh_CN");
				string url = "https://api.weixin.qq.com/sns/userinfo?" + data.ToUrl();

				//请求url以获取数据
				string result = HttpService.Get(url);

				Log.Debug(this.GetType().ToString(), "通过OpenID和AccessToken拉取用户信息(GetUserInfoFromOpenidAndAccessToken) response数据 : " + result);

				JsonData jd = JsonMapper.ToObject(result);
				if (!string.IsNullOrEmpty((string)jd["openid"]))
				{
					UserInfoResult = new WxData();
					UserInfoResult.SetValue("openid", (string)jd["openid"]);
					UserInfoResult.SetValue("nickname", (string)jd["nickname"]);
					UserInfoResult.SetValue("sex", (int)jd["sex"]);
					UserInfoResult.SetValue("language", (string)jd["language"]);
					UserInfoResult.SetValue("city", (string)jd["city"]);
					UserInfoResult.SetValue("province", (string)jd["province"]);
					UserInfoResult.SetValue("country", (string)jd["country"]);
					UserInfoResult.SetValue("headimgurl", (string)jd["headimgurl"]);
					if (jd["privilege"].IsArray)
					{
						string strArray = "";
						JsonData jdItems = jd["privilege"];
						for (int i = 0; i < jdItems.Count; i++)
						{
							strArray = strArray + (string)jdItems[i] + ",";
						}
						strArray = strArray.TrimEnd(',');
						UserInfoResult.SetValue("privilege", strArray);
					}
					if (((IDictionary)jd).Contains("unionid"))
					{
						UserInfoResult.SetValue("unionid", (string)jd["unionid"]);
					}

					Log.Debug(this.GetType().ToString(), "用户信息 openid: " + UserInfoResult.GetValue("openid"));
					Log.Debug(this.GetType().ToString(), "用户信息 nickname: " + UserInfoResult.GetValue("nickname"));
					Log.Debug(this.GetType().ToString(), "用户信息 sex: " + UserInfoResult.GetValue("sex"));
					Log.Debug(this.GetType().ToString(), "用户信息 language: " + UserInfoResult.GetValue("language"));
					Log.Debug(this.GetType().ToString(), "用户信息 city: " + UserInfoResult.GetValue("city"));
					Log.Debug(this.GetType().ToString(), "用户信息 province: " + UserInfoResult.GetValue("province"));
					Log.Debug(this.GetType().ToString(), "用户信息 country: " + UserInfoResult.GetValue("country"));
					Log.Debug(this.GetType().ToString(), "用户信息 headimgurl: " + UserInfoResult.GetValue("headimgurl"));
					Log.Debug(this.GetType().ToString(), "用户信息 privilege: " + UserInfoResult.GetValue("privilege"));
					if (UserInfoResult.IsSet("unionid"))
					{
						Log.Debug(this.GetType().ToString(), "用户信息 unionid: " + UserInfoResult.GetValue("unionid"));
					}
				}
				else
				{
					Log.Error(this.GetType().ToString(), "通过OpenID和AccessToken拉取用户信息失败 errcode : " + (string)jd["errcode"]);
					Log.Error(this.GetType().ToString(), "通过OpenID和AccessToken拉取用户信息失败 errmsg : " + (string)jd["errmsg"]);
				}
			}
			catch (Exception ex)
			{
				Log.Error(this.GetType().ToString(), ex.ToString());
				throw new WxException(ex.ToString());
			}
		}

		/// <summary>
		/// 调用统一下单，获得下单结果
		/// </summary>
		/// <returns></returns>
		public WxData GetUnifiedOrderResult()
		{
			//统一下单
			WxData data = new WxData();
			data.SetValue("body", body);
			data.SetValue("attach", attach);
			data.SetValue("out_trade_no", (string.IsNullOrEmpty(out_trade_no)) ? WxPayApi.GenerateOutTradeNo() : out_trade_no);
			data.SetValue("total_fee", total_fee);
			data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
			data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
			data.SetValue("goods_tag", goods_tag);
			data.SetValue("trade_type", "JSAPI");
			data.SetValue("openid", openid);
			data.SetValue("notify_url", notify_url);

			WxData result = WxPayApi.UnifiedOrder(data);
			if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
			{
				Log.Error(this.GetType().ToString(), "UnifiedOrder response error!");
				throw new WxException("UnifiedOrder response error!");
			}

			unifiedOrderResult = result;
			return result;
		}

		/// <summary>
		/// 从统一下单成功返回的数据中获取微信浏览器调起jsapi支付所需的参数，
		/// 微信浏览器调起JSAPI时的输入参数格式如下：
		/// {
		/// "appId" : "wx2421b1c4370ec43b",     //公众号名称，由商户传入
		/// "timeStamp":" 1395712654",         //时间戳，自1970年以来的秒数 
		/// "nonceStr" : "e61463f8efa94090b1f366cccfbbb444", //随机串
		/// "package" : "prepay_id=u802345jgfjsdfgsdg888",
		/// "signType" : "MD5",         //微信签名方式:
		/// "paySign" : "70EA570631E4BB79628FBCA90534C63FF7FADD89" //微信签名
		/// }
		/// 返回值：微信浏览器调起JSAPI时的输入参数，json格式可以直接做参数用。
		/// 更详细的说明请参考网页端调起支付API：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7
		/// </summary>
		/// <returns></returns>
		public string GetJsApiParameters()
		{
			Log.Debug(this.GetType().ToString(), "JsApiPay::GetJsApiParam is processing...");

			WxData jsApiParam = new WxData();
			jsApiParam.SetValue("appId", unifiedOrderResult.GetValue("appid"));
			jsApiParam.SetValue("timeStamp", WxPayApi.GenerateTimeStamp());
			jsApiParam.SetValue("nonceStr", WxPayApi.GenerateNonceStr());
			jsApiParam.SetValue("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
			jsApiParam.SetValue("signType", "MD5");
			jsApiParam.SetValue("paySign", jsApiParam.MakeSign());

			string parameters = jsApiParam.ToJson();

			Log.Debug(this.GetType().ToString(), "Get jsApiParam : " + parameters);
			return parameters;
		}

		/// <summary>
		/// 获取收货地址js函数入口参数,详情请参考收货地址共享接口：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_9
		/// 返回值：共享收货地址js函数需要的参数，json格式可以直接做参数使用
		/// </summary>
		/// <returns></returns>
		public string GetEditAddressParameters()
		{
			string parameter = "";
			try
			{
				string scheme = page.Request.Url.Scheme;
				string host = page.Request.Url.Host;
				string path = page.Request.Path;
				string queryString = page.Request.Url.Query;
				//这个地方要注意，参与签名的是网页授权获取用户信息时微信后台回传的完整url
				string url = scheme+"://" + host + path + queryString;

				//构造需要用SHA1算法加密的数据
				WxData signData = new WxData();
				signData.SetValue("appid", WxConfig.APPID);
				signData.SetValue("url", url);
				signData.SetValue("timestamp", WxPayApi.GenerateTimeStamp());
				signData.SetValue("noncestr", WxPayApi.GenerateNonceStr());
				signData.SetValue("accesstoken", access_token);
				string param = signData.ToUrl();

				Log.Debug(this.GetType().ToString(), "SHA1 encrypt param : " + param);
				//SHA1加密
				string addrSign = FormsAuthentication.HashPasswordForStoringInConfigFile(param, "SHA1");
				Log.Debug(this.GetType().ToString(), "SHA1 encrypt result : " + addrSign);

				//获取收货地址js函数入口参数
				WxData afterData = new WxData();
				afterData.SetValue("appId", WxConfig.APPID);
				afterData.SetValue("scope", "jsapi_address");
				afterData.SetValue("signType", "sha1");
				afterData.SetValue("addrSign", addrSign);
				afterData.SetValue("timeStamp", signData.GetValue("timestamp"));
				afterData.SetValue("nonceStr", signData.GetValue("noncestr"));

				//转为json格式
				parameter = afterData.ToJson();
				Log.Debug(this.GetType().ToString(), "Get EditAddressParam : " + parameter);
			}
			catch (Exception ex)
			{
				Log.Error(this.GetType().ToString(), ex.ToString());
				throw new WxException(ex.ToString());
			}

			return parameter;
		}
	}
}

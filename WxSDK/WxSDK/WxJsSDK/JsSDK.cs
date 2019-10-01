using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using WxSDK.Common;
using WxSDK.Config;

namespace WxSDK.WxJsSDK
{
	/// <summary>
	/// 网页开发SDK
	/// </summary>
	public class JsSDK
	{
		/// <summary>
		/// 得到jsapi_ticket的JSON数据，有效期2小时。
		/// 成功返回：{"errcode":0,"errmsg":"ok","ticket":"jsapi_ticket","expires_in":7200}。
		/// </summary>
		/// <param name="access_token"></param>
		/// <returns></returns>
		public static string GetJsApiTicket(string access_token)
		{
			string url = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token=" + access_token + "&type=jsapi";
			//请求url以获取数据
			string result = HttpService.Get(url);
			Log.Debug("得到jsapi_ticket的JSON数据", "请求地址："+ url+"\r\n返回数据 : " + result);
			return result;
		}

		/// <summary>
		/// 生成签名
		/// </summary>
		/// <param name="noncestr">随机字符串</param>
		/// <param name="timestamp">时间戳</param>
		/// <param name="jsapi_ticket"></param>
		/// <param name="url">调用JS接口页面的完整URL</param>
		/// <returns></returns>
		public static string MakeSign(string noncestr,string timestamp,string jsapi_ticket,string url)
		{
			if(string.IsNullOrEmpty(noncestr) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(jsapi_ticket) || string.IsNullOrEmpty(url))
			{
				return "";
			}
			//构造需要用SHA1算法加密的数据
			WxData signData = new WxData();
			signData.SetValue("url", url);
			signData.SetValue("timestamp", timestamp);
			signData.SetValue("noncestr", noncestr);
			signData.SetValue("jsapi_ticket", jsapi_ticket);
			string param = signData.ToUrl();

			Log.Debug("SHA1加密前参数", param);
			//SHA1加密
			string addrSign = FormsAuthentication.HashPasswordForStoringInConfigFile(param, "SHA1");
			Log.Debug("SHA1加密后参数", addrSign);

			return addrSign;
		}
	}
}

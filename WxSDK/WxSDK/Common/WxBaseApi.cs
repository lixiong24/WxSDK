using System;
using System.Collections.Generic;
using System.Text;
using WxSDK.Config;

namespace WxSDK.Common
{
	/// <summary>
	/// 微信基础API
	/// </summary>
	public class WxBaseApi
	{
		/// <summary>
		/// 获得微信接口调用凭据(access_token)的JSON数据，有效期2小时。与网页授权access_token不同。
		/// 成功返回：{"access_token":"ACCESS_TOKEN","expires_in":7200}。
		/// 失败返回：{"errcode":40013,"errmsg":"invalid appid"}
		/// </summary>
		/// <returns></returns>
		public static string GetAccessToken()
		{
			WxData data = new WxData();
			data.SetValue("appid", WxConfig.APPID);
			data.SetValue("secret", WxConfig.APPSECRET);
			data.SetValue("grant_type", "client_credential");
			string url = "https://api.weixin.qq.com/cgi-bin/token?" + data.ToUrl();
			//请求url以获取数据
			string result = HttpService.Get(url);
			Log.Debug("获得微信接口调用凭据(access_token)", "返回数据 : " + result);
			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace WxSDK.Common
{
	public class WxUtility
	{
		/// <summary>
		/// 获得客户端IP
		/// </summary>
		/// <returns></returns>
		public static string GetClientIP()
		{
			string userIP;
			HttpRequest Request = HttpContext.Current.Request;
			// 如果使用代理，获取真实IP
			if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != "")
				userIP = Request.ServerVariables["REMOTE_ADDR"];
			else
				userIP = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
			if (userIP == null || userIP == "")
				userIP = Request.UserHostAddress;
			return userIP;
		}
	}
}

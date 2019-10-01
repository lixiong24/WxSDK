using LitJson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using WxSDK.Config;

namespace WxSDK.Common
{
	/// <summary>
	/// 工具类
	/// </summary>
	public class WxUtility
	{
		private static Random rnd = new Random();

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

		#region 随机字符串
		/// <summary>
		/// 生成一个指定长度的随机字符串a-Z0-9_*
		/// </summary>
		/// <param name="pwdlen">字符串长度</param>
		/// <returns>随机字符串</returns>
		public static string MakeRandomString(int pwdlen)
		{
			return MakeRandomString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_*", pwdlen);
		}
		/// <summary>
		/// 从一个指定的字符串中取出指定长度的随机字符串
		/// </summary>
		/// <param name="pwdchars">用于抽取随机字符串的字符串</param>
		/// <param name="pwdlen">生成的随机字符串长度</param>
		/// <returns>随机字符串</returns>
		public static string MakeRandomString(string pwdchars, int pwdlen)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < pwdlen; i++)
			{
				int num = rnd.Next(pwdchars.Length);
				builder.Append(pwdchars[num]);
			}
			return builder.ToString();
		}
		#endregion

		#region 时间戳
		/// <summary>
		/// 得到当前时间戳(ms)
		/// </summary>
		/// <returns></returns>
		public static long CurrentTimeMillis()
		{
			return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
		}
		/// <summary>
		/// 时间戳转为C#格式时间
		/// </summary>
		/// <param name="timeStamp">Unix时间戳格式</param>
		/// <returns>C#格式时间</returns>
		public static DateTime GetTime(string timeStamp)
		{
			DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			long lTime = long.Parse(timeStamp);
			if (timeStamp.Length == 13)
			{
				return dtStart.AddMilliseconds(lTime);
			}
			else if (timeStamp.Length == 10)
			{
				return dtStart.AddSeconds(lTime);
			}
			else
			{
				return dtStart.AddMilliseconds(lTime);
			}
		}
		#endregion
	}
}

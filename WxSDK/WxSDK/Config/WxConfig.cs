using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace WxSDK.Config
{
	/// <summary>
	/// 配置账号信息
	/// </summary>
	public class WxConfig
	{
		//=======【基本信息设置】=====================================
		private static string m_APPID = ConfigurationManager.AppSettings["WxAPPID"];
		/// <summary>
		/// 应用ID。默认在AppSettings["WxAPPID"]中设置。
		/// </summary>
		public static string APPID
		{
			get
			{
				return m_APPID;
			}
			set
			{
				m_APPID = value;
			}
		}

		private static string m_APPSECRET = ConfigurationManager.AppSettings["WxAPPSECRET"];
		/// <summary>
		/// 应用密钥。默认在AppSettings["WxAPPSECRET"]中设置。
		/// </summary>
		public static string APPSECRET
		{
			get
			{
				return m_APPSECRET;
			}
			set
			{
				m_APPSECRET = value;
			}
		}

		private static string m_MCHID = ConfigurationManager.AppSettings["WxMCHID"];
		/// <summary>
		/// 微信支付商户号。默认在AppSettings["WxMCHID"]中设置。
		/// </summary>
		public static string MCHID
		{
			get
			{
				return m_MCHID;
			}
			set
			{
				m_MCHID = value;
			}
		}

		private static string m_KEY = ConfigurationManager.AppSettings["WxKEY"];
		/// <summary>
		/// 商户支付密钥。默认在AppSettings["WxKEY"]中设置。
		/// </summary>
		public static string KEY
		{
			get
			{
				return m_KEY;
			}
			set
			{
				m_KEY = value;
			}
		}

		//=======【证书路径设置】===================================== 
		private static string m_SSLCERT_PATH = ConfigurationManager.AppSettings["WxSSLCERT_PATH"];
		/// <summary>
		/// 证书路径,注意应该填写绝对路径（例：cert/apiclient_cert.p12）。
		/// 默认在AppSettings["WxSSLCERT_PATH"]中设置。
		/// （仅退款、撤销订单时需要）
		/// </summary>
		public static string SSLCERT_PATH
		{
			get
			{
				return m_SSLCERT_PATH;
			}
			set
			{
				m_SSLCERT_PATH = value;
			}
		}

		private static string m_SSLCERT_PASSWORD = ConfigurationManager.AppSettings["WxSSLCERT_PASSWORD"];
		/// <summary>
		/// 证书密码（仅退款、撤销订单时需要）
		/// 默认在AppSettings["WxSSLCERT_PASSWORD"]中设置。
		/// </summary>
		public static string SSLCERT_PASSWORD
		{
			get
			{
				return m_SSLCERT_PASSWORD;
			}
			set
			{
				m_SSLCERT_PASSWORD = value;
			}
		}

		//=======【支付结果通知url】===================================== 
		private static string m_NOTIFY_URL = ConfigurationManager.AppSettings["WxNOTIFY_URL"];
		/// <summary>
		/// 支付结果通知回调url，用于商户接收支付结果。
		/// 默认在AppSettings["WxNOTIFY_URL"]中设置。
		/// </summary>
		public static string NOTIFY_URL
		{
			get
			{
				return m_NOTIFY_URL;
			}
			set
			{
				m_NOTIFY_URL = value;
			}
		}

		//=======【商户系统后台机器IP】===================================== 
		private static string m_IP = ConfigurationManager.AppSettings["WxIP"];
		/// <summary>
		/// 商户系统后台机器IP。此参数可手动配置也可在程序中自动获取。
		/// 默认在AppSettings["WxIP"]中设置。如果为空，返回服务器IP。
		/// </summary>
		public static string IP
		{
			get
			{
				if (string.IsNullOrEmpty(m_IP))
				{ 
					IPAddress[] addressList = Dns.GetHostAddresses(Dns.GetHostName());
					foreach (IPAddress ip in addressList)
					{
						if (ip.AddressFamily.Equals(AddressFamily.InterNetwork))
						{
							m_IP = ip.ToString();
							break;
						}
					}
				}
				return m_IP;
			}
			set
			{
				m_IP = value;
			}
		}

		//=======【代理服务器设置】===================================
		private static string m_PROXY_URL = ConfigurationManager.AppSettings["WxPROXY_URL"];
		/// <summary>
		/// 代理服务器IP（例：http://10.152.18.220:8080）。
		/// 默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）
		/// 默认在AppSettings["WxPROXY_URL"]中设置。
		/// </summary>
		public static string PROXY_URL
		{
			get
			{
				if (string.IsNullOrEmpty(m_PROXY_URL))
				{
					return "0.0.0.0:0";
				}
				return m_PROXY_URL;
			}
			set
			{
				m_PROXY_URL = value;
			}
		}

		//=======【上报信息配置】===================================
		public static int m_REPORT_LEVENL = int.Parse(ConfigurationManager.AppSettings["WxREPORT_LEVENL"]);
		/// <summary>
		/// 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报。
		/// 默认在AppSettings["WxREPORT_LEVENL"]中设置。
		/// </summary>
		public static int REPORT_LEVENL
		{
			get
			{
				return m_REPORT_LEVENL;
			}
			set
			{
				m_REPORT_LEVENL = value;
			}
		}

		//=======【日志级别】===================================
		public static int m_LOG_LEVENL = int.Parse(ConfigurationManager.AppSettings["WxLOG_LEVENL"]);
		/// <summary>
		/// 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息。
		/// 默认在AppSettings["WxREPORT_LEVENL"]中设置。
		/// </summary>
		public static int LOG_LEVENL
		{
			get
			{
				return m_LOG_LEVENL;
			}
			set
			{
				m_LOG_LEVENL = value;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using WxSDK.Config;
using System.Security.Cryptography;
using LitJson;

namespace WxSDK.Common
{
	/// <summary>
	/// 微信协议接口数据类，所有的API接口通信都依赖这个数据结构，
	/// 在调用接口之前先填充各个字段的值，然后进行接口通信。
	/// </summary>
	public class WxData
	{
		public WxData()
        {
        }

		/// <summary>
		/// 采用排序的Dictionary的好处是，方便对数据包进行签名，不用在签名之前再做一次排序。
		/// </summary>
		private SortedDictionary<string, object> m_values = new SortedDictionary<string, object>();
		/// <summary>
		/// 获取Dictionary
		/// </summary>
		/// <returns></returns>
		public SortedDictionary<string, object> GetValues()
		{
			return m_values;
		}

		/// <summary>
		/// 设置某个字段的值
		/// </summary>
		/// <param name="key">字段名</param>
		/// <param name="value">字段值</param>
		public void SetValue(string key, object value)
		{
			m_values[key] = value;
		}

		/// <summary>
		/// 根据字段名获取某个字段的值
		/// </summary>
		/// <param name="key">字段名</param>
		/// <returns></returns>
		public object GetValue(string key)
		{
			object o = null;
			m_values.TryGetValue(key, out o);
			return o;
		}

		/// <summary>
		/// 判断某个字段是否已设置。若字段key已被设置，则返回true，否则返回false
		/// </summary>
		/// <param name="key">字段名</param>
		/// <returns></returns>
		public bool IsSet(string key)
		{
			object o = null;
			m_values.TryGetValue(key, out o);
			if (null != o)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 将Dictionary转成xml
		/// </summary>
		/// <returns></returns>
		public string ToXml()
		{
			//数据为空时不能转化为xml格式
			if (0 == m_values.Count)
			{
				Log.Error(this.GetType().ToString(), "WxData数据为空!");
				throw new WxException("WxData数据为空!");
			}

			string xml = "<xml>";
			foreach (KeyValuePair<string, object> pair in m_values)
			{
				//字段值不能为null，会影响后续流程
				if (pair.Value == null)
				{
					Log.Error(this.GetType().ToString(), "WxData内部字段名："+pair.Key+"的值为null!");
					throw new WxException("WxData内部字段名：" + pair.Key + "的值为null!");
				}

				if (pair.Value.GetType() == typeof(int))
				{
					xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
				}
				else if (pair.Value.GetType() == typeof(string))
				{
					xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
				}
				else//除了string和int类型不能含有其他数据类型
				{
					Log.Error(this.GetType().ToString(), "WxData中字段名：" + pair.Key + "的数据类型错误!");
					throw new WxException("WxData中字段名：" + pair.Key + "的数据类型错误!");
				}
			}
			xml += "</xml>";
			return xml;
		}

		/// <summary>
		/// 将xml转为WxData对象并返回对象内部的数据
		/// </summary>
		/// <param name="xml">待转换的xml串</param>
		/// <returns></returns>
		public SortedDictionary<string, object> FromXml(string xml)
		{
			if (string.IsNullOrEmpty(xml))
			{
				Log.Error(this.GetType().ToString(), "将空的xml串转换为WxData不合法!");
				throw new WxException("将空的xml串转换为WxData不合法!");
			}

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
			XmlNodeList nodes = xmlNode.ChildNodes;
			foreach (XmlNode xn in nodes)
			{
				XmlElement xe = (XmlElement)xn;
				m_values[xe.Name] = xe.InnerText;//获取xml的键值对到WxData内部的数据中
			}

			try
			{
				//2015-06-29 错误是没有签名
				if (m_values["return_code"] != "SUCCESS")
				{
					return m_values;
				}
				CheckSign();//验证签名,不通过会抛异常
			}
			catch (WxException ex)
			{
				throw new WxException(ex.Message);
			}

			return m_values;
		}

		/// <summary>
		/// Dictionary格式转化成url参数格式。返回值为url格式串, 该串不包含sign字段值
		/// </summary>
		/// <returns></returns>
		public string ToUrl()
		{
			string buff = "";
			foreach (KeyValuePair<string, object> pair in m_values)
			{
				if (pair.Value == null)
				{
					Log.Error(this.GetType().ToString(), "WxData内部字段名：" + pair.Key + "的值为null!");
					throw new WxException("WxData内部字段名：" + pair.Key + "的值为null!");
				}

				if (pair.Key != "sign" && pair.Value.ToString() != "")
				{
					buff += pair.Key + "=" + pair.Value + "&";
				}
			}
			buff = buff.Trim('&');
			return buff;
		}

		/// <summary>
		/// Dictionary格式化成Json。返回值为json串数据
		/// </summary>
		/// <returns></returns>
		public string ToJson()
		{
			string jsonStr = JsonMapper.ToJson(m_values);
			return jsonStr;
		}

		/// <summary>
		/// values格式化成能在Web页面上显示的结果
		/// （因为web页面上不能直接输出xml格式的字符串）
		/// </summary>
		/// <returns></returns>
		public string ToPrintStr()
		{
			string str = "";
			foreach (KeyValuePair<string, object> pair in m_values)
			{
				if (pair.Value == null)
				{
					Log.Error(this.GetType().ToString(), "WxData内部字段名：" + pair.Key + "的值为null!");
					throw new WxException("WxData内部字段名：" + pair.Key + "的值为null!");
				}

				str += string.Format("{0}={1}<br>", pair.Key, pair.Value.ToString());
			}
			Log.Debug(this.GetType().ToString(), "Print in Web Page : " + str);
			return str;
		}

		/// <summary>
		/// 生成签名，详见签名生成算法。返回值为签名, sign字段不参加签名。
		/// </summary>
		/// <returns></returns>
		public string MakeSign()
		{
			//转url格式
			string str = ToUrl();

			//在string后加入API KEY
			str += "&key=" + WxConfig.KEY;
			
			//MD5加密
			MD5 md5 = MD5.Create();
			byte[] bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
			StringBuilder sb = new StringBuilder();
			foreach (byte b in bs)
			{
				sb.Append(b.ToString("x2"));
			}
			//所有字符转为大写
			return sb.ToString().ToUpper();
		}

		/// <summary>
		/// 检测签名是否正确
		/// </summary>
		/// <returns></returns>
		public bool CheckSign()
		{
			//如果没有设置签名，则跳过检测
			if (!IsSet("sign"))
			{
				Log.Error(this.GetType().ToString(), "WxData签名字段不存在!");
				throw new WxException("WxData签名字段不存在!");
			}
			else if (GetValue("sign") == null || GetValue("sign").ToString() == "")
			{
				//如果设置了签名但是签名为空，则抛异常
				Log.Error(this.GetType().ToString(), "WxData签名存在但不合法!");
				throw new WxException("WxData签名存在但不合法!");
			}

			//获取接收到的签名
			string return_sign = GetValue("sign").ToString();

			//在本地计算新的签名
			string cal_sign = MakeSign();

			if (cal_sign == return_sign)
			{
				return true;
			}

			Log.Error(this.GetType().ToString(), "WxData签名验证错误!");
			throw new WxException("WxData签名验证错误!");
		}
	}
}

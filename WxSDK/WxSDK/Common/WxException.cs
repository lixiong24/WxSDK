using System;
using System.Collections.Generic;
using System.Text;

namespace WxSDK.Common
{
	/// <summary>
	/// 微信异常类
	/// </summary>
	public class WxException : Exception 
	{
		public WxException(string msg) : base(msg) 
        {
        }
	}
}

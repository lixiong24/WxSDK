using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using WxSDK.Common;

namespace WxSDK.WxPay
{
	/// <summary>
	/// 微信支付回调处理基类。
	/// 1、负责接收微信支付后台发送过来的数据，对数据进行签名验证。
	/// 2、查询订单，判断订单真实性。
	/// 3、子类在此类基础上进行派生，并重写自己的回调处理过程。
	/// </summary>
	public class Notify
	{
		public Page page { get; set; }
		public Notify(Page page)
		{
			this.page = page;
		}

		/// <summary>
		/// 接收从微信支付后台发送过来的数据并验证签名
		/// </summary>
		/// <returns>微信支付后台返回的数据</returns>
		public WxData GetNotifyData()
		{
			//接收从微信后台POST过来的数据
			System.IO.Stream s = page.Request.InputStream;
			int count = 0;
			byte[] buffer = new byte[1024];
			StringBuilder builder = new StringBuilder();
			while ((count = s.Read(buffer, 0, 1024)) > 0)
			{
				builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
			}
			s.Flush();
			s.Close();
			s.Dispose();

			Log.Info(this.GetType().ToString(), "从微信接收数据 : " + builder.ToString());

			//转换数据格式并验证签名
			WxData data = new WxData();
			try
			{
				data.FromXml(builder.ToString());
			}
			catch (WxException ex)
			{
				//若签名错误，则立即返回结果给微信支付后台
				ReturnErrorMsgToWx("签名检查错误 : " + ex.Message);
			}

			Log.Info(this.GetType().ToString(), "检查签名成功");
			return data;
		}

		/// <summary>
		/// 查询订单，判断订单真实性。
		/// 1、判断成功返回true。
		/// 2、判断失败返回false，并主动发消息告诉微信后台。
		/// </summary>
		/// <param name="notifyData">微信支付后台发送过来的数据</param>
		/// <returns></returns>
		public bool IsTrueOrder(WxData notifyData)
		{
			//检查支付结果中transaction_id(微信支付订单号)是否存在
			if (!notifyData.IsSet("transaction_id"))
			{
				//若transaction_id不存在，则立即返回结果给微信支付后台
				ReturnErrorMsgToWx("支付结果中微信订单号不存在");
			}

			//微信支付订单号
			string transaction_id = notifyData.GetValue("transaction_id").ToString();
			WxData req = new WxData();
			req.SetValue("transaction_id", transaction_id);
			WxData res = WxPayApi.OrderQuery(req);
			if (res.GetValue("return_code").ToString() == "SUCCESS" &&
				res.GetValue("result_code").ToString() == "SUCCESS")
			{
				return true;
			}
			else
			{
				ReturnErrorMsgToWx("订单查询失败！判断订单不存在！");
				return false;
			}
		}

		/// <summary>
		/// 返回失败结果给微信
		/// </summary>
		/// <param name="errMsg"></param>
		public void ReturnErrorMsgToWx(string errMsg)
		{
			WxData res = new WxData();
			res.SetValue("return_code", "FAIL");
			res.SetValue("return_msg", errMsg);
			ReturnMsgToWx(res);
		}

		/// <summary>
		/// 返回成功结果给微信
		/// </summary>
		public void ReturnSuccessMsgToWx()
		{
			WxData res = new WxData();
			res.SetValue("return_code", "SUCCESS");
			res.SetValue("return_msg", "OK");
			ReturnMsgToWx(res);
		}

		/// <summary>
		/// 返回结果给微信
		/// </summary>
		/// <param name="res"></param>
		public void ReturnMsgToWx(WxData res)
		{
			Log.Error(this.GetType().ToString(), "商户返回结果给微信：" + res.ToXml());
			page.Response.Write(res.ToXml());
			page.Response.End();
		}

		/// <summary>
		/// 虚方法：派生类需要重写这个方法，进行不同的回调处理
		/// </summary>
		public virtual void ProcessNotify()
		{

		}
	}
}

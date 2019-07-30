using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WxSDK.Common;

namespace WxSDK.WxPay
{
    /// <summary>
    /// 支付结果通知回调处理类.
    /// 负责接收微信支付后台发送的支付结果并对订单有效性进行验证，
	/// 将验证结果反馈给微信支付后台
    /// </summary>
    public class ResultNotify:Notify
    {
        public ResultNotify(Page page):base(page)
        {
        }

		/// <summary>
		/// 查询订单，判断订单真实性。处理支付回调结果。
		/// 1、判断成功，主动发消息告诉微信后台。
		/// 2、判断失败，主动发消息告诉微信后台。
		/// </summary>
        public override void ProcessNotify()
        {
            WxData notifyData = GetNotifyData();

			//检查支付结果中transaction_id(微信支付订单号)是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WxData res = new WxData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                Log.Error(this.GetType().ToString(), "The Pay result is error : " + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }

			//微信支付订单号
            string transaction_id = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxData res = new WxData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Log.Error(this.GetType().ToString(), "Order query failure : " + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
            //查询订单成功
            else
            {
                WxData res = new WxData();
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                Log.Info(this.GetType().ToString(), "order query success : " + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
        }

        /// <summary>
		/// 查询订单
        /// </summary>
        /// <param name="transaction_id"></param>
        /// <returns></returns>
        private bool QueryOrder(string transaction_id)
        {
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
                return false;
            }
        }
    }
}
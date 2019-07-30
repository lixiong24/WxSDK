using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WxSDK.WxPay;
using WxSDK.Common;

public partial class PayResultWx : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		Notify notify = new Notify(this);
		WxData notifyData = notify.GetNotifyData();
		if (notify.IsTrueOrder(notifyData))
		{
			//商户订单号
			string out_trade_no = notifyData.GetValue("out_trade_no").ToString();
			//根据商户订单号处理商户订单

			//返回成功消息给微信
			notify.ReturnSuccessMsgToWx();
		}
    }
}
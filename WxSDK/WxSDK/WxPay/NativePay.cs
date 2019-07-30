using System;
using System.Collections.Generic;
using System.Text;
using WxSDK.Common;
using WxSDK.Config;

namespace WxSDK.WxPay
{
	/// <summary>
	/// 扫码支付
	/// </summary>
	public class NativePay
	{
		/// <summary>
		/// 生成扫描支付模式一URL
		/// </summary>
		/// <param name="productId">商品ID</param>
		/// <returns></returns>
		public string GetPrePayUrl(string productId)
		{
			Log.Info(this.GetType().ToString(), "Native pay mode 1 url is producing...");

			WxData data = new WxData();
			data.SetValue("appid", WxConfig.APPID);//公众帐号id
			data.SetValue("mch_id", WxConfig.MCHID);//商户号
			data.SetValue("time_stamp", WxPayApi.GenerateTimeStamp());//时间戳
			data.SetValue("nonce_str", WxPayApi.GenerateNonceStr());//随机字符串
			data.SetValue("product_id", productId);//商品ID
			data.SetValue("sign", data.MakeSign());//签名
			string str = ToUrlParams(data.GetValues());//转换为URL串
			string url = "weixin://wxpay/bizpayurl?" + str;

			Log.Info(this.GetType().ToString(), "Get native pay mode 1 url : " + url);
			return url;
		}

		/// <summary>
		/// 生成直接支付url，支付url有效期为2小时,模式二
		/// </summary>
		/// <param name="productId">商品ID</param>
		/// <param name="total_fee">订单总金额（单位：分）</param>
		/// <returns></returns>
		public string GetPayUrl(string productId, int total_fee)
		{
			Log.Info(this.GetType().ToString(), "Native pay mode 2 url is producing...");

			WxData data = new WxData();
			data.SetValue("body", productId);//商品描述
			data.SetValue("attach", "");//附加数据
			data.SetValue("out_trade_no", productId);//随机字符串
			data.SetValue("total_fee", total_fee);//总金额
			data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
			data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));//交易结束时间
			data.SetValue("goods_tag", "");//商品标记
			data.SetValue("trade_type", "NATIVE");//交易类型
			data.SetValue("product_id", productId);//商品ID

			WxData result = WxPayApi.UnifiedOrder(data);//调用统一下单接口
			if (!result.IsSet("code_url"))
			{
				Log.Error(this.GetType().ToString(), "UnifiedOrder response error!");
				throw new WxException("UnifiedOrder response error!");
			}
			string url = result.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接

			Log.Info(this.GetType().ToString(), "Get native pay mode 2 url : " + url);
			return url;
		}

		/// <summary>
		/// 参数数组转换为url格式
		/// </summary>
		/// <param name="map">参数名与参数值的映射表</param>
		/// <returns></returns>
		private string ToUrlParams(SortedDictionary<string, object> map)
		{
			string buff = "";
			foreach (KeyValuePair<string, object> pair in map)
			{
				buff += pair.Key + "=" + pair.Value + "&";
			}
			buff = buff.Trim('&');
			return buff;
		}
	}
}

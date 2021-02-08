using System;
using System.Runtime.Remoting.Contexts;
using System.Web;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;

namespace OS_Stripe
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class Notify : IHttpHandler
    {
        private String _lang = "";

        /// <summary>
        /// This function needs to process and returned message from the bank.
        /// This processing may vary widely between banks.
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            var modCtrl = new NBrightBuyController();
            var info = modCtrl.GetPluginSinglePageData("OSStripepayment", "OSStripePAYMENT", Utils.GetCurrentCulture());

            try
            {

                var orderid = Utils.RequestQueryStringParam(context, "oid");
                if (Utils.IsNumeric(orderid))
                {
                    var orderData = new OrderData(Convert.ToInt32(orderid));
                    var paymentkey = orderData.PurchaseInfo.GetXmlProperty("genxml/paymentkey");
                    if (paymentkey != "")
                    {
                        var payPlugData = new StripeLimpet(orderData);
                        var paymentData = payPlugData.RetrievePayment(paymentkey);
                        if (paymentData != null && paymentData.ContainsKey("is_paid"))
                        {
                            if (paymentData["is_paid"])
                                orderData.PaymentOk();
                            else
                                orderData.PaymentFail();
                        }
                    }
                }

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Write("OK");
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.CacheControl = "no-cache";
                HttpContext.Current.Response.Expires = -1;
                HttpContext.Current.Response.End();

            }
            catch (Exception ex)
            {
                if (!ex.ToString().StartsWith("System.Threading.ThreadAbortException")) // we expect a thread abort from the End response.
                {
                    info.SetXmlProperty("genxml/debugmsg", "OS_Stripe ERROR: " + ex.ToString());
                    modCtrl.Update(info);
                }
            }


        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


    }
}
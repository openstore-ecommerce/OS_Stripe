using System;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Web;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;
using System.Threading.Tasks;
using Stripe;
using DotNetNuke.Entities.Portals;

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
                System.IO.Stream str;
                Int32 counter, strLen, strRead;

                str = context.Request.InputStream;

                strLen = Convert.ToInt32(str.Length);
                byte[] strArr = new byte[strLen];
                strRead = str.Read(strArr, 0, strLen);
                var jsonArray = System.Text.Encoding.UTF8.GetString(strArr).ToCharArray();
                var json = "";
                for (counter = 0; counter < strLen; counter++)
                {
                    json += jsonArray[counter];
                }

                var stripeEvent = EventUtility.ParseEvent(json);

                var settings = ProviderUtils.GetProviderSettings();
                if (settings.GetXmlPropertyBool("genxml/checkbox/debugmode"))
                {
                    System.IO.File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_stripeIPN.json", json);
                }

                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                var orderid =  paymentIntent.CustomerId;
                if (Utils.IsNumeric(orderid))
                {
                    var orderData = new OrderData(Convert.ToInt32(orderid));

                    // Handle the event
                    if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                    {
                        orderData.PaymentOk();
                    }
                    else
                    {
                        orderData.PaymentFail();
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
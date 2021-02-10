using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using DotNetNuke.Common.Utilities;

namespace OS_Stripe
{
    public class ProviderUtils
    {
        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OSStripepayment", "OSStripePAYMENT", Utils.GetCurrentCulture());
            return info;
        }


        public static String GetBankRemotePost(OrderData orderData)
        {

            var rPost = new RemotePost();

            var objCtrl = new NBrightBuyController();
            var settings = objCtrl.GetPluginSinglePageData("OSStripepayment", "OSStripePAYMENT", orderData.Lang);


            // get the order data
            var payData = new PayData(orderData);

            rPost.Add("publickey", payData.PublicKey);
            rPost.Add("sessionid", orderData.PurchaseInfo.GetXmlProperty("genxml/sessionid"));

            //Build the re-direct html 
            var rtnStr = "";
            rtnStr = rPost.GetPostHtml();

            if (settings.GetXmlPropertyBool("genxml/checkbox/debugmode"))
            {
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_OSStripepost.html", rtnStr);
            }
            return rtnStr;
        }


    }
}

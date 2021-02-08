using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.Globalization;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Util;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;

namespace OS_Stripe
{

    public class PayData
    {

        public PayData(OrderData oInfo)
        {
            LoadSettings(oInfo);
        }

        public void LoadSettings(OrderData oInfo)
        {
            var settings = ProviderUtils.GetProviderSettings();
            var appliedtotal = oInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal");
            var alreadypaid = oInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/alreadypaid");

            ItemId = oInfo.PurchaseInfo.ItemID.ToString("");
            PostUrl = oInfo.PurchaseInfo.GetXmlProperty("genxml/posturl");
            CartName = "NBrightStore";
            SecretKey = settings.GetXmlProperty("genxml/textbox/secretkey");
            ApiBaseUrl = settings.GetXmlProperty("genxml/textbox/apibaseurl");
            if (ApiBaseUrl == "") ApiBaseUrl = "https://api.payplug.com";
            ApiVersion = settings.GetXmlProperty("genxml/textbox/apiversion");
            UrlVersionPath = settings.GetXmlProperty("genxml/textbox/urlversionpath");
            if (UrlVersionPath == "") UrlVersionPath = "v1";

            CurrencyCode = oInfo.PurchaseInfo.GetXmlProperty("genxml/currencycode");
            if (CurrencyCode == "") CurrencyCode = settings.GetXmlProperty("genxml/textbox/currencycode");

            var param = new string[3];
            param[0] = "orderid=" + oInfo.PurchaseInfo.ItemID.ToString("");
            param[1] = "status=1";
            ReturnUrl = Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
            param[0] = "orderid=" + oInfo.PurchaseInfo.ItemID.ToString("");
            param[1] = "status=0";
            ReturnCancelUrl = Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
            NotifyUrl = Utils.ToAbsoluteUrl("/DesktopModules/NBright/OS_Stripe/notify.ashx");
            MerchantLanguage = Utils.GetCurrentCulture();
            Amount = ((appliedtotal - alreadypaid) * 100).ToString();
            Email = oInfo.PurchaseInfo.GetXmlProperty("genxml/billaddress/genxml/textbox/email");
            if (!Utils.IsEmail(Email)) Email = oInfo.PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/textbox/cartemailaddress");
        }

        public string ItemId { get; set; }
        public string PostUrl { get; set; }
        public string VerifyUrl { get; set; }
        public string PayPalId { get; set; }
        public string CartName { get; set; }
        public string CurrencyCode { get; set; }
        public string ReturnUrl { get; set; }
        public string ReturnCancelUrl { get; set; }
        public string NotifyUrl { get; set; }
        public string MerchantLanguage { get; set; }
        public string Amount { get; set; }
        public string Email { get; set; }
        public string ShippingAmount { get; set; }
        public string TaxAmount { get; set; }
        public string SecretKey { get; set; }
        public string ApiBaseUrl { get; set; }
        public string ApiVersion { get; set; }
        public string UrlVersionPath { get; set; }
        public string ApiBasePath { get { return ApiBaseUrl + "/" + UrlVersionPath.Trim('/');  }  }
        
    }



}

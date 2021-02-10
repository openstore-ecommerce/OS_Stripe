using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace OS_Stripe
{
    public class RemotePost
    {
        private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();
        public string Url = "";
        public string Method = "post";
        public string FormName = "form";
        public void Add(string name, string value)
        {
            Inputs.Add(name, value);
        }

        public string GetPostHtml()
        {
            string sipsHtml = "";

            sipsHtml = "<html><head><script src='https://js.stripe.com/v3'></script>";
            sipsHtml += "<script>";
            sipsHtml += "var stripe = Stripe(\"" + Inputs["publickey"] + "\");";
            sipsHtml += "function redirecttostripe() { ";
            sipsHtml += " stripe.redirectToCheckout({ \"sessionId\":\"" + Inputs["sessionid"] + "\" }); ";
            sipsHtml += " }";
            sipsHtml += "</script>";
            sipsHtml += "</head><body onload=\"redirecttostripe()\">";
            sipsHtml += "<form name=\"" + FormName + "\" method=\"" + Method + "\" action=\"" + Url + "\">";
            int i = 0;
            for (i = 0; i <= Inputs.Keys.Count - 1; i += 1)
            {
                sipsHtml += "<input type=\"hidden\" name=\"" + Inputs.Keys[i] + "\" value=\"" + Inputs[Inputs.Keys[i]] + "\" />";
            }
            sipsHtml += "</form>";

            sipsHtml += "  <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" height=\"100%\">";
            sipsHtml += "<tr><td width=\"100%\" height=\"100%\" valign=\"middle\" align=\"center\">";
            sipsHtml += "<font style=\"font-family: Trebuchet MS, Verdana, Helvetica;font-size: 14px;letter-spacing: 1px;font-weight: bold;\">";
            sipsHtml += "Processing...";
            sipsHtml += "</font><br /><br /><img src='/DesktopModules/NBright/OS_Stripe/Themes/config/img/loading.gif' style='width:80px;height:80px;' />     ";
            sipsHtml += "</td></tr>";
            sipsHtml += "</table>";
            sipsHtml += "</body></html>";

            return sipsHtml;

        }

    }

}

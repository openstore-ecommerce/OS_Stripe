using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Services.Exceptions;
using Nevoweb.DNN.NBrightBuy.Components;
using Newtonsoft.Json;

namespace OS_Stripe
{
    public class StripeLimpet
    {
        private OrderData _oData;
        private PayData _payData;
        public StripeLimpet(OrderData oData)
        {
            _oData = oData;
            _payData = new PayData(oData);
        }

        public string RetrievePaymentRaw(string paymentID)
        {
            if (string.IsNullOrEmpty(paymentID)) return "";
            var uri = new Uri(_payData.ApiBasePath + routeRetrievePayment.Replace("{payment_id}", paymentID));
            return Get(uri);
        }
        public Dictionary<string, dynamic> RetrievePayment(string paymentID)
        {
            if (string.IsNullOrEmpty(paymentID)) return new Dictionary<string, dynamic>();   
            var uri = new Uri(_payData.ApiBasePath + routeRetrievePayment.Replace("{payment_id}", paymentID));
            var response = Get(uri);
            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(response);
        }

        public Dictionary<string, dynamic> CreatePayment()
        {
            var paymentData = new Dictionary<string, dynamic>
            {
                { "amount", Convert.ToInt32(_payData.Amount)},
                { "currency", _payData.CurrencyCode},
                {
                "customer", new Dictionary<string, object>
                    {
                        {"email", _payData.Email},
                        {"first_name",  _oData.PurchaseInfo.GetXmlProperty("genxml/billaddress/genxml/textbox/firstname") },
                        {"last_name", _oData.PurchaseInfo.GetXmlProperty("genxml/billaddress/genxml/textbox/lastname") }
                    }
                },
                {
                "hosted_payment", new Dictionary<string, object>
                    {
                        { "return_url", _payData.ReturnUrl},
                        { "cancel_url", _payData.ReturnCancelUrl}
                    }
                },
                { "notification_url", _payData.NotifyUrl + "?oid=" + _oData.PurchaseInfo.ItemID},
                {
                "metadata", new Dictionary<string, object>
                    {
                        { "customer_id", _payData.ItemId}
                    }
                },
            };


            //var paymentData = new Dictionary<string, dynamic>
            //{
            //    { "amount", 3300},
            //    { "currency", "EUR"},
            //    {
            //       "customer", new Dictionary<string, object>

            //        {
            //            { "email", "john.watson@example.net"},
            //            { "first_name", "John"},
            //            { "last_name", "Watson"}
            //        }
            //    },
            //    {
            //       "hosted_payment", new Dictionary<string, object>

            //        {
            //            { "return_url", "https://example.net/success?id=42710"},
            //            { "cancel_url", "https://example.net/cancel?id=42710"}
            //        }
            //    },
            //    { "notification_url", "https://example.net/notifications?id=42710"},
            //    {
            //       "metadata", new Dictionary<string, object>

            //        {
            //            { "customer_id", "42710"}
            //        }
            //    }
            //};


            var paymentjson = JsonConvert.SerializeObject(paymentData);
            var uri = new Uri(_payData.ApiBasePath + routeCreatePayment);
            var response = Post(uri, paymentjson);
            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(response);
        }

        /// <summary>
        /// Send an authenticated GET request.
        /// </summary>
        /// <param name="uri">The URI to the resource queried.</param>
        /// <returns>The response content.</returns>
        public string Get(Uri uri)
        {
            return Request("GET", uri, null);
        }

        /// <summary>
        /// Send an authenticated POST request.
        /// </summary>
        /// <param name="uri">The URI to the resource queried.</param>
        /// <param name="data">The request content.</param>
        /// <returns>The response content.</returns>
        public string Post(Uri uri, string data)
        {
            return Request("POST", uri, data);
        }

        /// <summary>
        /// Send an authenticated PATCH request.
        /// </summary>
        /// <param name="uri">The URI to the resource queried.</param>
        /// <param name="data">The request content.</param>
        /// <returns>The response content.</returns>
        public string Patch(Uri uri, string data)
        {
            return Request("PATCH", uri, data);
        }

        /// <summary>
        /// Send an authenticated DELETE request.
        /// </summary>
        /// <param name="uri">The URI to the resource queried.</param>
        /// <param name="data">The request content.</param>
        public void Delete(Uri uri, string data = null)
        {
            Request("DELETE", uri, data);
        }

        /// <summary>
        /// Perform an HTTP request.
        /// </summary>
        /// <param name="method">The HTTP verb (GET, POST, PUT, …).</param>
        /// <param name="uri">The URI to the resource queried.</param>
        /// <param name="data">The request content.</param>
        /// <param name="authorizationHeader">Authorization header used to authenticate the request.</param>
        /// <returns>The response content.</returns>
        private string Request(string method, Uri uri, string data)
        {

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            try
            {
                var request = WebRequest.Create(uri) as HttpWebRequest;

                request.Headers.Add("Authorization", "Bearer " + _payData.SecretKey);
                //if (_payData.ApiVersion != "") request.Headers.Add("PayPlug-Version", _payData.ApiVersion);

                request.Accept = "application/json";
                request.UserAgent = string.Format("PayPlug-OpenStore/{0}", "v1.0.0");
                request.Method = method;
                if (!string.IsNullOrEmpty(data))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(data);
                    request.ContentType = "application/json";
                    request.ContentLength = buffer.Length;
                    using (var rs = request.GetRequestStream())
                    {
                        rs.Write(buffer, 0, buffer.Length);
                    }
                }

                string response;
                var webResponse = request.GetResponse() as HttpWebResponse;
                using (var sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                }

                webResponse.Close();
                return response;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            return "";
        }


        #region "Routes"

        /// <summary>
        /// Generates an absolute URL to an API resource.
        /// </summary>
        /// <param name="path">The path format to access a resource.</param>
        /// <param name="pathParameters">Parameters required by the route.</param>
        /// <param name="queryParameters">Parameters of the query string.</param>
        /// <returns>The URI to access a resource.</returns>
        public Uri Uri(string path, Dictionary<string, string> pathParameters = null, Dictionary<string, string> queryParameters = null)
        {
            var absolute = new Uri(_payData.ApiBaseUrl);
            var relative = "";
            if (path != null)
            {
                if (!path.StartsWith("/")) relative += "/";
                relative += FormatFromDictionary(path, pathParameters);
            }

            if (queryParameters != null)
            {
                relative += ToQueryString(queryParameters);
            }

            return new Uri(absolute, relative);
        }

        /// <summary>
        /// Helper that provide a way to use named string parameter.
        /// </summary>
        /// <param name="formatString">The string to be formatted</param>
        /// <param name="valueDict">A dictionary used to replace the named parameter by some values.</param>
        /// <returns>The formatted string.</returns>
        private string FormatFromDictionary(string formatString, Dictionary<string, string> valueDict)
        {
            if (valueDict != null)
            {
                foreach (var tuple in valueDict)
                {
                    formatString = formatString.Replace("{" + tuple.Key + "}", tuple.Value);
                }
            }

            return formatString;
        }

        /// <summary>
        /// Helper that provide a way to build a query string.
        /// </summary>
        /// <param name="queryParameters">Keys values used to build the query string.</param>
        /// <returns>A query string.</returns>
        private string ToQueryString(Dictionary<string, string> queryParameters)
        {
            if (queryParameters.Count == 0)
            {
                return string.Empty;
            }

            var query = new List<string>();
            foreach (KeyValuePair<string, string> entry in queryParameters)
            {
                query.Add(string.Format("{0}={1}", HttpUtility.UrlEncode(entry.Key), HttpUtility.UrlEncode(entry.Value)));
            }

            return "?" + string.Join("&", query);
        }

        private const string routeCreatePayment = "/payments";
        private const string routeRetrievePayment = "/payments/{payment_id}";
        private const string routeListPayments = "/payments";
        private const string routeAbortPayments = "/payments/{payment_id}";
        private const string routeCreateRefund = "/payments/{payment_id}/refunds";
        private const string routeRetrieveRefund = "/payments/{payment_id}/refunds/{refund_id}";
        private const string routeListRefunds = "/payments/{payment_id}/refunds";
        private const string routeCreateCustomer = "/customers/{customer_id}";
        private const string routeRetrieveCustomer = "/customers/{customer_id}";
        private const string routeListCustomers = "/customers";
        private const string routeUpdateCustomer = "/customers/{customer_id}";
        private const string routeDeleteCustomer = "/customers/{customer_id}";
        private const string routeCreateCard = "/customers/{customer_id}/cards";
        private const string routeRetrieveCard = "/customers/{customer_id}/cards/{card_token}";
        private const string routeListCards = "/customers/{customer_id}/cards";
        private const string routeDeleteCard = "/customers/{customer_id}/cards/{card_token}";

        #endregion

    }
}

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
using Stripe.Checkout;
using Stripe;

namespace OS_Stripe
{
    public class StripeOptions
    {
        public string option { get; set; }
    }

    public class StripeLimpet
    {
        private OrderData _oData;
        private PayData _payData;
        public StripeLimpet(OrderData oData)
        {
            _oData = oData;
            _payData = new PayData(oData);
            StripeConfiguration.ApiKey = _payData.SecretKey;
        }

        public string CreateSession()
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                      UnitAmount = Convert.ToInt32(_payData.Amount),
                      Currency = _payData.CurrencyCode.ToLower(),
                      ProductData = new SessionLineItemPriceDataProductDataOptions
                      {
                        Name = _oData.OrderNumber,
                      },
                    },
                    Quantity = 1,
                  },
                },
                Mode = "payment",
                SuccessUrl = _payData.ReturnUrl,
                CancelUrl = _payData.ReturnCancelUrl,
            };
            var service = new SessionService();
            Session session = service.Create(options);
            return session.Id;
        }

    }
}

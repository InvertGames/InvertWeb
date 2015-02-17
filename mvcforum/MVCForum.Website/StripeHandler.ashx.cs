using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.IOC;
using Stripe;

namespace MVCForum.Website
{
    public class StripeHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {

            var container = UnityMVC3.BuildUnityContainer();
            var marketService = (IMarketService)container.Resolve(typeof (IMarketService), null);
            
            var json = new StreamReader(context.Request.InputStream).ReadToEnd();
            var stripeEvent = StripeEventUtility.ParseEvent(json);
            if (stripeEvent != null)
            {
                marketService.EventReceived(stripeEvent);
                context.Response.StatusCode = 200;
            }
            context.Response.StatusCode = 300;


        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Website.Controllers
{
    public class ApiV1Controller : Controller
    {
        public IMarketService Market { get; set; }
        public IUnitOfWorkManager UnitOfWorkManager { get; set; }
        public IMembershipService Membership { get; set; }

        public ApiV1Controller(IMarketService market, IUnitOfWorkManager unitOfWorkManager, IMembershipService membership)
        {
            Market = market;
            UnitOfWorkManager = unitOfWorkManager;
            Membership = membership;
        }

        public ActionResult Login(string username, string password)
        {
            if (Membership.ValidateUser(username, password, 5))
            {
                var user = Membership.GetUser(username);
                
                return Content(Token(user));
            }
            return Json(false);
        }

        private string Token(MembershipUser user)
        {
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {
                user.ApiToken = Guid.NewGuid().ToString();
                work.Commit();
            }
            return user.ApiToken;
        }

        public ActionResult Downloads(string token)
        {
            var user = Membership.GetUserByToken(token);
            if (user != null)
            {
                return Json(new {Downloads = Market.GetUserDownloads(user).Select(p=>new
                {
                    p.Product.Name,p.Version,p.Id
                }).ToArray(), Token = Token(user)},JsonRequestBehavior.AllowGet);
            }
            return Error("Invalid Token.");
        }

        public ActionResult Error(string message)
        {
            return Json(new {Message = message},JsonRequestBehavior.AllowGet);
        }
    }
}

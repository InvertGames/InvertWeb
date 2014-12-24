using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Data.UnitOfWork;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;
using Stripe;

namespace MVCForum.Website.Controllers
{
    public class MarketController : BaseController
    {
        public IMarketService MarketService { get; set; }
        public IMembershipService Membership { get; set; }

        public MarketController(IMarketService marketService, IMembershipService membership, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            MarketService = marketService;
            Membership = membership;
        }

        public ActionResult PurchaseCheckout(Guid productId)
        {
            var user = Membership.GetUser(Username);
            var vm = MarketService.Get(productId).Map();
            // vm = new MarketProductViewModel();
            if (vm.Images.Count > 0)
            {
                vm.MainImageUrl = vm.Images.First().Url;
            }
            //vm.MainImageUrl = "http://i.imgur.com/OJJzT5h.png";
            // vm.Title = "uFrame Game Framework";
            
            vm.ReleaseDate = DateTime.Now;
            vm.OnSale = false;
            vm.LicenseName = "Invert License";
            vm.Videos = new[]
            {
                new MarketProductVideoViewModel() { Url="https://www.youtube.com/watch?v=KiTe5nyNXfQ"}, 
            };
            vm.Seller = "Invert Game Studios LLC";
            vm.AllowEditing = UserIsAuthenticated && User.IsInRole("Admin");

            return View(vm);
        }

        [Authorize]
        public ActionResult PurchaseProduct(Guid productOptionsId)
        {
            return View(new MarketProductPurchaseViewModel()
            {
                MarketProductPurchaseOptionId = productOptionsId,
                NumberOfSeats = 1,

            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SaveProperty(MarketProductViewModel product, string propertyName)
        {

            //var user = Membership.GetUser(Username);

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {

                MarketService.SaveProperty(product.ProductId, propertyName, product.GetType().GetProperty(propertyName).GetValue(product, null).ToString());
                unitOfWork.Commit();
            }



            return RedirectToAction("PurchaseCheckout", new { productId = product.ProductId });
        }

        [HttpPost]
        [Authorize]
        public ActionResult UploadProductImage(AttachFileToPostViewModel model)
        {

            foreach (var item in model.Files)
            {
                using (var work = UnitOfWorkManager.NewUnitOfWork())
                {
                    //item.SaveAs(Server.MapPath("~/Content/ProductImages/{0}",item));
                    var image = MarketService.AddProductImage(model.UploadPostId);
                    item.SaveAs(Server.MapPath(image.Url));
                    work.Commit();
                }
            }



            return RedirectToAction("PurchaseCheckout", new { productId = model.UploadPostId });
        }
        [HttpPost]
        [Authorize]
        public ActionResult PurchaseProduct(MarketProductPurchaseViewModel product)
        {
            if (product.NumberOfSeats < 0)
            {

                return ErrorToHomePage("Number of seats must be greater than one.");
            }
            MarketService.PurchaseProduct(MembershipService.GetUser(Username), product.MarketProductPurchaseOptionId, product.Card, product.NumberOfSeats);

            return View("Success");
        }

        // GET: Market
        public ActionResult Index()
        {

            var marketViewModel = new MarketViewModel();
            marketViewModel.NewestProducts = MarketService.GetNewestProducts().ToArray();
            marketViewModel.TopProducts = MarketService.GetTopProducts().ToArray();

            if (UserIsAuthenticated)
            {
                marketViewModel.OwnedProducts = MarketService.GetOwnedProducts(Membership.GetUser(Username));
            }

            return View(marketViewModel);
        }

        public ActionResult AddToSubscription()
        {
            return null;
        }
    }

    public static class Mappings
    {
        public static MarketProductViewModel Map(this MarketProduct product)
        {
            var vm = new MarketProductViewModel()
            {
                ProductId = product.Id,
                Title = product.Title,
                Description = product.Description,
                PurchaseOptions = product.PurchaseOptions,
                ProductType = product.ProductType,
                IsLive = product.IsLive,
                MarketSeller = product.MarketSeller,
                Reviews = product.Reviews,
                Images = product.Images
            };
            return vm;
        }

        //public static MarketProductPurchaseOptionViewModel Map(this MarketProductPurchaseOption option)
        //{
        //    var vm = new MarketProductPurchaseOptionViewModel()
        //    {
        //        Title = option.PlanName,
        //         asdf  = option.BuyInPrice
        //    };
        //    return vm;
        //}
    }
}
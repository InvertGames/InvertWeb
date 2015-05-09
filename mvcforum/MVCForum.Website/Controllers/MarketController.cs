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
  
    public class MarketController : PageEditController
    {
        public IMarketService MarketService { get; set; }
        public IMembershipService Membership { get; set; }

        public MarketController(IPageContentService service, IMarketService marketService, IMembershipService membership, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService)
            : base(service, loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            MarketService = marketService;
            Membership = membership;
        }

        public ActionResult ProductsMenu()
        {
            
            return View(new ProductsMenuViewModel()
            {
                Product = MarketService.GetInvertProducts().ToArray()
            });
        }

        [Route("{name}/overview")]
        public ActionResult ProductInfo(string name)
        {
            return View("ProductInfo", GetProductViewModelByName(name).SetName("Overview"));
        }
        [Route("{name}/documentation")]
        public ActionResult Documentation(string name)
        {
            return View("ProductInfo", GetProductViewModelByName(name).SetName("Documentation"));
        }
        [Route("{name}/faq")]
        public ActionResult FAQ(string name)
        {
            return View("ProductInfo", GetProductViewModelByName(name).SetName("FAQ"));
        }
        [Route("{name}/videos")]
        public ActionResult Videos(string name)
         {
             return View("ProductInfo", GetProductViewModelByName(name).SetName("Videos"));
         }
        [Route("{name}/features")]
        public ActionResult Features(string name)
         {
             return View("ProductInfo", GetProductViewModelByName(name).SetName("Features"));
         }
        [Route("{name}/screenshots")]
        public ActionResult Screenshots(string name)
         {
             return View("ProductInfo", GetProductViewModelByName(name).SetName("Screenshots"));
         }
        [Route("{name}/purchase")]
        public ActionResult Purchase(string name)
         {
             return View("ProductInfo", GetProductViewModelByName(name).SetName("Purchase"));
         }
        private MarketProductViewModel GetProductViewModelByName(string name)
        {
            //var user = Membership.GetUser(Username);
            var product = MarketService.GetByName(name);
            Guid = product.Id;
            var vm = product.Map();
            if (vm.Images.Count > 0)
            {
                vm.MainImageUrl = vm.Images.First().Url;
            }
            vm.OnSale = false;
            vm.LicenseName = "Invert License";
            
            vm.Videos = new[]
            {
                new MarketProductVideoViewModel() {Url = "https://www.youtube.com/watch?v=KiTe5nyNXfQ"},
            };
            vm.Seller = "Invert Game Studios LLC";
            vm.AllowEditing = UserIsAuthenticated && User.IsInRole("Admin");
            return vm;
        }
        private MarketProductViewModel GetProductViewModel(Guid productId)
        {
            Guid = productId;
            var user = Membership.GetUser(Username);
            
            var vm = MarketService.Get(productId).Map();
            if (vm.Images.Count > 0)
            {
                vm.MainImageUrl = vm.Images.First().Url;
            }
            vm.OnSale = false;
            vm.LicenseName = "Invert License";
            vm.Videos = new[]
            {
                new MarketProductVideoViewModel() {Url = "https://www.youtube.com/watch?v=KiTe5nyNXfQ"},
            };
            vm.Seller = "Invert Game Studios LLC";
            vm.AllowEditing = UserIsAuthenticated && User.IsInRole("Admin");
            return vm;
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



            return RedirectToAction("ProductInfo", new { productId = product.ProductId });
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



            return RedirectToAction("ProductInfo", new { productId = model.UploadPostId });
        }
        [HttpPost]
        [Authorize]
        public ActionResult PurchaseProduct(MarketProductPurchaseViewModel product)
        {
            if (product.NumberOfSeats < 0)
            {
                ModelState.AddModelError("NumberOfSeats", "Number of seats must be greater than one.");
                return View(product);
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
                Images = product.Images,
                ReleaseDate = product.ReleaseDate,
                PriceLow = product.PurchaseOptions.Min(p=>p.BuyInPrice),
                ProductName = product.Name
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
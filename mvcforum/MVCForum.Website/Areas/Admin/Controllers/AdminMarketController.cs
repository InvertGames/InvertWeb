using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class AdminMarketController : BaseAdminController
    {
        public IMarketService MarketService { get; set; }
        
        // GET: Admin/AdminMarket
        public AdminMarketController(IMarketService marketService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService) : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            MarketService = marketService;
        }

        public ActionResult Index(int pageIndex = 0, int pageSize = 10)
        {

            var vm = new AdminProductsViewModel();
            vm.Products = MarketService.GetPaged(pageIndex, pageSize);
            return View();
        }
    }

    public class AdminProductsViewModel
    {
        public PagedList<MarketProduct> Products { get; set; }
    }

    public class AdminProductEditViewModel
    {
        
    }
}
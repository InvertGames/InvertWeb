using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Website.Controllers
{
    public class PageContentController : BaseController
    {
        public IPageContentService PageContentService { get; set; }
        // GET: PageContent
        public PageContentController(IPageContentService pageContentService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService) : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            PageContentService = pageContentService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Get(string friendlyId, bool isMarkdown = false)
        {
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {
                var vm = new PageContentViewModel();

                vm.IsEditable = User.IsInRole("Admin");
                var content = PageContentService.GetPageContent(friendlyId);
                vm.ContentFriendlyId = friendlyId;
                vm.ContentId = content.Id.ToString();
                vm.Content = content.Content;
                vm.IsMarkdown = isMarkdown;
                vm.ContentTitle = content.ContentTitle;
                work.Commit();
                return View(vm);
                
            
            }
         
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Save(PageContentViewModel vm)
        {
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {
                PageContentService.SavePageContent(vm.ContentFriendlyId, vm.Content);
                work.Commit();
            }
            return Redirect(this.Request.UrlReferrer.AbsolutePath);
        }
    }

    public class PageContentViewModel
    {
        public bool IsEditable { get; set; }
        public string ContentId { get; set; }
        public string ContentFriendlyId { get; set; }
           [UIHint(AppConstants.EditorType), AllowHtml]
        public string Content { get; set; }
        public string ContentTitle { get; set; }
        public bool IsMarkdown { get; set; }
    }
}
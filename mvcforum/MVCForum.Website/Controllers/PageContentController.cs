using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;

namespace MVCForum.Website.Controllers
{
    public class PageContentController : BaseController
    {
        
        // GET: PageContent
        public PageContentController(IPageContentService pageContentService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService)
            : base(pageContentService, loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            PageContentService = pageContentService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PublishContent(Guid rootId)
        {
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {
                PageContentService.PublishContent(rootId);
                work.Commit();
            }
            return Redirect(this.Request.UrlReferrer.PathAndQuery);
        }
        public ActionResult Get(string friendlyId,string parentId = null, bool isMarkdown = false, bool renderEditLink = true)
        {
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {

                var content = PageContentService.GetPageContent(friendlyId, string.IsNullOrEmpty(parentId) ? (Guid?)null : new Guid(parentId),draftVersion: User.IsInRole("Admin"));
                var vm = MapContent(content, isMarkdown, User);
                if (renderEditLink == false)
                {
                    vm.IsEditable = false;
                }
                work.Commit();

                return View(vm);
            }

        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddItem(string friendlyId,Guid? parentId)
        {
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {
                var listId = PageContentService.GetPageContent(friendlyId,parentId, false).Id;
                PageContentService.SavePageContent(Guid.NewGuid().ToString(), "New Item", listId);
                work.Commit();
            }

            return Redirect(this.Request.UrlReferrer.PathAndQuery);
        }
        [Authorize(Roles="Admin")]
        public ActionResult DeleteContent(string id)
        {
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {
                PageContentService.DeletePageContentListItem(id);
                work.Commit();
            }
            return Redirect(this.Request.UrlReferrer.PathAndQuery);
        }
        public static PageContentViewModel MapContent(PageContent content, bool isMarkdown, IPrincipal user)
        {
            var vm = new PageContentViewModel();
            vm.IsEditable = user.IsInRole("Admin");
            vm.PropertyName = content.FriendlyId;
            vm.ContentId = content.Id.ToString();   
          
            vm.Content = content.Content;
            vm.IsMarkdown = isMarkdown;
            vm.IsDraft = content.IsDraft;
            return vm;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Save(PageContentViewModel vm)
        {
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {
                PageContentService.SavePageContent(vm.PropertyName, vm.Content, vm.ParentId);
                work.Commit();
            }
            return Redirect(this.Request.UrlReferrer.PathAndQuery);
        }

        //public ActionResult ContentEdit(string friendlyId, string listFriendlyId, bool isMarkdown)
        //{
        //    using (var work = UnitOfWorkManager.NewUnitOfWork())
        //    {
        //        var content = PageContentService.GetPageContent(friendlyId, listFriendlyId,draftVersion: User.IsInRole("Admin"));
        //        var vm = MapContent(content, isMarkdown, User);

        //        work.Commit();

        //        return View(vm);
        //    }
        //}

        public ActionResult SaveProperties()
        {
            foreach (var item in this.Request.Form.AllKeys)
            {
                if (item.StartsWith("PAGECONTENT_"))
                {
                    using (var work = UnitOfWorkManager.NewUnitOfWork())
                    {
                        var propertyName = item.Replace("PAGECONTENT_", "");
                        var guid = Request.Form["PARENT_" + propertyName];
                        if (!string.IsNullOrEmpty(guid))
                        {
                            PageContentService.SavePageContent(propertyName, this.Request[item], new Guid(guid));
                            
                        }
                        else
                        {
                            PageContentService.SavePageContent(propertyName, this.Request[item], null);
                        }
                        
                        work.Commit();
                    }
                   
                }
            }
            return Redirect(this.Request.UrlReferrer.PathAndQuery);
        }
    }
   
    public class PageContentViewModel
    {
        private string _content;
        public bool IsEditable { get; set; }
        public string ContentId { get; set; }
        public string PropertyName { get; set; }
        
        public Guid? ParentId { get; set; }

        [UIHint(AppConstants.EditorType), AllowHtml]
        public string Content
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_content) || string.IsNullOrEmpty(_content))
                    return "[Empty]";
                return _content;
            }
            set { _content = value; }
        }

        public bool IsMarkdown { get; set; }
        public Func<HtmlString> Render { get; set; }
        //public Func<string, bool, HtmlString> PageContent { get; set; }
        public Func<string,MvcHtmlString> DeleteLink { get; set; }
        public string Label { get; set; }
        public bool IsDraft { get; set; }
    }
}
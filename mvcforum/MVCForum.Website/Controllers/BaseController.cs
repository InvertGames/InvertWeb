using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Routing;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Controllers
{
    /// <summary>
    /// A base class for the white site controllers
    /// </summary>
    public class BaseController : Controller
    {
        public IPageContentService PageContentService { get; set; }
        protected readonly IUnitOfWorkManager UnitOfWorkManager;
        protected readonly IMembershipService MembershipService;
        protected readonly ILocalizationService LocalizationService;
        protected readonly IRoleService RoleService;
        protected readonly ISettingsService SettingsService;
        protected readonly ILoggingService LoggingService;

        //private readonly MembershipUser _loggedInUser;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageContentServiceparam>
        /// <param name="loggingService"> </param>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="roleService"> </param>
        /// <param name="settingsService"> </param>
        public BaseController(IPageContentService pageContentService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService)
        {
            PageContentService = pageContentService;
            UnitOfWorkManager = unitOfWorkManager;
            MembershipService = membershipService;
            LocalizationService = localizationService;
            RoleService = roleService;
            SettingsService = settingsService;
            LoggingService = loggingService;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.RouteData.Values["controller"];
            var area = filterContext.RouteData.DataTokens["area"] ?? string.Empty;

            //if (Session[AppConstants.GoToInstaller] != null && Session[AppConstants.GoToInstaller].ToString() == "True")
            //{
            //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Install" }, { "action", "Index" } });
            //}
            if (SettingsService.GetSettings().IsClosed && !filterContext.IsChildAction)
            {
                // Only redirect if its closed and user is NOT in the admin
                if (controller.ToString().ToLower() != "closed" && controller.ToString().ToLower() != "members" && !area.ToString().ToLower().Contains("admin"))
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Closed" }, { "action", "Index" } });
                }
            }

        }

        protected bool UserIsAuthenticated
        {
            get
            {
                return System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }

        protected string Username
        {
            get
            {
                return UserIsAuthenticated ? System.Web.HttpContext.Current.User.Identity.Name : null;
            }
        }

        internal ActionResult ErrorToHomePage(string errorMessage)
        {
            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = errorMessage,
                MessageType = GenericMessages.error
            };
            // Not allowed in here so
            return RedirectToAction("Index", "Home");
        }
    }

    public class UserNotLoggedOnException : System.Exception
    {

    }

    public class PageEditController : BaseController
    {
        private Guid? _guid;

        public PageEditController(IPageContentService pageContentService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService)
            : base(pageContentService, loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
        }

        public virtual Guid? Guid
        {
            get { return _guid ?? (_guid = new Guid("689fa173-cc75-43db-8c05-a40b0122bf96")); }
            set { _guid = value; }
        }

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            return base.BeginExecute(requestContext, callback, state);
        }

        public EditablePageContext EditablePage { get; set; }
        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            Guid = null;
        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
            if (HttpContext.Items["EditablePage"] != null)
            {
                return;
            }
            EditablePage = new EditablePageContext()
            {
                Id = Guid.Value
            };

            //if ()
            //{
            using (var work = UnitOfWorkManager.NewUnitOfWork())
            {

                var pageContent = PageContentService.GetPageContentById(Guid.Value, User.IsInRole("Admin"));
                EditablePage.CanEdit = pageContent.IsDraft;
                // Set the page content to the draft version
            
                EditablePage.Id = pageContent.Id;
                work.Commit();
            }
            //}
            //else
            //{
                
            //}
            EditablePage.GetList = (name, parentId) =>
            {
                using (var work = UnitOfWorkManager.NewUnitOfWork())
                {

                    var content = PageContentService.GetPageContentList(name, parentId, EditablePage.CanEdit);
                    var vm = new ListPageContext
                    {
                        Items = content.Children.OrderBy(p => p.Order).Select(p => PageContentController.MapContent(p, false, User)).ToArray(),
                        Id = content.Id,
                        ListId = parentId,
                        Name = name,
                        IsEditable = UserIsAuthenticated && User.IsInRole("Admin")
                    };
                    work.Commit();
                    return vm;
                }
            };
            EditablePage.GetContentAction = (propertyName, parentId) =>
            {
                using (var work = UnitOfWorkManager.NewUnitOfWork())
                {

                    var content = PageContentService.GetPageContent(propertyName, parentId, EditablePage.CanEdit);
                    var vm = MapContent(content, false, User);
                    work.Commit();
                    return vm;
                }
            };

            HttpContext.Items["EditablePage"] = EditablePage;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);


        }
        public static PageContentViewModel MapContent(PageContent content, bool isMarkdown, IPrincipal user)
        {
            var vm = new PageContentViewModel();
            vm.IsEditable = user.IsInRole("Admin");
            vm.PropertyName = content.FriendlyId;
            vm.ContentId = content.Id.ToString();
            vm.ParentId = content.ParentId;
            vm.Content = content.Content;
            vm.IsMarkdown = isMarkdown;
            vm.IsDraft = content.IsDraft;

            return vm;
        }


    }
}
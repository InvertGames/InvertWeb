using System.Web.Mvc;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Website.Controllers
{
    public class DocsController : BaseController
    {
        // GET: Documentation
        [Route("docs/{name}")]
        public ActionResult Index(string name)
        {
            return View(new DocsViewModel() {Name = name, Page = "ChangeLog"});
        }

        [Route("docs/{name}/{page}.html")]
        public ActionResult DocsPage(string name, string page = "changelog")
        {
            return View("Index", new DocsViewModel() { Name = name, Page = page });
        }
        public DocsController(IPageContentService pageContentService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService)
            : base(pageContentService, loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
        }
    }
     public class DocsViewModel
    {
        public string Name { get; set; }
         public string Page { get; set; }
    }
}
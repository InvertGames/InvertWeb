using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;
using MVCForum.Website.Controllers;

namespace MVCForum.Website.Application
{
    public static class AppHelpers
    {
        #region Application

        public static string GetCurrentMvcForumVersion()
        {
            var version = ConfigUtils.GetAppSetting("MVCForumVersion");
            return version;
        }

        public static bool SameVersionNumbers()
        {
            var version = HttpContext.Current.Application["Version"].ToString();
            return GetCurrentMvcForumVersion() == version;
        }

        public static bool InInstaller()
        {
            var url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);
            if (!string.IsNullOrEmpty(url))
            {
                url = url.ToLowerInvariant();
                return url.Contains(AppConstants.InstallerUrl);
            }
            return false;
        }

        /// <summary>
        /// Returns true if the requested resource is one of the typical resources that needn't be processed by the cms engine.
        /// </summary>
        /// <param name="request">HTTP Request</param>
        /// <returns>True if the request targets a static resource file.</returns>
        /// <remarks>
        /// These are the file extensions considered to be static resources:
        /// .css
        ///	.gif
        /// .png 
        /// .jpg
        /// .jpeg
        /// .js
        /// .axd
        /// .ashx
        /// </remarks>
        public static bool IsStaticResource(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            string path = request.Path;
            string extension = VirtualPathUtility.GetExtension(path);

            if (extension == null) return false;

            switch (extension.ToLower())
            {
                case ".axd":
                case ".ashx":
                case ".bmp":
                case ".css":
                case ".gif":
                case ".htm":
                case ".html":
                case ".ico":
                case ".jpeg":
                case ".jpg":
                case ".js":
                case ".png":
                case ".rar":
                case ".zip":
                    return true;
            }

            return false;
        }

        public static bool IsDbInstalled()
        {
            var filePath = Path.Combine(HostingEnvironment.MapPath("~/App_Data/"), AppConstants.SuccessDbFile);
            //if (!File.Exists(filePath))
            //{
            //    using (File.Create(filePath))
            //    {
            //        //we use 'using' to close the file after it's created
            //    }
            //}
            return File.Exists(filePath);
        }

        #endregion

        #region Themes

        /// <summary>
        /// Gets the theme folders currently installed
        /// </summary>
        /// <returns></returns>
        public static List<string> GetThemeFolders()
        {
            var folders = new List<string>();
            var themeRootFolder = HttpContext.Current.Server.MapPath(String.Format("~/{0}", AppConstants.ThemeRootFolderName));
            if (Directory.Exists(themeRootFolder))
            {
                folders.AddRange(Directory.GetDirectories(themeRootFolder)
                                .Select(Path.GetFileName)
                                .Where(x => !x.ToLower().Contains("base")));
            }
            else
            {
                throw new ApplicationException("Theme folder not found");
            }
            return folders;
        }


        #endregion

        #region SEO

        private const string CanonicalNext = "<link href=\"{0}\" rel=\"next\" />";
        private const string CanonicalPrev = "<link href=\"{0}\" rel=\"prev\" />";
        private const string Canonical = "<link href=\"{0}\" rel=\"canonical\" />";

        public static string CanonicalPagingTag(int totalItemCount, int pageSize, HtmlHelper helper)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
            var currentAction = helper.ViewContext.RouteData.GetRequiredString("Action");
            var url = urlHelper.Action(currentAction, new { });

            var pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            var nextTag = String.Empty;
            var previousTag = String.Empty;

            var req = HttpContext.Current.Request["p"];
            var page = req != null ? Convert.ToInt32(req) : 1;

            // Sort the canonical tag out
            var canonicalTag = String.Format(Canonical, page <= 1 ? url : String.Format(AppConstants.PagingUrlFormat, url, page));

            // On the first page       
            if (pageCount > 1 & page <= 1)
            {
                nextTag = String.Format(CanonicalNext, String.Format(AppConstants.PagingUrlFormat, url, (page + 1)));
            }

            // On a page greater than the first page, but not the last
            if (pageCount > 1 & page > 1 & page < pageCount)
            {
                nextTag = String.Format(CanonicalNext, String.Format(AppConstants.PagingUrlFormat, url, (page + 1)));
                previousTag = String.Format(CanonicalPrev, String.Format(AppConstants.PagingUrlFormat, url, (page - 1)));
            }

            // On the last page
            if (pageCount > 1 & pageCount == page)
            {
                previousTag = String.Format(CanonicalPrev, String.Format(AppConstants.PagingUrlFormat, url, (page - 1)));
            }

            // return the canoncal tags
            return String.Concat(canonicalTag, Environment.NewLine,
                                    nextTag, Environment.NewLine,
                                    previousTag);
        }

        public static string CreatePageTitle(Entity entity, string fallBack)
        {
            if (entity != null)
            {
                if (entity is Category)
                {
                    var cat = entity as Category;
                    return cat.Name;
                }
                if (entity is Topic)
                {
                    var topic = entity as Topic;
                    return topic.Name;
                }
            }
            return fallBack;
        }

        public static string CreateMetaDesc(Entity entity)
        {
            return "";
        }

        #endregion

        #region Urls

        public static string CategoryRssUrls(string slug)
        {
            return String.Format("/{0}/rss/{1}", AppConstants.CategoryUrlIdentifier, slug);
        }

        #endregion

        #region String





        public static HtmlString Panel(this HtmlHelper helper, string friendlyId)
        {
            return helper.Partial("_Panel", friendlyId);
        }
        public static HtmlString Tabs(this HtmlHelper helper, string friendlyId)
        {
            return helper.Partial("_Tabs", friendlyId);
        }
        public static string ConvertPostContent(string post)
        {
            if (!string.IsNullOrEmpty(post))
            {
                // Convert any BBCode
                //post = StringUtils.ConvertBbCodeToHtml(post, false);

                // If using the PageDown/MarkDown Editor uncomment this line
                post = StringUtils.ConvertMarkDown(post);

                // Allow video embeds
                post = StringUtils.EmbedVideosInPosts(post);

                // Add Google prettify code snippets
                post = post.Replace("<pre>", "<pre class='prettyprint'>");
            }

            return post;
        }

        public static string ReturnBadgeUrl(string badgeFile)
        {
            return String.Concat("~/content/badges/", badgeFile);
        }

        #endregion

        #region Installer

        /// <summary>
        /// Get the previous version number if there is one from the web.config
        /// </summary>
        /// <returns></returns>
        public static string PreviousVersionNo()
        {
            return ConfigUtils.GetAppSetting("MVCForumVersion");
        }

        /// <summary>
        /// Gets the main version number (Used by installer)
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentVersionNo()
        {
            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            return String.Format("{0}.{1}", version.Major, version.Minor);
        }

        /// <summary>
        /// Get the full version number shown in the admin
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentVersionNoFull()
        {
            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            return String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        /// <summary>
        /// This checks whether the installer should be called, it stops people trying to call the installer
        /// when the application is already installed
        /// </summary>
        /// <returns></returns>
        public static bool ShowInstall()
        {
            //Installer for new versions and first startup
            // Store the value for use in the app
            var currentVersionNo = GetCurrentVersionNo();

            // Now check the version in the web.config
            var previousVersionNo = PreviousVersionNo();

            // If the versions are different kick the installer into play
            return (currentVersionNo != previousVersionNo);
        }

        #endregion

        #region Files

        public static string MemberImage(string avatar, string email, Guid userId, int size)
        {
            if (!string.IsNullOrEmpty(avatar))
            {
                // Has an avatar image
                return VirtualPathUtility.ToAbsolute(string.Concat(AppConstants.UploadFolderPath, userId, "/", avatar, string.Format("?width={0}&crop=0,0,{0},{0}", size)));
            }
            return StringUtils.GetGravatarImage(email, size);
        }

        public static UploadFileResult UploadFile(HttpPostedFileBase file, string uploadFolderPath, ILocalizationService localizationService, bool onlyImages = false)
        {
            var upResult = new UploadFileResult { UploadSuccessful = true };

            var fileName = Path.GetFileName(file.FileName);
            if (fileName != null)
            {
                //Before we do anything, check file size
                if (file.ContentLength > Convert.ToInt32(ConfigUtils.GetAppSetting("FileUploadMaximumFileSizeInBytes")))
                {
                    //File is too big
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = localizationService.GetResourceString("Post.UploadFileTooBig");
                    return upResult;
                }

                // now check allowed extensions
                var allowedFileExtensions = ConfigUtils.GetAppSetting("FileUploadAllowedExtensions");

                if (onlyImages)
                {
                    allowedFileExtensions = "jpg,jpeg,png,gif";
                }

                if (!string.IsNullOrEmpty(allowedFileExtensions))
                {
                    // Turn into a list and strip unwanted commas as we don't trust users!
                    var allowedFileExtensionsList = allowedFileExtensions.ToLower().Trim()
                                                     .TrimStart(',').TrimEnd(',').Split(',').ToList();

                    // Get the file extension
                    var fileExtension = Path.GetExtension(fileName.ToLower());

                    // If can't work out extension then just error
                    if (string.IsNullOrEmpty(fileExtension))
                    {
                        upResult.UploadSuccessful = false;
                        upResult.ErrorMessage = localizationService.GetResourceString("Errors.GenericMessage");
                        return upResult;
                    }

                    // Remove the dot then check against the extensions in the web.config settings
                    fileExtension = fileExtension.TrimStart('.');
                    if (!allowedFileExtensionsList.Contains(fileExtension))
                    {
                        upResult.UploadSuccessful = false;
                        upResult.ErrorMessage = localizationService.GetResourceString("Post.UploadBannedFileExtension");
                        return upResult;
                    }
                }

                // Sort the file name
                var newFileName = string.Format("{0}_{1}", GuidComb.GenerateComb(), fileName.Trim(' ').Replace("_", "-").Replace(" ", "-").ToLower());
                var path = Path.Combine(uploadFolderPath, newFileName);

                // Save the file to disk
                file.SaveAs(path);

                var hostingRoot = HostingEnvironment.MapPath("~/") ?? "";
                var fileUrl = path.Substring(hostingRoot.Length).Replace('\\', '/').Insert(0, "/");

                upResult.UploadedFileName = newFileName;
                upResult.UploadedFileUrl = fileUrl;
            }

            return upResult;
        }

        #endregion
    }

    public static class PageHelpers
    {
        public static EditablePageContext Context(this HtmlHelper helper, string pageName = null)
        {
            var context = helper.ViewContext.RequestContext.HttpContext.Items["EditablePage"] as EditablePageContext;
            //var context = helper.ViewBag.EditablePageContext as EditablePageContext;
            if (context == null)
            {
                return null;
            }

            context.Helper = helper;
            return context;
        }

        public static string Property(this HtmlHelper helper, string propertyName)
        {
            var vm = helper.Context().CurrentContext.AddProperty(propertyName);
            vm.IsEditable = false;
            return vm.Content;
        }
        public static T PropertyAs<T>(this HtmlHelper helper, string propertyName, Func<string, T> convertValue)
        {
            try
            {
                var vm = helper.Context().CurrentContext.AddProperty(propertyName);
                vm.IsEditable = false;
                var content = vm.Content.Trim();
                return convertValue(content);
            }
            catch (Exception ex)
            {
                return default(T);
            }

        }

        public static bool PropertyAsBool(this HtmlHelper helper, string propertyName)
        {
            return PropertyAs<bool>(helper, propertyName, v => v.ToUpper() == "YES");
        }
        public static int PropertyAsInt(this HtmlHelper helper, string propertyName, int @default = 0)
        {

            var res = PropertyAs<int>(helper, propertyName, Convert.ToInt32);

            return res == 0 ? @default : res;
        }
        public static MvcHtmlString Content(this HtmlHelper helper, string propertyName)
        {
            var vm = helper.Context().CurrentContext.AddProperty(propertyName);

            return helper.Partial("Get", vm);
        }
        public static MvcHtmlString ModalProperty(this HtmlHelper helper, string propertyName)
        {
            if (helper.Context() == null) return MvcHtmlString.Empty;
            var vm = helper.Context().CurrentContext.AddModalProperty(propertyName);
            return helper.Partial("Get", vm);
        }
        public static MvcHtmlString MarkdownProperty(this HtmlHelper helper, string propertyName)
        {
            if (helper.Context() == null) return MvcHtmlString.Empty;
            var vm = helper.Context().CurrentContext.AddModalProperty(propertyName);
            vm.IsMarkdown = true;
            return helper.Partial("Get", vm);
        }

        public static MvcHtmlString EditProperties(this HtmlHelper helper, string label = "Edit Properties")
        {
            if (helper.Context() == null) return MvcHtmlString.Empty;
           
            return new MvcHtmlString(helper.Context().CurrentContext.EditPropertiesLink(label));
        }
        public static ListPageContext List(this HtmlHelper helper, string listName, bool isMarkdown = true, bool global = false)
        {
            var context = helper.Context();
            if (context == null) return null;
            var content = context.GetList(listName, global ? null : context.CurrentContext.Id);
            foreach (var item in content.Items)
            {
                var item1 = item;

                // item.PageContent = (fId, isMarkDown) => helper.ModalProperty(item1.PropertyName + fId);
                item.DeleteLink =
                    (s) =>
                    {
                        if (!item1.IsEditable) return MvcHtmlString.Empty;
                        return helper.ActionLink(s ?? "Delete", "DeleteContent", "PageContent", new { id = item1.ContentId, friendlyName=content.ListId },
                            new object { });
                    };
                item.MoveUpLink =
                  (s) =>
                  {
                      if (!item1.IsEditable) return MvcHtmlString.Empty;
                      return helper.ActionLink(s ?? "Move Up", "MoveContentUp", "PageContent", new { id = item1.ContentId, friendlyName = content.ListId },
                          new object { });
                  };
                item.MoveDownLink =
                  (s) =>
                  {
                      if (!item1.IsEditable) return MvcHtmlString.Empty;
                      return helper.ActionLink(s ?? "Move Down", "MoveContentDown", "PageContent", new { id = item1.ContentId, friendlyName = content.ListId },
                          new object { });
                  };
                item.Render = () =>
                {
                    item1.IsMarkdown = isMarkdown;
                    return helper.ModalProperty(item1.PropertyName);

                };
            }
            context.PushContext(content);
            return content;
        }


    }

    public class EditablePageContext : IDisposable
    {
        private Stack<ContentContext> _contextStack = new Stack<ContentContext>();
        private string _pageName;

        public HtmlHelper Helper { get; set; }

        public Stack<ContentContext> ContextStack
        {
            get { return _contextStack ?? (_contextStack = new Stack<ContentContext>()); }
            set { _contextStack = value; }
        }

        public EditablePageContext()
        {

        }

        public ContentContext CurrentContext
        {
            get
            {
                if (ContextStack.Count < 1)
                {
                    ContextStack.Push(new ContentContext()
                    {
                        Id = Id,
                        PageContext = this
                    });
                    return ContextStack.Peek();
                }
                return ContextStack.Peek();
            }
        }

        public string PageName
        {
            get { return _pageName; }
            set
            {
                _pageName = value;


            }
        }

        public bool CanEdit { get; set; }

        public ContentContext PushContext(ContentContext context)
        {
            context.Parent = CurrentContext;
            context.PageContext = this;
            ContextStack.Push(context);
            return context;
        }

        public void PopContext()
        {
            if (ContextStack.Count < 2) return;
            ContextStack.Pop();
        }
        public Func<string, Guid?, PageContentViewModel> GetContentAction { get; set; }
        public Func<string, Guid?, ListPageContext> GetList { get; set; }

        public PageContentViewModel GetContent(string name)
        {

            var result = GetContentAction(name, CurrentContext.Id);
            if (result.IsDraft)
            {
                DraftCount++;
            }
            return result;
        }

        public int DraftCount { get; set; }
        public Guid Id { get; set; }

        public void Dispose()
        {
            foreach (var item in ContextStack.ToArray())
            {
                item.PageContext = this;
                item.Dispose();
            }
        }


    }

    public class ContentContext : IDisposable
    {
        public ContentContext Parent { get; set; }
        private List<PageContentViewModel> _properties = new List<PageContentViewModel>();
        private List<PageContentViewModel> _modalProperties = new List<PageContentViewModel>();
        public EditablePageContext PageContext { get; set; }
        public Guid? Id { get; set; }

        public Guid? ParentId
        {
            get
            {
                if (Parent == null)
                    return null;

                return Parent.Id;
            }
        }

        public string EditPropertiesLink(string text = "Edit Properties")
        {

            return
                string.Format(
                    "<a class=' style='cursor: pointer; font-size: 10px !important;' data-toggle='modal' data-target='#{0}-Editor'>{1}</a>", Id, text);

        }
        public bool CanEdit
        {
            get { return PageContext.CanEdit; }
        }
        public HtmlHelper Helper
        {
            get { return PageContext.Helper; }
        }
        public PageContentViewModel AddProperty(string propertyName)
        {
            var existing = Properties.FirstOrDefault(p => p.PropertyName == propertyName);
            if (existing != null) return existing;

            var content = PageContext.GetContent(propertyName);
            content.Label = propertyName;
            content.IsEditable = false;
            Properties.Add(content);
            return content;
        }
        public PageContentViewModel AddContent(string propertyName)
        {
            var content = AddModalProperty(propertyName);
            var existing = ModalProperties.FirstOrDefault(p => p.PropertyName == propertyName);
            if (existing != null) return existing;
            content.Label = propertyName;
            content.IsEditable = CanEdit;
            content.IsMarkdown = true;
            return content;
        }
        public PageContentViewModel AddModalProperty(string propertyName)
        {
            var existing = ModalProperties.FirstOrDefault(p => p.PropertyName == propertyName);
            if (existing != null) return existing;
            var content = PageContext.GetContent(propertyName);
            content.Label = propertyName;
            ModalProperties.Add(content);
            return content;
        }
        public string Name { get; set; }

        public string FullName
        {
            get
            {
                return string.Join("_", PageContext.ContextStack.Reverse().Select(p => p.Name));
                var sb = new StringBuilder();
                var first = true;
                foreach (var item in PageContext.ContextStack.Reverse())
                {
                    if (!first)
                    {
                        sb.Append("_");
                    }
                    sb.Append(item.Name);
                    first = false;
                }
                return sb.ToString();
            }
        }

        public List<PageContentViewModel> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        public List<PageContentViewModel> ModalProperties
        {
            get { return _modalProperties; }
            set { _modalProperties = value; }
        }

        public virtual void Dispose()
        {
            if (Properties.Count > 0 || ModalProperties.Count > 0)
                Helper.RenderPartial("EditablePage", this);

            PageContext.PopContext();
        }
    }

    public class ListPageContext : ContentContext, IEnumerable<PageContentViewModel>, IEnumerator<PageContentViewModel>
    {

        public IEnumerable<PageContentViewModel> Items { get; set; }
        public bool IsEditable { get; set; }
        public Guid? ListId { get; set; }



        private bool _renderedAddNewLink = false;
        public MvcHtmlString AddNewLink
        {
            get
            {
                _renderedAddNewLink = true;
                return Helper.ActionLink("Add New", "AddItem", "PageContent", new { friendlyId = Name, parentId = ListId }, new object { });
            }

        }

        public override void Dispose()
        {
            base.Dispose();
            //if (IsEditable && !_renderedAddNewLink)
            //{
            //    Helper.ViewContext.Writer.Write(AddNewLink);

            //}


        }

        IEnumerator<PageContentViewModel> IEnumerable<PageContentViewModel>.GetEnumerator()
        {
            return this;
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        private int _start = -1;
        private PageContentViewModel[] _iteratorItems;
        private ListItemContext _lastIteratorContext;
        public bool MoveNext()
        {
            if (_iteratorItems == null)
            {
                Reset();
            }

            if (_lastIteratorContext != null)
                _lastIteratorContext.Dispose();

            _start++;

            if (_start >= _iteratorItems.Length)
            {
                _iteratorItems = null;
                return false;
            }
            Current = _iteratorItems[_start];

            _lastIteratorContext = new ListItemContext(_iteratorItems[_start])
            {
                Id = new Guid(_iteratorItems[_start].ContentId),
                Name = _iteratorItems[_start].PropertyName
            };
            PageContext.PushContext(_lastIteratorContext);

            return true;
        }

        public void Reset()
        {
            _start = -1;
            _iteratorItems = this.Items.ToArray();
            _lastIteratorContext = null;
        }

        public PageContentViewModel Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }

    public class ListItemContext : ContentContext
    {
        public PageContentViewModel IteratorItem { get; set; }

        public ListItemContext(PageContentViewModel iteratorItem)
        {
            IteratorItem = iteratorItem;

        }

    }
    public class EditablePageProperty
    {

    }
}
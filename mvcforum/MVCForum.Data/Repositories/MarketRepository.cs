using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public class MarketRepository : IMarketRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public MarketRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IEnumerable<MarketProduct> GetAll()
        {
            return _context.MarketProduct.ToList();
        }



        public PagedList<MarketProduct> GetPagedProducts(int pageIndex, int pageSize)
        {
            var total = _context.MarketProduct.Count();
            var result = _context.MarketProduct.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return new PagedList<MarketProduct>(result, pageIndex, pageSize, total);
        }

        public MarketProduct GetMarketProduct(Guid id)
        {
            return _context.MarketProduct
                .Include(p => p.PurchaseOptions)
                .Include(p => p.MarketSeller)
                .Include(p => p.Images)
                .Include(p => p.Videos)
                .FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<MarketProduct> GetProductsByMember(Guid sellerId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MarketProduct> GetProductsBySeller(Guid sellerId)
        {
            var info = _context.MarketSeller.FirstOrDefault(p => p.Id == sellerId);
            if (info == null) throw new Exception("Seller not found.");
            return info.Products;
        }

        public IEnumerable<MarketProduct> GetProductsByLicense(Guid licenseId)
        {
            throw new NotImplementedException();
        }

        public MarketProductPurchaseOption GetProductOption(Guid purchaseOptionId)
        {
            var purchaseOption = _context.MarketProductPurchaseOption.Include(p => p.Product).FirstOrDefault(p => p.Id == purchaseOptionId);
            return purchaseOption;
        }

        public void AddProductImage(MarketProductImage image)
        {
            _context.MarketProductImage.Add(image);
        }

        public IEnumerable<MarketProduct> GetProductsByStripePlanId(string planId)
        {
            return
                _context.MarketProductPurchaseOption.Where(p => p.StripePlanId == planId)
                    .Select(p => p.Product)
                    .Distinct();
        }

        public MarketProduct GetByName(string name)
        {
            return _context.MarketProduct
               .Include(p => p.PurchaseOptions)
               .Include(p => p.MarketSeller)
               .Include(p => p.Images)
               .Include(p => p.Videos)
               .FirstOrDefault(p => p.Name == name);
        }

        public IEnumerable<MarketProductDownload> GetDownloadsByRole( MembershipRole role)
        {
            //foreach (var item in role.CategoryPermissionForRole) { }
            return _context.MarketProductDownload.Include(x=>x.Product).Where(p => p.RoleName == role.RoleName);
        }
    }

    public class PageContentRepository : IPageContentRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public PageContentRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }
        public PageContent SavePageContent(string friendlyId, string content, Guid? parentId)
        {
            var pageContent = GetPageContent(friendlyId, parentId, true, true);

            pageContent.Content = content;
            pageContent.IsDraft = true;

            return pageContent;

        }


        public PageContent GetPageContentList(string friendlyId, Guid? parentId, bool includeDrafts)
        {

            var listContent = GetPageContent(friendlyId, parentId, includeDrafts, true);
            var list = new List<PageContent>();
            foreach (var item in _context.PageContent.Where(p => p.ParentId == listContent.Id && p.IsDraft == includeDrafts))
            {
                list.Add(item);
            }

            listContent.Children = list.OrderBy(p => p.Order).ToList();
            return listContent;
        }


        public PageContent CreateDraft(Guid id)
        {
            // Grab all of the current data that is not a draft
            var pageContent = _context.PageContent.First(p => p.Id == id && !p.IsDraft);
            var draft = EnsureDraft(pageContent);
            return draft;
        }
        // TODO right now this can only be called by admin
        public PageContent GetPageContentById(Guid guid, bool draftVersion)
        {
            // Look for the draft version first
            
            var result = _context.PageContent.FirstOrDefault(p => p.ContentId == guid && p.IsDraft == draftVersion);
            if (result != null)
            {
                return result;
            }
  
            // Look for the non draft version
            if (draftVersion)
            result = _context.PageContent.FirstOrDefault(p => p.ContentId == guid && !p.IsDraft);

            if (result == null)
            {
                var content = new PageContent()
                {
                    Id = guid,
                    Content = string.Empty,
                    IsDraft = false,
                    Order = 0,
                    ParentId = null,
                    FriendlyId = guid.ToString(),
                };
                content.ContentId = content.Id;
                _context.PageContent.Add(content);
                return content;
            }
            return result;
        }

        //public PageContent PublishDrafts(Guid? id)
        //{
        //    // Grab all of the page content for this draft
        //    var pageContent = _context.PageContent.FirstOrDefault(p => p.Id == id);
        //    WalkContent(id,);
        //}

        public void MovePageContentUp(string listId, Guid itemId, Guid? parentId)
        {
            var list = GetPageContentList(listId, parentId, true);
            var array = _context.PageContent.Where(p => p.ParentId == parentId && p.IsDraft).OrderBy(p => p.Order).ToList();
            var drafts = new List<PageContent>(array);

            var item = drafts.FirstOrDefault(p => p.Id == itemId);
            var index = drafts.IndexOf(item);


            drafts.RemoveAt(index);
            drafts.Insert(index -1, item);

            for (int index1 = 0; index1 < drafts.Count; index1++)
            {
                var i = drafts[index1];

                i.Order = index1;
            }

        }
        public void MovePageContentDown(string listId, Guid itemId, Guid? parentId)
        {

            var list = GetPageContentList(listId, parentId, true);
            var array = _context.PageContent.Where(p => p.ParentId == parentId && p.IsDraft).OrderBy(p => p.Order).ToList();
            var drafts = new List<PageContent>(array);

            var item = drafts.FirstOrDefault(p => p.Id == itemId);
            var index = drafts.IndexOf(item);


            drafts.RemoveAt(index);
            drafts.Insert(index + 1, item);

            for (int index1 = 0; index1 < drafts.Count; index1++)
            {
                var i = drafts[index1];

                i.Order = index1;
            }

        }

        public void DeletePageContentListItem(Guid itemId)
        {

            _context.PageContent.Remove(_context.PageContent.FirstOrDefault(p => p.Id == itemId));
            WalkContent(itemId, _ => _context.PageContent.Remove(_));
        }

        public void PublishContent(Guid? id)
        {
            // Publish the root item
            var root = _context.PageContent.First(p => p.Id == id);
            PublishIfDraft(root);
            // Publish each child of that page
            WalkContent(root.Id, PublishIfDraft);
            //WalkContent(null, PublishIfDraft);

        }

        private void PublishIfDraft(PageContent item)
        {
            if (item.IsDraft)
            {
                var originalContent =
                    _context.PageContent.FirstOrDefault(p => p.ContentId == item.ContentId && p.IsDraft == false);


                // Remove the original content if any
                if (originalContent != null)
                {
                    _context.PageContent.Remove(originalContent);
    
                }
                
                item.IsDraft = false;
            }
        }

        private PageContent EnsureDraft(PageContent item)
        {

            if (item.IsDraft)
            {

                return item;
            }
            var existingDraft =
                    _context.PageContent.FirstOrDefault(p => p.ContentId == item.ContentId && item.IsDraft);

            if (existingDraft != null)
            {
                existingDraft.ContentId = item.ContentId;
                return existingDraft;
            }

            var draftVersion = new PageContent()
            {
                Id = Guid.NewGuid(),
                Content = item.Content,
                Order = item.Order,
                FriendlyId = item.FriendlyId,
                IsDraft = true,
                ParentId = item.ParentId,
                ContentId = item.ContentId
            };
            foreach (var child in _context.PageContent.Where(p => p.ParentId == item.Id && !p.IsDraft))
            {
                var draft = EnsureDraft(child);
                draft.ParentId = draftVersion.Id;
                draft.ContentId = child.ContentId;
            }
            _context.PageContent.Add(draftVersion);
            return draftVersion;
        }
        public void WalkContent(Guid? id, Action<PageContent> contentAction)
        {
            WalkContent(id, p => true, contentAction);
        }

        public void WalkContent(Guid? id, Predicate<PageContent> filter, Action<PageContent> contentAction)
        {
            var items = _context.PageContent.Where(p => p.ParentId == id).ToArray();
            foreach (var item in items.Where(p => filter(p)))
            {
                contentAction(item);
                WalkContent(item.Id, filter, contentAction);
            }

        }
        public PageContent GetPageContent(string friendlyId, Guid? parentId, bool draftVersion = false, bool autoCreate = false)
        {
            var result =  _context.PageContent.FirstOrDefault(
                        p => p.FriendlyId == friendlyId && p.ParentId == parentId && p.IsDraft == draftVersion);
            if (result == null)
            {
                var content = new PageContent()
                {
                    Content = string.Empty,
                    IsDraft = draftVersion,
                    Order = 0,
                    ParentId = parentId,
                    FriendlyId = friendlyId,
                };
                content.ContentId = content.Id;
                _context.PageContent.Add(content);
                return content;
            }
            return result;
        }
    }

    public class UserLicenseRepository : IUserLicenseRepository
    {
        private readonly MVCForumContext _context;

        public UserLicenseRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public MarketProductUserLicense GetLicense(Guid userId, Guid licenseId)
        {
            return _context.UserLicense.FirstOrDefault(p => p.User.Id == userId);
        }

        public IEnumerable<MarketProductUserLicense> GetLicenses(Guid userId)
        {
            return _context.UserLicense.Include(p => p.PurchaseOption)
                .Include(p => p.PurchaseOption.Product).Where(p => p.User.Id == userId);
        }


        public MarketProductUserLicense CreateLicense(Guid purchaseOptionId, string username)
        {
            var license = new MarketProductUserLicense
            {
                PurchaseOption = _context.MarketProductPurchaseOption.FirstOrDefault(p => p.Id == purchaseOptionId),
                User = _context.MembershipUser.FirstOrDefault(p => p.UserName == username)
            };
            return license;
        }
    }
}

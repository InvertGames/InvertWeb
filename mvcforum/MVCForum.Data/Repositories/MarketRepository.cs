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

        //public void SavePageContentTitle(string friendlyId, string title)
        //{
        //    var pageContent = GetPageContent(friendlyId, null);
        //    pageContent.ContentTitle = title;

        //}

        public PageContent GetPageContentList(string friendlyId, Guid? parentId, bool includeDrafts)
        {

            var listContent = GetPageContent(friendlyId, parentId, false, true);
            listContent.IsDraft = false;
            var list = new List<PageContent>();
            foreach (var item in _context.PageContent.Where(p => p.ParentId == listContent.Id))
            {

                if (includeDrafts && item.IsDraft)
                {
                    //var content = GetPageContent(item.FriendlyId, item.ParentId, true);
                    
                    list.Add(item);
                }
                else
                {
                    if (!item.IsDraft)
                    {
                        list.Add(item);
                    }
                }



            }
            if (includeDrafts)
                foreach (var item in list.Where(p => p.IsDraft).ToArray())
                {
                    var nonDraftItem = list.FirstOrDefault(p => p.FriendlyId == item.FriendlyId && !p.IsDraft);
                    if (nonDraftItem != null)
                    {
                        list.Remove(nonDraftItem);
                    }
                }

            listContent.Children = list.OrderBy(p => p.Order).ToList();
            return listContent;
        }

        //public void SavePageContentListItem(string listFriendlyId, Guid itemId, string content)
        //{

        //    var contentItem = GetPageContentItem(itemId);
        //    contentItem.ContentList = _context.PageContentList.FirstOrDefault(p => p.FriendlyId == listFriendlyId);
        //    contentItem.Content = content;

        //}

        public void MovePageContentUp(string listId, Guid itemId, Guid? parentId)
        {
            var list = GetPageContentList(listId, parentId, true);
            var array = list.Children.OrderBy(p => p.Order).ToList();
            var drafts = new List<PageContent>();
            var newId = Guid.Empty;
            foreach (var original in array)
            {
                var draft = EnsureDraft(original);
                drafts.Add(draft);
                if (original.Id == itemId)
                {
                    newId = draft.Id;
                }
            }
            var item = drafts.FirstOrDefault(p => p.Id == newId);
            var index = drafts.IndexOf(item);


            drafts.RemoveAt(index);
            drafts.Insert(index - 1, item);

            for (int index1 = 0; index1 < drafts.Count; index1++)
            {
                var i = drafts[index1];

                i.Order = index1;
                //i.IsDraft = true;
            }

        }
        public void MovePageContentDown(string listId, Guid itemId, Guid? parentId)
        {

            var list = GetPageContentList(listId, parentId, true);
            var array = list.Children.OrderBy(p => p.Order).ToList();
            var drafts = new List<PageContent>();
            var newId = Guid.Empty;
            foreach (var original in array)
            {
                var draft = EnsureDraft(original);
                drafts.Add(draft);
                if (original.Id == itemId)
                {
                    newId = draft.Id;
                }
            }
            var item = drafts.FirstOrDefault(p => p.Id == newId);
            var index = drafts.IndexOf(item);


            drafts.RemoveAt(index);
            drafts.Insert(index + 1, item);

            for (int index1 = 0; index1 < drafts.Count; index1++)
            {
                var i = drafts[index1];

                i.Order = index1;
                i.IsDraft = true;
            }

        }

        public void DeletePageContentListItem(Guid itemId)
        {

            _context.PageContent.Remove(_context.PageContent.FirstOrDefault(p => p.Id == itemId));
            WalkContent(itemId, _ => _context.PageContent.Remove(_));
        }

        public void PublishContent(Guid? id)
        {
            WalkContent(null, PublishIfDraft);
            WalkContent(id, PublishIfDraft);
        }

        private void PublishIfDraft(PageContent item)
        {
            if (item.IsDraft)
            {
                var originalContent =
                    _context.PageContent.FirstOrDefault(
                        p => p.FriendlyId == item.FriendlyId && p.ParentId == item.ParentId && p.IsDraft == false);
                // If there isn't an original, make it the original
                if (originalContent == null)
                {
                    item.IsDraft = false;
                }
                else // if there is any original copy back to the original
                {
                    // Transfer draft stuff to original
                    //originalContent.Id = item.Id;
                    originalContent.Content = item.Content;
                    originalContent.Order = item.Order;
                    originalContent.IsDraft = false;
                    _context.PageContent.Remove(item);
                }
            }
        }

        private PageContent EnsureDraft(PageContent item)
        {

            if (item.IsDraft)
            {

                return item;
            }

            var draftVersion = new PageContent()
            {
                Id = Guid.NewGuid(),
                Content = item.Content,
                Order = item.Order,
                FriendlyId = item.FriendlyId,
                IsDraft = true,
                ParentId = item.ParentId,
            };

            foreach (var child in _context.PageContent.Where(p => p.ParentId == item.Id))
            {
                var draft = EnsureDraft(child);

                draft.ParentId = draftVersion.Id;
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
            var pageContent = _context.PageContent.FirstOrDefault(p => p.FriendlyId == friendlyId && p.ParentId == parentId && p.IsDraft == draftVersion);

            if (pageContent == null)
            {
                pageContent = new PageContent
                {
                    FriendlyId = friendlyId,
                    Content = string.Empty,
                    ParentId = parentId
                };

                if (draftVersion)
                {
                    var nonDraftVersion =
                          _context.PageContent
                              .FirstOrDefault(p => p.FriendlyId == friendlyId && p.ParentId == parentId && !p.IsDraft);
                    if (nonDraftVersion != null)
                    {

                        pageContent.Content = nonDraftVersion.Content;
                        if (!autoCreate)
                        {
                            return nonDraftVersion;
                        }
                    }

                }
                if (!autoCreate)
                {
                    return pageContent;
                }
                pageContent.IsDraft = true;
                _context.PageContent.Add(pageContent);
            }
            return pageContent;
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

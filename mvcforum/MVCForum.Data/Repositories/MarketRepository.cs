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
            var result =  _context.MarketProduct.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            
            return new PagedList<MarketProduct>(result,pageIndex,pageSize,total);
        }

        public MarketProduct GetMarketProduct(Guid id)
        {
            return _context.MarketProduct
                .Include(p=>p.PurchaseOptions)
                .Include(p=>p.MarketSeller)
                .Include(p=>p.Images)
                .Include(p=>p.Videos)
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
            var purchaseOption = _context.MarketProductPurchaseOption.Include(p=>p.Product).FirstOrDefault(p => p.Id == purchaseOptionId);
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
        public void SavePageContent(string friendlyId, string content)
        {
            var pageContent = GetPageContent(friendlyId);
            pageContent.Content = content;
            
        }
        public void SavePageContentTitle(string friendlyId, string title)
        {
            var pageContent = GetPageContent(friendlyId);
            pageContent.ContentTitle = title;

        }
        public PageContent GetPageContent(string friendlyId)
        {
            var pageContent = _context.PageContent.FirstOrDefault(p => p.FriendlyId == friendlyId);
            if (pageContent == null)
            {
                pageContent = new PageContent
                {
                    FriendlyId = friendlyId,
                    Content = string.Empty,
                    ContentTitle = string.Empty
                };
                _context.PageContent.Add(pageContent);
            }
            return pageContent;
        }
    }
}

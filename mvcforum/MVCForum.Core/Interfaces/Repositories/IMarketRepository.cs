using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IMarketRepository
    {
        IEnumerable<MarketProduct> GetAll();
        PagedList<MarketProduct> GetPagedProducts(int pageIndex, int pageSize);
        MarketProduct GetMarketProduct(Guid id);
        
        IEnumerable<MarketProduct> GetProductsByMember(Guid sellerId);
        IEnumerable<MarketProduct> GetProductsBySeller(Guid sellerId);
        IEnumerable<MarketProduct> GetProductsByLicense(Guid licenseId);


        MarketProductPurchaseOption GetProductOption(Guid purchaseOptionId);


        void AddProductImage(MarketProductImage image);


        IEnumerable<MarketProduct> GetProductsByStripePlanId(string planId);
    }

    public interface IPageContentRepository
    {
        void SavePageContent(string friendlyId, string content);
        PageContent GetPageContent(string friendlyId);


        void SavePageContentTitle(string friendlyId, string title);
    }
}
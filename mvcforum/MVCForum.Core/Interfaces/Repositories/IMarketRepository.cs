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
        PageContent SavePageContent(string friendlyId, string content, Guid? parentId);
        PageContent GetPageContent(string friendlyId, Guid? parentId, bool draftVersion = false, bool autoCreate = false);
        //void SavePageContentTitle(string friendlyId, string title);

        PageContent GetPageContentList(string friendlyId, Guid? parentId, bool draftVersions);

        void MovePageContentUp(string listId, Guid itemId, Guid? parentId);
        void MovePageContentDown(string listId, Guid itemId, Guid? parentId);
        void DeletePageContentListItem(Guid itemId);
        void PublishContent(Guid? id);
    }

    public interface IUserLicenseRepository
    {
        MarketProductUserLicense GetLicense(Guid userId, Guid licenseId);
        IEnumerable<MarketProductUserLicense> GetLicenses(Guid userId);
        MarketProductUserLicense CreateLicense(Guid purchaseOptionId, string username);
        
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Website.ViewModels
{

    public class MarketViewModel
    {
        public IEnumerable<MarketProduct> NewestProducts { get; set; }
        public IEnumerable<MarketProduct> OwnedProducts { get; set; }
        public IEnumerable<MarketProduct> TopProducts { get; set; }

    }



    public class MarketProductViewModel
    {
        public string PageName { get; set; }

        public MarketProductViewModel SetName(string name)
        {
            PageName = name;
            return this;
        }
        public Guid ProductId { get; set; }
        public bool AllowEditing { get; set; }
        public string Title { get; set; }
        [UIHint(AppConstants.EditorType), AllowHtml]
        public string Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string MainImageUrl { get; set; }
        public bool UserOwnsProduct
        {
            get { return Ownership != null; }
        }

        public UserProductOwnershipViewModel Ownership { get; set; }
        public string RequiredVersion { get; set; }

        public string Seller { get; set; }
        public bool OnSale { get; set; }

        public decimal PriceLow { get; set; }

        public IList<MarketProductImage> Images { get; set; }
        public IEnumerable<MarketProductVideoViewModel> Videos { get; set; }
        public IEnumerable<MarketProductPurchaseOption> PurchaseOptions { get; set; }
        public IEnumerable<MarketProductReview> Reviews { get; set; }

        public string LicenseName { get; set; }
        public string ProductType { get; set; }
        public bool IsLive { get; set; }
        public MarketSellerInfo MarketSeller { get; set; }
        public string ProductName { get; set; }
    }

    public class AdminProductViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        [UIHint(AppConstants.EditorType), AllowHtml]
        public string Description { get; set; }

        public string RequiredVersion { get; set; }

        public string DocumentationUrl { get; set; }

        public string SupportEmail { get; set; }


    }



    public class UserProductOwnershipViewModel
    {

    }
    public class MarketProductImageViewModel
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }

    public class MarketProductVideoViewModel
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }

    public class MarketProductPurchaseOptionViewModel
    {


    }

    public class UploadProductDownloadViewModel
    {   
        public HttpPostedFileBase File { get; set; }
        public SelectList Products { get; set; }
        public IEnumerable<MembershipRole> Roles { get; set; }

        public Guid SelectedProduct { get; set; }
        public Guid[] SelectedRoles { get; set; }

    }
    public class DownloadsViewModel
    {
        public IEnumerable<MarketProductDownload> Downloads { get; set; }
        public UploadProductDownloadViewModel UploadVM { get; set; }
    }
    public class PurchasesViewModel
    {
        public PaymentInfo[] Payments { get; set; }
        public SubscriptionInfo[] Subscriptions { get; set; }
    }
    public class MarketProductPurchaseViewModel
    {
        private CardInfo _card = new CardInfo();
        public string ErrorMessage { get; set; }
        public Guid MarketProductPurchaseOptionId { get; set; }

        public CardInfo Card
        {
            get { return _card; }
            set { _card = value; }
        }

        public int NumberOfSeats { get; set; }
    }
}
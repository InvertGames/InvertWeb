using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IPostService
    {
        Post SanitizePost(Post post);
        Post GetTopicStarterPost(Guid topicId);
        IEnumerable<Post> GetAll();
        IList<Post> GetLowestVotedPost(int amountToTake);
        IList<Post> GetHighestVotedPost(int amountToTake);
        IList<Post> GetByMember(Guid memberId, int amountToTake);
        PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm);
        PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order);
        PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize);
        Post Add(Post post);
        Post Get(Guid postId);
        void SaveOrUpdate(Post post);
        bool Delete(Post post);
        IList<Post> GetSolutionsByMember(Guid memberId);
        int PostCount();
        Post AddNewPost(string postContent, Topic topic, MembershipUser user, out PermissionSet permissions);
    }

    public interface IMarketService
    {
        void Add(MarketProduct product);
        MarketProduct Get(Guid productId);
        
        void PurchaseProduct(MembershipUser user, Guid purchaseOptionId, CardInfo cardInfo = null, int numberOfLicenses = 1);

        //void AddSeats(MarketProduct product);
        void Delete(MarketProduct product);

        PagedList<MarketProduct> GetPaged(int pageIndex, int pageSize);
        IEnumerable<MarketProduct> GetTopProducts();
        IEnumerable<MarketProduct> GetNewestProducts();

        IEnumerable<MarketProduct> GetOwnedProducts(MembershipUser getUser);
        IEnumerable<SubscriptionInfo> GetUserSubscriptions(MembershipUser user);
        IEnumerable<PaymentInfo> GetCharges(MembershipUser user);
        void CancelSubscription(MembershipUser currentUser, string id);
        void SaveProperty(Guid productId, string propertyName, string propertyValue);
        MarketProductImage AddProductImage(Guid productId);


        IEnumerable<MarketProduct> GetUserOwnedProducts(MembershipUser user);
        IEnumerable<MarketProductDownload> GetUserDownloads(MembershipUser user);


    }

    public interface IPageContentService
    {
        void SavePageContent(string friendlyId, string content);
        PageContent GetPageContent(string friendlyId);

        void SavePageContentTitle(string friendlyId, string title);
        PageContentList GetPageContentList(string friendlyId);
        void SavePageContentListItem(string listFriendlyId, string itemId, string content);
        void MovePageContentUp(string itemId);
        void MovePageContentDown(string itemId);
        void DeletePageContentListItem(string itemId);
    }
    public class OwnershipType
    {
        
    }
    public class PaymentInfo
    {
        public string FailureCode { get; set; }
        public string FailureMessage { get; set; }
        public DateTime? Date { get; set; }
        public bool? Paid { get; set; }
        public int? Total { get; set; }
        public bool? IsRefunded { get; set; }
        public string SubscriptionId { get; set; }
        public string Last4Digits { get; set; }
        public string Description { get; set; }
        public string For { get; set; }
        public MarketProduct Product { get; set; }
    }

    public enum ChargeStatus
    {
        Refunded,
        Paid,

    }
    public class CardInfo
    {
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string CardCity { get; set; }
        public string Country { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string State { get; set; }
        public string ExpYear { get; set; }
        public string ExpMonth { get; set; }
        public string Zip { get; set; }
        public string Cvv { get; set; }


    }

    public class SubscriptionInfo
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public DateTime? CanceledOn { get; set; }
        public decimal Amount { get; set; }
        public string PlanId { get; set; }
        public DateTime? StartedOn { get; set; }
        public string Name { get; set; }
    }
}

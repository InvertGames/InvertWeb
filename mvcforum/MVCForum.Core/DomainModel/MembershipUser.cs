using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// Status values returned when creating a user
    /// </summary>
    public enum MembershipCreateStatus
    {
        Success,
        DuplicateUserName,
        DuplicateEmail,
        InvalidPassword,
        InvalidEmail,
        InvalidAnswer,
        InvalidQuestion,
        InvalidUserName,
        ProviderError,
        UserRejected
    }

    /// <summary>
    /// A membership user 
    /// </summary>
    public partial class MembershipUser : Entity
    {
        public MembershipUser()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public int FailedPasswordAnswerAttempt { get; set; }
        public string Comment { get; set; }
        public string Slug { get; set; }
        public string Signature { get; set; }
        public int? Age { get; set; }
        public string Location { get; set; }
        public string Website { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }

        public string Avatar { get; set; }
        public string FacebookAccessToken { get; set; }
        public long? FacebookId { get; set; }
        public string TwitterAccessToken { get; set; }
        public string TwitterId { get; set; }
        public string GoogleAccessToken { get; set; }
        public string GoogleId { get; set; }
        public bool? IsExternalAccount { get; set; }
        public bool? TwitterShowFeed { get; set; }
        public DateTime? LoginIdExpires { get; set; }
        public string MiscAccessToken { get; set; }

        public bool? DisableEmailNotifications { get; set; }
        public bool? DisablePosting { get; set; }
        public bool? DisablePrivateMessages { get; set; }
        public bool? DisableFileUploads { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public virtual IList<MembershipRole> Roles { get; set; }
        public virtual IList<Post> Posts { get; set; }
        public virtual IList<Topic> Topics { get; set; }
        public virtual IList<Vote> Votes { get; set; }
        public virtual IList<Badge> Badges { get; set; }
        public virtual IList<BadgeTypeTimeLastChecked> BadgeTypesTimeLastChecked { get; set; }

        public virtual IList<CategoryNotification> CategoryNotifications { get; set; }
        public virtual IList<TopicNotification> TopicNotifications { get; set; }
        public virtual IList<MembershipUserPoints> Points { get; set; }

        public virtual IList<PrivateMessage> PrivateMessagesReceived { get; set; }
        public virtual IList<PrivateMessage> PrivateMessagesSent { get; set; }

        public virtual IList<Poll> Polls { get; set; }
        public virtual IList<PollVote> PollVotes { get; set; } 

        public int TotalPoints 
        { 
            get {
                return Points != null ? Points.Select(x => x.Points).Sum() : 0;
            }
        }

        public string NiceUrl
        {
            get { return UrlTypes.GenerateUrl(UrlType.Member, Slug); }
        }

        public MarketSellerInfo MarketSellerInfo { get; set; }

        public string StripeCustomerId { get; set; }
        public string StripeTokenId { get; set; }
        public string StripeSubscriptionPlanId { get; set; }
    }

    public class MarketSellerInfo : Entity
    {
        public MarketSellerInfo()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public MembershipUser User { get; set; }

        public string Url { get; set; }
        public string SupportEmail { get; set; }
        public virtual IList<MarketProduct> Products { get; set; }

    }

    public class MarketProduct : Entity
    {
        
        public MarketProduct()
        {
            Id = GuidComb.GenerateComb();
        }
        
        public Guid Id { get; set; }

        public virtual MarketSellerInfo MarketSeller { get; set; } 
        public virtual IList<MarketProductPurchaseOption> PurchaseOptions { get; set; }
        public virtual IList<MarketProductDownload> Downloads { get; set; }
        public virtual IList<MarketProductImage> Images { get; set; }
        public virtual IList<MarketProductVideo> Videos { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RequiredVersion { get; set; }
        public bool IsLive { get; set; }
        public string ProductType { get; set; }
        public virtual IList<MarketProductReview> Reviews { get; set; }

        


    }

    public class PageContent : Entity
    {
        public PageContent()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }

        public string FriendlyId { get; set; }

        public string ContentTitle { get; set; }
        public string Content { get; set; }

    }

    public class MarketProductReview: Entity
    {
        public MarketProductReview()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }

        public MarketProduct Product { get; set; }
        public MembershipUser User { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }

       
    }
    public class MarketProductPurchaseOption: Entity
    {
        public MarketProductPurchaseOption()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public MarketProduct Product { get; set; }
        public string StripePlanId { get; set; }
        public decimal BuyInPrice { get; set; }
        public string Description { get; set; }
        public decimal RecurringPrice { get; set; }
        
        public string PlanName { get; set; }
        public MarketProductLicense License { get; set; }

        public bool IsFree
        {
            get { return BuyInPrice < 1 && RecurringPrice < 1; }
        }
    }
    
    public class MarketProductImage : Entity
    {
        public MarketProductImage()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        public MarketProduct Product { get; set; }

        public string Url
        {
            get { return string.Format("~/Content/ProductImages/{0}.png", Id); }
        }

    }
    public class MarketProductVideo : Entity
    {
        public MarketProductVideo()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        
        public string YoutubeUrl { get; set; }

        public MarketProduct Product { get; set; }
        public bool Enabled { get; set; }


    }
    public class MarketProductLicense : Entity
    {
        public MarketProductLicense()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string File { get; set; }
        
        public virtual IList<MarketProductPurchaseOption> PurchaseOptions { get; set; }
    }
    public class MarketProductDownload : Entity
    {
        public MarketProductDownload()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public MarketProduct Product { get; set; }
        public string File { get; set; }
        public string Version { get; set; }

        public string Url
        {
            get { return string.Format("~/members/download/{0}", Id.ToString()); }
        }
    }
}

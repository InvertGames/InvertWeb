using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class MembershipUserMapping : EntityTypeConfiguration<MembershipUser>
    {
        public MembershipUserMapping()
        {
            HasKey(x => x.Id);
            
            HasOptional(p => p.MarketSellerInfo);

            HasMany(x => x.Topics).WithRequired(x => x.User)
                .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            // Has Many, as a user has many posts
            HasMany(x => x.Posts).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.Votes).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.TopicNotifications).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.Polls).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.PollVotes).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.CategoryNotifications).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.Points).WithRequired(x => x.User)
               .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            HasMany(x => x.PrivateMessagesReceived).WithRequired(x => x.UserFrom)
                .WillCascadeOnDelete();

            HasMany(x => x.PrivateMessagesSent)
                .WithRequired(x => x.UserTo)
                .WillCascadeOnDelete();

            HasMany(x => x.BadgeTypesTimeLastChecked).WithRequired(x => x.User)
                .Map(x => x.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete();

            // Many-to-many join table - a user may belong to many roles
            HasMany(t => t.Roles)
            .WithMany(t => t.Users)
            .Map(m =>
            {
                m.ToTable("MembershipUsersInRoles");
                m.MapLeftKey("UserIdentifier");
                m.MapRightKey("RoleIdentifier");
            });
           
            // Many-to-many join table - a badge may belong to many users
            HasMany(t => t.Badges)
           .WithMany(t => t.Users)
           .Map(m =>
           {
               m.ToTable("MembershipUser_Badge");
               m.MapLeftKey("MembershipUser_Id");
               m.MapRightKey("Badge_Id");
           });
        }
    }

    public class MarketSellerMapping : EntityTypeConfiguration<MarketSellerInfo>
    {
        public MarketSellerMapping()
        {
            HasKey(p => p.Id);
            
            HasRequired(p => p.User)
                .WithOptional(p=>p.MarketSellerInfo)
                .Map(x=>x.MapKey("User_Id"));
            
            
            HasMany(p => p.Products)
                .WithOptional(p => p.MarketSeller)
                .Map(x => x.MapKey("MarketSeller_Id"));


        }
    }
    public class MarketProductMapping : EntityTypeConfiguration<MarketProduct>
    {
        public MarketProductMapping()
        {
            HasKey(p => p.Id);
            HasOptional(p => p.MarketSeller)
                .WithMany(p=>p.Products)
                .Map(x=>x.MapKey("MarketSeller_Id"));

            HasMany(p => p.Downloads).WithRequired(p => p.Product)
                .Map(x => x.MapKey("MarketProduct_Id"))
                .WillCascadeOnDelete();
            
            HasMany(p => p.Videos).WithRequired(p => p.Product)
                .Map(x => x.MapKey("MarketProduct_Id"))
                .WillCascadeOnDelete();
            
            HasMany(p => p.PurchaseOptions).WithRequired(p => p.Product)
                .Map(x => x.MapKey("MarketProduct_Id"))
                .WillCascadeOnDelete();

            HasMany(p => p.Images).WithRequired(p => p.Product)
                .Map(x => x.MapKey("MarketProduct_Id"))
                .WillCascadeOnDelete();

            HasMany(p => p.Reviews).WithRequired(p => p.Product)
                .Map(x => x.MapKey("MarketProduct_Id"))
                .WillCascadeOnDelete();

        }
    }

    public class MarketProductReviewMapping : EntityTypeConfiguration<MarketProductReview>
    {
        public MarketProductReviewMapping()
        {
            HasKey(p => p.Id);
            HasRequired(_ => _.Product)
                .WithMany(p=>p.Reviews)
                .Map(p=>p.MapKey("MarketProduct_Id"))
                .WillCascadeOnDelete()
                ;
            
        }
    }
    public class MarketProductImageMapping : EntityTypeConfiguration<MarketProductImage>
    {
        public MarketProductImageMapping()
        {
            HasKey(p => p.Id);
            HasRequired(p => p.Product)
                .WithMany(p => p.Images)
                .Map(p => p.MapKey("MarketProduct_Id"));
            
        }
    }

    public class MarketProductVideoMapping : EntityTypeConfiguration<MarketProductVideo>
    {
        public MarketProductVideoMapping()
        {
            HasKey(p => p.Id);
            HasRequired(p => p.Product)
                .WithMany(p => p.Videos)
                .Map(p => p.MapKey("MarketProduct_Id"));
       
        }
    }
    public class MarketProductDownloadMapping : EntityTypeConfiguration<MarketProductDownload>
    {
        public MarketProductDownloadMapping()
        {
            HasKey(p => p.Id);
            HasRequired(p => p.Product)
                .WithMany(p => p.Downloads)
                .Map(p => p.MapKey("MarketProduct_Id"));

        }
    }

    public class MarketProductPurchaseOptionMapping : EntityTypeConfiguration<MarketProductPurchaseOption>
    {
        public MarketProductPurchaseOptionMapping()
        {
            HasKey(p => p.Id);
            HasRequired(p => p.Product)
                .WithMany(p => p.PurchaseOptions)
                .Map(p => p.MapKey("MarketProduct_Id"));

            Ignore(p => p.RecurringPrice);
            // HasRequired(p => p.PlanName);


        }
    }
    public class MarketProductLicenseMapping : EntityTypeConfiguration<MarketProductLicense>
    {
        public MarketProductLicenseMapping()
        {
            HasKey(p => p.Id);
            HasMany(p => p.PurchaseOptions)
                .WithRequired(p => p.License)
                .Map(x=>x.MapKey("MarketProductLicense_Id"))
                .WillCascadeOnDelete();

        }
    }
}

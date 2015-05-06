using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using Stripe;

namespace MVCForum.Services
{
    public class MarketService : IMarketService
    {
        public MarketService(IActivationService activationServce, StripeChargeService chargeService, IUnitOfWorkManager workManager, StripeSubscriptionService subscriptionService, StripePlanService planService, IMembershipService membership, IMarketRepository marketRepository, StripeCustomerService customerService)
        {
            ActivationServce = activationServce;
            ChargeService = chargeService;
            WorkManager = workManager;
            SubscriptionService = subscriptionService;
            PlanService = planService;
            Membership = membership;
            Market = marketRepository;
            ChargeService = chargeService;
            CustomerService = customerService;
        }
        public StripeCustomerService CustomerService { get; set; }
        public IUnitOfWorkManager WorkManager { get; set; }
        public StripeSubscriptionService SubscriptionService { get; set; }
        public StripePlanService PlanService { get; set; }
        public IMembershipService Membership { get; set; }
        public IMarketRepository Market { get; set; }
        public IActivationService ActivationServce { get; set; }
        public StripeChargeService ChargeService { get; set; }


        public void Add(MarketProduct product)
        {

        }

        public PagedList<MarketProduct> GetPaged(int pageIndex, int pageSize)
        {
            return Market.GetPagedProducts(pageIndex, pageSize);
        }

        public IEnumerable<MarketProduct> GetTopProducts()
        {
            return Market.GetAll().Take(5);
        }

        public IEnumerable<MarketProduct> GetNewestProducts()
        {
            return Market.GetAll().Reverse().Take(5);
        }

        public IEnumerable<MarketProduct> GetOwnedProducts(MembershipUser getUser)
        {
            return null;
        }

        public MarketProduct Get(Guid productId)
        {
            var marketProduct = Market.GetMarketProduct(productId);
            foreach (var option in marketProduct.PurchaseOptions)
            {
                if (string.IsNullOrEmpty(option.StripePlanId))
                {
                    continue;
                }
                var stripeItem = PlanService.Get(option.StripePlanId);
                option.RecurringPrice = stripeItem.Amount / 100;
            }
            return marketProduct;
        }

        public void PurchaseProduct(MembershipUser user, Guid purchaseOptionId, CardInfo cardInfo = null, int numberOfLicenses = 1)
        {
            using (var unitOfWork = WorkManager.NewUnitOfWork())
            {
                var purchaseOption = Market.GetProductOption(purchaseOptionId);
                if (purchaseOption == null) throw new Exception("Purchase Option not available.");
                var product = purchaseOption.Product;
                StripeCustomer stripeCustomer = null;
                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    if (cardInfo == null)
                    {
                        throw new Exception("Card info is not available.");
                    }
                    var options = new StripeCustomerCreateOptions()
                    {
                        Email = user.Email,

                        CardName = cardInfo.CardName,
                        CardNumber = cardInfo.CardNumber,
                        CardAddressCity = cardInfo.CardCity,
                        CardAddressCountry = cardInfo.Country,
                        CardAddressLine1 = cardInfo.AddressLine1,
                        CardAddressLine2 = cardInfo.AddressLine2,
                        CardAddressState = cardInfo.State,
                        CardExpirationYear = cardInfo.ExpYear,
                        CardExpirationMonth = cardInfo.ExpMonth,
                        CardAddressZip = cardInfo.Zip,
                        CardCvc = cardInfo.Cvv,
                        PlanId = purchaseOption.StripePlanId,
                    };
                    stripeCustomer = CustomerService.Create(options);
                    user.StripeCustomerId = stripeCustomer.Id;

                }
                else
                {
                    stripeCustomer = CustomerService.Get(user.StripeCustomerId);

                }

                // Now make the initial buy-in payment
                var stripeCharge = new StripeChargeCreateOptions();
                stripeCharge.CustomerId = stripeCustomer.Id;
                stripeCharge.Capture = true;
                stripeCharge.Amount = Convert.ToInt32(purchaseOption.BuyInPrice * 100);
                stripeCharge.Metadata = new Dictionary<string, string> { { "MarketProductId", product.Id.ToString() } };
                stripeCharge.Currency = "usd";
                // Process the Payment
                var charge = ChargeService.Create(stripeCharge);
                // Now set up the subscription if possible
                if (!string.IsNullOrEmpty(purchaseOption.StripePlanId))
                {
                    var subscription = SubscriptionService.Create(stripeCustomer.Id, purchaseOption.StripePlanId, new StripeSubscriptionCreateOptions()
                    {
                        Quantity = numberOfLicenses,
                    });
                }

                unitOfWork.Commit();
            }



        }

        public IEnumerable<SubscriptionInfo> GetUserSubscriptions(MembershipUser user)
        {
            var subscriptions = SubscriptionService.List(user.StripeCustomerId);
            foreach (var subscription in subscriptions)
            {
                yield return new SubscriptionInfo()
                {
                    Id = subscription.Id,
                    Status = subscription.Status,
                    CanceledOn = subscription.CanceledAt,
                    Name = subscription.StripePlan.Name,
                    Amount = subscription.StripePlan.Amount / 100,
                    PlanId = subscription.StripePlan.Id,
                    StartedOn = subscription.Start
                };
            }
        }

        public IEnumerable<PaymentInfo> GetCharges(MembershipUser user)
        {

            var charges = ChargeService.List(new StripeChargeListOptions()
            {
                CustomerId = user.StripeCustomerId
            });
            foreach (var charge in charges)
            {

                var item = new PaymentInfo();
                item.FailureCode = charge.FailureCode;
                item.FailureMessage = charge.FailureMessage;
                if (charge.Invoice != null)
                {
                    item.Date = charge.Created;
                    item.Paid = charge.Invoice.Paid;
                    if (charge.Metadata.ContainsKey("MarketProductId"))
                    {
                        var productId = charge.Metadata["MarketProductId"];
                        if (!string.IsNullOrEmpty(productId))
                        {
                            item.Product = Market.GetMarketProduct(new Guid(productId));

                        }

                    }
                    item.Total = charge.Invoice.Total;
                    item.SubscriptionId = charge.Invoice.SubscriptionId;
                    item.Description = charge.StatementDescription;
                    var subscription = SubscriptionService.Get(user.StripeCustomerId, charge.Invoice.SubscriptionId);
                    item.For = subscription.StripePlan.Name;
                }
                else
                {
                    item.Total = charge.Amount / 100;
                    item.Paid = charge.Captured;
                    item.Date = charge.Created;
                }


                item.IsRefunded = charge.Refunded;

                item.Last4Digits = charge.StripeCard.Last4;
                yield return item;
            }
        }

        public void CancelSubscription(MembershipUser currentUser, string id)
        {
            var subscription = SubscriptionService.Get(currentUser.StripeCustomerId, id);
            SubscriptionService.Cancel(currentUser.StripeCustomerId, subscription.Id, false);
        }

        public void SaveProperty(Guid productId, string propertyName, string propertyValue)
        {
            var product = Get(productId);
            var property = product.GetType().GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(product, Convert.ChangeType(propertyValue, property.PropertyType));
                if (property.PropertyType == typeof(int))
                {

                }

            }
        }

        public MarketProductImage AddProductImage(Guid productId)
        {

            var product = Market.GetMarketProduct(productId);


            var image = new MarketProductImage()
            {
                Product = product,

            };
            Market.AddProductImage(image);

            return image;
        }

        public void Delete(MarketProduct product)
        {

        }

        public void SaveProduct(MembershipUser user, MarketProduct product)
        {

        }

        public void AddProductImage(MarketProduct product)
        {

        }

        public void RemooveProductImage(Guid id)
        {

        }

        public void SaveProductVideo(MarketProduct product)
        {

        }

        public IEnumerable<MarketProduct> GetUserOwnedProducts(MembershipUser user)
        {
            var products = new List<MarketProduct>();
            foreach (var item in GetUserSubscriptions(user))
            {
                products.AddRange(Market.GetProductsByStripePlanId(item.PlanId));
            }
            foreach (var item in GetCharges(user))
            {
                if (item.Product == null) continue;
                if (item.Paid.HasValue && item.Paid.Value == true && item.IsRefunded.HasValue && !item.IsRefunded.Value)
                {
                    products.Add(item.Product);
                }
            }

            return products.Distinct();
        }

        public IEnumerable<MarketProductDownload> GetUserDownloads(MembershipUser user)
        {
            return GetUserOwnedProducts(user).SelectMany(p => p.Downloads);
        }

        public void EventReceived(StripeEvent stripeEvent)
        {
            switch (stripeEvent.Type)
            {
                case "charge.updated":
                case "charge.captured":
                case "charge.succeeded":
                case "charge.refunded":  // take a look at all the types here: https://stripe.com/docs/api#event_types
                    var stripeCharge = Mapper<StripeCharge>.MapFromJson(stripeEvent.Data.Object.ToString());

                    break;
                case "customer.subscription.trial_will_end":
                    var subscription = Mapper<StripeSubscription>.MapFromJson(stripeEvent.Data.Object.ToString());

                    break;

            }
        }

        public IEnumerable<MarketProduct> GetInvertProducts()
        {
            return Market.GetAll();
        }

        public void SaveProductOption(string productId, MarketProductPurchaseOption option)
        {

        }

        public void RemoveProductOption(string optionId)
        {

        }

    }

    public class PageContentService : IPageContentService
    {
        public IPageContentRepository Repository { get; set; }

        public PageContentService(IPageContentRepository repository)
        {
            Repository = repository;
        }
     
        public PageContent SavePageContent(string friendlyId, string content, Guid? parentId)
        {
            return Repository.SavePageContent(friendlyId, content,parentId);
        }

        public PageContent GetPageContent(string friendlyId, Guid? parentId, bool draftVersion = false)
        {
            return Repository.GetPageContent(friendlyId, parentId, draftVersion: draftVersion); 
        }

        public void PublishContent(Guid? contentId)
        {
            Repository.PublishContent(contentId);
        }


        public PageContent GetPageContentList(string friendlyId, Guid? parentId, bool includeDrafts)
        {
            return Repository.GetPageContentList(friendlyId, parentId, includeDrafts);
        }



        public void MovePageContentUp(string itemId)
        {
            Repository.MovePageContentUp(new Guid(itemId));
        }
        public void MovePageContentDown(string itemId)
        {
            Repository.MovePageContentDown(new Guid(itemId));
        }

        public void DeletePageContentListItem(string itemId)
        {
            Repository.DeletePageContentListItem(new Guid(itemId));
        }
    }

    public class ActivationService : IActivationService
    {
        public IUserLicenseRepository LicenseRepository { get; set; }

        public ActivationService(IUserLicenseRepository licenseRepository)
        {
            LicenseRepository = licenseRepository;
        }

        public MarketProductUserLicense[] GetLicenses(Guid userId)
        {
            return LicenseRepository.GetLicenses(userId).ToArray();
        }

        public string ActivateLicense(Guid userId, Guid licenseId)
        {
            var license = LicenseRepository.GetLicense(userId, licenseId);
            if (license.Activations >= license.MaxActivations)
            {
                return "License has reached the max activations, please de-activate a license to continue.";
            }
            license.Activations++;
            return null;
        }

        public void DeactivateLicense(Guid userId, Guid licenseId)
        {
            var license = LicenseRepository.GetLicense(userId, licenseId);
            if (license.Activations > 1)
            {
                license.Activations--;
            }
        }



    }
}

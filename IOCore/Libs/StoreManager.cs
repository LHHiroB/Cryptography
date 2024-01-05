using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Services.Store;
using WinRT.Interop;

namespace IOCore.Libs
{
    public class StoreManager
    {
        public enum License
        {
            Default,
            Trial,
            Premium
        }

#if DEBUG
        public static License DevLicense;
#endif
        public static License ProLicense;

        public enum PurchaseType
        {
            New,
            Restore
        }

        public interface IInAppPurchase
        {
            void OnLicensesChanged();
        }

        public class ProductItem
        {
            public string StoreId { get; private set; }
            public TextBlock TitleTextBlock { get; private set; }
            public TextBlock PriceTextBlock { get; private set; }
            public Control PurchaseButton { get; private set; }
            public bool Load { get; set; }

            public ProductItem(string storeId, TextBlock titleTextBlock, TextBlock priceTextBlock, Control purchaseButton)
            {
                StoreId = storeId;
                TitleTextBlock = titleTextBlock;
                PriceTextBlock = priceTextBlock;
                PurchaseButton = purchaseButton;
                Load = false;

                if (PurchaseButton != null)
                    PurchaseButton.IsEnabled = false;
            }
        }

        public class PremiumFeature
        {
            public BitmapImage Thumbnail { get; set; }
            public string Support { get; set; }
            public string Title { get; set; }
            public string Headline { get; set; }
            public string Description { get; set; }

            public PremiumFeature(string thumbnail, string support, string title, string headline, string description)
            {
                Thumbnail = new(new(Path.Combine(Utils.GetAssetsFolderPath(), thumbnail)));
                Support = support;
                Title = title;
                Headline = headline;
                Description = description;
            }
        }

        public class Status
        {
            public bool IsPurchased { get; set; }
            public bool IsExpired { get; set; }

            public bool IsPurchasedExpired => IsPurchased && IsExpired;
            public bool IsPremium => IsPurchased && !IsExpired;
            public bool IsTrial => !IsPremium;

            public Status(bool isPurchased, bool isExpired)
            {
                IsPurchased = isPurchased;
                IsExpired = isExpired;
            }
        }

        private StoreManager()
        {
        }

        private static readonly Lazy<StoreManager> lazy = new(() => new());
        public static StoreManager Inst => lazy.Value;

        //

        private StoreContext _context;
        public StoreContext GetContext()
        {
            if (_context == null)
            {
                _context = StoreContext.GetDefault();
                InitializeWithWindow.Initialize(_context, IOWindow.Inst.HandleIntPtr);
            }

            return _context;
        }

        //

        public Status LicenseStatus { get; private set; } = new(false, false);

        private IProgress<bool> _progress;

        public void CreateOfflineLicensesChanged(Action action)
        {
            _progress = new Progress<bool>(_ => action?.Invoke());
            GetContext().OfflineLicensesChanged += StoreManager_OfflineLicensesChanged;
        }

        public void ReleaseOfflineLicensesChanged() => GetContext().OfflineLicensesChanged -= StoreManager_OfflineLicensesChanged;
        private void StoreManager_OfflineLicensesChanged(StoreContext sender, object args) => _progress?.Report(true);

        //

        public static async void LoadProduct(ProductItem product)
        {
            if (product.PurchaseButton != null && !product.PurchaseButton.IsEnabled)
            {
                var result = await Inst.GetContext().GetStoreProductForCurrentAppAsync();
                
                if (result.ExtendedError == null)
                {
                    if (product.TitleTextBlock != null)
                        product.TitleTextBlock.Text = result.Product.Title;
                    if (product.PriceTextBlock != null)
                        product.PriceTextBlock.Text = result.Product.Price.FormattedPrice;
                    if (product.PurchaseButton != null)
                        product.PurchaseButton.IsEnabled = true;

                    product.Load = true;
                }
            }
        }

        public static async void LoadAddons(ProductItem[] addons)
        {
            var addOns = await Inst.GetAddOns(addons.Select(i => i.StoreId).ToArray(), false);
            if (addOns == null) return;

            foreach (var i in addons)
            {
                var addOn = addOns.FirstOrDefault(a => a.StoreId == i.StoreId);
                if (addOn != null)
                {
                    if (i.TitleTextBlock != null)
                        i.TitleTextBlock.Text = addOn.Title;
                    if (i.PriceTextBlock != null)
                        i.PriceTextBlock.Text = addOn.Price.FormattedPrice;
                    if (i.PurchaseButton != null)
                        i.PurchaseButton.IsEnabled = true;

                    i.Load = true;
                }
            }
        }

        //

        public async Task<Status> LoadProductLicenseStatus(bool useLoading)
        {
#if DEBUG
            if (DevLicense == License.Trial)
                return LicenseStatus = new(false, false);
            else if (DevLicense == License.Premium)
                return LicenseStatus = new(true, false);
#endif
            if (ProLicense == License.Trial)
                return LicenseStatus = new(false, false);
            else if (ProLicense == License.Premium)
                return LicenseStatus = new(true, false);

            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(true);
            var license = await GetContext()?.GetAppLicenseAsync();
            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(false);

            return LicenseStatus = new(license.IsActive && !license.IsTrial, false);
        }

        public async Task<StoreProductResult> GetCurrentProduct(bool useLoading)
        {
            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(true);
            var storeProductResult = await GetContext()?.GetStoreProductForCurrentAppAsync();
            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(false);

            return storeProductResult;
        }

        public async Task<StoreProductQueryResult> GetProductsByStoreIds(bool useLoading, IEnumerable<string> storeIds, Action<StoreProductQueryResult> action = null)
        {
            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(true);
            var storeProductResults = await GetContext()?.GetStoreProductsAsync(new List<string> { "Application" }, storeIds);
            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(false);

            action?.Invoke(storeProductResults);

            return storeProductResults;
        }

        private async Task<bool> PurchaseProduct()
        {
            var productResult = await GetContext().GetStoreProductForCurrentAppAsync();

            if (productResult.ExtendedError != null)
            {
                IOWindow.Inst.ShowMessageTeachingTip(null, "Error", productResult.ExtendedError.Message);
                return false;
            }

            var result = await productResult.Product.RequestPurchaseAsync();

            if (result.ExtendedError != null)
            {
                IOWindow.Inst.ShowMessageTeachingTip(null, "Purchase failed", result.ExtendedError.Message);
                return false;
            }

            var errorMessage = result.ExtendedError != null ? result.ExtendedError.Message : string.Empty;

            var status = false;

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                    status = true;
                    break;
                case StorePurchaseStatus.Succeeded:
                    status = true;
                    IOWindow.Inst.ShowMessageTeachingTip(null, "Thank you for purchasing", $"{Package.Current.DisplayName} Premium Forever!");
                    break;
                case StorePurchaseStatus.NotPurchased:
                    IOWindow.Inst.ShowMessageTeachingTip(null, "The purchase did not complete, it may have been canceled", errorMessage);
                    break;
                case StorePurchaseStatus.NetworkError:
                    IOWindow.Inst.ShowMessageTeachingTip(null, "Product was not purchased due to a Network Error", errorMessage);
                    break;
                case StorePurchaseStatus.ServerError:
                    IOWindow.Inst.ShowMessageTeachingTip(null, "Product was not purchased due to a Server Error", errorMessage);
                    break;
                default:
                    IOWindow.Inst.ShowMessageTeachingTip(null, "Product was not purchased due to an Unknown Error", errorMessage);
                    break;
            }

            return status;
        }

        public async Task<List<StoreProduct>> GetAddOns(string[] storeIds, bool useLoading)
        {
            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(true);
            var queryResult = await GetContext()?.GetStoreProductsAsync(new[] { "Durable" }, storeIds);
            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(false);

            if (queryResult.ExtendedError != null)
                return null;

            var addOns = new List<StoreProduct>();

            foreach (var i in queryResult.Products)
                addOns.Add(i.Value);

            return addOns;
        }

        public async Task<Status> LoadAddOnLicenseStatus(string[] addOnStoreIds, bool useLoading)
        {
#if DEBUG
            if (DevLicense == License.Trial)
                return LicenseStatus = new(false, false);
            else if (DevLicense == License.Premium)
                return LicenseStatus = new(true, false);
#endif
            if (ProLicense == License.Trial)
                return LicenseStatus = new(false, false);
            else if (ProLicense == License.Premium)
                return LicenseStatus = new(true, false);

            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(true);
            var license = await GetContext().GetAppLicenseAsync();
            if (useLoading) IOWindow.Inst.Backdrop.ShowLoading(false);

            if (license == null)
            {
                IOWindow.Inst.ShowMessageTeachingTip(null, "An error occurred while retrieving the license");
                return LicenseStatus = new(false, false);
            }

            var isPurchased = false;

            foreach (var item in license.AddOnLicenses)
            {
                foreach (var storeId in addOnStoreIds)
                {
                    if (item.Value.SkuStoreId.StartsWith(storeId))
                    {
                        isPurchased = true;

                        if ((item.Value.ExpirationDate - DateTime.Now).TotalDays >= 0)
                            return LicenseStatus = new(true, false);
                    }
                }
            }

            if (isPurchased)
                return LicenseStatus = new(true, true);

            return LicenseStatus = new(false, false);
        }

        private async Task<bool> PurchaseAddOn(string storeId)
        {
            var result = await GetContext().RequestPurchaseAsync(storeId);

            string errorMessage = result.ExtendedError != null ? result.ExtendedError.Message : string.Empty;

            var status = false;

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                    status = true;
                    IOWindow.Inst.ShowMessageTeachingTip(null, "You have already purchased the product");
                    break;
                case StorePurchaseStatus.Succeeded:
                    status = true;
                    IOWindow.Inst.ShowMessageTeachingTip(null, "Thank you for purchasing");
                    break;
                case StorePurchaseStatus.NotPurchased:
                    IOWindow.Inst.ShowMessageTeachingTip(null, "The purchase did not complete, it may have been canceled", errorMessage);
                    break;
                case StorePurchaseStatus.NetworkError:
                    IOWindow.Inst.ShowMessageTeachingTip(null, "The purchase was unsuccessful due to a network error", errorMessage);
                    break;
                case StorePurchaseStatus.ServerError:
                    IOWindow.Inst.ShowMessageTeachingTip(null, "The purchase was unsuccessful due to a server error", errorMessage);
                    break;
                default:
                    IOWindow.Inst.ShowMessageTeachingTip(null, "The purchase was unsuccessful due to an unknown error", errorMessage);
                    break;
            }

            return status;
        }

        //

        public async void PurchaseOrRestoreProduct(PurchaseType purchaseType, Action success = null)
        {
            if (purchaseType == PurchaseType.New)
            {
                if (await PurchaseProduct()) success?.Invoke();
            }
            else if (purchaseType == PurchaseType.Restore)
            {
                await LoadProductLicenseStatus(true);
                if (LicenseStatus.IsPremium) success?.Invoke();
                else IOWindow.Inst.ShowMessageTeachingTip(null, "Restore failed");
            }
        }

        public async void PurchaseOrRestoreAddOn(PurchaseType purchaseType, string[] addOnStoreIds, Action success = null)
        {
            if (purchaseType == PurchaseType.New)
            {
                if (await PurchaseAddOn(addOnStoreIds[0])) success?.Invoke();
            }
            else if (purchaseType == PurchaseType.Restore)
            {
                await LoadAddOnLicenseStatus(addOnStoreIds, true);
                if (LicenseStatus.IsPremium) success?.Invoke();
                else IOWindow.Inst.ShowMessageTeachingTip(null, "Restore failed");
            }
        }
    }
}

using DurableFunctionsCLI.Core.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DurableFunctionsCLI.Core.Discovery
{
    internal abstract class StorageAccountFinder
    {
        protected string subscriptionId;
        protected string bearerToken;

        public StorageAccountFinder(string subscriptionId, string bearerToken)
        {
            this.subscriptionId = subscriptionId;
            this.bearerToken = bearerToken;
        }

        public abstract Task<IEnumerable<StorageAccount>> FindAllStorageAccountsAsync();

        protected async Task<IEnumerable<StorageAccount>> GetStorageAccountsFromAzureAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                var response = await httpClient.GetFromJsonAsync<StorageAccountApiResponse>(url);
                return response.Value;
            }  
        }

        protected class StorageAccountApiResponse
        {
            public List<StorageAccount> Value { get; set; }
        }
    }

    internal class SubscriptionStorageAccountFinder : StorageAccountFinder
    {
        private static readonly string apiUrl = "https://management.azure.com/subscriptions/{0}/providers/Microsoft.Storage/storageAccounts?api-version=2019-06-01";
        private string formattedUrl;

        public SubscriptionStorageAccountFinder(string subscriptionId, string bearerToken) : base(subscriptionId, bearerToken)
        {
            FormatApiUrl();
        }

        private void FormatApiUrl()
        {
            formattedUrl = string.Format(apiUrl, subscriptionId);
        }

        public override async Task<IEnumerable<StorageAccount>> FindAllStorageAccountsAsync()
        {
            return await base.GetStorageAccountsFromAzureAsync(formattedUrl);
        }
    }

    internal class ResourceGroupStorageAccountFinder : StorageAccountFinder
    {
        private static readonly string apiUrl = "https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Storage/storageAccounts?api-version=2019-06-01";
        private string resourceGroupName;
        private string formattedUrl;

        public ResourceGroupStorageAccountFinder(string subscriptionId, string resourceGroupName, string bearerToken)
            : base(subscriptionId, bearerToken)
        {
            this.resourceGroupName = resourceGroupName;
            FormatApiUrl();
        }

        private void FormatApiUrl()
        {
            formattedUrl = string.Format(apiUrl, subscriptionId, resourceGroupName);
        }

        public override async Task<IEnumerable<StorageAccount>> FindAllStorageAccountsAsync()
        {
            return await base.GetStorageAccountsFromAzureAsync(formattedUrl);
        }
    }
}
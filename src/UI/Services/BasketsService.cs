using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ModernArchitectureShop.ShopUI.Models;

namespace ModernArchitectureShop.ShopUI.Services
{
    public class BasketsService
    {
        private readonly HttpClient _basketHttpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BasketsService(HttpClient basketHttpClient, IHttpContextAccessor httpContextAccessor)
        {
            _basketHttpClient = basketHttpClient ?? throw new ArgumentNullException(nameof(basketHttpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task AttachAccessTokenToHeader()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            if (accessToken != null)
            {
                var auth = _basketHttpClient.DefaultRequestHeaders.Authorization?.Parameter;
                if (auth == null)
                    _basketHttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            }
        }

        public async Task<ServiceResult<string>> AddItemAsync(string url, ItemModel itemModel)
        {
            await AttachAccessTokenToHeader();

            HttpResponseMessage response;
            try
            {
                var json = JsonSerializer.Serialize(itemModel);

                //Needed to setup the body of the request
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                response = await _basketHttpClient.PostAsync(url, data);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                return new ServiceResult<string>
                {
                    Content = null!,
                    StatusCode = 500, // Server Error!
                    Error = e.Message
                };
            }

            return new ServiceResult<string>
            {
                Content = null!,
                StatusCode = (int)response.StatusCode,
                Error = string.Empty
            };

        }

        public async Task<ServiceResult<string>> RemoveItemAsync(string url)
        {
            await AttachAccessTokenToHeader();

            HttpResponseMessage response;
            try
            {
                response = await _basketHttpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                return new ServiceResult<string>
                {
                    Content = null!,
                    StatusCode = 500, // Server Error!
                    Error = e.Message
                };
            }

            return new ServiceResult<string>
            {
                Content = null!,
                StatusCode = (int)response.StatusCode,
                Error = string.Empty
            };

        }
        public async Task<ServiceResult<string>> GetBasketItemsAsync(string url)
        {
            return await GetAsync(url);
        }

        public async Task<ServiceResult<string>> BasketTotalPrice(string url)
        {
            return await GetAsync(url);
        }

        private async Task<ServiceResult<string>> GetAsync(string url)
        {
            await AttachAccessTokenToHeader();

            HttpResponseMessage response;
            try
            {
                response = await _basketHttpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                return new ServiceResult<string>
                {
                    Content = string.Empty!,
                    StatusCode = 500, // Server Error!
                    Error = e.Message
                };
            }

            return new ServiceResult<string>
            {
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int) response.StatusCode,
                Error = string.Empty
            };
        }
    }
}

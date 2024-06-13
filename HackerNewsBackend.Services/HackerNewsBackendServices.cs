namespace HackerNewsBackend.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HackerNewsBackend.Services.Interfaces;
    using HackerNewsBackend.Domain.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Caching.Memory;

    public class HackerNewsBackendServices : IHackerNewsBackendServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private IMemoryCache _cache;
        private MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(60))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
            .SetPriority(CacheItemPriority.Normal)
            .SetSize(1024);
        public const string baseUrl = "https://hacker-news.firebaseio.com/v0";
        public const string storiesPath = "topstories.json?print=pretty";
        public const string itemPath = "item";

        public HackerNewsBackendServices(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMemoryCache cache)
        {
            this._httpClientFactory = httpClientFactory;
            this._configuration = configuration;
            this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private async Task<List<int>> GetListOfItem(int page, int pageSize)
        {
            const string url = $"{baseUrl}/{storiesPath}";
            var httpClient = this._httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var listItems = await response.Content.ReadAsStringAsync();
                var listItemsDeserialize = JsonSerializer.Deserialize<List<int>>(listItems).Skip((page - 1) * pageSize)
                    .Take(pageSize).ToList();
                return listItemsDeserialize;
            }

            throw new Exception(response.StatusCode.ToString());
        }

        public async Task<IEnumerable<TopStories>> GetListTopStoriesAsync(int page, int pageSize)
        {
            var listStories = new List<TopStories?>();
            var listItemIds = await GetListOfItem(page, pageSize);
            var httpClient = this._httpClientFactory.CreateClient();
            if (_cache.TryGetValue("listItemIds", out listStories))
            {
                List<int> cacheIdList = listStories.Select(selector: x => x.Id).ToList();
                bool isConteined = listItemIds.All(item => cacheIdList.Contains(item));
                if (isConteined)
                {
                    return listStories;
                }
            }
            var listTopStories = new List<TopStories?>();
            foreach (var id in listItemIds)
            {
                var url = $"{baseUrl}/{itemPath}/{id}.json?print=pretty";
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var item = await response.Content.ReadAsStringAsync();
                    var story = JsonSerializer.Deserialize<TopStories>(item);
                    listTopStories.Add(story);
                }
            }

            _cache.Set("listItemIds", listTopStories, this._cacheEntryOptions);
            return listTopStories;
        }

        public async Task<Item> GetItem(int id)
        {
            if (!_cache.TryGetValue("id", out Item item))
            {
                var httpClient = this._httpClientFactory.CreateClient();
                var url = $"{baseUrl}/{itemPath}/{id}.json?print=pretty";
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<Item>((string?)await response.Content.ReadAsStringAsync());
                }
                throw new Exception(response.StatusCode.ToString());
            }
            return item;
        }
    }
}

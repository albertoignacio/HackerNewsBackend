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
        private readonly HttpClient _httpClientFactory;
        private readonly string _baseUrl;
        private readonly string _storiesPath;
        private readonly string _itemPath;
        private IMemoryCache _cache;
        private MemoryCacheEntryOptions _cacheEntryOptions;

        public HackerNewsBackendServices(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory.CreateClient();
            _baseUrl = configuration["HackerNewsApi:baseUrl"];
            _storiesPath = configuration["HackerNewsApi:storiesPath"];
            _itemPath = configuration["HackerNewsApi:itemPath"];
            this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1024);
        }

        private async Task<List<int>> GetListOfItem(int page, int pageSize)
        {
            var response = await this._httpClientFactory.GetAsync($"{this._baseUrl}/{this._storiesPath}.json");
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
                var url = $"{this._baseUrl}/{this._itemPath}/{id}.json";
                var response = await this._httpClientFactory.GetAsync(url);
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
            var cacheKey = $"Item_{id}";

            if (!_cache.TryGetValue(cacheKey, out Item cachedItem))
            {
                var url = $"{_baseUrl}/{_itemPath}/{id}.json";
                var response = await this._httpClientFactory.GetAsync(url);
                response.EnsureSuccessStatusCode();

                cachedItem = JsonSerializer.Deserialize<Item>(await response.Content.ReadAsStringAsync());
                _cache.Set(cacheKey, cachedItem, _cacheEntryOptions);
            }

            return cachedItem;
        }
    }
}

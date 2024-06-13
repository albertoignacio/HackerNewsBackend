namespace HackerNewsBackend.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using HackerNewsBackend.Domain.Models;

    public interface IHackerNewsBackendServices
    {
        Task<IEnumerable<TopStories>> GetListTopStoriesAsync(int page, int pageSize);
        Task<Item> GetItem(int id);
    }
}

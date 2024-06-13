using HackerNewsBackend.Domain.Models;
using HackerNewsBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsBackend.API.Controllers;

[ApiController]
public class HackerNewsBackendController : ControllerBase
{
    private readonly ILogger<HackerNewsBackendController> _logger;
    private readonly IHackerNewsBackendServices _hackerNewsBackendServices;

    public HackerNewsBackendController(IHackerNewsBackendServices hackerNewsBackendServices, ILogger<HackerNewsBackendController> logger)
    {
        _hackerNewsBackendServices = hackerNewsBackendServices;
        _logger = logger;
    }

    [HttpGet]
    [Route("/getlisttopstories")]
    public async Task<IEnumerable<TopStories>> GetListTopStories(int page, int pageSize)
    {
        var result = await this._hackerNewsBackendServices.GetListTopStoriesAsync(page, pageSize);
        return result;
    }

    [HttpGet]
    [Route("/getitem")]
    public async Task<Item> GetItem(int id)
    {
        var result = await this._hackerNewsBackendServices.GetItem(id);
        return result;
    }
}


using System.Text.Json.Serialization;
using GraphBackend.Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RestSharp;

namespace GraphBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class VkSearchController : ControllerBase
{
    private readonly RestClient _restClient;
    private readonly VkSettings _vkSettings;

    public VkSearchController(IOptions<VkSettings> vkSettings)
    {
        _vkSettings = vkSettings.Value;
        var options = new RestClientOptions("https://api.vk.com")
        {
            ThrowOnAnyError = true
        };

        _restClient = new RestClient(options);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        string query,
        DateTime? startDate,
        DateTime? endDate,
        int count = 10)
    {
        var request = new RestRequest("/method/newsfeed.search");

        request.AddQueryParameter("access_token", _vkSettings.AccessToken);
        request.AddQueryParameter("v", "5.199");
        request.AddQueryParameter("q", query);
        request.AddQueryParameter("count", count.ToString());

        if (startDate.HasValue)
            request.AddQueryParameter("start_time",
                ((DateTimeOffset)startDate.Value).ToUnixTimeSeconds().ToString());

        if (endDate.HasValue)
            request.AddQueryParameter("end_time",
                ((DateTimeOffset)endDate.Value).ToUnixTimeSeconds().ToString());

        // Выполняем запрос и автоматически десериализуем JSON в VkApiResponse
        var response = await _restClient.GetAsync<VkApiResponse>(request);

        var list = new List<VkPostDto>();

        if (response?.Response?.Items != null)
        {
            foreach (var item in response.Response.Items)
            {
                var link = $"https://vk.com/wall{item.SourceId}_{item.PostId}";

                list.Add(new VkPostDto
                {
                    Date = DateTimeOffset.FromUnixTimeSeconds(item.Date).DateTime,
                    Text = item.Text,
                    Link = link
                });
            }
        }

        return Ok(list);
    }
}

public class VkPostDto
{
    public DateTime Date { get; set; }
    public string Text { get; set; }
    public string Link { get; set; }
}

public class VkApiResponse
{
    public VkResponse Response { get; set; }
}

public class VkResponse
{
    public List<VkItem> Items { get; set; }
}

public class VkItem
{
    [JsonPropertyName("source_id")]
    public long SourceId { get; set; }

    [JsonPropertyName("post_id")]
    public long PostId { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("date")]
    public long Date { get; set; }
}
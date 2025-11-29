using GraphBackend.Application.Services;
using GraphBackend.Domain.Models;
using GraphBackend.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using RestSharp;

namespace GraphBackend.Infrastructure.Services;

public class VkClient : IVkClient
{
    private readonly RestClient _restClient;
    private readonly VkSettings _vkSettings;

    public VkClient(IOptions<VkSettings> vkSettings)
    {
        _vkSettings = vkSettings.Value;
        var options = new RestClientOptions("https://api.vk.com")
        {
            ThrowOnAnyError = true
        };

        _restClient = new RestClient(options);
    }

    public async Task<IReadOnlyList<HeroRecord>> SearchPostsAsync(
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        int count,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest("/method/newsfeed.search");

        request.AddQueryParameter("access_token", _vkSettings.AccessToken);
        request.AddQueryParameter("v", "5.199");
        request.AddQueryParameter("q", query);
        request.AddQueryParameter("count", count.ToString());
        request.AddQueryParameter("extended", "1");
        request.AddQueryParameter("fields", "members_count,followers_count");

        request.AddQueryParameter("start_time", start.ToUnixTimeSeconds().ToString());
        request.AddQueryParameter("end_time", end.ToUnixTimeSeconds().ToString());

        var response = await _restClient.GetAsync<VkApiResponse>(request, cancellationToken);

        var list = new List<HeroRecord>();

        if (response?.Response?.Items != null)
        {
            var groupMap = response.Response.Groups.ToDictionary(g => g.Id, g => g.MembersCount);
            var profileMap = response.Response.Profiles.ToDictionary(g => g.Id, g => g.FollowersCount);
            
            foreach (var item in response.Response.Items)
            {
                var link = $"https://vk.com/wall{item.OwnerId}_{item.Id}";
                
                var ownerIdAbsolute = Math.Abs(item.OwnerId);
                var isClub = item.OwnerId < 0;
                var owner = $"https://vk.com/{(isClub ? "club" : "id")}{ownerIdAbsolute}";
                
                list.Add(new HeroRecord(
                    link,
                    "",
                    owner,
                    owner,
                    DateTimeOffset.FromUnixTimeSeconds(item.Date).UtcDateTime,
                    item.Text,
                    item.Likes.Count,
                    item.Reposts.Count,
                    item.Comments.Count,
                    item.Views.Count,
                    null,
                    "",
                    isClub ? groupMap[ownerIdAbsolute] : profileMap[ownerIdAbsolute],
                    HeroRecordClassification.Unmarked
                ));
            }
        }

        return list;
    }
}
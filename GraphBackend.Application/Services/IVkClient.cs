using System.Text.Json.Serialization;
using GraphBackend.Domain.Models;

namespace GraphBackend.Application.Services;

public interface IVkClient
{
    Task<IReadOnlyList<HeroRecord>> SearchPostsAsync(
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        int count,
        CancellationToken cancellationToken);
}

public class VkApiResponse
{
    public VkResponse Response { get; set; }
}

public class VkResponse
{
    public List<VkItem> Items { get; set; }
    public List<VkProfile> Profiles { get; set; }
    public List<VkGroup> Groups { get; set; }
}

public class VkProfile
{
    public int Id { get; set; }
    [JsonPropertyName("followers_count")]
    public int FollowersCount { get; set; }
}

public class VkGroup
{
    public int Id { get; set; }
    [JsonPropertyName("members_count")]
    public int MembersCount { get; set; }
}


public class VkItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("date")]
    public long Date { get; set; }

    [JsonPropertyName("owner_id")]
    public int OwnerId { get; set; }
    
    [JsonPropertyName("likes")]
    public CountDto Likes { get; set; }
    
    [JsonPropertyName("reposts")]
    public CountDto Reposts { get; set; }
    
    [JsonPropertyName("comments")]
    public CountDto Comments { get; set; }
    
    [JsonPropertyName("views")]
    public CountDto Views { get; set; }
}

public class CountDto
{
    public int Count { get; set; }
}
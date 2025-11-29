using GraphBackend.Domain.Models;

namespace GraphBackend.Application.Services;

public interface IPostAnalyzer
{
    Task<AnalyzedPostResult> AnalyzeAsync(HeroRecord post, CancellationToken cancellationToken);
}

public class AnalyzedPostResult
{
    public HeroRecordClassification Classification { get; set; }
}
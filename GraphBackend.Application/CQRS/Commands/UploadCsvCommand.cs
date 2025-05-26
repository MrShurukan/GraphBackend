using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using GraphBackend.Domain.Common;
using GraphBackend.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.CQRS.Commands;

public record UploadCsvCommand(Stream CsvFileStream) : IRequest<int>;

public class UploadCsvCommandHandler(
    IApplicationContext context,
    IJwtTokenGenerator tokenGenerator) 
    : IRequestHandler<UploadCsvCommand, int>
{
    public async Task<int> Handle(UploadCsvCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.CsvFileStream, Encoding.UTF8);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null
        };
        using var csv = new CsvReader(reader, config);

        // Настройка заголовков
        await csv.ReadAsync();
        csv.ReadHeader();

        var addedCount = 0;
        var index = 0;
        
        while (await csv.ReadAsync())
        {
            ConsoleWriter.WriteProgress(index, 50000);
            index++;
            var url = csv.GetField("ССЫЛКА НА ЗАПИСЬ")?.Trim();
            // ConsoleWriter.WriteInfoLn($"Читаю {index}");
            
            if (string.IsNullOrWhiteSpace(url)) continue;

            var exists = await context.HeroRecords.AnyAsync(r => r.Url == url, cancellationToken);
            if (exists)
            {
                ConsoleWriter.WriteWarningLn($"Уже есть {url}");
                continue;
            }

            var record = new HeroRecord
            {
                Url = url,
                UrlWithOwner = csv.GetField("ССЫЛКА НА ЗАПИСЬ С УЧЁТОМ ВЛАДЕЛЬЦА") ?? "",
                WallOwner = csv.GetField("ВЛАДЕЛЕЦ СТЕНЫ") ?? "",
                PostAuthor = csv.GetField("АВТОР ЗАПИСИ") ?? "",
                DateTime = DateTime.Parse(csv.GetField("ДАТА И ВРЕМЯ") ?? string.Empty).ToUniversalTime(),
                Text = csv.GetField("ТЕКСТ ПОСТА") ?? "",
                Likes = int.TryParse(csv.GetField("ЛАЙКОВ"), out var likes) ? likes : 0,
                Reposts = int.TryParse(csv.GetField("РЕПОСТОВ"), out var reposts) ? reposts : 0,
                Comments = int.TryParse(csv.GetField("КОММЕНТАРИЕВ"), out var comments) ? comments : 0,
                Views = int.TryParse(csv.GetField("ПРОСМОТРОВ"), out var views) ? views : 0,
                CommentUrl = string.IsNullOrWhiteSpace(csv.GetField("ССЫЛКА НА КОММЕНТАРИЙ")) ? null : csv.GetField("ССЫЛКА НА КОММЕНТАРИЙ"),
                AuthorName = csv.GetField("НАЗВАНИЕ АВТОРА") ?? "",
                Subscribers = int.TryParse(csv.GetField("ПОДПИСЧИКОВ"), out var subs) ? subs : 0
            };

            context.HeroRecords.Add(record);
            addedCount++;
        }

        await context.SaveChangesAsync(cancellationToken);
        ConsoleWriter.WriteSuccessLn($"Выгрузил {addedCount} записей");
        return addedCount;
    }
}
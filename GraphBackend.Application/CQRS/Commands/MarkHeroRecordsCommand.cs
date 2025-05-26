using GraphBackend.Domain.Common;
using GraphBackend.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.CQRS.Commands;

public record MarkHeroRecordsCommand : IRequest<MarkResults>;

public class MarkHeroRecordsCommandHandler(
    IApplicationContext context)
    : IRequestHandler<MarkHeroRecordsCommand, MarkResults>
{
    public async Task<MarkResults> Handle(MarkHeroRecordsCommand request, CancellationToken token)
    {
        ConsoleWriter.WriteInfoLn("Выгружаю в память записи с 'герой'...");
        var records = await context.HeroRecords
            .Where(x => EF.Functions.ILike(x.Text, "%геро%"))
            .ToListAsync(token);

        ConsoleWriter.WriteSuccessLn($"Выгружено {records.Count} из {await context.HeroRecords.CountAsync(token)}");
        var unknownCategoryCount = 0;

        var classificationCounts = new Dictionary<HeroRecordClassification, int>();
        for (var index = 0; index < records.Count; index++)
        {
            var record = records[index];
            if (ProcessRecord(index, records.Count, record, classificationCounts) == HeroRecordClassification.Unmarked)
                unknownCategoryCount += 1;
        }
        
        ConsoleWriter.WriteInfoLn("Сохраняю изменения...");
        await context.SaveChangesAsync(token);
        ConsoleWriter.WriteSuccessLn("Успешно!");

        ConsoleWriter.WriteInfoLn("Отмечаю записи без 'герой'...");
        var count = await context.HeroRecords
            .Where(x => !EF.Functions.ILike(x.Text, "%геро%"))
            .ExecuteUpdateAsync(x =>
                x.SetProperty(z => z.Classification, HeroRecordClassification.NoHero), token);
        ConsoleWriter.WriteSuccessLn($"Отмечено {count} записей");

        return new MarkResults(records.Count, count, unknownCategoryCount);
    }

    private HeroRecordClassification ProcessRecord(int index, int totalRecordsCount, HeroRecord record,
        Dictionary<HeroRecordClassification, int> classificationCounts)
    {
        ConsoleWriter.WriteProgress(index, totalRecordsCount, $"Id: {record.Id}     ");

        // Разбиение на предложения
        var sentences = record.Text.Split(['\t', '\n', '.']);
        // Оставляем актуальные
        sentences = sentences.Where(x => x.Contains("геро", StringComparison.InvariantCultureIgnoreCase)).ToArray();

        InitDictionary(classificationCounts);
        foreach (var sentence in sentences)
        {
            ProcessSentence(sentence, classificationCounts);
            var maxScore = classificationCounts.MaxBy(x => x.Value);
            // Если есть ненулевое значение - мы смогли определить
            if (maxScore.Value > 0)
            {
                record.Classification = maxScore.Key;
                return maxScore.Key;
            }
        }

        return record.Classification;
    }

    private static void ProcessSentence(string sentence, Dictionary<HeroRecordClassification, int> classificationCounts)
    {
        foreach (var keywords in Classifications.Keywords)
        {
            foreach (var keyword in keywords.Value)
            {
                // Поиск целого слова
                if (keyword.Contains('!'))
                {
                    var wholeWord = keyword.Replace("!", "");
                    // Хотя бы одно слово должно полностью совпасть
                    if (sentence.Split(" ").Any(x =>
                            string.Equals(x.Trim(), wholeWord.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                    {
                        classificationCounts[keywords.Key] += 1;
                        break;
                    }
                }
                // Поиск просто наличия в тексте
                else
                {
                    if (sentence.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                    {
                        classificationCounts[keywords.Key] += 1;
                        break;
                    }
                }
            }
        }
    }

    private void InitDictionary(Dictionary<HeroRecordClassification, int> dict)
    {
        foreach (var heroRecordClassification in Enum.GetValues<HeroRecordClassification>())
        {
            dict[heroRecordClassification] = 0;
        }
    }
}

public record MarkResults(int MarkedCount, int NoHeroCount, int UnknownCategoryCount);
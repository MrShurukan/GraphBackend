﻿using GraphBackend.Domain.Common;
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
        var recordsQueryable = context.HeroRecords
            .Where(x => x.Classification == 0)
            .OrderBy(x => x.Id);

        ConsoleWriter.WriteInfoLn($"Подсчитываю количество...");
        
        var recordsToProcessCount = recordsQueryable.Count();

        ConsoleWriter.WriteInfoLn($"Обрабатываю {recordsToProcessCount} из {await context.HeroRecords.CountAsync(token)}");
        var unknownCategoryCount = 0;
        var noHeroCount = 0;

        const int startingChunkPosition = 0;
        
        var classificationCounts = new Dictionary<HeroRecordClassification, int>();
        const int chunkSize = 1_000;
        var lastId = 0;
        var globalIndex = startingChunkPosition * chunkSize;
        
        while (true)
        {
            var heroRecordChunk = await recordsQueryable
                .Where(x => x.Id > lastId)
                .Take(chunkSize)
                .ToListAsync(token);
            
            foreach (var heroRecord in heroRecordChunk)
            {
                globalIndex++;
                switch (ProcessRecord(globalIndex, recordsToProcessCount, heroRecord, classificationCounts))
                {
                    case HeroRecordClassification.Unmarked:
                        unknownCategoryCount += 1;
                        break;
                    case HeroRecordClassification.NoHero:
                        noHeroCount += 1;
                        break;
                }
            }

            ConsoleWriter.WriteProgress(globalIndex, recordsToProcessCount, "Сохраняю...");
            if (heroRecordChunk.Count == 0)
                break;
            await context.SaveChangesAsync(token);

            lastId = heroRecordChunk.Last().Id;
            
            context.ChangeTracker.Clear();
        }

        ConsoleWriter.WriteInfoLn("");
        ConsoleWriter.WriteSuccessLn("Готово!");
        return new MarkResults(globalIndex, noHeroCount, unknownCategoryCount);
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
        record.Classification = HeroRecordClassification.Unmarked;
        foreach (var sentence in sentences)
        {
            ProcessSentence(sentence, classificationCounts);
            ProcessSentencePersonal(sentence, classificationCounts);
            var maxScore = 
                // Понижение приоритета Personal
                classificationCounts
                    .Where(x => x.Key != HeroRecordClassification.Personal)
                    .MaxBy(x => x.Value);

            var scorePersonal = classificationCounts[HeroRecordClassification.Personal];
            
            // Если есть ненулевое значение - мы смогли определить
            if (maxScore.Value > 0 && maxScore.Value >= scorePersonal)
            {
                record.Classification = maxScore.Key;
                return maxScore.Key;
            }

            if (scorePersonal > 0)
            {
                record.Classification = HeroRecordClassification.Personal;
                return HeroRecordClassification.Personal;
            }
        }

        record.Classification = sentences.Length > 0 ? HeroRecordClassification.Unmarked : HeroRecordClassification.NoHero;
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

    private static void ProcessSentencePersonal(string sentence, Dictionary<HeroRecordClassification, int> classificationCounts)
    {
        var words = sentence.Split(' ').ToList();
        var heroWordIndex = words.FindIndex(x => x.Contains("геро", StringComparison.InvariantCultureIgnoreCase));

        if (heroWordIndex == -1) return;
        
        var previousWord = heroWordIndex > 0 ? words[heroWordIndex - 1] : null;
        var nextWord = heroWordIndex < words.Count - 1 ? words[heroWordIndex + 1] : null;

        if (IsPersonalWord(previousWord) || IsPersonalWord(nextWord))
        {
            classificationCounts[HeroRecordClassification.Personal] += 1;
        }
    }

    private static bool IsPersonalWord(string? word)
    {
        if (word is null) return false;
        
        word = word.ToLowerInvariant().Trim();
        return word is "мой" or "моя" or "мои" or "моё" or "наш" or "сын" or "дочь" or "муж" or "брат" or "отец" or "мать";
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
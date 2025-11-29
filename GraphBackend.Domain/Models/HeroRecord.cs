using GraphBackend.Domain.Common;

namespace GraphBackend.Domain.Models;

public class HeroRecord : BaseEntity
{
    // Ссылка на запись
    // (УНИКАЛЬНЫЙ ИНДЕКС)
    public HeroRecord(string url, string urlWithOwner, string wallOwner, string postAuthor, DateTime dateTime, string text, int likes, int reposts, int comments, int views, string? commentUrl, string authorName, int subscribers, HeroRecordClassification classification)
    {
        Url = url;
        UrlWithOwner = urlWithOwner;
        WallOwner = wallOwner;
        PostAuthor = postAuthor;
        DateTime = dateTime;
        Text = text;
        Likes = likes;
        Reposts = reposts;
        Comments = comments;
        Views = views;
        CommentUrl = commentUrl;
        AuthorName = authorName;
        Subscribers = subscribers;
        Classification = classification;

        if (subscribers != 0)
        {
            ER = (likes + comments + reposts) * 100.0f / subscribers;
            VR = views * 100.0f / subscribers;
        }
    }

    public string Url { get; set; }
    // Ссылка на запись с учётом владельца
    public string UrlWithOwner { get; set; }
    // Владелец стены
    public string WallOwner { get; set; }
    // Автор записи
    public string PostAuthor { get; set; }
    // Дата и время
    public DateTime DateTime { get; set; }
    // Текст поста
    public string Text { get; set; }
    // Лайков
    public int Likes { get; set; }
    // Репостов
    public int Reposts { get; set; }
    // Комментариев
    public int Comments { get; set; }
    // Просмотров
    public int Views { get; set; }
    // Ссылка на комментарий
    public string? CommentUrl { get; set; }
    // Название автора
    public string AuthorName { get; set; }
    // Подписчиков
    public int Subscribers { get; set; }
    
    public HeroRecordClassification Classification { get; set; }
    // Коэффициент вовлеченности
    public float ER { get; set; }
    // Коэффициент просмотров
    public float VR { get; set; }
}

public class HeroRecordDto
{
    public int Id { get; set; }
    // Ссылка на запись
    // (УНИКАЛЬНЫЙ ИНДЕКС)
    public string Url { get; set; }
    // Ссылка на запись с учётом владельца
    public string UrlWithOwner { get; set; }
    // Владелец стены
    public string WallOwner { get; set; }
    // Автор записи
    public string PostAuthor { get; set; }
    // Дата и время
    public DateTime DateTime { get; set; }
    // Текст поста
    public string Text { get; set; }
    // Лайков
    public int Likes { get; set; }
    // Репостов
    public int Reposts { get; set; }
    // Комментариев
    public int Comments { get; set; }
    // Просмотров
    public int Views { get; set; }
    // Ссылка на комментарий
    public string? CommentUrl { get; set; }
    // Название автора
    public string AuthorName { get; set; }
    // Подписчиков
    public int Subscribers { get; set; }
    
    public HeroRecordClassification Classification { get; set; }
    // Коэффициент вовлеченности
    public float ER { get; set; }
    // Коэффициент просмотров
    public float VR { get; set; }
}
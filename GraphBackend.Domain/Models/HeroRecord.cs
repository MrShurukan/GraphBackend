namespace GraphBackend.Domain.Models;

public class HeroRecord
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
}
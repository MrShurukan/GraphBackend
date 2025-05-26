namespace GraphBackend.Domain.Models;

public enum HeroRecordClassification
{
    // Герои СВО
    Svo = 1,
    // Герои ВОВ
    Vov = 2,
    // Герои Труда
    Work = 3,
    // МЧС + полиция
    Police = 4,
    // Герои военных конфликтов
    Combat = 5,
    // Личный контекст
    Personal = 6,
    // Неразмечено
    Unmarked = 7,
    
    // Нет слова герой
    NoHero = 8
}

public static class Classifications
{
    public static Dictionary<HeroRecordClassification, string[]> Keywords { get; } = new()
    {
        { HeroRecordClassification.Svo,      ["СВО!", "спецоперац", "Зеленск", "Украин", "ДНР", "ЛНР", "Артёмовск", "Бахмут"] },
        { HeroRecordClassification.Vov,      ["ВОВ!", "Великая Отечественная", "1941-1945", "фашист", "вермахт", "Сталинград", "блокада Ленинграда", "Курская дуга", "РККА"] },
        { HeroRecordClassification.Work,     ["стахановец", "ударник", "передовик производства", "соцсоревнован", "звание Герой Труда"] },
        { HeroRecordClassification.Police,   ["МЧС", "полиц", "спасател", "пожарн", "скорой помощи", "розыск", "ГИБДД", "полиции"] },
        { HeroRecordClassification.Combat,   ["Афганск", "Чечн", "Сири", "Приднестровье", "Югослави", "миротворц"] },
        { HeroRecordClassification.Personal, ["мой", "наш", "родн", "друзья", "семь", "сын", "дочь", "муж", "отец", "брат"] }
    };
} 
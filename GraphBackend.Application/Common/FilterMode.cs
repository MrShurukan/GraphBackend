namespace GraphBackend.Application.Common;

public class FilterMode
{
    public FilterModes Id { get; set; }
    public string Title { get; set; }
}

public class FilterModeDto
{
    public FilterModes Id { get; set; }
    public string Title { get; set; }
}

public enum FilterModes
{
    AtLeastOne = 1,  // Хотя бы одно из списка
    AllOfThem = 2,   // Всё из списка, даже если есть другие статусы, главное чтоб были указанные
    OneToOne = 3,    // 1 к 1
    Empty = 4         // Пусто (не указаны статусы)
}


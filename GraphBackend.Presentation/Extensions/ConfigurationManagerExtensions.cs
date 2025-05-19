using GraphBackend.Domain.Common;

namespace GraphBackend.Extensions;

public static class ConfigurationManagerExtensions
{
    public static void CheckIfSectionExists(this ConfigurationManager configurationManager, string sectionNamesString, string? customExceptionMessage = null)
    {
        var exception = customExceptionMessage ?? $"Пожалуйста, добавьте '{sectionNamesString}' в appsettings.json";

        var sectionsArray = sectionNamesString.Split(".");
        if (sectionsArray.Length == 0)
            throw new Exception("Имя секции для проверки в appsettings.json было пустым");

        var section = configurationManager.GetSection(sectionsArray[0]);
        for (var i = 1; i < sectionsArray.Length; i++)
        {
            section = section.GetSection(sectionsArray[i]);
        }

        if (!section.Exists())
        {
            ConsoleWriter.WriteDangerLn(exception);
            throw new Exception(exception);
        }
    }
}
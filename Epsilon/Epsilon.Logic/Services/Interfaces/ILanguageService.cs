using Epsilon.Logic.Entities;
using System.Collections.Generic;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ILanguageService
    {
        IList<Language> GetAvailableLanguages();

        Language GetLanguage(string languageId);
    }
}

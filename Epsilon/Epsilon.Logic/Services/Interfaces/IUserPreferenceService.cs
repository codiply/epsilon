using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class ChangePreferencesOutcome
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public bool ReturnToForm { get; set; }
    }

    public interface IUserPreferenceService
    {
        Task Create(string userId, string languageId);

        UserPreference Get(string userId, bool allowCaching = true);

        Task<UserPreference> GetAsync(string userId, bool allowCaching = true);

        Task<ChangePreferencesOutcome> ChangePreferences(string userId, ChangePreferencesForm form);
    }
}

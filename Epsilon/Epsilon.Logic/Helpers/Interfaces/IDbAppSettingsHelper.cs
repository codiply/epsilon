using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IDbAppSettingsHelper : IAppSettingsHelper
    {
        Task<IList<AppSetting>> GetAllAppSettingEntities();

        Task<AppSetting> GetAppSettingEntity(Guid id);
    }
}

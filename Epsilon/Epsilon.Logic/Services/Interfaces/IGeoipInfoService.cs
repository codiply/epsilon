using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IGeoipInfoService
    {
        GeoipInfo GetInfo(string ipAddress);

        Task<GeoipInfo> GetInfoAsync(string ipAddress);
    }
}

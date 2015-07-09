﻿using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IUserPreferenceService
    {
        Task Create(string userId, string languageId);

        Task<UserPreference> Get(string userId);
    }
}

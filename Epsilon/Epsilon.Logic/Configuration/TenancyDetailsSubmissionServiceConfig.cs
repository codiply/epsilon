﻿using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration
{
    public class TenancyDetailsSubmissionServiceConfig : ITenancyDetailsSubmissionServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public TenancyDetailsSubmissionServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public Frequency Create_MaxFrequencyPerAddress
        {
            get
            {
                return _dbAppSettingsHelper.GetFrequency(
                    DbAppSettingKey.TenancyDetailsSubmission_Create_MaxFrequencyPerAddress,
                    _dbAppSettingDefaultValue.TenancyDetailsSubmission_Create_MaxFrequencyPerAddress);
            }
        }

        public bool Create_DisableFrequencyPerAddressCheck
        {
            get
            {
                return _dbAppSettingsHelper
                    .GetBool(DbAppSettingKey.TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck) == true;
            }
        }
    }
}
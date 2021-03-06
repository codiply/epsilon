﻿using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using System;

namespace Epsilon.Logic.Configuration
{
    public class OutgoingVerificationServiceConfig : IOutgoingVerificationServiceConfig
    {
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;

        public OutgoingVerificationServiceConfig(
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue)
        {
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
        }

        public bool GlobalSwitch_DisablePickOutgoingVerification
        {
            get
            {
                return _dbAppSettingsHelper.GetBool(DbAppSettingKey.GlobalSwitch_DisablePickOutgoingVerification) == true;
            }
        }

        public double Instructions_ExpiryPeriodInDays
        {
            get
            {
                return _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.OutgoingVerification_Instructions_ExpiryPeriodInDays,
                    _dbAppSettingDefaultValue.OutgoingVerification_Instructions_ExpiryPeriodInDays);
            }
        }

        public TimeSpan MyOutgoingVerificationsSummary_CachingPeriod
        {
            get
            {
                var periodInMinutes = _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.OutgoingVerification_MyOutgoingVerificationsSummary_CachingPeriodInMinutes,
                    _dbAppSettingDefaultValue.OutgoingVerification_MyOutgoingVerificationsSummary_CachingPeriodInMinutes);
                return TimeSpan.FromMinutes(periodInMinutes);
            }
        }

        public int MyOutgoingVerificationsSummary_ItemsLimit
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit,
                    _dbAppSettingDefaultValue.OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit);
            }
        }

        public double Pick_MinDegreesDistanceInAnyDirection
        {
            get
            {
                return _dbAppSettingsHelper.GetDouble(
                    DbAppSettingKey.OutgoingVerification_Pick_MinDegreesDistanceInAnyDirection,
                    _dbAppSettingDefaultValue.OutgoingVerification_Pick_MinDegreesDistanceInAnyDirection);
            }
        }

        public int VerificationsPerTenancyDetailsSubmission
        {
            get
            {
                return _dbAppSettingsHelper.GetInt(
                    DbAppSettingKey.OutgoingVerification_VerificationsPerTenancyDetailsSubmission,
                    _dbAppSettingDefaultValue.OutgoingVerification_VerificationsPerTenancyDetailsSubmission);
            }
        }
    }
}

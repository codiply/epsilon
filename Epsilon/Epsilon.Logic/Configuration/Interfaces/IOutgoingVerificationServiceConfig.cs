﻿using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IOutgoingVerificationServiceConfig
    {
        bool GlobalSwitch_DisablePickOutgoingVerification { get; }
        double Instructions_ExpiryPeriodInDays { get; }
        TimeSpan MyOutgoingVerificationsSummary_CachingPeriod { get; }
        int MyOutgoingVerificationsSummary_ItemsLimit { get; }
        double Pick_MinDegreesDistanceInAnyDirection { get; }
        int VerificationsPerTenancyDetailsSubmission { get; }
    }
}

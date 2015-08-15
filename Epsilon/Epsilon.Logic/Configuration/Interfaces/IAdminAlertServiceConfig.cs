using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IAdminAlertServiceConfig
    {
        string ApplicationName { get; }
        string EmailList { get; }
        TimeSpan SnoozePeriod { get; }
    }
}

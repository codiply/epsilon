﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface ISmtpServiceConfig
    {
        string Host { get; }
        string UserName { get; }
        string Password { get; }
        string FromAddress { get; }
        string FromDisplayName { get; }
        int Port { get; }
        int TimeoutMilliseconds { get; }
        bool EnableSsl { get; }
    }
}
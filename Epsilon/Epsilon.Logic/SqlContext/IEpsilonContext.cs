using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext
{
    public interface IEpsilonContext
    {
        IDbSet<Address> Addresses { get; set; }
        IDbSet<TenancyDetailsSubmission> TenancyDetailsSubmissions { get; set; }
        IDbSet<TenantVerification> TenantVerifications { get; set; }
        IDbSet<User> Users { get; set; }
    }
}

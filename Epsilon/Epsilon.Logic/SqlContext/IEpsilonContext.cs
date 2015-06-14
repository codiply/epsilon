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
        IDbSet<Country> Countries { get; set; }
        IDbSet<User> Users { get; set; }
    }
}

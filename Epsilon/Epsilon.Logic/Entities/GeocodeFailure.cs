﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class GeocodeFailure
    {
        public virtual long Id { get; set; }
        public virtual string Address { get; set; }
        public virtual string Region { get; set; }
        public virtual string QueryType { get; set; }
        public virtual string FailureType { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual string CreatedById { get; set; }
        public virtual string CreatedByIpAddress { get; set; }

        public virtual User CreatedBy { get; set; }
    }
}

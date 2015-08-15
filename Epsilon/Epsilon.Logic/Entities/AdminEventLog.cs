using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Epsilon.Logic.Entities
{
    public class AdminEventLog
    {
        public virtual long Id { get; set; }
        public virtual string Key { get; set; }
        public virtual string ExtraInfo { get; set; }
        public virtual DateTimeOffset RecordedOn { get; set; }

        public Dictionary<string, object> ExtraInfoDict()
        {
            if (ExtraInfo == null)
                return new Dictionary<string, object>();
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(ExtraInfo);
        }
    }
}

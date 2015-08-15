using System;

namespace Epsilon.Logic.Entities
{
    public class AdminAlert
    {
        public virtual int Id { get; set; }
        public virtual string Key { get; set; }
        public virtual DateTimeOffset SentOn { get; set; }
    }
}

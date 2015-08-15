using System;

namespace Epsilon.Logic.Entities
{
    public class ResponseTiming
    {
        public virtual long Id { get; set; }
        public virtual DateTimeOffset MeasuredOn { get; set; }
        public virtual string LanguageId { get; set; }
        public virtual string ControllerName { get; set; }
        public virtual string ActionName { get; set; }
        public virtual string HttpVerb { get; set; }
        public virtual bool IsApi { get; set; }
        public virtual double TimeInMilliseconds { get; set; }

        public virtual Language Language { get; set; }
    }
}

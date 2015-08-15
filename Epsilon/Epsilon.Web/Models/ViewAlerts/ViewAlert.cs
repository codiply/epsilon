namespace Epsilon.Web.Models.ViewAlerts
{
    public class ViewAlert
    {
        public const string TempDataKey = "TempDataViewAlerts";

        public string AlertStyle { get; set; }
        public string Message { get; set; }
        public bool Dismissable { get; set; }
    }
}

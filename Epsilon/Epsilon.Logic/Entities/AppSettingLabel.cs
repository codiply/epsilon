namespace Epsilon.Logic.Entities
{
    public class AppSettingLabel
    {
        public string AppSettingId { get; set; }
        public string Label { get; set; }

        public virtual AppSetting AppSetting { get; set; }
    }
}

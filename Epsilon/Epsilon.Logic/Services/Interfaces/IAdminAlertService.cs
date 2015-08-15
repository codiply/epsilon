namespace Epsilon.Logic.Services.Interfaces
{
    public interface IAdminAlertService
    {
        void SendAlert(string key, bool doNotUseDatabase = false);
    }
}

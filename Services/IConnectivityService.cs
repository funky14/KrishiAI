namespace KrishiAI.App.Services;

public interface IConnectivityService
{
    bool IsConnected();
    event EventHandler<bool> ConnectivityChanged;
}

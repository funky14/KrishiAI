namespace KrishiAI.App.Services;

public class ConnectivityService : IConnectivityService
{
    public event EventHandler<bool>? ConnectivityChanged;

    public ConnectivityService()
    {
        try
        {
            // Try to register connectivity change listener
            // On Android 13+, this may fail with SecurityException due to receiver export requirements
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }
        catch (Exception ex)
        {
            // Log the error but don't crash the app
            System.Diagnostics.Debug.WriteLine($"⚠️ ConnectivityService: Could not register connectivity listener: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Connectivity checks will use polling instead of events.");
        }
    }

    public bool IsConnected()
    {
        try
        {
            var access = Connectivity.Current.NetworkAccess;
            return access != NetworkAccess.None;
        }
        catch
        {
            // If connectivity check fails, assume we're connected
            return true;
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var isConnected = e.NetworkAccess != NetworkAccess.None;
        ConnectivityChanged?.Invoke(this, isConnected);
    }
}

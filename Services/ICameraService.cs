namespace KrishiAI.App.Services;

public interface ICameraService
{
    Task<string?> CapturePhotoAsync();
    Task<string?> PickPhotoAsync();
    Task<bool> CheckCameraPermissionsAsync();
    Task<bool> RequestCameraPermissionsAsync();
}

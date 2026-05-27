using System.Diagnostics;

namespace KrishiAI.App.Services;

public class CameraService : ICameraService
{
    public async Task<string?> CapturePhotoAsync()
    {
        try
        {
            if (!await CheckCameraPermissionsAsync())
            {
                if (!await RequestCameraPermissionsAsync())
                {
                    return null;
                }
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture crop image"
            });

            if (photo == null)
                return null;

            // Save the file
            var localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using var sourceStream = await photo.OpenReadAsync();
            using var localFileStream = File.OpenWrite(localFilePath);
            await sourceStream.CopyToAsync(localFileStream);

            return localFilePath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CapturePhotoAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select crop image"
            });

            if (photo == null)
                return null;

            // Save the file
            var localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using var sourceStream = await photo.OpenReadAsync();
            using var localFileStream = File.OpenWrite(localFilePath);
            await sourceStream.CopyToAsync(localFileStream);

            return localFilePath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PickPhotoAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CheckCameraPermissionsAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
    }

    public async Task<bool> RequestCameraPermissionsAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
    }
}

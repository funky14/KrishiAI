using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models.Irrigation;
using KrishiAI.App.Services;
using KrishiAI.App.Services.Location;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class CropManagementViewModel : BaseViewModel
{
    private readonly IDatabaseService _database;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    private ObservableCollection<CropInfo> crops = new();

    [ObservableProperty]
    private CropInfo? selectedCrop;

    [ObservableProperty]
    private bool hasCrops;

    [ObservableProperty]
    private CropType selectedCropType = CropType.Rice;

    [ObservableProperty]
    private GrowthStage selectedGrowthStage = GrowthStage.Seedling;

    [ObservableProperty]
    private double fieldSizeAcres = 1.0;

    [ObservableProperty]
    private DateTime plantedDate = DateTime.Today;

    [ObservableProperty]
    private string customCropName = string.Empty;

    public List<CropType> AvailableCropTypes { get; } = Enum.GetValues<CropType>().ToList();
    public List<GrowthStage> AvailableGrowthStages { get; } = Enum.GetValues<GrowthStage>().ToList();

    public CropManagementViewModel(
        IDatabaseService database,
        ILocationService locationService,
        ILocalizationService localizationService)
    {
        _database = database;
        _locationService = locationService;
        InitializeLocalization(localizationService);
        Title = "Crop Management";
    }

    public override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCropsAsync();
    }

    [RelayCommand]
    private async Task LoadCropsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var cropList = await _database.GetAllCropProfilesAsync();

            Crops.Clear();
            foreach (var crop in cropList)
            {
                Crops.Add(crop);
            }

            HasCrops = Crops.Count > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading crops: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddCropAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Get location
            var location = await _locationService.GetCurrentLocationAsync();

            // Create crop profile
            var crop = CropDefaults.CreateDefaultProfile(
                SelectedCropType,
                location?.LocationName ?? "My Farm",
                location?.Latitude ?? 0,
                location?.Longitude ?? 0);

            crop.GrowthStage = SelectedGrowthStage;
            crop.FieldSizeAcres = FieldSizeAcres;
            crop.PlantedDate = PlantedDate;
            crop.ExpectedHarvestDate = PlantedDate.AddDays(CropDefaults.GetGrowthDurationDays(SelectedCropType));

            if (SelectedCropType == CropType.Custom && !string.IsNullOrWhiteSpace(CustomCropName))
            {
                crop.CustomCropName = CustomCropName;
            }

            // Make this the active crop if it's the first one
            crop.IsActive = Crops.Count == 0;

            await _database.SaveCropProfileAsync(crop);

            await LoadCropsAsync();

            await Shell.Current.DisplayAlert("Success", $"{crop.DisplayName} added successfully!", "OK");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error adding crop: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SetActiveCropAsync(CropInfo crop)
    {
        try
        {
            await _database.SetActiveCropAsync(crop.Id);
            await LoadCropsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteCropAsync(CropInfo crop)
    {
        try
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Confirm Delete",
                $"Delete {crop.DisplayName}?",
                "Yes",
                "No");

            if (confirm)
            {
                await _database.DeleteCropProfileAsync(crop);
                await LoadCropsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
    }
}

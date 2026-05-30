using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class FarmerTipsViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<FarmerTip> tips = new();

    public FarmerTipsViewModel()
    {
        Title = Resources.Strings.AppStrings.FarmingTips;
        LoadTips();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        Title = Resources.Strings.AppStrings.FarmingTips;
        System.Diagnostics.Debug.WriteLine("🌍 FarmerTipsViewModel: Language changed");
    }

    private void LoadTips()
    {
        // Sample tips - in production, load from database or service
        Tips = new ObservableCollection<FarmerTip>
        {
            new FarmerTip
            {
                Id = 1,
                Icon = "🔄",
                Title = "Practice crop rotation",
                Description = "Practice crop rotation to maintain soil fertility and reduce pests.",
                Category = TipCategory.CropRotation
            },
            new FarmerTip
            {
                Id = 2,
                Icon = "💧",
                Title = "Water early in the morning",
                Description = "Water your crops early in the morning to reduce evaporation.",
                Category = TipCategory.Irrigation
            },
            new FarmerTip
            {
                Id = 3,
                Icon = "🍃",
                Title = "Use organic mulch",
                Description = "Use organic mulch to conserve soil moisture and suppress weeds.",
                Category = TipCategory.SoilHealth
            }
        };
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.GoToAsync("..");
    }
}

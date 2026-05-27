using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KrishiAI.App.Models;
using KrishiAI.App.Services;
using System.Collections.ObjectModel;

namespace KrishiAI.App.ViewModels;

public partial class HistoryViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<DiseaseDetectionResult> historyItems = new();

    [ObservableProperty]
    private bool hasHistory;

    public HistoryViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Detection History";
    }

    public override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistory();
    }

    [RelayCommand]
    private async Task LoadHistory()
    {
        try
        {
            IsBusy = true;
            var items = await _databaseService.GetHistoryAsync();
            HistoryItems.Clear();
            foreach (var item in items)
            {
                HistoryItems.Add(item);
            }
            HasHistory = HistoryItems.Any();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading history: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteItem(DiseaseDetectionResult item)
    {
        try
        {
            await _databaseService.DeleteDetectionAsync(item);
            HistoryItems.Remove(item);
            HasHistory = HistoryItems.Any();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting item: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ClearAllHistory()
    {
        try
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Clear History",
                "Are you sure you want to delete all history?",
                "Yes",
                "No");

            if (confirm)
            {
                await _databaseService.ClearHistoryAsync();
                HistoryItems.Clear();
                HasHistory = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error clearing history: {ex.Message}";
        }
    }
}

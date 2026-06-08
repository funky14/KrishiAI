using CommunityToolkit.Mvvm.ComponentModel;

namespace KrishiAI.App.Models;

public partial class CategoryItem : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string icon;

    [ObservableProperty]
    private bool isSelected;

    public CategoryItem(string name, string icon)
    {
        Name = name;
        Icon = icon;
        IsSelected = false;
    }
}

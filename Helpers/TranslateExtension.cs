using System.ComponentModel;
using System.Globalization;

namespace KrishiAI.App.Helpers;

/// <summary>
/// XAML Markup Extension for localized strings.
/// Usage: Text="{helpers:Translate Welcome}"
/// </summary>
[ContentProperty(nameof(Key))]
public class TranslateExtension : IMarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        var translation = Resources.Strings.AppStrings.GetLocalizedString(Key);
        return translation ?? Key;
    }
}

/// <summary>
/// Notifies XAML bindings when language changes.
/// </summary>
public class LocalizationManager : INotifyPropertyChanged
{
    private static LocalizationManager? _instance;
    public static LocalizationManager Instance => _instance ??= new LocalizationManager();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key]
    {
        get => Resources.Strings.AppStrings.GetLocalizedString(key) ?? key;
    }

    public void NotifyLanguageChanged()
    {
        System.Diagnostics.Debug.WriteLine($"🔔 LocalizationManager: NotifyLanguageChanged called");
        System.Diagnostics.Debug.WriteLine($"🔔 PropertyChanged subscribers: {PropertyChanged?.GetInvocationList().Length ?? 0}");

        // Notify all subscribers that ALL properties have changed
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        System.Diagnostics.Debug.WriteLine($"🔔 LocalizationManager: Notification sent!");
    }
}

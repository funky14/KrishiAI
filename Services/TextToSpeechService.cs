using System.Diagnostics;

namespace KrishiAI.App.Services;

public class TextToSpeechService : ITextToSpeechService
{
    private CancellationTokenSource? _cts;
    private static IEnumerable<Locale>? _cachedLocales;

    public async Task SpeakAsync(string text, string languageCode)
    {
        try
        {
            // Cancel any ongoing speech
            CancelCurrentSpeech();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // Run on Main Thread to prevent JNI/ObjectDisposed issues in MAUI Android
            await MainThread.InvokeOnMainThreadAsync(async () => 
            {
                try 
                {
                    if (token.IsCancellationRequested) return;

                    if (_cachedLocales == null)
                    {
                        _cachedLocales = await TextToSpeech.Default.GetLocalesAsync();
                    }

                    var locale = _cachedLocales.FirstOrDefault(l => l.Language.Equals(languageCode, StringComparison.OrdinalIgnoreCase)) 
                                 ?? _cachedLocales.FirstOrDefault(l => l.Language.StartsWith(languageCode.Split('-')[0], StringComparison.OrdinalIgnoreCase));

                    var options = new SpeechOptions
                    {
                        Pitch = 1.0f,
                        Volume = 1.0f
                    };

                    if (locale != null)
                    {
                        options.Locale = locale;
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await TextToSpeech.Default.SpeakAsync(text, options, token);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("SpeakAsync inner canceled.");
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.WriteLine($"SpeakAsync ObjectDisposed Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SpeakAsync Inner Error: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SpeakAsync Outer Error: {ex.Message}");
        }
    }

    private void CancelCurrentSpeech()
    {
        try
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }
        catch { }
        
        // Do not dispose immediately to avoid JNI ObjectDisposed exceptions
        // _cts?.Dispose();
        _cts = null;
    }

    public async Task PauseAsync()
    {
        await Task.CompletedTask;
    }

    public async Task ResumeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        CancelCurrentSpeech();
        await Task.CompletedTask;
    }
}

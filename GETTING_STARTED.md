# KrishiAI - Next Steps & Setup Guide

## ⚠️ IMPORTANT: Common Issues & Fixes

**If you encounter build errors, follow these solutions:**

### Issue 1: Android SDK Not Found
**Error:** "The Android SDK directory could not be found"
**Solution:**
```powershell
# Install Android workload
dotnet workload install android

# Or install via Visual Studio Installer:
# Modify VS 2026 → Check ".NET Multi-platform App UI development"
```

### Issue 2: Java JDK Not Found
**Error:** "The Java SDK directory could not be found"
**Solution:**
```powershell
# Install Microsoft OpenJDK 17
winget install Microsoft.OpenJDK.17

# Restart terminal after installation
```

### Issue 3: Missing Resources (Splash/Icon/Fonts)
**Error:** "Failed to compute hash for file 'Resources\Splash\splash.svg'"
**Solution:** These are already commented out in the .csproj file. If you see errors:
- Remove references to splash screens and app icons from .csproj
- Comment out font configuration in MauiProgram.cs
- Use default resources initially

### Issue 4: .NET Version Outdated
**Error:** "The workload 'net8.0-android' is out of support"
**Solution:** Project uses .NET 9.0 (already updated)
```xml
<TargetFrameworks>net9.0-android</TargetFrameworks>
```

### Issue 5: XAML DataTemplate Multiple Children
**Error:** "Multiple child elements in DataTemplate"
**Solution:** Wrap multiple elements in a single container (Grid/StackLayout)
```xml
<DataTemplate>
    <Grid>  <!-- Single root element -->
        <Frame IsVisible="{Binding IsUserMessage}">...</Frame>
        <Frame IsVisible="{Binding IsAIMessage}">...</Frame>
    </Grid>
</DataTemplate>
```

### Issue 6: Locale Constructor Not Found
**Error:** "Locale does not contain a constructor that takes 2 arguments"
**Solution:** Remove locale parameter from TextToSpeech (already fixed)
```csharp
await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
{
    Pitch = 1.0f,
    Volume = 1.0f
});
```

### Issue 7: Android Target SDK Version
**Warning:** "targetSdkVersion '34' is less than TargetFrameworkVersion"
**Solution:** Updated to API 35 in AndroidManifest.xml
```xml
<uses-sdk android:minSdkVersion="24" android:targetSdkVersion="35" />
```

### Issue 8: Build Cache Issues
**Solution:** Clean and rebuild
```powershell
dotnet clean
Remove-Item -Recurse -Force bin,obj
dotnet restore
dotnet build -f net9.0-android
```

**Or in Visual Studio:**
- Build → Clean Solution
- Manually delete bin/obj folders from project directory
- Build → Rebuild Solution

---

## ✅ Implementation Complete!

The complete KrishiAI MAUI application has been implemented with all core features:

### Implemented Features
✅ Project structure and configuration (.csproj, MauiProgram.cs)  
✅ MVVM architecture with BaseViewModel  
✅ Data models (DiseaseDetectionResult, VoiceCommand, etc.)  
✅ All service interfaces and implementations  
✅ Complete ViewModels for all pages  
✅ XAML UI for all pages (Home, Disease Detection, Voice Assistant, History, Settings)  
✅ Platform-specific configurations (Android only - iOS removed)  
✅ Permissions setup for camera, microphone, storage  
✅ Value converters and helpers  
✅ Styling and theming  
✅ .NET 9.0 with Android API 35 support

---

## 🚀 Next Steps to Run the App

### 1. Install Prerequisites

**Required Software:**
- Visual Studio 2026 with ".NET Multi-platform App UI development" workload
- OR .NET 9 SDK with Android workload: `dotnet workload install android`
- Microsoft OpenJDK 17: `winget install Microsoft.OpenJDK.17`

### 2. Build the Project

**Using Visual Studio 2026:**
1. Open KrishiAI.App.sln
2. Build → Clean Solution
3. Build → Rebuild Solution
4. Select net9.0-android from dropdown
5. Select Android Emulator
6. Press F5

**Using Command Line:**
```powershell
cd "c:\Chetan\Projects\Hackathon\AI Farmer Assistant\KrishiAI.App"
dotnet clean
dotnet restore
dotnet build -f net9.0-android
```

### 3. Add MobileNetV2 ONNX Model (Optional)

**Option A: Use your trained model**
- Place your `mobilenetv2_cropdisease.onnx` file in `Resources/Raw/` folder
- Update the disease labels in `Services/CropDiseaseAIService.cs` (line 13) to match your model

**Option B: Use mock predictions (for testing)**
- The app will work without the model file using mock predictions
- Perfect for UI/UX testing before integrating the real model

### 3. Configure Azure Services (Optional)

If you want real AI responses instead of mock data:

**For Speech Recognition:**
- Open `Services/SpeechRecognitionService.cs`
- Add Azure Speech SDK integration
- Replace mock transcription with real API calls

**For AI Chat:**
- Open `Services/AIChatService.cs`
- Add Azure OpenAI or OpenAI API key
- Replace mock responses with real API calls

**For Text-to-Speech:**
- The app already uses MAUI's built-in TTS
- Optionally integrate Azure Neural Voices for better quality

### 4. Run on Android Emulator

```powershell
# List available emulators
dotnet build -f net8.0-android -t:Run

# Or use Visual Studio
# Press F5 to build and run
```

### 5. Run on Physical Android Device

```powershell
# Enable USB debugging on your device
# Connect via USB
dotnet build -f net8.0-android -t:Run
```

### 6. Run on iOS (Mac only)

```powershell
dotnet build -f net8.0-ios
dotnet run -f net8.0-ios
```

---

## 📁 Project Structure Summary

```
KrishiAI.App/
├── Models/
│   ├── AppSettings.cs
│   ├── DiseaseDetectionResult.cs
│   ├── DiseaseRecommendation.cs
│   ├── PredictionResult.cs
│   ├── SupportedLanguage.cs
│   └── VoiceCommand.cs
│
├── Services/
│   ├── AIChatService.cs              # AI chatbot responses
│   ├── CameraService.cs              # Camera & gallery access
│   ├── ConnectivityService.cs        # Network monitoring
│   ├── CropDiseaseAIService.cs       # MobileNetV2 inference
│   ├── DatabaseService.cs            # SQLite operations
│   ├── RecommendationService.cs      # Treatment recommendations
│   ├── SpeechRecognitionService.cs   # Voice input
│   └── TextToSpeechService.cs        # Voice output
│
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── CropDiseaseViewModel.cs       # Disease detection logic
│   ├── HistoryViewModel.cs           # History management
│   ├── HomeViewModel.cs              # Home page logic
│   ├── SettingsViewModel.cs          # Settings management
│   └── VoiceAssistantViewModel.cs    # Voice assistant logic
│
├── Views/
│   ├── CropDiseasePage.xaml          # Disease detection UI
│   ├── HistoryPage.xaml              # History UI
│   ├── HomePage.xaml                 # Home UI
│   ├── SettingsPage.xaml             # Settings UI
│   └── VoiceAssistantPage.xaml       # Voice assistant UI
│
├── Helpers/
│   └── Converters.cs                 # Value converters
│
├── Platforms/
│   ├── Android/
│   │   ├── AndroidManifest.xml       # Permissions
│   │   ├── MainActivity.cs
│   │   └── MainApplication.cs
│   └── iOS/
│       ├── Info.plist                # Permissions
│       ├── AppDelegate.cs
│       └── Program.cs
│
├── Resources/
│   └── Styles/
│       ├── Colors.xaml               # Color definitions
│       └── Styles.xaml               # UI styles
│
├── App.xaml                          # App resources
├── AppShell.xaml                     # Navigation shell
├── MauiProgram.cs                    # DI configuration
└── KrishiAI.App.csproj              # Project file
```

---

## 🔧 Key Configuration Points

### 1. Disease Labels
Update in `CropDiseaseAIService.cs`:
```csharp
private readonly string[] _labels = new[]
{
    "Rice Blast",
    "Brown Spot",
    "Bacterial Blight",
    // Add your disease classes here
};
```

### 2. Treatment Recommendations
Update in `RecommendationService.cs`:
```csharp
private Dictionary<string, DiseaseRecommendation> GetDefaultRecommendations()
{
    // Add recommendations for each disease
}
```

### 3. Supported Languages
Already configured in `SpeechRecognitionService.cs`:
- English, Hindi, Marathi, Tamil, Telugu, Punjabi, Gujarati, Bengali

### 4. Database Path
SQLite database is automatically created at:
- Android: `/data/data/com.krishiai.farmerassistant/files/krishiai.db3`
- iOS: App's Documents directory

---

## 🎨 UI Customization

### Change Colors
Edit `Resources/Styles/Colors.xaml`:
```xml
<Color x:Key="Primary">#4CAF50</Color>        <!-- Main green -->
<Color x:Key="Accent">#2196F3</Color>         <!-- Blue accent -->
<Color x:Key="Error">#F44336</Color>          <!-- Red for errors -->
```

### Adjust Button Sizes
Edit `Resources/Styles/Styles.xaml`:
```xml
<Style TargetType="Button">
    <Setter Property="HeightRequest" Value="60"/>
    <Setter Property="FontSize" Value="18"/>
</Style>
```

---

## 🐛 Troubleshooting

### Build Errors

**Missing SDK:**
```powershell
dotnet workload install maui
```

**NuGet restore issues:**
```powershell
dotnet nuget locals all --clear
dotnet restore --force
```

### Runtime Errors

**Camera not working:**
- Check permissions in device settings
- Verify AndroidManifest.xml has camera permission
- For iOS, check Info.plist has usage descriptions

**Model not loading:**
- Check ONNX file is in `Resources/Raw/`
- Verify file is set as `MauiAsset` in .csproj
- Check file name matches in `CropDiseaseAIService.cs`

**Database errors:**
- Clear app data on device
- Reinstall the app

---

## 📱 Testing Checklist

### Disease Detection Feature
- [ ] Capture photo from camera
- [ ] Select photo from gallery
- [ ] Analyze image (shows loading)
- [ ] Display results with confidence
- [ ] Show treatment recommendations
- [ ] Save to history
- [ ] View in history page

### Voice Assistant Feature
- [ ] Select language
- [ ] Record voice input
- [ ] Show transcription
- [ ] Get AI response
- [ ] Play response audio
- [ ] View conversation history
- [ ] Clear history

### Settings
- [ ] Change language
- [ ] Toggle settings
- [ ] Save settings (persist on restart)
- [ ] Clear cache
- [ ] Clear history

---

## 🚢 Production Deployment

### Android Release Build

```powershell
dotnet publish -f net8.0-android -c Release
```

Output: `bin/Release/net8.0-android/publish/`

### iOS Release Build (Mac)

```powershell
dotnet publish -f net8.0-ios -c Release
```

### App Store Preparation
1. Update version in `.csproj`:
   ```xml
   <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
   <ApplicationVersion>1</ApplicationVersion>
   ```

2. Create app icons (1024x1024 for iOS, various sizes for Android)

3. Prepare store listings:
   - Screenshots (5-8 images)
   - App description
   - Privacy policy
   - Keywords

---

## 🔐 Security Notes

### For Production:
1. **Never hardcode API keys** - Use secure storage:
   ```csharp
   await SecureStorage.SetAsync("api_key", "your-key");
   ```

2. **Validate user input** before sending to AI services

3. **Sanitize image uploads** to prevent malicious files

4. **Use HTTPS** for all API calls

5. **Implement rate limiting** for AI services

---

## 📚 Additional Resources

### Documentation
- [.NET MAUI Docs](https://docs.microsoft.com/dotnet/maui/)
- [ONNX Runtime](https://onnxruntime.ai/)
- [Azure Speech Services](https://azure.microsoft.com/services/cognitive-services/speech-services/)
- [Azure OpenAI](https://azure.microsoft.com/products/cognitive-services/openai-service/)

### Model Training
- Train MobileNetV2 on your crop disease dataset
- Export to ONNX format
- Optimize for mobile (quantization)
- Test inference speed on target devices

---

## 🎯 Future Enhancements

### Suggested Features
- [ ] Crop growth tracking
- [ ] Weather integration
- [ ] Market price information
- [ ] Government scheme notifications
- [ ] Community forum
- [ ] Fertilizer calculator
- [ ] Pest identification
- [ ] Soil health analysis
- [ ] Cloud backup & sync
- [ ] Multi-user support

---

## 📞 Support

For issues or questions:
1. Check this guide first
2. Review error logs in device logcat (Android) or Xcode console (iOS)
3. Contact the development team

---

**Happy Farming! 🌾**

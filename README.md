# KrishiAI - Smart Farming Assistant

## Overview
KrishiAI is a cross-platform mobile application built with .NET MAUI that helps farmers detect crop diseases and get agricultural advice through AI-powered features.

## Features

### 🔬 Crop Disease Detection
- **On-Device AI**: Uses pretrained MobileNetV2 ONNX model for offline disease detection
- **Instant Analysis**: Capture or select crop images for immediate disease identification
- **Treatment Recommendations**: Get organic, chemical, and preventive treatment suggestions
- **History Tracking**: Save and review past disease detections

### 🎤 Voice-Based Multilingual AI Assistant
- **7 Indian Languages**: English, Hindi, Marathi, Tamil, Telugu, Punjabi, Gujarati, Bengali
- **Speech-to-Text**: Speak your farming questions naturally
- **AI-Powered Responses**: Get intelligent agricultural advice
- **Text-to-Speech**: Listen to responses in your language
- **Conversation History**: Review past interactions

### 💰 Finance Tracker
- **Income & Expense Logs**: Track all crop sales and farming costs
- **Loan & Subsidy Tracking**: Manage agricultural loans and government subsidies
- **Advanced Reports**: Microcharts-powered visual breakdowns and 6-month profit trends
- **AI Financial Insights**: Get AI-generated actionable suggestions for cost optimization and loan readiness
- **Detailed History**: Filterable ledger for all transactions

### 💾 Offline-First Architecture
- Disease detection works completely offline
- SQLite local database for history
- Cached recommendations
- Network-aware features

## Technology Stack

- **.NET 9.0**: Latest .NET framework
- **.NET MAUI**: Cross-platform framework (Android focused)
- **MVVM Pattern**: CommunityToolkit.Mvvm
- **ML.NET**: Microsoft.ML.OnnxRuntime for AI inference
- **SQLite**: Local data persistence (sqlite-net-pcl)
- **Azure Services**: Speech Recognition, Text-to-Speech, OpenAI (optional)
- **Android API 35**: Target Android 15 (minimum API 24 / Android 7.0)

## Project Structure

```
KrishiAI.App/
├── Models/              # Data models
├── Views/               # XAML UI pages
├── ViewModels/          # MVVM ViewModels
├── Services/            # Business logic services
│   ├── CameraService
│   ├── CropDiseaseAIService
│   ├── RecommendationService
│   ├── SpeechRecognitionService
│   ├── TextToSpeechService
│   ├── AIChatService
│   └── DatabaseService
├── Helpers/             # Converters and utilities
└── Resources/           # Images, fonts, styles
```

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio 2026 with ".NET Multi-platform App UI development" workload
- OR Visual Studio Code with MAUI extensions
- Android SDK (automatically installed with MAUI workload)
- Java JDK 17 (Microsoft OpenJDK recommended)
- Android Emulator or physical Android device

### Setup

1. **Install Prerequisites**
   ```powershell
   # Install .NET workloads
   dotnet workload install android maui
   
   # Install Java JDK
   winget install Microsoft.OpenJDK.17
   ```

2. **Open Project**
   - Open `KrishiAI.App.sln` in Visual Studio 2026
   - Or navigate to project directory for command-line build

3. **Restore NuGet Packages**
   ```powershell
   dotnet restore
   ```

4. **Add MobileNetV2 ONNX Model (Optional)**
   - Place your trained `mobilenetv2_cropdisease.onnx` model in `Resources/Raw/` folder
   - Update the labels in `CropDiseaseAIService.cs` to match your model's classes
   - App uses mock predictions if model is not provided

5. **Configure Azure Services (Optional)**
   - Add Azure Speech API keys for speech recognition
   - Add Azure OpenAI credentials for chat service
   - Update configuration in respective services
   - App uses mock responses for demo if not configured

### Build and Run

**Using Visual Studio 2026:**
1. Select **net9.0-android** from target framework dropdown
2. Select your Android Emulator device
3. Press **F5** to build and run

**Using Command Line:**
```powershell
# Clean and build
dotnet clean
dotnet build -f net9.0-android

# Run on emulator/device
dotnet build -f net9.0-android -t:Run
```

**Note:** iOS support has been removed. Project targets Android only.

## Configuration

### Camera Permissions

**Android** (`Platforms/Android/AndroidManifest.xml`):
```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-sdk android:minSdkVersion="24" android:targetSdkVersion="35" />
```

**Note:** Permissions are automatically requested at runtime when features are used.

## Usage

### Disease Detection
1. Navigate to the **Disease** tab
2. Tap **Capture** to take a photo or **Gallery** to select an existing image
3. Tap **Analyze Disease** to run AI inference
4. View results with disease name, confidence, and recommendations
5. Results are automatically saved to history

### Voice Assistant
1. Navigate to the **Voice** tab
2. Select your preferred language from the dropdown
3. Tap the microphone button to start recording
4. Speak your farming question
5. Listen to or read the AI-generated response
6. Tap the speaker icon on responses to replay audio

### History
1. Navigate to the **History** tab
2. View all past disease detections
3. Tap delete icon to remove individual items
4. Use **Clear All History** to delete all records

### Finance Tracker
1. Navigate to the **Finance** tab
2. Use Quick Actions to log Income, Expense, Loan, or Subsidy
3. Tap **Miscellaneous** for specialized costs (e.g., equipment rental, maintenance)
4. View **Reports & Analytics** for visual Donut and Bar charts of your finances
5. Check **Profit Summary** to see your 6-month historical line chart
6. Consult **AI Insights** for smart suggestions on improving profitability

### Settings
1. Navigate to the **Settings** tab
2. Change default language
3. Toggle save history and auto-play options
4. Clear cache or history data
5. Tap **Save Settings** to persist changes

## Customization

### Adding New Diseases
Update `CropDiseaseAIService.cs` labels array and `RecommendationService.cs` with new disease data.

### Adding New Languages
Add language to `SpeechRecognitionService.GetSupportedLanguages()` and update voice names.

### Styling
Modify colors and styles in `Resources/Styles/Colors.xaml` and `Resources/Styles/Styles.xaml`.

## Production Deployment

### Android Release Build

**Using Visual Studio 2026:**
1. Set configuration to **Release**
2. Right-click project → **Publish** → **Google Play Store**
3. Follow wizard to create signed AAB/APK

**Using Command Line:**
```powershell
# Create release build
dotnet publish -f net9.0-android -c Release

# Output location:
# bin/Release/net9.0-android/publish/
```

**Google Play Store Submission:**
1. Create app listing in Google Play Console
2. Upload APK/AAB file
3. Complete store listing (description, screenshots, privacy policy)
4. Submit for review

**App Versioning:**
Update in `.csproj` file:
```xml
<ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
<ApplicationVersion>1</ApplicationVersion>
```

## Troubleshooting

### Build Errors

**Error: "The workload 'net8.0-android' is out of support"**
- **Solution**: Project uses .NET 9.0. Update Visual Studio 2026 and install .NET 9 SDK
- Already configured in project: `<TargetFrameworks>net9.0-android</TargetFrameworks>`

**Error: "Android SDK directory could not be found"**
```powershell
# Solution 1: Install Android workload
dotnet workload install android

# Solution 2: Install via Visual Studio Installer
# Modify VS 2026 → Check ".NET Multi-platform App UI development"
```

**Error: "Java SDK directory could not be found"**
```powershell
# Install Microsoft OpenJDK 17
winget install Microsoft.OpenJDK.17
# Restart terminal after installation
```

**Error: "Failed to compute hash for file 'Resources\Splash\splash.svg'"**
- **Solution**: Splash screen and fonts are commented out in .csproj and MauiProgram.cs
- If you see this error, ensure these are commented out or create the files

**Error: "Multiple child elements in DataTemplate"**
- **Solution**: Wrap multiple elements in a single Grid/StackLayout container
- Already fixed in VoiceAssistantPage.xaml

**Error: "Locale does not contain a constructor"**
- **Solution**: Removed locale parameter from TextToSpeech
- Already fixed in TextToSpeechService.cs

**Error: "resource mipmap/appicon not found"**
- **Solution**: Icon references removed from AndroidManifest.xml
- App uses default icon until custom icons are added

**Warning: "targetSdkVersion '34' is less than TargetFrameworkVersion"**
- **Solution**: Updated to API 35 in AndroidManifest.xml
- `<uses-sdk android:minSdkVersion="24" android:targetSdkVersion="35" />`

**Build Cache Issues:**
```powershell
# PowerShell
dotnet clean
Remove-Item -Recurse -Force bin,obj
dotnet restore
dotnet build -f net9.0-android

# Or in Visual Studio
# Build → Clean Solution
# Manually delete bin/obj folders
# Build → Rebuild Solution
```

### Runtime Errors

**Model not loading**: 
- Ensure ONNX model is in `Resources/Raw/` and set as `MauiAsset`
- App will use mock predictions if model is not found

**Camera not working**: 
- Check permissions are granted in device settings
- Verify AndroidManifest.xml has camera permission
- For iOS, check Info.plist has usage descriptions

**Speech recognition failing**: 
- Verify microphone permissions
- Check internet connectivity (required for cloud-based recognition)
- Mock responses are used for demo if Azure services not configured

**App crashes on startup**:
- Check Output window in Visual Studio for detailed error logs
- Ensure all NuGet packages are restored
- Delete bin/obj folders and rebuild
- Check Android emulator API level (minimum API 24 required)

**Emulator not starting**:
- Open Android Device Manager in Visual Studio
- Create new device with Android 13.0+ (API 33+)
- Ensure hardware acceleration is enabled (Intel HAXM or AMD-V)

### Performance Issues

**Slow ML inference**: 
- Ensure model is loaded once at startup (implemented in CropDiseaseAIService)
- Run predictions on background thread (already implemented)
- Consider model quantization for faster inference

**UI freezing**: 
- All long-running operations run on background threads
- If UI freezes, check for missing async/await in ViewModels

## Contributing
This is a hackathon project. Feel free to fork and enhance!

## License
MIT License

## Contact
For questions or support, contact the KrishiAI development team.

---

**Built with ❤️ for farmers**

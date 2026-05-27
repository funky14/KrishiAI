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

### 💾 Offline-First Architecture
- Disease detection works completely offline
- SQLite local database for history
- Cached recommendations
- Network-aware features

## Technology Stack

- **.NET MAUI 8.0**: Cross-platform framework
- **MVVM Pattern**: CommunityToolkit.Mvvm
- **ML.NET**: Microsoft.ML.OnnxRuntime for AI inference
- **SQLite**: Local data persistence
- **Azure Services**: Speech Recognition, Text-to-Speech, OpenAI (optional)

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
- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with MAUI workload
- Android SDK (for Android development)
- Xcode (for iOS development on Mac)

### Setup

1. **Clone the repository**
   ```bash
   cd "c:\Chetan\Projects\Hackathon\AI Farmer Assistant\KrishiAI.App"
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Add MobileNetV2 ONNX Model**
   - Place your trained `mobilenetv2_cropdisease.onnx` model in `Resources/Raw/` folder
   - Update the labels in `CropDiseaseAIService.cs` to match your model's classes

4. **Configure Azure Services (Optional)**
   - Add Azure Speech API keys for speech recognition
   - Add Azure OpenAI credentials for chat service
   - Update configuration in respective services

### Build and Run

**For Android:**
```bash
dotnet build -f net8.0-android
dotnet run -f net8.0-android
```

**For iOS:**
```bash
dotnet build -f net8.0-ios
dotnet run -f net8.0-ios
```

## Configuration

### Camera Permissions

**Android** (`Platforms/Android/AndroidManifest.xml`):
```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

**iOS** (`Platforms/iOS/Info.plist`):
```xml
<key>NSCameraUsageDescription</key>
<string>This app needs camera access to capture crop images for disease detection</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>This app needs photo library access to select crop images</string>
<key>NSMicrophoneUsageDescription</key>
<string>This app needs microphone access for voice assistant features</string>
```

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

### Android
1. Set configuration to Release
2. Generate signed APK/AAB
3. Upload to Google Play Console

### iOS
1. Configure signing certificates
2. Archive for App Store
3. Submit to App Store Connect

## Troubleshooting

**Model not loading**: Ensure ONNX model is in `Resources/Raw/` and set as `MauiAsset`

**Camera not working**: Check permissions are granted in device settings

**Speech recognition failing**: Verify microphone permissions and internet connectivity

**Build errors**: Clean solution and restore NuGet packages

## Contributing
This is a hackathon project. Feel free to fork and enhance!

## License
MIT License

## Contact
For questions or support, contact the KrishiAI development team.

---

**Built with ❤️ for farmers**

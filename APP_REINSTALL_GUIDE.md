# KrishiAI App - Uninstall & Reinstall Guide

## ✅ Completed Steps

1. **Emulator Status**: ✅ Emulator is running and booted
2. **App Uninstalled**: ✅ `com.krishiai.farmerassistant` successfully uninstalled
3. **Android SDK API 36**: ✅ Installed and configured
4. **Project Build**: ✅ Builds successfully

---

## 📋 Next Steps - Reinstall App

### Option 1: Deploy from Visual Studio (Recommended)

1. **Open Visual Studio** with the KrishiAI.App project open
2. **Ensure Debug Configuration**:
   - Select Configuration: **Debug**
   - Select Platform: **Any CPU**
3. **Start Debugging**:
   - Press **F5** or
   - Go to **Debug → Start Debugging**
   - Or click the green Play button (▶️)

4. **Wait for Deployment**:
   - Visual Studio will:
	 - Build the app
	 - Package it as APK
	 - Install on the emulator
	 - Launch the app
   - This typically takes 2-5 minutes

---

### Option 2: Manual Deploy via Command Line

```powershell
# Set variables
$androidSdk = "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk"
$adbPath = "$androidSdk\platform-tools\adb.exe"
$projectPath = "C:\KrishiAI\KrishiAI"
$apkPath = "$projectPath\bin\Debug\net9.0-android36.0\com.krishiai.farmerassistant.apk"

# Build the APK
cd $projectPath
Write-Host "Building APK..."
dotnet build KrishiAI.App.csproj -c Debug -f net9.0-android36.0

# Wait for emulator
Write-Host "Waiting for emulator..."
for ($i = 1; $i -le 30; $i++) {
	$result = & $adbPath devices 2>&1
	if ($result -match "emulator.*device") {
		Write-Host "✅ Emulator online!"
		break
	}
	Start-Sleep -Seconds 2
}

# Install APK
Write-Host "Installing app on emulator..."
& $adbPath install -r $apkPath

# Launch app
Write-Host "Launching app..."
& $adbPath shell am start -n com.krishiai.farmerassistant/.MainActivity
```

---

### Option 3: Use Android Device Manager

1. **Open Android Device Manager**:
   - Tools → Android → Android Device Manager

2. **Find `pixel_7_-_api_36_0`** and click **Start**

3. **Wait for emulator to fully boot** (green circle indicator)

4. **In Visual Studio**, press **F5** to deploy

---

## 🐛 If Deploy Fails

### "Emulator offline" or "Device not found"
```powershell
# Kill ADB and restart
$adbPath = "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk\platform-tools\adb.exe"
& $adbPath kill-server
Start-Sleep -Seconds 2
& $adbPath start-server
Start-Sleep -Seconds 3
& $adbPath devices
```

### "APK not found"
- Make sure you built in Debug configuration
- Try: Build → Clean Solution → Rebuild Solution

### "Build fails"
```powershell
cd "C:\KrishiAI\KrishiAI"
# Clean all build artifacts
Remove-Item "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "obj" -Recurse -Force -ErrorAction SilentlyContinue
# Rebuild
dotnet build KrishiAI.App.csproj -c Debug
```

---

## ✅ Verification

After successful installation, verify:

1. **App appears on emulator home screen** with "KrishiAI" icon
2. **Can tap to open** the app
3. **App loads without crashing**

To check app logs:
```powershell
$adbPath = "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk\platform-tools\adb.exe"
& $adbPath logcat | Select-String "KrishiAI|farmerassistant"
```

---

## 📝 Current Status Summary

- ✅ Emulator: **Running**
- ✅ App Uninstalled: **com.krishiai.farmerassistant removed**
- ✅ Android SDK API 36: **Installed**
- ✅ Project: **Builds successfully**
- ⏳ Next: **Deploy & Test on Emulator**

**You're ready to deploy! Press F5 in Visual Studio now.** 🚀

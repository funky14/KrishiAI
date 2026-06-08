# Android Emulator Deployment Troubleshooting Guide

## Issue: Deployment Cancelled - "Waiting for emulator to be ready..."

The deployment cancels after ~1-2 minutes because the Android emulator is not starting or is too slow to boot.

---

## Quick Fixes (Try in Order)

### 1. **Kill Stuck Emulator & Clean Build**
```powershell
# Kill all emulator processes
Get-Process qemu* -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process emulator* -ErrorAction SilentlyContinue | Stop-Process -Force

# Clean and rebuild
# In Visual Studio: Build → Clean Solution
# Then: Build → Rebuild Solution
```

### 2. **Restart ADB (Android Debug Bridge)**
```powershell
$androidSdk = "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk"
$adbPath = "$androidSdk\platform-tools\adb.exe"

& $adbPath kill-server
Start-Sleep -Seconds 2
& $adbPath start-server
Start-Sleep -Seconds 2
& $adbPath devices
```

### 3. **Cold Boot the Emulator**
In Visual Studio:
- Tools → Android → Android Device Manager
- Find `pixel_7_-_api_36_0`
- Click the dropdown arrow → Cold Boot Now
- Wait 3-5 minutes for it to fully boot (green circle)
- Then rebuild your project

---

## Root Causes & Solutions

### **Cause: Insufficient RAM for Emulator**
- **Fix**: Allocate more RAM to the emulator
  1. Open Android Device Manager
  2. Right-click `pixel_7_-_api_36_0` → Edit
  3. Increase RAM to at least 4GB (recommend 6-8GB if available)
  4. Click Finish and cold boot

### **Cause: Emulator Snapshots Corrupted**
- **Fix**: Delete emulator data and restart fresh
  ```powershell
  $androidHome = "C:\Users\$env:USERNAME\.android"
  Remove-Item "$androidHome\avd\pixel_7_-_api_36_0.avd\snapshots" -Recurse -Force -ErrorAction SilentlyContinue
  ```

### **Cause: Slow System Performance**
- **Fix**: Close unnecessary applications
  - Close Chrome, IDEs, Docker, etc.
  - Check Task Manager for high CPU/memory usage
  - Emulators require significant resources

### **Cause: Hardware Virtualization Not Enabled**
- **Fix**: Enable in BIOS (Windows 10/11)
  1. Restart computer and enter BIOS (usually F2, Del, or F12)
  2. Look for "Virtualization Technology" or "VT-x"
  3. Enable it and save
  4. Restart Windows

### **Cause: Hyper-V Conflicts (if using Docker/WSL)**
- **Fix**: Disable Hyper-V if not needed
  ```powershell
  # Run as Administrator
  bcdedit /set hypervisorlaunchtype off
  # Restart computer
  ```
  Or use Hyper-V emulator instead of QEMU

---

## Alternative: Use Physical Android Device

If emulator continues to fail, use a physical Android device:

1. **Enable Developer Mode** on your Android phone
   - Go to Settings → About Phone
   - Tap "Build Number" 7 times
   - Enable "USB Debugging" in Developer Options

2. **Connect via USB**
   - Plug phone into computer via USB cable
   - Accept the "Allow USB debugging?" prompt on phone
   - In Visual Studio: Run project → it will deploy to phone

3. **Verify connection**
   ```powershell
   $androidSdk = "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk"
   & "$androidSdk\platform-tools\adb.exe" devices
   ```

---

## Verify Configuration

✅ **Project file updated with:**
- Extended timeout values
- Disabled Fast Deployment
- Optimized Android settings
- AndroidManifest.xml set to API 36

✅ **Try deploying now:**
- Press F5 to start debugging
- Wait 3-5 minutes for emulator to boot
- If still times out, follow fixes above

---

## Emergency Reset

If nothing works, perform a complete reset:

```powershell
# 1. Kill all Android processes
Get-Process *adb* -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process *qemu* -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process *emulator* -ErrorAction SilentlyContinue | Stop-Process -Force

# 2. Delete emulator instance and recreate
$androidSdk = "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk"
Remove-Item "C:\Users\$env:USERNAME\.android\avd\pixel_7_-_api_36_0.avd" -Recurse -Force -ErrorAction SilentlyContinue

# 3. Open Android Device Manager in Visual Studio
# Tools → Android → Android Device Manager
# Create new emulator with name: pixel_7_-_api_36_0, API Level 36

# 4. Rebuild and deploy
```

---

## Still Stuck?

Check the following:
- ✅ Windows 10/11 latest updates installed
- ✅ Visual Studio latest updates installed
- ✅ .NET 9 SDK installed: `dotnet --version`
- ✅ Android SDK API 36 installed: Open SDK Manager in VS
- ✅ At least 20GB free disk space for emulator
- ✅ CPU supports virtualization (check BIOS)

If issue persists, use a **physical Android device** for testing instead.

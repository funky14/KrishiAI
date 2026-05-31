# Complete Kaggle Setup and PlantVillage Download Script
# Run: .\setup_kaggle_and_download.ps1

Write-Host "=" -NoNewline -ForegroundColor Cyan
Write-Host ("=" * 69) -ForegroundColor Cyan
Write-Host "🔑 Kaggle API Setup & PlantVillage Download" -ForegroundColor Green
Write-Host "=" -NoNewline -ForegroundColor Cyan
Write-Host ("=" * 69) -ForegroundColor Cyan

# Step 1: Create .kaggle folder
Write-Host "`n📁 Step 1: Creating .kaggle folder..." -ForegroundColor Yellow
$kaggleDir = "$env:USERPROFILE\.kaggle"
New-Item -Path $kaggleDir -ItemType Directory -Force | Out-Null
Write-Host "✅ Created: $kaggleDir" -ForegroundColor Green

# Step 2: Check for kaggle.json in Downloads
Write-Host "`n🔍 Step 2: Looking for kaggle.json in Downloads..." -ForegroundColor Yellow
$downloadPath = "$env:USERPROFILE\Downloads\kaggle.json"
$kagglePath = "$kaggleDir\kaggle.json"

if (Test-Path $downloadPath) {
    Write-Host "✅ Found kaggle.json in Downloads" -ForegroundColor Green
    Move-Item -Path $downloadPath -Destination $kagglePath -Force
    Write-Host "✅ Moved to: $kagglePath" -ForegroundColor Green
}
elseif (Test-Path $kagglePath) {
    Write-Host "✅ kaggle.json already exists at: $kagglePath" -ForegroundColor Green
}
else {
    Write-Host "❌ kaggle.json not found!" -ForegroundColor Red
    Write-Host "`n📋 Manual Steps Required:" -ForegroundColor Yellow
    Write-Host "   1. Open: https://www.kaggle.com/settings/account" -ForegroundColor White
    Write-Host "   2. Scroll to 'API' section" -ForegroundColor White
    Write-Host "   3. Click 'Create New API Token'" -ForegroundColor White
    Write-Host "   4. Save kaggle.json to Downloads" -ForegroundColor White
    Write-Host "   5. Re-run this script" -ForegroundColor White
    Write-Host ""
    $openBrowser = Read-Host "Open Kaggle settings now? (Y/N)"
    if ($openBrowser -eq "Y" -or $openBrowser -eq "y") {
        Start-Process "https://www.kaggle.com/settings/account"
    }
    exit
}

# Step 3: Install Kaggle CLI
Write-Host "`n📦 Step 3: Installing Kaggle CLI..." -ForegroundColor Yellow
py -3.11 -m pip install kaggle --quiet
Write-Host "✅ Kaggle CLI installed" -ForegroundColor Green

# Step 4: Test Kaggle CLI
Write-Host "`n🧪 Step 4: Testing Kaggle CLI..." -ForegroundColor Yellow
try {
    $datasets = kaggle datasets list 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Kaggle CLI working!" -ForegroundColor Green
    }
    else {
        throw "Kaggle CLI error"
    }
}
catch {
    Write-Host "❌ Kaggle CLI test failed" -ForegroundColor Red
    Write-Host "   Check your kaggle.json credentials" -ForegroundColor Yellow
    exit
}

# Step 5: Download PlantVillage Dataset
Write-Host "`n📥 Step 5: Downloading PlantVillage Dataset (500MB)..." -ForegroundColor Yellow
Write-Host "   This may take 5-10 minutes..." -ForegroundColor Gray

$datasetDir = "PlantVillage"
if (Test-Path $datasetDir) {
    $classCount = (Get-ChildItem $datasetDir -Directory | Measure-Object).Count
    if ($classCount -gt 0) {
        Write-Host "⚠️  PlantVillage folder already exists with $classCount classes" -ForegroundColor Yellow
        $download = Read-Host "   Re-download? (Y/N)"
        if ($download -ne "Y" -and $download -ne "y") {
            Write-Host "`n✅ Using existing dataset" -ForegroundColor Green
            Write-Host "`n🎓 Ready to train! Run: py -3.11 train_mobilenetv2_pytorch.py" -ForegroundColor Green
            exit
        }
        Remove-Item $datasetDir -Recurse -Force
    }
}

kaggle datasets download -d abdallahalidev/plantvillage-dataset

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Download complete!" -ForegroundColor Green
    
    # Extract ZIP
    Write-Host "`n📂 Extracting dataset..." -ForegroundColor Yellow
    Expand-Archive -Path "plantvillage-dataset.zip" -DestinationPath $datasetDir -Force
    
    # Clean up ZIP
    Remove-Item "plantvillage-dataset.zip"
    
    Write-Host "✅ Extraction complete!" -ForegroundColor Green
}
else {
    Write-Host "❌ Download failed" -ForegroundColor Red
    Write-Host "   Try manual download from: https://www.kaggle.com/datasets/abdallahalidev/plantvillage-dataset" -ForegroundColor Yellow
    exit
}

# Step 6: Verify Dataset
Write-Host "`n✅ Verifying dataset..." -ForegroundColor Yellow
$classes = Get-ChildItem $datasetDir -Directory
$classCount = ($classes | Measure-Object).Count
$imageCount = (Get-ChildItem $datasetDir -Recurse -Include *.jpg,*.JPG,*.png | Measure-Object).Count

Write-Host "`n" + ("=" * 70) -ForegroundColor Cyan
Write-Host "🎉 SUCCESS! PlantVillage Dataset Ready!" -ForegroundColor Green
Write-Host ("=" * 70) -ForegroundColor Cyan
Write-Host "`n📊 Dataset Statistics:" -ForegroundColor Yellow
Write-Host "   • Disease Classes: $classCount" -ForegroundColor White
Write-Host "   • Total Images: $imageCount" -ForegroundColor White
Write-Host "   • Location: $((Get-Item $datasetDir).FullName)" -ForegroundColor White

Write-Host "`n📋 Sample Classes:" -ForegroundColor Yellow
$classes | Select-Object -First 10 | ForEach-Object {
    $imgCount = (Get-ChildItem $_.FullName -Include *.jpg,*.JPG,*.png | Measure-Object).Count
    Write-Host "   • $($_.Name): $imgCount images" -ForegroundColor White
}
if ($classCount -gt 10) {
    Write-Host "   ... and $($classCount - 10) more classes" -ForegroundColor Gray
}

Write-Host "`n🎓 Next Step - Train MobileNetV2:" -ForegroundColor Green
Write-Host "   py -3.11 train_mobilenetv2_pytorch.py" -ForegroundColor Cyan
Write-Host "`n" + ("=" * 70) -ForegroundColor Cyan

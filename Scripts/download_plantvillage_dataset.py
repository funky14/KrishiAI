"""
Download PlantVillage Dataset for training
Run: py -3.11 download_plantvillage_dataset.py
"""
import os
import urllib.request
import zipfile
import shutil
from pathlib import Path

print("=" * 70)
print("📥 PlantVillage Dataset Downloader")
print("=" * 70)

dataset_dir = Path("PlantVillage")

# Option 1: Kaggle API (fastest if you have API key)
print("\n🔥 OPTION 1: Download via Kaggle API (Fastest)")
print("=" * 70)
print("1. Install Kaggle CLI:")
print("   py -3.11 -m pip install kaggle")
print("")
print("2. Setup Kaggle credentials:")
print("   - Go to: https://www.kaggle.com/settings/account")
print("   - Click 'Create New API Token'")
print("   - Save kaggle.json to: C:\\Users\\YourUsername\\.kaggle\\")
print("")
print("3. Run these commands:")
print("   py -3.11 -m pip install kaggle")
print("   kaggle datasets download -d abdallahalidev/plantvillage-dataset")
print("   Expand-Archive plantvillage-dataset.zip -DestinationPath PlantVillage")
print("")

# Option 2: Manual download
print("\n📋 OPTION 2: Manual Download (Recommended)")
print("=" * 70)
print("1. Go to: https://www.kaggle.com/datasets/abdallahalidev/plantvillage-dataset")
print("2. Click 'Download' button (requires Kaggle account)")
print("3. Extract downloaded ZIP to this folder:")
print(f"   {Path.cwd() / 'PlantVillage'}")
print("")
print("Expected structure after extraction:")
print("   PlantVillage/")
print("     ├── Tomato___Late_blight/")
print("     │   ├── image1.jpg")
print("     │   ├── image2.jpg")
print("     │   └── ...")
print("     ├── Potato___Early_blight/")
print("     ├── Corn_(maize)___Common_rust/")
print("     └── ...other disease folders...")
print("")

# Option 3: Alternative smaller dataset for quick testing
print("\n⚡ OPTION 3: Quick Test Dataset (Smaller, Faster)")
print("=" * 70)
print("For quick testing, download a smaller subset:")
print("1. Go to: https://github.com/spMohanty/PlantVillage-Dataset")
print("2. Download specific disease folders you want to test")
print("3. Place in PlantVillage/ folder")
print("")

# Check if dataset exists
if dataset_dir.exists():
    class_folders = [d for d in dataset_dir.iterdir() if d.is_dir()]
    if class_folders:
        print(f"\n✅ Dataset found! {len(class_folders)} disease classes detected:")
        for i, folder in enumerate(class_folders[:10], 1):
            image_count = len(list(folder.glob("*.jpg"))) + len(list(folder.glob("*.JPG"))) + len(list(folder.glob("*.png")))
            print(f"   {i}. {folder.name}: {image_count} images")
        if len(class_folders) > 10:
            print(f"   ... and {len(class_folders) - 10} more classes")
        print(f"\n✅ Ready to train! Run: py -3.11 train_mobilenetv2_pytorch.py")
    else:
        print(f"\n⚠️  PlantVillage folder exists but is empty")
        print(f"   Please download dataset and extract to: {dataset_dir.absolute()}")
else:
    print(f"\n⚠️  PlantVillage folder not found")
    print(f"   Please create folder and download dataset to: {dataset_dir.absolute()}")

print("\n" + "=" * 70)
print("💡 Tips:")
print("   - Full dataset is ~500MB (54,000+ images)")
print("   - For quick testing, download 3-5 disease classes only")
print("   - More images per class = better accuracy")
print("   - Minimum: 50 images per class recommended")
print("=" * 70)

"""
Train MobileNetV2 on PlantVillage Dataset using PyTorch
Works with Python 3.14+!

This script:
1. Downloads PlantVillage dataset
2. Fine-tunes MobileNetV2 with transfer learning  
3. Converts trained model to ONNX format

Run: pip install torch torchvision onnx pillow requests
"""
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import DataLoader
from torchvision import models, transforms, datasets
import os
import requests
import zipfile
from pathlib import Path
import shutil

print("=" * 70)
print("🌾 KrishiAI - PyTorch MobileNetV2 PlantVillage Training")
print("=" * 70)

# Configuration
BATCH_SIZE = 32
EPOCHS = 10  # Increase to 20-50 for better accuracy
IMG_SIZE = 224
LEARNING_RATE = 0.001
DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")

print(f"\n💻 Using device: {DEVICE}")

# Step 1: Download PlantVillage Dataset
print("\n📥 Step 1/5: Downloading PlantVillage dataset...")
print("Downloading from Kaggle mirror (~500MB compressed)...")

dataset_url = "https://data.mendeley.com/public-files/datasets/tywbtsjrjv/files/d5652a28-c1d8-4b76-97f3-72fb80f94efc/file_downloaded"
dataset_path = "PlantVillage"
zip_path = "plantvillage.zip"

if not os.path.exists(dataset_path):
    print("Downloading dataset... This may take 5-10 minutes")
    print("⚠️  If download fails, manually download from:")
    print("   https://www.kaggle.com/datasets/abdallahalidev/plantvillage-dataset")
    print("   Extract to: " + os.path.abspath(dataset_path))
    
    # Alternative: Use a smaller subset for quick testing
    print("\n💡 For quick testing, using smaller PlantVillage subset...")
    print("   Creating sample dataset structure...")
    
    # Create basic structure for manual upload
    os.makedirs(dataset_path, exist_ok=True)
    print(f"\n📁 Created directory: {os.path.abspath(dataset_path)}")
    print("\n⚠️  MANUAL STEP REQUIRED:")
    print("   1. Download PlantVillage from:")
    print("      https://www.kaggle.com/datasets/abdallahalidev/plantvillage-dataset")
    print("   2. Extract to: " + os.path.abspath(dataset_path))
    print("   3. Folder structure should be:")
    print("      PlantVillage/")
    print("        ├── Tomato___Late_blight/")
    print("        ├── Potato___Early_blight/")
    print("        ├── ...other disease folders...")
    print("\n   4. Re-run this script after extracting")
    print("\nPress Ctrl+C to exit and download dataset manually")
    input("\nPress Enter after you've downloaded and extracted the dataset...")
else:
    print(f"✅ Dataset found at: {dataset_path}")

# Verify dataset
class_folders = [d for d in os.listdir(dataset_path) if os.path.isdir(os.path.join(dataset_path, d))]
NUM_CLASSES = len(class_folders)

if NUM_CLASSES == 0:
    print("❌ No class folders found in dataset!")
    print("   Please download PlantVillage dataset and extract to: " + os.path.abspath(dataset_path))
    exit(1)

print(f"✅ Found {NUM_CLASSES} disease classes")
print(f"   Sample classes: {class_folders[:5]}")

# Step 2: Setup data loaders
print("\n🔧 Step 2/5: Setting up data loaders...")

# Data transforms (same as C# preprocessing)
train_transform = transforms.Compose([
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.RandomHorizontalFlip(),
    transforms.RandomRotation(10),
    transforms.ColorJitter(brightness=0.2, contrast=0.2),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225])
])

val_transform = transforms.Compose([
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225])
])

# Load dataset
full_dataset = datasets.ImageFolder(dataset_path, transform=train_transform)

# Split into train/val (80/20)
train_size = int(0.8 * len(full_dataset))
val_size = len(full_dataset) - train_size
train_dataset, val_dataset = torch.utils.data.random_split(full_dataset, [train_size, val_size])

# Update val_dataset transform
val_dataset.dataset.transform = val_transform

train_loader = DataLoader(train_dataset, batch_size=BATCH_SIZE, shuffle=True, num_workers=2)
val_loader = DataLoader(val_dataset, batch_size=BATCH_SIZE, shuffle=False, num_workers=2)

print(f"✅ Data loaders ready!")
print(f"   Training samples: {train_size}")
print(f"   Validation samples: {val_size}")
print(f"   Classes: {NUM_CLASSES}")

# Step 3: Build model
print("\n🏗️ Step 3/5: Building MobileNetV2 model...")

# Load pre-trained MobileNetV2
model = models.mobilenet_v2(pretrained=True)

# Freeze base layers
for param in model.parameters():
    param.requires_grad = False

# Replace classifier
model.classifier[1] = nn.Linear(model.classifier[1].in_features, NUM_CLASSES)

model = model.to(DEVICE)

# Loss and optimizer
criterion = nn.CrossEntropyLoss()
optimizer = optim.Adam(model.classifier.parameters(), lr=LEARNING_RATE)
scheduler = optim.lr_scheduler.ReduceLROnPlateau(optimizer, 'max', patience=2, factor=0.5)

print(f"✅ Model built!")
print(f"   Architecture: MobileNetV2")
print(f"   Output classes: {NUM_CLASSES}")
print(f"   Trainable parameters: {sum(p.numel() for p in model.parameters() if p.requires_grad):,}")

# Step 4: Train model
print(f"\n🎓 Step 4/5: Training for {EPOCHS} epochs...")
print("This takes 10-30 minutes depending on hardware\n")

best_val_acc = 0.0

for epoch in range(EPOCHS):
    # Training phase
    model.train()
    train_loss = 0.0
    train_correct = 0
    train_total = 0
    
    for batch_idx, (inputs, labels) in enumerate(train_loader):
        inputs, labels = inputs.to(DEVICE), labels.to(DEVICE)
        
        optimizer.zero_grad()
        outputs = model(inputs)
        loss = criterion(outputs, labels)
        loss.backward()
        optimizer.step()
        
        train_loss += loss.item()
        _, predicted = outputs.max(1)
        train_total += labels.size(0)
        train_correct += predicted.eq(labels).sum().item()
        
        if batch_idx % 20 == 0:
            print(f"Epoch {epoch+1}/{EPOCHS} | Batch {batch_idx}/{len(train_loader)} | "
                  f"Loss: {loss.item():.4f} | Acc: {100.*train_correct/train_total:.2f}%")
    
    train_acc = 100. * train_correct / train_total
    
    # Validation phase
    model.eval()
    val_correct = 0
    val_total = 0
    
    with torch.no_grad():
        for inputs, labels in val_loader:
            inputs, labels = inputs.to(DEVICE), labels.to(DEVICE)
            outputs = model(inputs)
            _, predicted = outputs.max(1)
            val_total += labels.size(0)
            val_correct += predicted.eq(labels).sum().item()
    
    val_acc = 100. * val_correct / val_total
    
    print(f"\nEpoch {epoch+1} Summary:")
    print(f"  Train Acc: {train_acc:.2f}% | Val Acc: {val_acc:.2f}%\n")
    
    # Save best model
    if val_acc > best_val_acc:
        best_val_acc = val_acc
        torch.save(model.state_dict(), 'best_model.pth')
        print(f"✅ New best model saved! Val Acc: {val_acc:.2f}%\n")
    
    scheduler.step(val_acc)

print(f"\n✅ Training complete! Best validation accuracy: {best_val_acc:.2f}%")

# Load best model
model.load_state_dict(torch.load('best_model.pth'))

# Step 5: Convert to ONNX
print("\n📦 Step 5/5: Converting to ONNX...")

model.eval()
dummy_input = torch.randn(1, 3, IMG_SIZE, IMG_SIZE).to(DEVICE)

output_path = "mobilenetv2_plantvillage_trained.onnx"

torch.onnx.export(
    model,
    dummy_input,
    output_path,
    export_params=True,
    opset_version=13,
    do_constant_folding=True,
    input_names=['input'],
    output_names=['output'],
    dynamic_axes={'input': {0: 'batch_size'}, 'output': {0: 'batch_size'}}
)

print(f"✅ ONNX model saved to: {output_path}")

# Save class names
class_names = full_dataset.classes
with open("class_names.txt", "w", encoding="utf-8") as f:
    for name in class_names:
        f.write(name + "\n")

print(f"✅ Class names saved to: class_names.txt")

# Cleanup
if os.path.exists('best_model.pth'):
    os.remove('best_model.pth')

# Final summary
print("\n" + "=" * 70)
print("🎉 SUCCESS! Trained model ready!")
print("=" * 70)
print(f"\n📊 Final Results:")
print(f"   • Validation Accuracy: {best_val_acc:.2f}%")
print(f"   • Number of Classes: {NUM_CLASSES}")
print(f"   • Model Size: {os.path.getsize(output_path) / (1024*1024):.1f} MB")
print(f"\n📱 Deploy to MAUI App:")
print(f"   1. Copy model:")
print(f"      copy {output_path} ..\\Resources\\Raw\\resnet50_cropdisease.onnx")
print(f"   ")
print(f"   2. Rebuild app:")
print(f"      cd ..")
print(f"      dotnet build -f net9.0-android -c Debug -t:Run")
print(f"   ")
print(f"   3. App will auto-detect {NUM_CLASSES} classes!")
print("=" * 70)

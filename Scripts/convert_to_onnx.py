"""
Convert trained PyTorch model (.pth) to ONNX format
"""
import torch
import torch.nn as nn
from torchvision import models
import os

print("=" * 70)
print("🔄 Converting PyTorch model to ONNX")
print("=" * 70)

# Configuration
IMG_SIZE = 224
NUM_CLASSES = 38  # Update if your dataset has different number of classes
DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")

print(f"\n💻 Using device: {DEVICE}")

# Check if best_model.pth exists
if not os.path.exists('best_model.pth'):
    print("❌ Error: best_model.pth not found!")
    print("   Make sure you're in the Scripts folder where the model was saved")
    exit(1)

print("\n🏗️ Building MobileNetV2 architecture...")

# Rebuild the exact same model architecture
model = models.mobilenet_v2(pretrained=False)
model.classifier[1] = nn.Linear(model.classifier[1].in_features, NUM_CLASSES)
model = model.to(DEVICE)

print("📂 Loading trained weights from best_model.pth...")
model.load_state_dict(torch.load('best_model.pth', map_location=DEVICE))
model.eval()

print("✅ Model loaded successfully!")

print("\n📦 Converting to ONNX format...")

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

# Get file size
size_mb = os.path.getsize(output_path) / (1024 * 1024)
print(f"   File size: {size_mb:.2f} MB")

# Cleanup .pth file
if os.path.exists('best_model.pth'):
    os.remove('best_model.pth')
    print(f"🗑️  Removed best_model.pth (no longer needed)")

print("\n" + "=" * 70)
print("🎉 CONVERSION COMPLETE!")
print("=" * 70)
print(f"\n📁 Your trained ONNX model is ready:")
print(f"   {os.path.abspath(output_path)}")
print(f"\n📋 Next steps:")
print(f"   1. Copy to: C:\\Chetan\\Projects\\Hackathon\\KrishiAI\\Resources\\Raw\\mobilenetv2_cropdisease.onnx")
print(f"   2. Rebuild your MAUI app")
print(f"   3. Deploy to device and test!")

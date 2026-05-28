"""
Download ResNet50 from PyTorch and convert to ONNX
Run: pip install torch torchvision onnx
"""
import torch
import torchvision.models as models
import torch.nn as nn

# ============================================
# CONFIGURATION: Change this to match your dataset
# ============================================
NUM_CLASSES = 38  # Number of disease classes in your dataset
                  # The app will automatically detect this from the model!
# ============================================

print(f"🔽 Downloading ResNet50 for {NUM_CLASSES} disease classes...")

# Load pre-trained ResNet50
model = models.resnet50(pretrained=True)

# Modify the classifier for plant diseases
num_ftrs = model.fc.in_features
model.fc = nn.Linear(num_ftrs, NUM_CLASSES)

print("✅ Model architecture created")
print(f"📊 Output classes: {NUM_CLASSES}")

# Set to evaluation mode
model.eval()

print("🔄 Converting to ONNX format...")

# Create dummy input (batch_size=1, channels=3, height=224, width=224)
dummy_input = torch.randn(1, 3, 224, 224)

# Export to ONNX
output_path = "resnet50_cropdisease.onnx"
torch.onnx.export(
    model,
    dummy_input,
    output_path,
    export_params=True,
    opset_version=13,
    do_constant_folding=True,
    input_names=['input'],
    output_names=['output'],
    dynamic_axes={
        'input': {0: 'batch_size'},
        'output': {0: 'batch_size'}
    }
)

print(f"✅ ONNX model saved to: {output_path}")
print("\n📋 Next steps:")
print(f"   1. Copy '{output_path}' to: C:\\Chetan\\Projects\\Hackathon\\KrishiAI\\Resources\\Raw\\")
print("   2. Rebuild the app: dotnet build -f net9.0-android -c Debug -t:Run")
print("\n⚠️  Note: This is a base ResNet50 model. For production, train on actual plant disease images!")

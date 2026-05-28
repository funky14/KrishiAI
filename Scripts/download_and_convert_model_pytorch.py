"""
Download MobileNetV2 from PyTorch and convert to ONNX
Run: pip install torch torchvision onnx
"""
import torch
import torchvision.models as models
import torch.nn as nn

# Number of disease classes
NUM_CLASSES = 38  # Customize based on your dataset

print("🔽 Downloading MobileNetV2 pre-trained on ImageNet...")

# Load pre-trained MobileNetV2
model = models.mobilenet_v2(pretrained=True)

# Modify the classifier for plant diseases
model.classifier[1] = nn.Linear(model.last_channel, NUM_CLASSES)

print("✅ Model architecture created")
print(f"📊 Output classes: {NUM_CLASSES}")

# Set to evaluation mode
model.eval()

print("🔄 Converting to ONNX format...")

# Create dummy input (batch_size=1, channels=3, height=224, width=224)
dummy_input = torch.randn(1, 3, 224, 224)

# Export to ONNX
output_path = "mobilenetv2_cropdisease.onnx"
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
print("   2. Rebuild the app")
print("\n⚠️  Note: This is a base model. For production, train on actual plant disease images!")

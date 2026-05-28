"""
Download ResNet50 from TensorFlow and convert to ONNX
Run: pip install tensorflow tf2onnx numpy pillow
"""
import tensorflow as tf
import tf2onnx
import numpy as np
import subprocess
import shutil

# Number of disease classes (customize based on your dataset)
NUM_CLASSES = 38  # PlantVillage has 38 classes

print("🔽 Downloading ResNet50 pre-trained on ImageNet...")

# Load ResNet50 pre-trained on ImageNet
base_model = tf.keras.applications.ResNet50(
    input_shape=(224, 224, 3),
    include_top=False,
    weights='imagenet'
)

# Add classification layers for plant diseases
model = tf.keras.Sequential([
    base_model,
    tf.keras.layers.GlobalAveragePooling2D(),
    tf.keras.layers.Dropout(0.2),
    tf.keras.layers.Dense(NUM_CLASSES, activation='softmax')
])

print("✅ Model architecture created")
print(f"📊 Output classes: {NUM_CLASSES}")

# Compile model (needed for conversion)
model.compile(
    optimizer='adam',
    loss='categorical_crossentropy',
    metrics=['accuracy']
)

print("🔄 Converting to ONNX format...")

# Export model to SavedModel format (Keras 3 syntax)
saved_model_path = "temp_saved_model"
model.export(saved_model_path)  # Keras 3: use export() for SavedModel

print("✅ Model exported in TensorFlow SavedModel format")

# Convert from SavedModel to ONNX (more stable than direct Keras conversion)
output_path = "resnet50_cropdisease.onnx"

result = subprocess.run([
    'python', '-m', 'tf2onnx.convert',
    '--saved-model', saved_model_path,
    '--output', output_path,
    '--opset', '13'
], capture_output=True, text=True)

if result.returncode == 0:
    print(f"✅ ONNX model saved to: {output_path}")
else:
    print(f"❌ Conversion failed: {result.stderr}")
    raise Exception("ONNX conversion failed")

# Clean up temporary SavedModel
shutil.rmtree(saved_model_path)
print("🧹 Cleaned up temporary files")
print("\n📋 Next steps:")
print(f"   1. Copy '{output_path}' to: C:\\Chetan\\Projects\\Hackathon\\KrishiAI\\Resources\\Raw\\")
print("   2. Rebuild the app: dotnet build -f net9.0-android -c Debug -t:Run")
print("\n⚠️  Note: This is a base ResNet50 model. For production, train on actual plant disease images!")

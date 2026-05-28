"""
Download MobileNetV2 from TensorFlow and convert to ONNX
Run: pip install tensorflow tf2onnx numpy pillow
"""
import tensorflow as tf
import tf2onnx
import numpy as np

# Number of disease classes (customize based on your dataset)
NUM_CLASSES = 38  # PlantVillage has 38 classes

print("🔽 Downloading MobileNetV2 pre-trained on ImageNet...")

# Load MobileNetV2 pre-trained on ImageNet
base_model = tf.keras.applications.MobileNetV2(
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

# Convert to ONNX
spec = (tf.TensorSpec((None, 224, 224, 3), tf.float32, name="input"),)
output_path = "mobilenetv2_cropdisease.onnx"

model_proto, _ = tf2onnx.convert.from_keras(
    model,
    input_signature=spec,
    opset=13,
    output_path=output_path
)

print(f"✅ ONNX model saved to: {output_path}")
print("\n📋 Next steps:")
print(f"   1. Copy '{output_path}' to: C:\\Chetan\\Projects\\Hackathon\\KrishiAI\\Resources\\Raw\\")
print("   2. Rebuild the app")
print("\n⚠️  Note: This is a base model. For production, train on actual plant disease images!")

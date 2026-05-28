# Crop Disease Detection Setup Guide

## Current Status
✅ **Voice Assistant**: Now auto-detects device language (no dropdown selection needed)  
⚠️ **Crop Detection**: Requires MobileNetV2 ONNX model setup

---

## Option 1: Use Mock Predictions (Testing Only)

**Current default behavior** - No setup needed!

The app will generate **random mock predictions** for testing UI/UX:
- Random disease from the list
- Random confidence (75-95%)
- Works completely offline
- Perfect for demonstration

**To use:**
- Just capture/select an image
- Tap "Analyze"
- Mock results will display

---

## Option 2: Train Your Own Model (Recommended for Production)

### **Step 1: Collect Training Data**

Create a dataset with crop disease images organized by class:

```
dataset/
├── Rice_Blast/
│   ├── img1.jpg
│   ├── img2.jpg
├── Brown_Spot/
│   ├── img1.jpg
│   ├── img2.jpg
├── Tomato_Leaf_Curl/
│   ├── img1.jpg
│   ├── img2.jpg
└── Healthy_Plant/
    ├── img1.jpg
    ├── img2.jpg
```

**Public Datasets:**
- PlantVillage: https://github.com/spMohanty/PlantVillage-Dataset
- Kaggle Plant Diseases: https://www.kaggle.com/datasets/vipoooool/new-plant-diseases-dataset

### **Step 2: Train MobileNetV2 Model**

**Using Python with TensorFlow:**

```python
import tensorflow as tf
from tensorflow.keras.applications import MobileNetV2
from tensorflow.keras.layers import Dense, GlobalAveragePooling2D
from tensorflow.keras.models import Model

# Load pre-trained MobileNetV2
base_model = MobileNetV2(weights='imagenet', include_top=False, input_shape=(224, 224, 3))

# Freeze base model
base_model.trainable = False

# Add custom classification head
x = base_model.output
x = GlobalAveragePooling2D()(x)
x = Dense(128, activation='relu')(x)
predictions = Dense(10, activation='softmax')(x)  # 10 disease classes

model = Model(inputs=base_model.input, outputs=predictions)

# Compile
model.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])

# Train
model.fit(train_data, epochs=10, validation_data=val_data)

# Save as .h5
model.save('mobilenetv2_cropdisease.h5')
```

### **Step 3: Convert to ONNX**

```python
import tf2onnx
import tensorflow as tf

# Load trained model
model = tf.keras.models.load_model('mobilenetv2_cropdisease.h5')

# Convert to ONNX
spec = (tf.TensorSpec((None, 224, 224, 3), tf.float32, name="input"),)
model_proto, _ = tf2onnx.convert.from_keras(model, input_signature=spec, opset=13)

# Save ONNX file
with open("mobilenetv2_cropdisease.onnx", "wb") as f:
    f.write(model_proto.SerializeToString())

print("✅ Model converted to ONNX successfully!")
```

### **Step 4: Add Model to App**

1. Copy `mobilenetv2_cropdisease.onnx` to:
   ```
   c:\Chetan\Projects\Hackathon\AI Farmer Assistant\KrishiAI.App\Resources\Raw\
   ```

2. Ensure `.csproj` includes Raw assets:
   ```xml
   <MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
   ```

3. Update disease labels in `Services/CropDiseaseAIService.cs` to match your training classes:
   ```csharp
   private readonly string[] _labels = new[]
   {
       "Rice Blast",
       "Brown Spot",
       "Bacterial Blight",
       "Tomato Leaf Curl",
       "Early Blight",
       "Late Blight",
       "Potato Blight",
       "Wheat Rust",
       "Cotton Leaf Disease",
       "Healthy Plant"
   };
   ```

4. Rebuild the app:
   ```powershell
   dotnet clean
   dotnet build -f net9.0-android
   ```

---

## Option 3: Use Pre-trained General Model (Quick Start)

Download a general-purpose MobileNetV2 model for quick testing:

### **Download from ONNX Model Zoo:**

```powershell
# Download MobileNetV2 (ImageNet pre-trained)
Invoke-WebRequest -Uri "https://github.com/onnx/models/raw/main/vision/classification/mobilenet/model/mobilenetv2-7.onnx" -OutFile "mobilenetv2-7.onnx"

# Rename
Rename-Item "mobilenetv2-7.onnx" "mobilenetv2_cropdisease.onnx"

# Copy to project
Copy-Item "mobilenetv2_cropdisease.onnx" "c:\Chetan\Projects\Hackathon\AI Farmer Assistant\KrishiAI.App\Resources\Raw\"
```

**Note:** This model uses ImageNet classes (1000 general objects), not crop diseases. You'll need to update the labels or use it for basic testing only.

---

## Verify Model is Loaded

After adding the model and rebuilding:

1. Run the app
2. Open **Output** window in Visual Studio
3. Look for:
   ```
   ✅ ONNX Model loaded successfully from: /data/user/0/com.krishiai.farmerassistant/files/mobilenetv2_cropdisease.onnx
   ```

If you see:
```
⚠️ ONNX Model not found - using mock predictions
```

**Troubleshooting:**
- Verify file exists in `Resources/Raw/`
- Check `.csproj` has `<MauiAsset Include="Resources\Raw\**" .../>`
- Clean and rebuild: `dotnet clean && dotnet build -f net9.0-android`
- Ensure file name is exactly: `mobilenetv2_cropdisease.onnx`

---

## Model Requirements

**Input:**
- Size: 224×224 pixels
- Format: RGB (3 channels)
- Type: Float32
- Value range: 0-1 (normalized)

**Output:**
- Array of class probabilities (length = number of disease classes)
- Softmax activation (probabilities sum to 1.0)

---

## Update Labels for Your Model

Edit `Services/CropDiseaseAIService.cs`:

```csharp
private readonly string[] _labels = new[]
{
    // Replace with YOUR model's disease classes in exact order
    "Your_Disease_Class_1",
    "Your_Disease_Class_2",
    "Your_Disease_Class_3",
    // ... etc
};
```

**Important:** Labels must match the exact order your model was trained with!

---

## Testing Checklist

- [ ] Model file exists in `Resources/Raw/mobilenetv2_cropdisease.onnx`
- [ ] Labels in code match model's training classes
- [ ] App rebuilt after adding model
- [ ] Output window shows "✅ Model loaded successfully"
- [ ] Captured image shows real predictions (not random mock data)
- [ ] Confidence scores are realistic (not always 75-95% random)

---

## Performance Tips

**For faster inference:**
1. Use smaller input size (e.g., 160×160 instead of 224×224)
2. Quantize model to INT8 (reduces size and improves speed)
3. Run inference on background thread (already implemented)

**For better accuracy:**
1. Train on diverse dataset (different lighting, angles, backgrounds)
2. Use data augmentation during training
3. Fine-tune on your specific crop types
4. Collect real-world images from farmers

---

## Next Steps

1. ✅ **Voice Assistant auto-language**: Already updated - test it!
2. **For testing UI**: Keep using mock predictions (no model needed)
3. **For production**: Train your own model (Option 2)
4. **For quick demo**: Use pre-trained model (Option 3) with generic labels

---

**Questions?** Check the troubleshooting section or review the Output window logs when the app starts.

# Get Pre-trained MobileNetV2 Model for KrishiAI

## 🎯 Dynamic Architecture - No Label Files Needed!

**The app automatically detects the number of disease classes from your model!**  
No need for `disease_labels.txt` - just change `NUM_CLASSES` in the Python script and the app adapts.

---

## Quick Start - Download Pre-trained Model

### **Method 1: Download Ready-made ONNX Model** (Fastest)

Download from these sources:

1. **ONNX Model Zoo:**
   - https://github.com/onnx/models/tree/main/vision/classification/mobilenet

2. **Kaggle Models:**
   - https://www.kaggle.com/models?search=plant+disease+mobilenet
   - https://www.kaggle.com/datasets/vipoooool/new-plant-diseases-dataset

3. **Hugging Face:**
   - https://huggingface.co/models?search=mobilenet+plant+disease

After downloading, rename to `mobilenetv2_cropdisease.onnx` and place in `Resources/Raw/`

---

### **Method 2: Convert from TensorFlow** (Recommended)

1. **Install dependencies:**
   ```powershell
   pip install tensorflow tf2onnx numpy pillow
   ```

2. **Run conversion script:**
   ```powershell
   cd "C:\Chetan\Projects\Hackathon\KrishiAI\Scripts"
   python download_and_convert_model.py
   ```

3. **Copy generated model:**
   ```powershell
   copy resnet50_cropdisease.onnx ..\Resources\Raw\
   ```

---

### **Method 3: Convert from PyTorch**

1. **Install dependencies:**
   ```powershell
   pip install torch torchvision onnx
   ```

2. **Run conversion script:**
   ```powershell
   cd "C:\Chetan\Projects\Hackathon\KrishiAI\Scripts"
   python download_and_convert_model_pytorch.py
   ```

3. **Copy generated model:**
   ```powershell
   copy resnet50_cropdisease.onnx ..\Resources\Raw\
   ```

---

## After Getting the Model

1. **Copy model to Resources/Raw:**
   ```powershell
   copy mobilenetv2_cropdisease.onnx ..\Resources\Raw\
   ```

2. **Rebuild and deploy:**
   ```powershell
   cd "C:\Chetan\Projects\Hackathon\KrishiAI"
   dotnet build -f net9.0-android -c Debug -t:Run
   ```

3. **Check logs - the app auto-detects the model's output size:**
   ```
   ✅ MobileNetV2 ONNX Model loaded successfully
   📊 Auto-detected 38 output classes from model
   ```

---

## 🎨 Customizing Number of Classes

Want a different number of disease classes? Just change one line!

**1. Edit the Python script:**
   ```python
   # In download_and_convert_model.py or download_and_convert_model_pytorch.py
   NUM_CLASSES = 50  # Change from 38 to whatever you need
   ```

**2. Regenerate the model:**
   ```powershell
   python download_and_convert_model.py
   copy resnet50_cropdisease.onnx ..\Resources\Raw\
   ```

**3. The app automatically adapts!**
   ```
   📊 Auto-detected 50 output classes from model  ← Updated automatically!
   ```

**Predictions will show:**
- Disease Class 1
- Disease Class 2
- ...
- Disease Class 50

---

## Important Notes

⚠️ **Base models from ImageNet are NOT trained on plant diseases!**

For **production use**, you should:
1. Collect actual crop disease images
2. Fine-tune the model on your dataset
3. Convert the trained model to ONNX

For **demo/testing purposes**, the base model will still work - it will make predictions, but they won't be accurate until you train it.

---

## Training Your Own Model

See `TRAINING_GUIDE.md` for instructions on fine-tuning MobileNetV2 on PlantVillage or custom datasets.

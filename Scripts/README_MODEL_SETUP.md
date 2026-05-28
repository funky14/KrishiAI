# Get Pre-trained MobileNetV2 Model for KrishiAI

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
   copy mobilenetv2_cropdisease.onnx ..\Resources\Raw\
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
   copy mobilenetv2_cropdisease.onnx ..\Resources\Raw\
   ```

---

## After Getting the Model

1. **Verify model exists:**
   ```
   C:\Chetan\Projects\Hackathon\KrishiAI\Resources\Raw\mobilenetv2_cropdisease.onnx
   ```

2. **(Optional) Update disease labels** in `Resources/Raw/disease_labels.txt` to match your model

3. **Rebuild and deploy:**
   ```powershell
   cd "C:\Chetan\Projects\Hackathon\KrishiAI"
   dotnet build -f net9.0-android -c Debug -t:Run
   ```

4. **Check logs** for successful loading:
   ```
   ✅ MobileNetV2 ONNX Model loaded successfully
   📊 Supporting 38 disease classes
   ```

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

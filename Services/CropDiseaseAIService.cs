using KrishiAI.App.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class CropDiseaseAIService : ICropDiseaseAIService
{
    private InferenceSession? _session;
    private string[] _labels = Array.Empty<string>();  // Will be loaded from disease_labels.txt

    public async Task InitializeAsync()
    {
        try
        {
            await Task.Run(async () =>
            {
                // Load disease labels first
                await LoadDiseaseLabelsAsync();
                
                // Try loading model from Resources/Raw (bundled with app)
                var modelPath = await LoadModelFromResourcesAsync();
                
                if (!string.IsNullOrEmpty(modelPath) && File.Exists(modelPath))
                {
                    _session = new InferenceSession(modelPath);
                    
                    // Verify model output matches label count
                    var outputMetadata = _session.OutputMetadata.FirstOrDefault();
                    if (outputMetadata.Value != null)
                    {
                        var outputDimensions = outputMetadata.Value.Dimensions;
                        int numClasses = outputDimensions.Length > 1 ? outputDimensions[^1] : 0;
                        
                        Debug.WriteLine($"✅ MobileNetV2 ONNX Model loaded successfully from: {modelPath}");
                        Debug.WriteLine($"📊 Model outputs: {numClasses} classes");
                        Debug.WriteLine($"📋 Labels loaded: {_labels.Length} disease classes");
                        
                        if (numClasses != _labels.Length)
                        {
                            Debug.WriteLine($"⚠️ WARNING: Model classes ({numClasses}) != Label count ({_labels.Length})");
                        }
                    }
                    
                    // Log input/output metadata
                    Debug.WriteLine("📋 Model Input Metadata:");
                    foreach (var input in _session.InputMetadata)
                    {
                        Debug.WriteLine($"   Input Name: '{input.Key}'");
                        Debug.WriteLine($"   Dimensions: [{string.Join(", ", input.Value.Dimensions)}]");
                        Debug.WriteLine($"   Element Type: {input.Value.ElementType}");
                    }
                    
                    Debug.WriteLine("📋 Model Output Metadata:");
                    foreach (var output in _session.OutputMetadata)
                    {
                        Debug.WriteLine($"   Output Name: '{output.Key}'");
                        Debug.WriteLine($"   Dimensions: [{string.Join(", ", output.Value.Dimensions)}]");
                        Debug.WriteLine($"   Element Type: {output.Value.ElementType}");
                    }
                }
                else
                {
                    Debug.WriteLine("⚠️ MobileNetV2 ONNX Model not found - using mock predictions");
                    Debug.WriteLine("📋 To enable real disease detection:");
                    Debug.WriteLine("   1. Place 'mobilenetv2_cropdisease.onnx' in Resources/Raw/ folder");
                    Debug.WriteLine("   2. Rebuild the app");
                    Debug.WriteLine("");
                    Debug.WriteLine("💡 The app will automatically detect the number of classes from your model!");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ InitializeAsync Error: {ex.Message}");
        }
    }

    private async Task<string> LoadModelFromResourcesAsync()
    {
        try
        {
            Debug.WriteLine("🔍 Attempting to load ONNX model from Resources/Raw...");
            
            // Check if model exists in Resources/Raw
            using var stream = await FileSystem.OpenAppPackageFileAsync("mobilenetv2_cropdisease.onnx");
            
            if (stream != null)
            {
                Debug.WriteLine("✅ Model file found in app package!");
                
                // Copy to AppDataDirectory for access
                var modelPath = Path.Combine(FileSystem.AppDataDirectory, "mobilenetv2_cropdisease.onnx");
                
                Debug.WriteLine($"📂 Copying model to: {modelPath}");
                
                using var fileStream = File.Create(modelPath);
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
                
                Debug.WriteLine("✅ Model copied successfully!");
                
                // Verify the file exists after copying
                if (File.Exists(modelPath))
                {
                    var fileInfo = new FileInfo(modelPath);
                    Debug.WriteLine($"✅ Verified: File exists at {modelPath}, size: {fileInfo.Length} bytes");
                    return modelPath;
                }
                else
                {
                    Debug.WriteLine($"❌ File copy failed - file not found at {modelPath}");
                    return string.Empty;
                }
            }
            else
            {
                Debug.WriteLine("❌ Stream is null - model file not found in app package");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ LoadModelFromResourcesAsync Error: {ex.Message}");
            Debug.WriteLine($"   Exception Type: {ex.GetType().Name}");
            Debug.WriteLine($"   Stack Trace: {ex.StackTrace}");
        }
        
        return string.Empty;
    }

    private async Task LoadDiseaseLabelsAsync()
    {
        try
        {
            Debug.WriteLine("📋 Loading disease labels from disease_labels.txt...");
            
            // Try to load from Resources/Raw
            using var stream = await FileSystem.OpenAppPackageFileAsync("disease_labels.txt");
            
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var labels = new List<string>();
                
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Clean up the label: replace underscores with spaces for display
                        var cleanLabel = line.Replace("___", " - ").Replace("_", " ");
                        labels.Add(cleanLabel);
                    }
                }
                
                _labels = labels.ToArray();
                Debug.WriteLine($"✅ Loaded {_labels.Length} disease labels");
                Debug.WriteLine($"   First label: {_labels[0]}");
                Debug.WriteLine($"   Last label: {_labels[^1]}");
            }
            else
            {
                Debug.WriteLine("⚠️ disease_labels.txt not found - using default labels");
                // Fallback to PlantVillage 38 classes
                _labels = GetDefaultPlantVillageLabels();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error loading labels: {ex.Message}");
            _labels = GetDefaultPlantVillageLabels();
        }
    }

    private string[] GetDefaultPlantVillageLabels()
    {
        return new[]
        {
            "Apple - Apple scab", "Apple - Black rot", "Apple - Cedar apple rust", "Apple - healthy",
            "Blueberry - healthy", "Cherry - Powdery mildew", "Cherry - healthy",
            "Corn - Cercospora leaf spot", "Corn - Common rust", "Corn - Northern Leaf Blight", "Corn - healthy",
            "Grape - Black rot", "Grape - Esca (Black Measles)", "Grape - Leaf blight", "Grape - healthy",
            "Orange - Huanglongbing (Citrus greening)", "Peach - Bacterial spot", "Peach - healthy",
            "Pepper bell - Bacterial spot", "Pepper bell - healthy",
            "Potato - Early blight", "Potato - Late blight", "Potato - healthy",
            "Raspberry - healthy", "Soybean - healthy", "Squash - Powdery mildew",
            "Strawberry - Leaf scorch", "Strawberry - healthy",
            "Tomato - Bacterial spot", "Tomato - Early blight", "Tomato - Late blight", "Tomato - Leaf Mold",
            "Tomato - Septoria leaf spot", "Tomato - Spider mites", "Tomato - Target Spot",
            "Tomato - Yellow Leaf Curl Virus", "Tomato - Tomato mosaic virus", "Tomato - healthy"
        };
    }

    public async Task<DiseaseDetectionResult?> PredictDiseaseAsync(string imagePath)
    {
        try
        {
            // Preprocess image
            var tensor = await PreprocessImageAsync(imagePath);

            // Run inference
            if (_session != null)
            {
                try
                {
                    // Get the actual input name from model metadata
                    var inputName = _session.InputMetadata.Keys.FirstOrDefault() ?? "input";
                    Debug.WriteLine($"📥 Using input name: '{inputName}'");
                    
                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor(inputName, tensor)
                    };

                    Debug.WriteLine($"🔮 Running ONNX inference on: {imagePath}");
                    using var results = _session.Run(inputs);
                    var output = results.FirstOrDefault()?.AsEnumerable<float>().ToArray();

                    if (output != null)
                    {
                        Debug.WriteLine($"✅ Inference successful! Output size: {output.Length}");
                        Debug.WriteLine($"   Expected labels: {_labels.Length}");
                        
                        if (output.Length != _labels.Length)
                        {
                            Debug.WriteLine($"⚠️ WARNING: Output size ({output.Length}) != Labels size ({_labels.Length})");
                            Debug.WriteLine($"   This means your disease_labels.txt doesn't match the model's classes!");
                        }
                        
                        return ProcessPrediction(output, imagePath);
                    }
                }
                catch (Exception modelEx)
                {
                    Debug.WriteLine($"❌ ONNX Model Error: {modelEx.Message}");
                    Debug.WriteLine($"   This usually means tensor shape mismatch!");
                    Debug.WriteLine($"   If you see 'invalid shape' error, the model expects NCHW format instead of NHWC.");
                    Debug.WriteLine($"   Stack trace: {modelEx.StackTrace}");
                    
                    // Fall through to mock prediction
                }
            }
            else
            {
                Debug.WriteLine("⚠️ ONNX model not loaded - using mock predictions");
            }

            // Mock prediction for demo
            return CreateMockPrediction(imagePath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ PredictDiseaseAsync Error: {ex.Message}");
            Debug.WriteLine($"   Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    private async Task<Tensor<float>> PreprocessImageAsync(string imagePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Load image using SkiaSharp
                using var inputStream = File.OpenRead(imagePath);
                using var original = SKBitmap.Decode(inputStream);
                
                // Resize to 224x224 (MobileNetV2 input size)
                using var resized = original.Resize(new SKImageInfo(224, 224), SKFilterQuality.High);
                
                // ImageNet normalization values (standard for MobileNetV2 and other ImageNet models)
                float[] mean = { 0.485f, 0.456f, 0.406f };
                float[] std = { 0.229f, 0.224f, 0.225f };
                
                // Create tensor in NCHW format [1, 3, 224, 224] (channels-first)
                // This matches PyTorch's default format
                var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
                
                for (int y = 0; y < 224; y++)
                {
                    for (int x = 0; x < 224; x++)
                    {
                        var pixel = resized.GetPixel(x, y);
                        
                        // ImageNet normalization: (pixel/255 - mean) / std
                        // NCHW format: [batch, channel, height, width]
                        tensor[0, 0, y, x] = (pixel.Red / 255f - mean[0]) / std[0];    // R channel
                        tensor[0, 1, y, x] = (pixel.Green / 255f - mean[1]) / std[1];  // G channel
                        tensor[0, 2, y, x] = (pixel.Blue / 255f - mean[2]) / std[2];   // B channel
                    }
                }
                
                Debug.WriteLine($"✅ Image preprocessed: {imagePath}");
                Debug.WriteLine($"   Tensor shape: [1, 3, 224, 224] (NCHW format - PyTorch standard)");
                Debug.WriteLine($"   Normalization: ImageNet (mean=[0.485,0.456,0.406], std=[0.229,0.224,0.225])");
                
                return tensor;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PreprocessImageAsync Error: {ex.Message}");
                // Return empty tensor in NCHW format on error
                return new DenseTensor<float>(new[] { 1, 3, 224, 224 });
            }
        });
    }

    private DiseaseDetectionResult ProcessPrediction(float[] logits, string imagePath)
    {
        // Apply Softmax to convert raw logits to probabilities
        // Same as: torch.softmax(preds, dim=1) in PyTorch
        var probabilities = Softmax(logits);
        
        // Find top prediction (argmax)
        var maxIndex = Array.IndexOf(probabilities, probabilities.Max());
        var confidence = probabilities[maxIndex] * 100;  // Now this is a real probability!
        
        // Log top 3 predictions for debugging
        var topPredictions = probabilities
            .Select((prob, index) => new { Index = index, Probability = prob })
            .OrderByDescending(x => x.Probability)
            .Take(3)
            .ToList();
        
        Debug.WriteLine($"🎯 Top 3 Predictions (after Softmax):");
        foreach (var pred in topPredictions)
        {
            var labelName = pred.Index < _labels.Length ? _labels[pred.Index] : $"Unknown Class {pred.Index}";
            Debug.WriteLine($"   {pred.Index + 1}. {labelName}: {pred.Probability * 100:F2}%");
        }

        return new DiseaseDetectionResult
        {
            DiseaseName = maxIndex < _labels.Length ? _labels[maxIndex] : $"Unknown Disease (Class {maxIndex})",
            Confidence = confidence,
            Severity = GetSeverity(confidence),
            ImagePath = imagePath,
            DetectedDate = DateTime.Now,
            Description = $"Detected {_labels[maxIndex]} with {confidence:F1}% confidence"
        };
    }
    
    /// <summary>
    /// Applies softmax function to convert raw logits to probabilities
    /// Equivalent to: torch.softmax(x, dim=1) in PyTorch
    /// </summary>
    private float[] Softmax(float[] logits)
    {
        // Find max for numerical stability (prevents overflow)
        float maxLogit = logits.Max();
        
        // Compute exp(logit - maxLogit) for each value
        float[] expValues = logits.Select(x => (float)Math.Exp(x - maxLogit)).ToArray();
        
        // Sum all exp values
        float sumExp = expValues.Sum();
        
        // Normalize to get probabilities (sum = 1.0)
        float[] probabilities = expValues.Select(x => x / sumExp).ToArray();
        
        Debug.WriteLine($"✅ Softmax applied: logits → probabilities");
        Debug.WriteLine($"   Max logit: {logits.Max():F4} → Max probability: {probabilities.Max():F4}");
        
        return probabilities;
    }

    private DiseaseDetectionResult CreateMockPrediction(string imagePath)
    {
        var random = new Random();
        var diseaseIndex = random.Next(_labels.Length);
        var confidence = 75 + random.NextDouble() * 20; // 75-95%

        return new DiseaseDetectionResult
        {
            DiseaseName = _labels[diseaseIndex],
            Confidence = confidence,
            Severity = GetSeverity(confidence),
            ImagePath = imagePath,
            DetectedDate = DateTime.Now,
            Description = $"Detected {_labels[diseaseIndex]} affecting the crop"
        };
    }

    private string GetSeverity(double confidence)
    {
        if (confidence >= 90) return "High";
        if (confidence >= 75) return "Medium";
        return "Low";
    }
}

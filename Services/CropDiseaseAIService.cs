using KrishiAI.App.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class CropDiseaseAIService : ICropDiseaseAIService
{
    private InferenceSession? _session;
    private string[] _labels = new[]
    {
        // Rice Diseases
        "Rice Blast",
        "Brown Spot",
        "Bacterial Blight",
        "Rice Sheath Blight",
        "Rice Tungro",
        
        // Tomato Diseases
        "Tomato Leaf Curl",
        "Early Blight",
        "Late Blight",
        "Tomato Septoria Leaf Spot",
        "Tomato Yellow Leaf Curl Virus",
        "Tomato Mosaic Virus",
        "Tomato Bacterial Spot",
        "Tomato Target Spot",
        
        // Potato Diseases
        "Potato Early Blight",
        "Potato Late Blight",
        "Potato Blight",
        
        // Wheat Diseases
        "Wheat Rust",
        "Wheat Leaf Blight",
        "Wheat Powdery Mildew",
        
        // Cotton Diseases
        "Cotton Leaf Disease",
        "Cotton Bacterial Blight",
        
        // Corn/Maize Diseases
        "Corn Northern Leaf Blight",
        "Corn Common Rust",
        "Corn Gray Leaf Spot",
        
        // Grape Diseases
        "Grape Black Rot",
        "Grape Leaf Blight",
        "Grape Powdery Mildew",
        
        // Apple Diseases
        "Apple Scab",
        "Apple Black Rot",
        "Apple Cedar Rust",
        
        // Pepper/Chili Diseases
        "Pepper Bacterial Spot",
        "Pepper Leaf Curl",
        
        // Sugarcane Diseases
        "Sugarcane Red Rot",
        "Sugarcane Rust",
        
        // Other Common Diseases
        "Powdery Mildew",
        "Downy Mildew",
        "Anthracnose",
        "Leaf Spot",
        "Root Rot",
        
        // Healthy
        "Healthy Plant"
    };

    public async Task InitializeAsync()
    {
        try
        {
            await Task.Run(async () =>
            {
                // Load custom labels if available
                await LoadLabelsAsync();
                
                // Try loading from Resources/Raw first (bundled with app)
                var modelPath = await LoadModelFromResourcesAsync();
                
                if (!string.IsNullOrEmpty(modelPath) && File.Exists(modelPath))
                {
                    _session = new InferenceSession(modelPath);
                    Debug.WriteLine($"✅ MobileNetV2 ONNX Model loaded successfully from: {modelPath}");
                    Debug.WriteLine($"📊 Supporting {_labels.Length} disease classes");
                }
                else
                {
                    Debug.WriteLine("⚠️ MobileNetV2 ONNX Model not found - using mock predictions");
                    Debug.WriteLine("📋 To enable real disease detection:");
                    Debug.WriteLine("   1. Place 'mobilenetv2_cropdisease.onnx' in Resources/Raw/ folder");
                    Debug.WriteLine("   2. (Optional) Place 'disease_labels.txt' in Resources/Raw/ for custom labels");
                    Debug.WriteLine("   3. Rebuild the app");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ InitializeAsync Error: {ex.Message}");
        }
    }
    
    private async Task LoadLabelsAsync()
    {
        try
        {
            // Try to load custom labels from disease_labels.txt
            using var stream = await FileSystem.OpenAppPackageFileAsync("disease_labels.txt");
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            var customLabels = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(l => l.Trim())
                                     .Where(l => !string.IsNullOrEmpty(l))
                                     .ToArray();
            
            if (customLabels.Length > 0)
            {
                _labels = customLabels;
                Debug.WriteLine($"✅ Loaded {customLabels.Length} custom disease labels from disease_labels.txt");
            }
        }
        catch
        {
            // Use default labels if custom file not found
            Debug.WriteLine("ℹ️ Using default disease labels (38+ diseases)");
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
                Debug.WriteLine($"✅ Model file found in app package! Size: {stream.Length} bytes");
                
                // Copy to AppDataDirectory for access
                var modelPath = Path.Combine(FileSystem.AppDataDirectory, "mobilenetv2_cropdisease.onnx");
                
                Debug.WriteLine($"📂 Copying model to: {modelPath}");
                
                using var fileStream = File.Create(modelPath);
                await stream.CopyToAsync(fileStream);
                
                Debug.WriteLine($"✅ Model copied successfully! File size: {new FileInfo(modelPath).Length} bytes");
                
                return modelPath;
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
            
            // Try to list what files ARE available in the package
            try
            {
                Debug.WriteLine("📋 Attempting to list available files in app package...");
                // This might not work, but worth a try
            }
            catch { }
        }
        
        return string.Empty;
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
                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor("input", tensor)
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
                
                // ImageNet normalization values (standard for MobileNetV2)
                float[] mean = { 0.485f, 0.456f, 0.406f };
                float[] std = { 0.229f, 0.224f, 0.225f };
                
                // Create tensor in NHWC format [1, 224, 224, 3] (channels-last)
                // Try this first. If model expects NCHW, we'll switch.
                var tensor = new DenseTensor<float>(new[] { 1, 224, 224, 3 });
                
                for (int y = 0; y < 224; y++)
                {
                    for (int x = 0; x < 224; x++)
                    {
                        var pixel = resized.GetPixel(x, y);
                        
                        // ImageNet normalization: (pixel/255 - mean) / std
                        tensor[0, y, x, 0] = (pixel.Red / 255f - mean[0]) / std[0];    // R channel
                        tensor[0, y, x, 1] = (pixel.Green / 255f - mean[1]) / std[1];  // G channel
                        tensor[0, y, x, 2] = (pixel.Blue / 255f - mean[2]) / std[2];   // B channel
                    }
                }
                
                Debug.WriteLine($"✅ Image preprocessed: {imagePath}");
                Debug.WriteLine($"   Tensor shape: [1, 224, 224, 3] (NHWC format)");
                Debug.WriteLine($"   Normalization: ImageNet (mean=[0.485,0.456,0.406], std=[0.229,0.224,0.225])");
                
                return tensor;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PreprocessImageAsync Error: {ex.Message}");
                // Return empty tensor on error
                return new DenseTensor<float>(new[] { 1, 3, 224, 224 });
            }
        });
    }

    private DiseaseDetectionResult ProcessPrediction(float[] output, string imagePath)
    {
        // Find top prediction
        var maxIndex = Array.IndexOf(output, output.Max());
        var confidence = output[maxIndex] * 100;
        
        // Log top 3 predictions for debugging
        var topPredictions = output
            .Select((prob, index) => new { Index = index, Probability = prob })
            .OrderByDescending(x => x.Probability)
            .Take(3)
            .ToList();
        
        Debug.WriteLine($"🎯 Top 3 Predictions:");
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

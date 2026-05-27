using KrishiAI.App.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class CropDiseaseAIService : ICropDiseaseAIService
{
    private InferenceSession? _session;
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

    public async Task InitializeAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                var modelPath = Path.Combine(FileSystem.AppDataDirectory, "mobilenetv2_cropdisease.onnx");
                
                // For demo purposes, we'll skip actual model loading if file doesn't exist
                // In production, you would bundle the ONNX model with the app
                if (File.Exists(modelPath))
                {
                    _session = new InferenceSession(modelPath);
                    Debug.WriteLine("ONNX Model loaded successfully");
                }
                else
                {
                    Debug.WriteLine("ONNX Model not found - using mock predictions");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"InitializeAsync Error: {ex.Message}");
        }
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
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input", tensor)
                };

                using var results = _session.Run(inputs);
                var output = results.FirstOrDefault()?.AsEnumerable<float>().ToArray();

                if (output != null)
                {
                    return ProcessPrediction(output, imagePath);
                }
            }

            // Mock prediction for demo
            return CreateMockPrediction(imagePath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PredictDiseaseAsync Error: {ex.Message}");
            return null;
        }
    }

    private async Task<Tensor<float>> PreprocessImageAsync(string imagePath)
    {
        return await Task.Run(() =>
        {
            // In production, you would:
            // 1. Load image using SkiaSharp or ImageSharp
            // 2. Resize to 224x224
            // 3. Normalize RGB values (0-1)
            // 4. Convert to tensor format

            // For now, create a dummy tensor
            var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
            return tensor;
        });
    }

    private DiseaseDetectionResult ProcessPrediction(float[] output, string imagePath)
    {
        var maxIndex = Array.IndexOf(output, output.Max());
        var confidence = output[maxIndex] * 100;

        return new DiseaseDetectionResult
        {
            DiseaseName = _labels[maxIndex],
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

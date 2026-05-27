using KrishiAI.App.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace KrishiAI.App.Services;

public class RecommendationService : IRecommendationService
{
    private Dictionary<string, DiseaseRecommendation>? _recommendations;

    public async Task InitializeAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                // In production, load from JSON file
                _recommendations = GetDefaultRecommendations();
                Debug.WriteLine("Recommendations loaded successfully");
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"InitializeAsync Error: {ex.Message}");
        }
    }

    public async Task<DiseaseRecommendation?> GetRecommendationAsync(string diseaseName)
    {
        await Task.CompletedTask;

        if (_recommendations == null)
        {
            await InitializeAsync();
        }

        if (_recommendations?.TryGetValue(diseaseName, out var recommendation) == true)
        {
            return recommendation;
        }
        
        return null;
    }

    private Dictionary<string, DiseaseRecommendation> GetDefaultRecommendations()
    {
        return new Dictionary<string, DiseaseRecommendation>
        {
            ["Rice Blast"] = new DiseaseRecommendation
            {
                DiseaseName = "Rice Blast",
                Description = "Fungal disease causing gray-green lesions on leaves",
                OrganicTreatment = new List<string>
                {
                    "Apply neem oil spray",
                    "Use Trichoderma viride bio-fungicide",
                    "Maintain proper field drainage"
                },
                ChemicalTreatment = new List<string>
                {
                    "Tricyclazole 75% WP @ 0.6 g/liter",
                    "Carbendazim 50% WP @ 1 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use resistant varieties",
                    "Avoid excess nitrogen fertilizer",
                    "Maintain proper plant spacing"
                },
                Severity = "High",
                AffectedCropPart = "Leaves and grains"
            },
            ["Tomato Leaf Curl"] = new DiseaseRecommendation
            {
                DiseaseName = "Tomato Leaf Curl",
                Description = "Viral disease transmitted by whiteflies",
                OrganicTreatment = new List<string>
                {
                    "Remove infected plants immediately",
                    "Use yellow sticky traps for whiteflies",
                    "Spray neem oil to control vectors"
                },
                ChemicalTreatment = new List<string>
                {
                    "Imidacloprid 17.8% SL @ 0.5 ml/liter",
                    "Thiamethoxam 25% WG @ 0.2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use virus-free transplants",
                    "Control whitefly population",
                    "Remove weeds around field"
                },
                Severity = "Critical",
                AffectedCropPart = "Leaves"
            },
            ["Healthy Plant"] = new DiseaseRecommendation
            {
                DiseaseName = "Healthy Plant",
                Description = "No disease detected - plant appears healthy",
                OrganicTreatment = new List<string>
                {
                    "Continue regular monitoring",
                    "Maintain good agricultural practices"
                },
                ChemicalTreatment = new List<string>
                {
                    "No treatment needed"
                },
                PreventionTips = new List<string>
                {
                    "Regular crop monitoring",
                    "Proper irrigation and nutrition",
                    "Crop rotation"
                },
                Severity = "None",
                AffectedCropPart = "None"
            }
        };
    }
}

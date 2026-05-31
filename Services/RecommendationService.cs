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

        // Try exact match first
        if (_recommendations?.TryGetValue(diseaseName, out var recommendation) == true)
        {
            return recommendation;
        }
        
        // Try matching after removing crop prefix (e.g., "Tomato - Early blight" → "Early Blight")
        if (diseaseName.Contains(" - "))
        {
            var parts = diseaseName.Split(" - ", 2);
            if (parts.Length == 2)
            {
                var diseaseOnly = parts[1].Trim();
                
                // Try case-insensitive partial match
                var match = _recommendations?.FirstOrDefault(r => 
                    r.Key.Equals(diseaseOnly, StringComparison.OrdinalIgnoreCase) ||
                    r.Key.Contains(diseaseOnly, StringComparison.OrdinalIgnoreCase) ||
                    diseaseOnly.Contains(r.Key, StringComparison.OrdinalIgnoreCase)
                );
                
                if (match?.Value != null)
                {
                    Debug.WriteLine($"✅ Found recommendation for '{diseaseName}' using disease part: '{diseaseOnly}'");
                    return match.Value.Value;
                }
            }
        }
        
        // Return generic recommendation for unknown diseases
        Debug.WriteLine($"⚠️ No specific recommendation found for: {diseaseName}");
        return new DiseaseRecommendation
        {
            DiseaseName = diseaseName,
            Description = $"Detected disease: {diseaseName}. Consult local agricultural experts for specific treatment.",
            OrganicTreatment = new List<string>
            {
                "Remove and destroy infected plant parts",
                "Spray neem oil solution (5 ml/liter)",
                "Apply Trichoderma viride biocontrol agent",
                "Improve field sanitation"
            },
            ChemicalTreatment = new List<string>
            {
                "Consult agricultural extension officer",
                "Get soil and plant tissue tested",
                "Use broad-spectrum fungicide/bactericide as recommended"
            },
            PreventionTips = new List<string>
            {
                "Use certified disease-free seeds",
                "Practice crop rotation",
                "Maintain proper plant spacing",
                "Ensure good drainage and sanitation",
                "Monitor crops regularly for early detection"
            },
            Severity = "Unknown - Requires Expert Consultation",
            AffectedCropPart = "Consult expert for accurate diagnosis"
        };
    }

    private Dictionary<string, DiseaseRecommendation> GetDefaultRecommendations()
    {
        return new Dictionary<string, DiseaseRecommendation>
        {
            ["Rice Blast"] = new DiseaseRecommendation
            {
                DiseaseName = "Rice Blast",
                Description = "Fungal disease causing gray-green lesions on leaves and panicles",
                OrganicTreatment = new List<string>
                {
                    "Apply neem oil spray (5 ml/liter)",
                    "Use Trichoderma viride bio-fungicide",
                    "Maintain proper field drainage",
                    "Apply cow urine solution (1:10 ratio)"
                },
                ChemicalTreatment = new List<string>
                {
                    "Tricyclazole 75% WP @ 0.6 g/liter",
                    "Carbendazim 50% WP @ 1 g/liter",
                    "Isoprothiolane 40% EC @ 1.5 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use resistant varieties like Pusa Basmati",
                    "Avoid excess nitrogen fertilizer",
                    "Maintain proper plant spacing (15cm x 15cm)",
                    "Remove crop residues after harvest"
                },
                Severity = "High",
                AffectedCropPart = "Leaves, stems, and grains"
            },
            ["Brown Spot"] = new DiseaseRecommendation
            {
                DiseaseName = "Brown Spot",
                Description = "Fungal disease causing brown spots with yellow halo on leaves",
                OrganicTreatment = new List<string>
                {
                    "Spray Pseudomonas fluorescens",
                    "Apply potassium-rich organic manure",
                    "Use garlic extract spray"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Copper oxychloride 50% WP @ 2.5 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use disease-free certified seeds",
                    "Apply balanced NPK fertilizer",
                    "Avoid water stress during grain filling"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves and grains"
            },
            ["Bacterial Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Bacterial Blight",
                Description = "Bacterial disease causing water-soaked lesions that turn yellow",
                OrganicTreatment = new List<string>
                {
                    "Spray copper sulfate solution (1 g/liter)",
                    "Remove and burn infected plants",
                    "Apply Bacillus subtilis bioagent"
                },
                ChemicalTreatment = new List<string>
                {
                    "Streptocycline 300 ppm + Copper oxychloride",
                    "Plantomycin 100 ppm"
                },
                PreventionTips = new List<string>
                {
                    "Use resistant varieties",
                    "Avoid injury to plants during transplanting",
                    "Drain water from fields after heavy rain",
                    "Treat seeds with hot water (52-54°C for 30 min)"
                },
                Severity = "High",
                AffectedCropPart = "Leaves and stems"
            },
            ["Tomato Leaf Curl"] = new DiseaseRecommendation
            {
                DiseaseName = "Tomato Leaf Curl",
                Description = "Viral disease transmitted by whiteflies causing upward leaf curling",
                OrganicTreatment = new List<string>
                {
                    "Remove and destroy infected plants immediately",
                    "Use yellow sticky traps for whitefly control",
                    "Spray neem oil (5 ml/liter) weekly",
                    "Apply reflective mulch to repel whiteflies"
                },
                ChemicalTreatment = new List<string>
                {
                    "Imidacloprid 17.8% SL @ 0.5 ml/liter",
                    "Thiamethoxam 25% WG @ 0.2 g/liter",
                    "Acetamiprid 20% SP @ 0.2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use virus-free transplants",
                    "Install insect-proof nets in nursery",
                    "Maintain weed-free environment",
                    "Plant resistant varieties like Arka Vikas"
                },
                Severity = "Critical",
                AffectedCropPart = "Leaves"
            },
            ["Early Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Early Blight",
                Description = "Fungal disease causing dark concentric rings on older leaves",
                OrganicTreatment = new List<string>
                {
                    "Spray baking soda solution (1 tablespoon/liter)",
                    "Apply compost tea weekly",
                    "Use copper-based organic fungicides"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Chlorothalonil 75% WP @ 2 g/liter",
                    "Azoxystrobin 23% SC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Remove lower leaves touching soil",
                    "Improve air circulation between plants",
                    "Avoid overhead irrigation",
                    "Rotate crops with non-solanaceous plants"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves and stems"
            },
            ["Late Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Late Blight",
                Description = "Fungal disease causing water-soaked lesions with white mold",
                OrganicTreatment = new List<string>
                {
                    "Spray Bordeaux mixture (1%)",
                    "Apply copper sulfate solution",
                    "Remove infected plant parts immediately"
                },
                ChemicalTreatment = new List<string>
                {
                    "Metalaxyl 8% + Mancozeb 64% WP @ 2 g/liter",
                    "Cymoxanil 8% + Mancozeb 64% WP @ 2 g/liter",
                    "Dimethomorph 50% WP @ 1 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Plant resistant varieties",
                    "Avoid planting tomatoes near potatoes",
                    "Ensure good drainage",
                    "Apply preventive sprays during humid weather"
                },
                Severity = "Critical",
                AffectedCropPart = "Leaves, stems, and fruits"
            },
            ["Potato Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Potato Blight",
                Description = "Fungal disease causing dark lesions on leaves and tuber rot",
                OrganicTreatment = new List<string>
                {
                    "Spray Bordeaux mixture (1%)",
                    "Use disease-free seed tubers",
                    "Apply copper-based fungicides"
                },
                ChemicalTreatment = new List<string>
                {
                    "Metalaxyl 8% + Mancozeb 64% @ 2.5 g/liter",
                    "Cymoxanil 8% + Mancozeb 64% @ 2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Hill up soil around plants",
                    "Destroy infected tubers",
                    "Rotate crops every 3 years",
                    "Harvest during dry weather"
                },
                Severity = "High",
                AffectedCropPart = "Leaves and tubers"
            },
            ["Wheat Rust"] = new DiseaseRecommendation
            {
                DiseaseName = "Wheat Rust",
                Description = "Fungal disease causing reddish-brown pustules on leaves and stems",
                OrganicTreatment = new List<string>
                {
                    "Spray sulfur-based fungicides",
                    "Apply potassium-rich organic fertilizer",
                    "Use resistant varieties"
                },
                ChemicalTreatment = new List<string>
                {
                    "Propiconazole 25% EC @ 1 ml/liter",
                    "Tebuconazole 25.9% EC @ 1 ml/liter",
                    "Mancozeb 75% WP @ 2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Sow rust-resistant varieties",
                    "Avoid late sowing",
                    "Remove volunteer wheat plants",
                    "Apply balanced fertilization"
                },
                Severity = "High",
                AffectedCropPart = "Leaves and stems"
            },
            ["Cotton Leaf Disease"] = new DiseaseRecommendation
            {
                DiseaseName = "Cotton Leaf Disease",
                Description = "Fungal/bacterial disease causing leaf spots and blight",
                OrganicTreatment = new List<string>
                {
                    "Spray neem oil (5 ml/liter)",
                    "Apply Trichoderma viride",
                    "Use panchagavya spray"
                },
                ChemicalTreatment = new List<string>
                {
                    "Copper oxychloride 50% WP @ 3 g/liter",
                    "Carbendazim 50% WP @ 1 g/liter",
                    "Mancozeb 75% WP @ 2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use disease-free seeds",
                    "Maintain proper spacing (60cm x 30cm)",
                    "Remove infected leaves",
                    "Practice crop rotation"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            
            // Additional Rice Diseases
            ["Rice Sheath Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Rice Sheath Blight",
                Description = "Fungal disease causing elliptical lesions on leaf sheaths",
                OrganicTreatment = new List<string>
                {
                    "Apply Pseudomonas fluorescens @ 10 g/liter",
                    "Spray neem oil solution",
                    "Use Trichoderma harzianum"
                },
                ChemicalTreatment = new List<string>
                {
                    "Validamycin 3% L @ 2 ml/liter",
                    "Hexaconazole 5% EC @ 2 ml/liter",
                    "Carbendazim 50% WP @ 1 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Avoid excess nitrogen fertilizer",
                    "Maintain 20cm plant spacing",
                    "Drain water periodically",
                    "Remove infected plant debris"
                },
                Severity = "High",
                AffectedCropPart = "Leaf sheaths and stems"
            },
            ["Rice Tungro"] = new DiseaseRecommendation
            {
                DiseaseName = "Rice Tungro",
                Description = "Viral disease causing yellow-orange discoloration of leaves",
                OrganicTreatment = new List<string>
                {
                    "Remove and destroy infected plants",
                    "Control leafhopper vectors with neem spray",
                    "Use yellow sticky traps"
                },
                ChemicalTreatment = new List<string>
                {
                    "Imidacloprid 17.8% SL @ 0.5 ml/liter (for vectors)",
                    "Thiamethoxam 25% WG @ 0.2 g/liter",
                    "No direct chemical treatment for virus"
                },
                PreventionTips = new List<string>
                {
                    "Use resistant varieties like IR74",
                    "Control green leafhopper populations",
                    "Avoid planting near infected fields",
                    "Synchronize planting in area"
                },
                Severity = "Critical",
                AffectedCropPart = "Leaves"
            },
            
            // Additional Tomato Diseases
            ["Tomato Septoria Leaf Spot"] = new DiseaseRecommendation
            {
                DiseaseName = "Tomato Septoria Leaf Spot",
                Description = "Fungal disease causing small circular spots with dark borders",
                OrganicTreatment = new List<string>
                {
                    "Remove infected lower leaves",
                    "Apply copper-based fungicides",
                    "Spray compost tea weekly"
                },
                ChemicalTreatment = new List<string>
                {
                    "Chlorothalonil 75% WP @ 2 g/liter",
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Copper oxychloride 50% WP @ 2.5 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Mulch around plants",
                    "Avoid overhead watering",
                    "Remove plant debris",
                    "Rotate crops for 3 years"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            ["Tomato Yellow Leaf Curl Virus"] = new DiseaseRecommendation
            {
                DiseaseName = "Tomato Yellow Leaf Curl Virus",
                Description = "Viral disease causing severe leaf curling and yellowing",
                OrganicTreatment = new List<string>
                {
                    "Remove infected plants immediately",
                    "Install reflective silver mulch",
                    "Use neem oil for whitefly control",
                    "Plant marigold as trap crop"
                },
                ChemicalTreatment = new List<string>
                {
                    "Imidacloprid 17.8% SL @ 0.5 ml/liter",
                    "Spiromesifen 22.9% SC @ 1 ml/liter",
                    "Pyriproxyfen 10% EC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use insect-proof nets (40-50 mesh)",
                    "Plant resistant varieties",
                    "Avoid planting near cucurbits",
                    "Monitor whitefly populations weekly"
                },
                Severity = "Critical",
                AffectedCropPart = "Leaves"
            },
            ["Tomato Mosaic Virus"] = new DiseaseRecommendation
            {
                DiseaseName = "Tomato Mosaic Virus",
                Description = "Viral disease causing mottled light and dark green patterns",
                OrganicTreatment = new List<string>
                {
                    "Remove and destroy infected plants",
                    "Disinfect tools with 10% bleach solution",
                    "Wash hands with soap before handling plants"
                },
                ChemicalTreatment = new List<string>
                {
                    "No chemical treatment available",
                    "Focus on prevention and vector control"
                },
                PreventionTips = new List<string>
                {
                    "Use certified disease-free seeds",
                    "Avoid smoking near plants",
                    "Sterilize tools regularly",
                    "Plant resistant varieties like 'Momor'"
                },
                Severity = "High",
                AffectedCropPart = "Leaves and fruits"
            },
            ["Tomato Bacterial Spot"] = new DiseaseRecommendation
            {
                DiseaseName = "Tomato Bacterial Spot",
                Description = "Bacterial disease causing small dark spots on leaves and fruits",
                OrganicTreatment = new List<string>
                {
                    "Spray copper sulfate solution",
                    "Apply Bacillus subtilis bioagent",
                    "Remove infected plant parts"
                },
                ChemicalTreatment = new List<string>
                {
                    "Copper oxychloride 50% WP @ 2.5 g/liter",
                    "Streptocycline 500 ppm",
                    "Copper hydroxide 77% WP @ 2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use pathogen-free seeds",
                    "Avoid overhead irrigation",
                    "Space plants for good air flow",
                    "Rotate with non-host crops"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves, stems, and fruits"
            },
            ["Tomato Target Spot"] = new DiseaseRecommendation
            {
                DiseaseName = "Tomato Target Spot",
                Description = "Fungal disease causing concentric ring patterns on leaves",
                OrganicTreatment = new List<string>
                {
                    "Apply copper-based fungicides",
                    "Remove infected leaves",
                    "Improve air circulation"
                },
                ChemicalTreatment = new List<string>
                {
                    "Azoxystrobin 23% SC @ 1 ml/liter",
                    "Chlorothalonil 75% WP @ 2 g/liter",
                    "Mancozeb 75% WP @ 2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Mulch to prevent soil splash",
                    "Avoid working in wet fields",
                    "Remove lower leaves",
                    "Practice crop rotation"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves and fruits"
            },
            
            // Additional Potato Diseases
            ["Potato Early Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Potato Early Blight",
                Description = "Fungal disease with dark concentric spots on lower leaves",
                OrganicTreatment = new List<string>
                {
                    "Spray Bordeaux mixture (1%)",
                    "Apply compost tea",
                    "Use baking soda solution"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Chlorothalonil 75% WP @ 2 g/liter",
                    "Azoxystrobin 23% SC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Remove volunteer potato plants",
                    "Hill up soil around plants",
                    "Avoid overhead irrigation",
                    "Plant resistant varieties"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            
            // Additional Wheat Diseases
            ["Wheat Leaf Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Wheat Leaf Blight",
                Description = "Fungal disease causing brown lesions on leaves",
                OrganicTreatment = new List<string>
                {
                    "Remove infected crop residues",
                    "Apply Trichoderma viride",
                    "Use sulfur-based fungicides"
                },
                ChemicalTreatment = new List<string>
                {
                    "Propiconazole 25% EC @ 1 ml/liter",
                    "Tebuconazole 50% + Trifloxystrobin 25% @ 0.4 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use resistant varieties",
                    "Maintain proper spacing",
                    "Avoid excess nitrogen",
                    "Crop rotation with legumes"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            ["Wheat Powdery Mildew"] = new DiseaseRecommendation
            {
                DiseaseName = "Wheat Powdery Mildew",
                Description = "Fungal disease with white powdery growth on leaves",
                OrganicTreatment = new List<string>
                {
                    "Spray sulfur solution (3 g/liter)",
                    "Apply neem oil",
                    "Increase potassium fertilization"
                },
                ChemicalTreatment = new List<string>
                {
                    "Sulfur 80% WP @ 3 g/liter",
                    "Tridemorph 80% EC @ 1 ml/liter",
                    "Propiconazole 25% EC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Plant resistant varieties",
                    "Avoid late sowing",
                    "Reduce nitrogen fertilizer",
                    "Ensure good air circulation"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves and stems"
            },
            
            // Cotton Diseases
            ["Cotton Bacterial Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Cotton Bacterial Blight",
                Description = "Bacterial disease causing angular leaf spots",
                OrganicTreatment = new List<string>
                {
                    "Remove and burn infected plants",
                    "Spray copper sulfate solution",
                    "Apply Pseudomonas fluorescens"
                },
                ChemicalTreatment = new List<string>
                {
                    "Streptocycline 500 ppm + Copper oxychloride",
                    "Copper hydroxide 77% WP @ 3 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use certified seeds",
                    "Treat seeds with hot water",
                    "Avoid overhead irrigation",
                    "Practice crop rotation"
                },
                Severity = "High",
                AffectedCropPart = "Leaves and bolls"
            },
            
            // Corn/Maize Diseases
            ["Corn Northern Leaf Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Corn Northern Leaf Blight",
                Description = "Fungal disease causing long cigar-shaped lesions",
                OrganicTreatment = new List<string>
                {
                    "Remove infected leaves",
                    "Apply Trichoderma viride",
                    "Use crop rotation"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Propiconazole 25% EC @ 1 ml/liter",
                    "Azoxystrobin 23% SC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Plant resistant hybrids",
                    "Bury crop residues",
                    "Rotate with non-host crops",
                    "Avoid dense planting"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            ["Corn Common Rust"] = new DiseaseRecommendation
            {
                DiseaseName = "Corn Common Rust",
                Description = "Fungal disease with reddish-brown pustules on leaves",
                OrganicTreatment = new List<string>
                {
                    "Spray sulfur-based fungicides",
                    "Remove infected lower leaves",
                    "Apply potassium-rich fertilizer"
                },
                ChemicalTreatment = new List<string>
                {
                    "Propiconazole 25% EC @ 1 ml/liter",
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Tebuconazole 25.9% EC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Plant rust-resistant hybrids",
                    "Early planting to avoid peak spore season",
                    "Balanced fertilization",
                    "Remove volunteer corn plants"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            ["Corn Gray Leaf Spot"] = new DiseaseRecommendation
            {
                DiseaseName = "Corn Gray Leaf Spot",
                Description = "Fungal disease causing rectangular gray lesions",
                OrganicTreatment = new List<string>
                {
                    "Bury crop residues",
                    "Apply Trichoderma species",
                    "Maintain field hygiene"
                },
                ChemicalTreatment = new List<string>
                {
                    "Azoxystrobin 23% SC @ 1 ml/liter",
                    "Pyraclostrobin 25% EC @ 0.5 ml/liter",
                    "Propiconazole 25% EC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use resistant hybrids",
                    "Rotate crops for 2-3 years",
                    "Bury residues to reduce inoculum",
                    "Avoid continuous corn planting"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            
            // Grape Diseases
            ["Grape Black Rot"] = new DiseaseRecommendation
            {
                DiseaseName = "Grape Black Rot",
                Description = "Fungal disease causing black mummified berries",
                OrganicTreatment = new List<string>
                {
                    "Remove mummified fruits",
                    "Apply copper fungicides",
                    "Prune for better air flow"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Copper oxychloride 50% WP @ 2.5 g/liter",
                    "Myclobutanil 10% WP @ 1 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Remove all mummies from vines",
                    "Apply dormant spray",
                    "Maintain canopy management",
                    "Fungicide from bud break to harvest"
                },
                Severity = "High",
                AffectedCropPart = "Fruits and leaves"
            },
            ["Grape Leaf Blight"] = new DiseaseRecommendation
            {
                DiseaseName = "Grape Leaf Blight",
                Description = "Fungal disease causing brown spots on leaves",
                OrganicTreatment = new List<string>
                {
                    "Remove infected leaves",
                    "Apply Bordeaux mixture",
                    "Improve air circulation"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Copper oxychloride 50% WP @ 2.5 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Prune for airflow",
                    "Remove fallen leaves",
                    "Avoid overhead irrigation",
                    "Apply preventive sprays"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            ["Grape Powdery Mildew"] = new DiseaseRecommendation
            {
                DiseaseName = "Grape Powdery Mildew",
                Description = "Fungal disease with white powdery coating on leaves and fruits",
                OrganicTreatment = new List<string>
                {
                    "Spray sulfur solution (3 g/liter)",
                    "Apply baking soda solution",
                    "Prune affected parts"
                },
                ChemicalTreatment = new List<string>
                {
                    "Sulfur 80% WP @ 3 g/liter",
                    "Myclobutanil 10% WP @ 1 g/liter",
                    "Hexaconazole 5% EC @ 2 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Prune for good air circulation",
                    "Remove infected shoots",
                    "Apply dormant spray",
                    "Regular monitoring"
                },
                Severity = "High",
                AffectedCropPart = "Leaves, fruits, and shoots"
            },
            
            // Apple Diseases
            ["Apple Scab"] = new DiseaseRecommendation
            {
                DiseaseName = "Apple Scab",
                Description = "Fungal disease causing dark olive-green spots",
                OrganicTreatment = new List<string>
                {
                    "Rake and destroy fallen leaves",
                    "Apply lime sulfur spray",
                    "Use resistant varieties"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Captan 50% WP @ 2 g/liter",
                    "Myclobutanil 10% WP @ 1 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Plant resistant varieties",
                    "Remove fallen leaves",
                    "Apply fungicides at green tip stage",
                    "Prune for air circulation"
                },
                Severity = "High",
                AffectedCropPart = "Leaves and fruits"
            },
            ["Apple Black Rot"] = new DiseaseRecommendation
            {
                DiseaseName = "Apple Black Rot",
                Description = "Fungal disease causing fruit rot and cankers",
                OrganicTreatment = new List<string>
                {
                    "Remove mummified fruits",
                    "Prune dead branches",
                    "Apply copper fungicides"
                },
                ChemicalTreatment = new List<string>
                {
                    "Captan 50% WP @ 2 g/liter",
                    "Thiophanate methyl 70% WP @ 1 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Remove all mummies",
                    "Prune cankers 15cm below infection",
                    "Apply dormant spray",
                    "Maintain tree vigor"
                },
                Severity = "High",
                AffectedCropPart = "Fruits and branches"
            },
            ["Apple Cedar Rust"] = new DiseaseRecommendation
            {
                DiseaseName = "Apple Cedar Rust",
                Description = "Fungal disease causing orange spots on leaves",
                OrganicTreatment = new List<string>
                {
                    "Remove nearby juniper trees",
                    "Apply sulfur fungicides",
                    "Pick infected leaves"
                },
                ChemicalTreatment = new List<string>
                {
                    "Myclobutanil 10% WP @ 1 g/liter",
                    "Propiconazole 25% EC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Plant resistant varieties",
                    "Remove alternate hosts (junipers)",
                    "Apply fungicides at pink bud stage",
                    "Space trees properly"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            
            // Pepper Diseases
            ["Pepper Bacterial Spot"] = new DiseaseRecommendation
            {
                DiseaseName = "Pepper Bacterial Spot",
                Description = "Bacterial disease causing raised spots on leaves and fruits",
                OrganicTreatment = new List<string>
                {
                    "Remove infected plants",
                    "Spray copper sulfate solution",
                    "Apply Bacillus subtilis"
                },
                ChemicalTreatment = new List<string>
                {
                    "Copper oxychloride 50% WP @ 2.5 g/liter",
                    "Streptocycline 500 ppm"
                },
                PreventionTips = new List<string>
                {
                    "Use certified seeds",
                    "Avoid overhead irrigation",
                    "Rotate crops for 3 years",
                    "Disinfect tools regularly"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves and fruits"
            },
            ["Pepper Leaf Curl"] = new DiseaseRecommendation
            {
                DiseaseName = "Pepper Leaf Curl",
                Description = "Viral disease causing leaf curling and stunting",
                OrganicTreatment = new List<string>
                {
                    "Remove infected plants",
                    "Control whitefly with neem oil",
                    "Use yellow sticky traps"
                },
                ChemicalTreatment = new List<string>
                {
                    "Imidacloprid 17.8% SL @ 0.5 ml/liter",
                    "Thiamethoxam 25% WG @ 0.2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Use virus-free seedlings",
                    "Install insect-proof nets",
                    "Control whitefly populations",
                    "Remove weed hosts"
                },
                Severity = "High",
                AffectedCropPart = "Leaves"
            },
            
            // Sugarcane Diseases
            ["Sugarcane Red Rot"] = new DiseaseRecommendation
            {
                DiseaseName = "Sugarcane Red Rot",
                Description = "Fungal disease causing reddening of internal tissues",
                OrganicTreatment = new List<string>
                {
                    "Remove and burn infected stalks",
                    "Use healthy seed cane",
                    "Solarize soil before planting"
                },
                ChemicalTreatment = new List<string>
                {
                    "Treat setts with Carbendazim @ 2 g/liter",
                    "Propiconazole 25% EC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Plant resistant varieties",
                    "Use disease-free seed cane",
                    "Avoid waterlogging",
                    "Roguing infected plants"
                },
                Severity = "Critical",
                AffectedCropPart = "Stalks"
            },
            ["Sugarcane Rust"] = new DiseaseRecommendation
            {
                DiseaseName = "Sugarcane Rust",
                Description = "Fungal disease with orange-brown pustules on leaves",
                OrganicTreatment = new List<string>
                {
                    "Remove infected lower leaves",
                    "Apply sulfur-based fungicides",
                    "Increase potassium fertilization"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Propiconazole 25% EC @ 1 ml/liter"
                },
                PreventionTips = new List<string>
                {
                    "Plant rust-resistant varieties",
                    "Balanced fertilization",
                    "Remove crop residues",
                    "Avoid late planting"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            
            // Common/Generic Diseases
            ["Powdery Mildew"] = new DiseaseRecommendation
            {
                DiseaseName = "Powdery Mildew",
                Description = "Fungal disease with white powdery growth on plant surfaces",
                OrganicTreatment = new List<string>
                {
                    "Spray sulfur solution (3 g/liter)",
                    "Apply baking soda + dish soap solution",
                    "Spray neem oil weekly"
                },
                ChemicalTreatment = new List<string>
                {
                    "Sulfur 80% WP @ 3 g/liter",
                    "Hexaconazole 5% EC @ 2 ml/liter",
                    "Myclobutanil 10% WP @ 1 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Improve air circulation",
                    "Avoid overhead watering",
                    "Remove infected plant parts",
                    "Plant resistant varieties"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves, stems, and flowers"
            },
            ["Downy Mildew"] = new DiseaseRecommendation
            {
                DiseaseName = "Downy Mildew",
                Description = "Fungal disease with fuzzy gray-purple growth on leaf undersides",
                OrganicTreatment = new List<string>
                {
                    "Apply copper-based fungicides",
                    "Remove infected leaves",
                    "Improve drainage"
                },
                ChemicalTreatment = new List<string>
                {
                    "Metalaxyl 8% + Mancozeb 64% @ 2 g/liter",
                    "Copper oxychloride 50% WP @ 2.5 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Space plants for air flow",
                    "Water at soil level only",
                    "Remove plant debris",
                    "Rotate crops"
                },
                Severity = "Medium",
                AffectedCropPart = "Leaves"
            },
            ["Anthracnose"] = new DiseaseRecommendation
            {
                DiseaseName = "Anthracnose",
                Description = "Fungal disease causing dark sunken lesions on fruits and leaves",
                OrganicTreatment = new List<string>
                {
                    "Remove infected plant parts",
                    "Apply copper fungicides",
                    "Use Trichoderma viride"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Carbendazim 50% WP @ 1 g/liter",
                    "Chlorothalonil 75% WP @ 2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Avoid wounding fruits",
                    "Improve air circulation",
                    "Remove crop debris",
                    "Practice crop rotation"
                },
                Severity = "Medium",
                AffectedCropPart = "Fruits and leaves"
            },
            ["Leaf Spot"] = new DiseaseRecommendation
            {
                DiseaseName = "Leaf Spot",
                Description = "Fungal/bacterial disease causing spots on leaves",
                OrganicTreatment = new List<string>
                {
                    "Remove spotted leaves",
                    "Apply neem oil spray",
                    "Use copper-based fungicides"
                },
                ChemicalTreatment = new List<string>
                {
                    "Mancozeb 75% WP @ 2 g/liter",
                    "Copper oxychloride 50% WP @ 2.5 g/liter",
                    "Chlorothalonil 75% WP @ 2 g/liter"
                },
                PreventionTips = new List<string>
                {
                    "Avoid overhead watering",
                    "Space plants properly",
                    "Remove plant debris",
                    "Rotate crops annually"
                },
                Severity = "Low to Medium",
                AffectedCropPart = "Leaves"
            },
            ["Root Rot"] = new DiseaseRecommendation
            {
                DiseaseName = "Root Rot",
                Description = "Fungal disease causing browning and decay of roots",
                OrganicTreatment = new List<string>
                {
                    "Improve soil drainage",
                    "Apply Trichoderma harzianum to soil",
                    "Reduce watering frequency",
                    "Add organic matter to soil"
                },
                ChemicalTreatment = new List<string>
                {
                    "Metalaxyl 8% + Mancozeb 64% (soil drench)",
                    "Carbendazim 50% WP @ 2 g/liter (soil drench)",
                    "Copper oxychloride (soil application)"
                },
                PreventionTips = new List<string>
                {
                    "Ensure proper drainage",
                    "Avoid overwatering",
                    "Use raised beds",
                    "Treat seeds with fungicides",
                    "Rotate with non-host crops"
                },
                Severity = "High",
                AffectedCropPart = "Roots and lower stem"
            },
            
            ["Healthy Plant"] = new DiseaseRecommendation
            {
                DiseaseName = "Healthy Plant",
                Description = "No disease detected - plant appears healthy",
                OrganicTreatment = new List<string>
                {
                    "Continue regular monitoring",
                    "Maintain good agricultural practices",
                    "Apply organic compost monthly"
                },
                ChemicalTreatment = new List<string>
                {
                    "No treatment needed",
                    "Preventive sprays if weather is conducive to disease"
                },
                PreventionTips = new List<string>
                {
                    "Regular crop monitoring (weekly)",
                    "Proper irrigation and nutrition",
                    "Crop rotation every season",
                    "Maintain field hygiene"
                },
                Severity = "None",
                AffectedCropPart = "None"
            }
        };
    }
}
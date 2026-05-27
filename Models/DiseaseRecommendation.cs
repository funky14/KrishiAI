namespace KrishiAI.App.Models;

public class DiseaseRecommendation
{
    public string DiseaseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> OrganicTreatment { get; set; } = new();
    public List<string> ChemicalTreatment { get; set; } = new();
    public List<string> PreventionTips { get; set; } = new();
    public string Severity { get; set; } = string.Empty;
    public string AffectedCropPart { get; set; } = string.Empty;
}

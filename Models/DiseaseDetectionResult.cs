using SQLite;

namespace KrishiAI.App.Models;

[Table("DiseaseHistory")]
public class DiseaseDetectionResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    public string DiseaseName { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public string Severity { get; set; } = string.Empty;

    public DateTime DetectedDate { get; set; } = DateTime.Now;

    public string Description { get; set; } = string.Empty;

    [Ignore]
    public List<string> TreatmentRecommendations { get; set; } = new();

    public string AffectedCropPart { get; set; } = string.Empty;
}

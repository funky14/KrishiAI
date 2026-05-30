namespace KrishiAI.App.Models;

public class FarmerTip
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🌱";
    public TipCategory Category { get; set; }
}

public enum TipCategory
{
    CropRotation,
    Irrigation,
    SoilHealth,
    PestControl,
    Weather,
    General
}

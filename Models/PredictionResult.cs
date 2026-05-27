namespace KrishiAI.App.Models;

public class PredictionResult
{
    public string ClassName { get; set; } = string.Empty;
    public float Probability { get; set; }
}

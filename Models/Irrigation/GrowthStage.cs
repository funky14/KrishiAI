namespace KrishiAI.App.Models.Irrigation;

/// <summary>
/// Growth stages of crops
/// </summary>
public enum GrowthStage
{
    Seedling,
    Vegetative,
    Flowering,
    Fruiting,
    Harvest
}

/// <summary>
/// Extension methods for GrowthStage
/// </summary>
public static class GrowthStageExtensions
{
    public static string GetDisplayName(this GrowthStage stage)
    {
        return stage switch
        {
            GrowthStage.Seedling => "Seedling",
            GrowthStage.Vegetative => "Vegetative",
            GrowthStage.Flowering => "Flowering",
            GrowthStage.Fruiting => "Fruiting",
            GrowthStage.Harvest => "Harvest",
            _ => stage.ToString()
        };
    }

    public static string GetDescription(this GrowthStage stage)
    {
        return stage switch
        {
            GrowthStage.Seedling => "Initial growth phase, high water sensitivity",
            GrowthStage.Vegetative => "Active growth phase, moderate water needs",
            GrowthStage.Flowering => "Critical phase, consistent water required",
            GrowthStage.Fruiting => "Fruit development, high water demand",
            GrowthStage.Harvest => "Maturity phase, reduced water needs",
            _ => string.Empty
        };
    }

    public static double GetWaterMultiplier(this GrowthStage stage)
    {
        return stage switch
        {
            GrowthStage.Seedling => 0.7,
            GrowthStage.Vegetative => 1.0,
            GrowthStage.Flowering => 1.2,
            GrowthStage.Fruiting => 1.3,
            GrowthStage.Harvest => 0.6,
            _ => 1.0
        };
    }

    public static string GetEmoji(this GrowthStage stage)
    {
        return stage switch
        {
            GrowthStage.Seedling => "🌱",
            GrowthStage.Vegetative => "🌿",
            GrowthStage.Flowering => "🌸",
            GrowthStage.Fruiting => "🍇",
            GrowthStage.Harvest => "🌾",
            _ => "🌱"
        };
    }
}

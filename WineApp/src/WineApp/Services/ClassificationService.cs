using WineApp.Models;

namespace WineApp.Services;

public class ClassificationService : IClassificationService
{
    public string ClassifyWine(
        decimal totalScore,
        decimal avgAppearance,
        decimal avgNose,
        decimal avgTaste,
        bool isDefective,
        bool meetsGateValues,
        Event eventConfig)
    {
        // Rule 1: If defective (any dimension = 0 or taste <= 1), not approved
        if (isDefective)
            return "IkkeGodkjent";

        // Rule 2: Must meet gate values for Acceptable or higher
        if (!meetsGateValues)
            return "IkkeGodkjent";

        // Rule 3: Apply medal thresholds
        var goldThreshold = eventConfig.UseAdjustedThresholds 
            ? eventConfig.AdjustedGoldThreshold 
            : eventConfig.GoldThreshold;
        
        var silverThreshold = eventConfig.UseAdjustedThresholds 
            ? eventConfig.AdjustedSilverThreshold 
            : eventConfig.SilverThreshold;
        
        var bronzeThreshold = eventConfig.UseAdjustedThresholds 
            ? eventConfig.AdjustedBronzeThreshold 
            : eventConfig.BronzeThreshold;
        
        var specialMeritThreshold = eventConfig.UseAdjustedThresholds 
            ? eventConfig.AdjustedSpecialMeritThreshold 
            : eventConfig.SpecialMeritThreshold;

        if (totalScore >= goldThreshold)
            return "Gull";
        
        if (totalScore >= silverThreshold)
            return "Sølv";
        
        if (totalScore >= bronzeThreshold)
            return "Bronse";
        
        if (totalScore >= specialMeritThreshold)
            return "Særlig";
        
        return "Akseptabel";
    }

    public bool ShouldUseAdjustedThresholds(List<WineResult> results)
    {
        // If no wines achieved Gold classification, suggest using adjusted thresholds
        return !results.Any(r => r.Classification == "Gull");
    }

    public decimal GetThreshold(string classification, Event eventConfig)
    {
        var useAdjusted = eventConfig.UseAdjustedThresholds;

        return classification switch
        {
            "Gull" => useAdjusted ? eventConfig.AdjustedGoldThreshold : eventConfig.GoldThreshold,
            "Sølv" => useAdjusted ? eventConfig.AdjustedSilverThreshold : eventConfig.SilverThreshold,
            "Bronse" => useAdjusted ? eventConfig.AdjustedBronzeThreshold : eventConfig.BronzeThreshold,
            "Særlig" => useAdjusted ? eventConfig.AdjustedSpecialMeritThreshold : eventConfig.SpecialMeritThreshold,
            _ => 0
        };
    }
}

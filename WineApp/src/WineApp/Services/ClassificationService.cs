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
            return Classification.NotApproved;

        // Rule 2: Must meet gate values for Acceptable or higher
        if (!meetsGateValues)
            return Classification.NotApproved;

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
            return Classification.Gold;
        
        if (totalScore >= silverThreshold)
            return Classification.Silver;
        
        if (totalScore >= bronzeThreshold)
            return Classification.Bronze;
        
        if (totalScore >= specialMeritThreshold)
            return Classification.SpecialMerit;
        
        return Classification.Acceptable;
    }

    public bool ShouldUseAdjustedThresholds(List<WineResult> results)
    {
        // If no wines achieved Gold classification, suggest using adjusted thresholds
        return !results.Any(r => r.Classification == Classification.Gold);
    }

    public decimal GetThreshold(string classification, Event eventConfig)
    {
        var useAdjusted = eventConfig.UseAdjustedThresholds;

        return classification switch
        {
            Classification.Gold => useAdjusted ? eventConfig.AdjustedGoldThreshold : eventConfig.GoldThreshold,
            Classification.Silver => useAdjusted ? eventConfig.AdjustedSilverThreshold : eventConfig.SilverThreshold,
            Classification.Bronze => useAdjusted ? eventConfig.AdjustedBronzeThreshold : eventConfig.BronzeThreshold,
            Classification.SpecialMerit => useAdjusted ? eventConfig.AdjustedSpecialMeritThreshold : eventConfig.SpecialMeritThreshold,
            _ => 0
        };
    }
}

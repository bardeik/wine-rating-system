using WineApp.Models;

namespace WineApp.Services;

public interface IWineValidationService
{
    /// <summary>
    /// Validates that grape blend percentages sum to 100%
    /// </summary>
    (bool isValid, string errorMessage) ValidateGrapeBlend(Dictionary<string, decimal> grapeBlend);
    
    /// <summary>
    /// Validates wine eligibility for Vinbonde status
    /// </summary>
    (bool isValid, string errorMessage) ValidateVinbondeEligibility(Wine wine);
    
    /// <summary>
    /// Validates wine registration completeness
    /// </summary>
    (bool isValid, List<string> errors) ValidateWineRegistration(Wine wine);
    
    /// <summary>
    /// Validates that a wine can be rated (is paid and has wine number)
    /// </summary>
    (bool isValid, string errorMessage) ValidateWineForRating(Wine wine);
}

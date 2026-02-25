using WineApp.Models;

namespace WineApp.Services;

public class WineValidationService : IWineValidationService
{
    public (bool isValid, string errorMessage) ValidateGrapeBlend(Dictionary<string, decimal> grapeBlend)
    {
        if (grapeBlend == null || !grapeBlend.Any())
            return (false, "Drueblanding er påkrevd");

        var total = grapeBlend.Values.Sum();
        var tolerance = 0.01m; // Allow 0.01% rounding tolerance

        if (Math.Abs(total - 100m) > tolerance)
            return (false, $"Drueblanding må summere til 100%. Nåværende sum: {total:F2}%");

        // Check for negative values
        if (grapeBlend.Values.Any(v => v < 0))
            return (false, "Drueprosenter kan ikke være negative");

        // Check for zero-value entries
        if (grapeBlend.Values.Any(v => v == 0))
            return (false, "Fjern druesorter med 0% fra blandingen");

        return (true, string.Empty);
    }

    public (bool isValid, string errorMessage) ValidateVinbondeEligibility(Wine wine)
    {
        // Vinbonde requires: Group A1 (Norwegian outdoor approved grapes) + 100+ vinstokker
        if (!wine.IsVinbonde)
            return (true, string.Empty); // Not claiming Vinbonde status

        if (wine.Group != WineGroup.A1)
            return (false, "Vinbonde-status krever Gruppe A1 (godkjente sorter friland, Norge)");

        // Note: The 100 vinstokker verification is declared by producer, not validated here
        return (true, string.Empty);
    }

    public (bool isValid, List<string> errors) ValidateWineRegistration(Wine wine)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(wine.Name))
            errors.Add("Vinnavn er påkrevd");

        if (string.IsNullOrWhiteSpace(wine.RatingName))
            errors.Add("Vurderingsnavn er påkrevd");

        if (wine.Vintage < 1900 || wine.Vintage > DateTime.Now.Year + 1)
            errors.Add($"Årgang må være mellom 1900 og {DateTime.Now.Year + 1}");

        if (wine.AlcoholPercentage < 0 || wine.AlcoholPercentage > 100)
            errors.Add("Alkoholprosent må være mellom 0 og 100");

        if (string.IsNullOrWhiteSpace(wine.Country))
            errors.Add("Land er påkrevd");

        if (string.IsNullOrWhiteSpace(wine.WineProducerId))
            errors.Add("Vinprodusent er påkrevd");

        // Validate grape blend
        var (blendValid, blendError) = ValidateGrapeBlend(wine.GrapeBlend);
        if (!blendValid)
            errors.Add(blendError);

        // Validate Vinbonde eligibility
        var (vinbondeValid, vinbondeError) = ValidateVinbondeEligibility(wine);
        if (!vinbondeValid)
            errors.Add(vinbondeError);

        // Validate Group A2 must have Country != Norge
        if (wine.Group == WineGroup.A2 && wine.Country.Equals("Norge", StringComparison.OrdinalIgnoreCase))
            errors.Add("Gruppe A2 (Nordiske gjesteviner) kan ikke være fra Norge. Bruk A1 for norske viner.");

        return (!errors.Any(), errors);
    }

    public (bool isValid, string errorMessage) ValidateWineForRating(Wine wine)
    {
        if (!wine.IsPaid)
            return (false, "Vinen må være betalt før den kan bedømmes");

        if (!wine.WineNumber.HasValue)
            return (false, "Vinen må ha tildelt vinnummer før den kan bedømmes");

        return (true, string.Empty);
    }
}

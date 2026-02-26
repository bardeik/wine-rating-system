namespace WineApp.Models;

/// <summary>
/// String constants for wine classifications, preventing magic strings across the codebase.
/// </summary>
public static class Classification
{
    public const string Gold = "Gull";
    public const string Silver = "Sølv";
    public const string Bronze = "Bronse";
    public const string SpecialMerit = "Særlig";
    public const string Acceptable = "Akseptabel";
    public const string NotApproved = "IkkeGodkjent";

    /// <summary>
    /// Classifications that count as a medal (Bronze or higher).
    /// </summary>
    public static readonly IReadOnlyList<string> MedalClassifications = [Gold, Silver, Bronze];
}

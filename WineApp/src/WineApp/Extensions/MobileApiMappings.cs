using System.Text;
using WineApp.Models;

namespace WineApp.Extensions;

/// <summary>
/// Pure mapping helpers and URL-safe Base64 utilities shared by the mobile API layer.
/// Kept in a separate internal class so unit tests can cover them directly.
/// </summary>
internal static class MobileApiMappings
{
    // -------------------------------------------------------------------------
    // Model → response DTO
    // -------------------------------------------------------------------------

    internal static WineResponse MapWine(Wine wine) => new(
        WineId: wine.WineId,
        WineNumber: wine.WineNumber,
        Name: wine.Name,
        RatingName: wine.RatingName,
        Vintage: wine.Vintage,
        AlcoholPercentage: wine.AlcoholPercentage,
        Country: wine.Country,
        Group: wine.Group.ToString(),
        Class: wine.Class.ToString(),
        Category: wine.Category.ToString(),
        WineProducerId: wine.WineProducerId,
        EventId: wine.EventId,
        IsPaid: wine.IsPaid);

    internal static RatingResponse MapRating(WineRating r) => new(
        WineRatingId: r.WineRatingId,
        Appearance: r.Appearance,
        Nose: r.Nose,
        Taste: r.Taste,
        Comment: r.Comment,
        JudgeId: r.JudgeId,
        WineId: r.WineId,
        Total: r.Total);

    /// <summary>
    /// Maps an <see cref="Event"/> to its response DTO.
    /// When <see cref="Event.UseAdjustedThresholds"/> is <c>true</c> the adjusted
    /// threshold set is used; otherwise the standard set is used.
    /// </summary>
    internal static EventResponse MapEvent(Event e) => new(
        EventId: e.EventId,
        Name: e.Name,
        Year: e.Year,
        GoldThreshold: e.UseAdjustedThresholds ? e.AdjustedGoldThreshold : e.GoldThreshold,
        SilverThreshold: e.UseAdjustedThresholds ? e.AdjustedSilverThreshold : e.SilverThreshold,
        BronzeThreshold: e.UseAdjustedThresholds ? e.AdjustedBronzeThreshold : e.BronzeThreshold,
        SpecialMeritThreshold: e.UseAdjustedThresholds ? e.AdjustedSpecialMeritThreshold : e.SpecialMeritThreshold,
        AppearanceGateValue: e.AppearanceGateValue,
        NoseGateValue: e.NoseGateValue,
        TasteGateValue: e.TasteGateValue,
        IsActive: e.IsActive);

    // -------------------------------------------------------------------------
    // URL-safe Base64 helpers (RFC 4648 §5, no padding)
    // -------------------------------------------------------------------------

    internal static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    internal static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}

// -------------------------------------------------------------------------
// Response record types (server-side only; match WineApp.Shared.Dtos shapes)
// -------------------------------------------------------------------------

internal sealed record WineResponse(
    string WineId,
    int? WineNumber,
    string Name,
    string RatingName,
    int Vintage,
    decimal AlcoholPercentage,
    string Country,
    string Group,
    string Class,
    string Category,
    string WineProducerId,
    string? EventId,
    bool IsPaid);

internal sealed record RatingResponse(
    string WineRatingId,
    decimal Appearance,
    decimal Nose,
    decimal Taste,
    string Comment,
    string JudgeId,
    string WineId,
    decimal Total);

internal sealed record EventResponse(
    string EventId,
    string Name,
    int Year,
    decimal GoldThreshold,
    decimal SilverThreshold,
    decimal BronzeThreshold,
    decimal SpecialMeritThreshold,
    decimal AppearanceGateValue,
    decimal NoseGateValue,
    decimal TasteGateValue,
    bool IsActive);

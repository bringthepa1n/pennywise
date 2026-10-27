using PennyWise.Models;

namespace PennyWise.Services;

/// <summary>
/// Maps a category to its illustration in Resources/Images. MAUI lowercases and
/// sanitises resource names, so the identifiers used here are the same as the
/// file names without the extension.
/// </summary>
public static class CategoryArt
{
    public const string Food = "fork_and_knife_with_plate_3d";
    public const string Housing = "house_with_garden_3d";
    public const string Transport = "automobile_3d";
    public const string Fun = "admission_tickets_3d";
    public const string Health = "red_heart_3d";
    public const string Income = "money_bag_3d";
    public const string Other = "red_question_mark_3d";

    public static string For(Category category) => category switch
    {
        Category.Food => Food,
        Category.Housing => Housing,
        Category.Transport => Transport,
        Category.Fun => Fun,
        Category.Health => Health,
        Category.Income => Income,
        _ => Other,
    };

    /// <summary>Artwork for a Czech category label (used by the filter chips).</summary>
    public static string ForLabel(string label)
    {
        var category = Enum.GetValues<Category>()
            .FirstOrDefault(c => c.ToCzechLabel() == label);

        return For(category);
    }
}

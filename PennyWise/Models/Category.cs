namespace PennyWise.Models;

public enum Category
{
    Food,
    Housing,
    Transport,
    Fun,
    Health,
    Income,
    Other,
}

public static class CategoryExtensions
{
    public static string ToCzechLabel(this Category category) => category switch
    {
        Category.Food => "Jídlo",
        Category.Housing => "Bydlení",
        Category.Transport => "Doprava",
        Category.Fun => "Zábava",
        Category.Health => "Zdraví",
        Category.Income => "Příjem",
        _ => "Ostatní",
    };
}

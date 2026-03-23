namespace Spendly.Helpers;

public static class CategoryIconProvider
{
    public static string GetIcon(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return Default();

        var key = categoryName.ToLower().Replace(" ", "-");

        return $"pack://application:,,,/Resources/Icons/Categories/{key}-category.svg";
    }

    private static string Default()
        => "pack://application:,,,/Resources/Icons/categories/other-category.svg";
}
using UnityEngine;

/// <summary>
/// Helper class for consistent currency display throughout the game
/// Changes from $ (dollars) to Coins
/// </summary>
public static class CurrencyFormatter
{
    /// <summary>
    /// Format an amount as coins with icon
    /// </summary>
    public static string FormatCoins(int amount)
    {
        return $"{amount:N0} Coins"; // e.g., "1,500 Coins"
    }

    /// <summary>
    /// Format coins with custom suffix
    /// </summary>
    public static string FormatCoinsCustom(int amount, string suffix = "Coins")
    {
        return $"{amount:N0} {suffix}";
    }

    /// <summary>
    /// Format coins compact (no commas, just number)
    /// </summary>
    public static string FormatCoinsCompact(int amount)
    {
        return $"{amount} Coins";
    }

    /// <summary>
    /// Format coins with icon (if you have a coin sprite)
    /// </summary>
    public static string FormatCoinsWithIcon(int amount)
    {
        return $"⚬ {amount:N0}"; // Using a circle symbol
    }
}
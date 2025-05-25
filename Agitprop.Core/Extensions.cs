namespace Agitprop.Core.Extensions;

/// <summary>
/// Provides extension methods for common operations.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Truncates the input string to the specified length.
    /// </summary>
    /// <param name="stringIn">The input string to truncate.</param>
    /// <param name="length">The maximum length of the truncated string.</param>
    /// <returns>The truncated string, or the original string if the length exceeds the string's length.</returns>
    public static string Truncate(this string stringIn, int length)
    {
        try
        {
            return stringIn[..length].Trim();
        }
        catch (ArgumentOutOfRangeException)
        {
            return stringIn;
        }
    }
}

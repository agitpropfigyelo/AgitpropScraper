using System;

namespace Agitprop.Core.Extensions;

public static class Extensions
{

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

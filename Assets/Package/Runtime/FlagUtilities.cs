using System.Collections.Generic;

public static class FlagUtilities
{
    /// <summary>
    /// Checks if given value contains given bits
    /// </summary>
    /// <param name="value">The value being checked</param>
    /// <param name="bytes">the bytes it must contain</param>
    /// <returns>true if value contains all bytes, otherwise false</returns>
    public static bool ContainsBytes(int value, int bytes) => (value ^ bytes) == (value - bytes);

    /// <summary>
    /// Checks if given value contains any of the given bits
    /// </summary>
    /// <param name="value">The value being checked</param>
    /// <param name="bytes">the bytes it must contain</param>
    /// <returns>true if value contains any of the bytes, otherwise false</returns>
    public static bool ContainsAnyBits(int value, int bytes)
    {
        int[] bits = SeparateBits(bytes);
        for (int i = 0; i < bits.Length; i++)
            if (ContainsBytes(value, bits[i]))
                return true;
        return false;
    }

    /// <summary>
    /// Separate a value into its individual bits
    /// </summary>
    /// <param name="value">Value being divided</param>
    /// <returns>An array with each bit</returns>
    public static int[] SeparateBits(int value)
    {
        List<int> bits = new List<int>();
        int comparingValue = 1;
        for (int i = 0; i < 32; i++)
        {
            if ((comparingValue & value) != 0)
                bits.Add(comparingValue);
            comparingValue <<= 1;
        }

        return bits.ToArray();
    }
}
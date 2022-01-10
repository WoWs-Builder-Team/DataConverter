using System;

namespace DataConverter
{
    public static class Extensions
    {
        public static string CapitalizeFirstChar(this string input) =>
            input switch
            {
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpperInvariant(), input.AsSpan(1)),
            };
    }
}

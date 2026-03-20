using System;
using System.Collections.Generic;
using System.Text;

namespace GlavnayaKniga.Infrastructure.Data
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsUpper(c) && i > 0)
                {
                    builder.Append('_');
                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(char.ToLowerInvariant(c));
                }
            }
            return builder.ToString();
        }
    }
}

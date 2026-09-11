using System.Globalization;
using System.Text;

namespace ConnectFour.Evaluation
{
    // Write csv
    public static class CsvHelpers
    {
        // Join a row's values into a comma-separated line
        public static string Row(params object?[] fields)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }
                sb.Append(Field(fields[i]));
            }
            return sb.ToString();
        }

        // Format a single column value - null becomes empty, numbers use InvariantCulture, and
        // everything else (strings, bools, enums) uses its normal ToString().
        private static string Field(object? value)
        {
            if (value == null)
            {
                return "";
            }

            // Numbers implement IFormattable; force InvariantCulture
            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            string? result = value.ToString();

            if (result == null)
            {
                return "";
            }
            else
            {
                return result;
            }
        }
    }
}

using System.Globalization;
using System.Text;

namespace Ipfs;

/// <summary>
///   Parsing and stringifying of a <see cref="TimeSpan"/> according to IPFS.
/// </summary>
/// <remarks>
///   From the <see href="https://godoc.org/time#ParseDuration">go spec</see>.
///   <para>
///   A duration string is a possibly signed sequence of decimal numbers,
///   each with optional fraction and a unit suffix, such as "300ms", "-1.5h" or "2h45m".
///   Valid time units are "ns", "us" (or "µs"), "ms", "s", "m", "h".
///   </para>
/// </remarks>
public static class Duration
{
    private const double TicksPerNanosecond = TimeSpan.TicksPerMillisecond * 0.000001;
    private const double TicksPerMicrosecond = TimeSpan.TicksPerMillisecond * 0.001;

    /// <summary>
    ///   Converts the string representation of an IPFS duration
    ///   into its <see cref="TimeSpan"/> equivalent.
    /// </summary>
    /// <param name="s">
    ///   A string that contains the duration to convert.
    /// </param>
    /// <returns>
    ///   A <see cref="TimeSpan"/> that is equivalent to <paramref name="s"/>.
    /// </returns>
    /// <exception cref="FormatException">
    ///   <paramref name="s"/> is not a valid IPFS duration.
    /// </exception>
    /// <remarks>
    ///   An empty string or "n/a" or "unknown" returns <see cref="TimeSpan.Zero"/>.
    ///   <para>
    ///   A duration string is a possibly signed sequence of decimal numbers,
    ///   each with optional fraction and a unit suffix, such as "300ms", "-1.5h" or "2h45m".
    ///   Valid time units are "ns", "us" (or "µs"), "ms", "s", "m", "h".
    ///   </para>
    /// </remarks>
    public static TimeSpan Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s) || s == "n/a" || s == "unknown")
        {
            return TimeSpan.Zero;
        }

        TimeSpan result = TimeSpan.Zero;
        bool negative = false;
        using (var sr = new StringReader(s))
        {
            if (sr.Peek() == '-')
            {
                negative = true;
                _ = sr.Read();
            }
            while (sr.Peek() != -1)
            {
                result += ParseComponent(sr);
            }
        }

        return negative ? TimeSpan.FromTicks(-result.Ticks) : result;
    }

    private static TimeSpan ParseComponent(StringReader reader)
    {
        double value = ParseNumber(reader);
        string unit = ParseUnit(reader);

        return unit switch
        {
            "h" => TimeSpan.FromHours(value),
            "m" => TimeSpan.FromMinutes(value),
            "s" => TimeSpan.FromSeconds(value),
            "ms" => TimeSpan.FromMilliseconds(value),
            "us" or "µs" => TimeSpan.FromTicks((long)(value * TicksPerMicrosecond)),
            "ns" => TimeSpan.FromTicks((long)(value * TicksPerNanosecond)),
            "" => throw new FormatException("Missing IPFS duration unit."),
            _ => throw new FormatException($"Unknown IPFS duration unit '{unit}'."),
        };
    }

    private static double ParseNumber(StringReader reader)
    {
        var s = new StringBuilder();
        while (true)
        {
            char c = (char)reader.Peek();
            if (char.IsDigit(c) || c == '.')
            {
                _ = s.Append(c);
                _ = reader.Read();
                continue;
            }
            return double.Parse(s.ToString(), CultureInfo.InvariantCulture);
        }
    }

    private static string ParseUnit(StringReader reader)
    {
        var s = new StringBuilder();
        while (true)
        {
            char c = (char)reader.Peek();
            if (char.IsDigit(c) || c == '.' || c == (char)0xFFFF)
            {
                break;
            }

            _ = s.Append(c);
            _ = reader.Read();
        }

        return s.ToString();
    }

    /// <summary>
    ///   Converts the <see cref="TimeSpan"/> to the equivalent string representation of an
    ///   IPFS duration.
    /// </summary>
    /// <param name="duration">
    ///   The <see cref="TimeSpan"/> to convert.
    /// </param>
    /// <param name="zeroValue">
    ///   The string representation, when the <paramref name="duration"/>
    ///   is equal to <see cref="TimeSpan.Zero"/>/
    /// </param>
    /// <returns>
    ///   The IPFS duration string representation.
    /// </returns>
    public static string Stringify(TimeSpan duration, string zeroValue = "0s")
    {
        if (duration.Ticks == 0)
        {
            return zeroValue;
        }

        var s = new StringBuilder();
        if (duration.Ticks < 0)
        {
            _ = s.Append('-');
            duration = TimeSpan.FromTicks(-duration.Ticks);
        }

        Stringify(Math.Floor(duration.TotalHours), "h", s);
        Stringify(duration.Minutes, "m", s);
        Stringify(duration.Seconds, "s", s);
        Stringify(duration.Milliseconds, "ms", s);
        Stringify((long)(duration.Ticks / TicksPerMicrosecond) % 1000, "us", s);
        Stringify((long)(duration.Ticks / TicksPerNanosecond) % 1000, "ns", s);

        return s.ToString();
    }

    private static void Stringify(double value, string unit, StringBuilder sb)
    {
        if (value == 0)
        {
            return;
        }

        _ = sb.Append(value.ToString(CultureInfo.InvariantCulture));
        _ = sb.Append(unit);
    }
}

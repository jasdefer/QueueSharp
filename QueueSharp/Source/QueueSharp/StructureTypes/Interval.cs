namespace QueueSharp.StructureTypes;

/// <summary>
/// Represents a range between two integer values, with a defined start and end. 
/// Provides methods to check if a value lies within the range, if two intervals overlap, 
/// and overrides for equality comparisons and string representation.
/// 
/// Use this struct to define intervals or ranges in time, position, or any other dimension 
/// where an ordered start and end are meaningful.
/// </summary>
public readonly struct Interval : IEquatable<Interval>
{
    public int Start { get; }
    public int End { get; }

    // Constructor to initialize start and end of the interval
    public Interval(int start, int end)
    {
        if (start > end)
        {
            throw new ArgumentException("Start cannot be greater than End.");
        }
        Start = start;
        End = end;
    }

    /// <summary>
    /// Check if the given value is within the interval.
    /// </summary>
    /// <returns>Returns true, if the value is between the start and end (or equal to start or end).</returns>
    public bool IsInRange(int value)
    {
        return value >= Start && value <= End;
    }

    /// <summary>
    /// Determines if this interval overlaps with another interval. 
    /// Overlap occurs if the intervals share any portion of their ranges, 
    /// excluding cases where one interval ends exactly when the other begins.
    /// </summary>
    /// <param name="other">The interval to compare for overlap.</param>
    /// <returns>True if the intervals overlap, false if they are adjacent or do not overlap.</returns>
    public bool Overlaps(Interval other)
    {
        return Start < other.End && End > other.Start;
    }

    public override string ToString()
    {
        return $"[{Start}, {End}]";
    }

    public override bool Equals(object? obj)
    {
        return obj is Interval interval && Equals(interval);
    }

    public bool Equals(Interval other)
    {
        return Start == other.Start &&
               End == other.End;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public static bool operator ==(Interval left, Interval right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Interval left, Interval right)
    {
        return !(left == right);
    }
}

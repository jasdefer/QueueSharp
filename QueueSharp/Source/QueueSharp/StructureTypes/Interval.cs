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
    /// <summary>
    /// The start of the interval. 
    /// </summary>
    public int Start { get; }
    /// <summary>
    /// The end of the interval.
    /// </summary>
    public int End { get; }

    /// <summary>
    /// Create a new interval instance. The start value cannot be bigger than the end.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <exception cref="ArgumentException">If start > end.</exception>
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

    /// <summary>
    /// Converts this interval to a string in the format [Start, End]
    /// </summary>
    public override string ToString()
    {
        return $"[{Start}, {End}]";
    }

    /// <summary>
    /// Two intervals are equal if their start and end properties are equal.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        return obj is Interval interval && Equals(interval);
    }

    /// <summary>
    /// Two intervals are equal if their start and end properties are equal.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Interval other)
    {
        return Start == other.Start &&
               End == other.End;
    }

    /// <summary>
    /// Computes the hash code based on the <see cref="Start"/> and <see cref="End"/> properties.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    /// <summary>
    /// Two intervals are equal if their start and end properties are equal.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Interval left, Interval right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Two intervals are equal if their start and end properties are equal.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Interval left, Interval right)
    {
        return !(left == right);
    }
}

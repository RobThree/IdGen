using System;

namespace IdGen;

/// <summary>
/// Represents the options an <see cref="IdGenerator"/> can be configured with.
/// </summary>
public class IdGeneratorOptions
{
    /// <summary>
    /// Returns the default epoch.
    /// </summary>
    public static readonly DateTime DefaultEpoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly IdStructure _defaultidstructure = IdStructure.Default;
    private static readonly ITimeSource _defaulttimesource = new DefaultTimeSource(DefaultEpoch);
    private static readonly SequenceOverflowStrategy _defaultsequenceoverflowstrategy = SequenceOverflowStrategy.Throw;

    /// <summary>
    /// Returns a default instance of <see cref="IdGeneratorOptions"/>.
    /// </summary>
    public static readonly IdGeneratorOptions Default = new()
    {
        IdStructure = _defaultidstructure,
        TimeSource = _defaulttimesource,
        SequenceOverflowStrategy = _defaultsequenceoverflowstrategy
    };

    /// <summary>
    /// Gets the <see cref="IdStructure"/> of the generated ID's
    /// </summary>
    public IdStructure IdStructure { get; init; } = _defaultidstructure;

    /// <summary>
    /// Gets the <see cref="ITimeSource"/> to use when generating ID's.
    /// </summary>
    public ITimeSource TimeSource { get; init; } = _defaulttimesource;

    /// <summary>
    /// Gets the <see cref="SequenceOverflowStrategy"/> to use when generating ID's.
    /// </summary>
    public SequenceOverflowStrategy SequenceOverflowStrategy { get; init; } = _defaultsequenceoverflowstrategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdGeneratorOptions"/> class.
    /// </summary>
    /// <param name="idStructure">The <see cref="IdStructure"/> for ID's to be generated.</param>
    /// <param name="timeSource">The <see cref="ITimeSource"/> to use when generating ID's.</param>
    /// <param name="sequenceOverflowStrategy">The <see cref="SequenceOverflowStrategy"/> to use when generating ID's.</param>
    public IdGeneratorOptions(
        IdStructure? idStructure = null,
        ITimeSource? timeSource = null,
        SequenceOverflowStrategy sequenceOverflowStrategy = SequenceOverflowStrategy.Throw)
    {
        IdStructure = idStructure ?? _defaultidstructure;
        TimeSource = timeSource ?? _defaulttimesource;
        SequenceOverflowStrategy = sequenceOverflowStrategy;
    }
}
namespace IdGen
{
    /// <summary>
    /// Specifies the strategy to use when a sequence overflow occurs during generation of an ID.
    /// </summary>
    public enum SequenceOverflowStrategy
    {
        /// <summary>
        /// Throw a <see cref="SequenceOverflowException"/> on sequence overflow.
        /// </summary>
        Throw = 0,
        /// <summary>
        /// Wait, using a <see cref="System.Threading.SpinWait"/>, for the tick te pass before generating a new ID.
        /// </summary>
        SpinWait = 1
    }
}

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Defines a base infrastructure of a frequency waveform generator.
    /// </summary>
    public interface IFrequencyGeneratorSource
    {
        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        double Frequency { get; set; }
    }
}

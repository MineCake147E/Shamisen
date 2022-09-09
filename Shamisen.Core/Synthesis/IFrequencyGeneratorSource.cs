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

    /// <summary>
    /// Defines a base infrastructure of a periodic waveform generator.
    /// </summary>
    public interface IPeriodicGeneratorSource<TPhase> where TPhase : unmanaged
    {
        /// <summary>
        /// Gets or sets the current phase of this <see cref="IPeriodicGeneratorSource{TPhase}"/>.
        /// </summary>
        TPhase Theta { get; set; }

        /// <summary>
        /// Gets or sets the angular velocity of this <see cref="IPeriodicGeneratorSource{TPhase}"/>.
        /// </summary>
        TPhase AngularVelocity { get; set; }
    }
}

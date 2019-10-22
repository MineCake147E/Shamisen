namespace MonoAudio.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {
        internal enum ResampleStrategy
        {
            /// <summary>
            /// Calculate on demand
            /// </summary>
            Direct,

            /// <summary>
            /// Pre-Calculated
            /// </summary>
            CachedDirect,

            /// <summary>
            /// Pre-Calculated and Wrapped
            /// </summary>
            CachedWrappedOdd,

            CachedWrappedEven
        }
    }
}

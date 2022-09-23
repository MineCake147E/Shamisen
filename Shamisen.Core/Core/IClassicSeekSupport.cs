namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure that contains seek support of <see cref="IAudioSource{TSample, TFormat}"/> or <see cref="IDataSource{TSample}"/>.
    /// </summary>
    public interface IClassicSeekSupport : ISeekSupport
    {
        /// <summary>
        /// Seeks the <see cref="IAudioSource{TSample, TFormat}"/> with the specified offset in frames.
        /// </summary>
        /// <param name="offset">
        /// The offset in frames.
        /// For <see cref="SeekOrigin.Begin"/> and <see cref="SeekOrigin.End"/>, the value will be treated as unsigned number.
        /// </param>
        /// <param name="origin">The origin.</param>
        void Seek(long offset, SeekOrigin origin);
    }
}
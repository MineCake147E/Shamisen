namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a seekable audio source.
    /// </summary>
    public interface ISeekableAudioSource
    {
        
        long Length { get; }
        long Position { get; set; }
    }
}
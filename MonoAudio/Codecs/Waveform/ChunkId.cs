using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Represents a chunk ID for waveform file format.
    /// </summary>
    public enum ChunkId : uint
    {
        /// <summary>
        /// The RIFF chunk read in Little Endian.
        /// </summary>
        Riff = 0x4646_4952,

        /// <summary>
        /// The RF64 chunk read in Little Endian.
        /// </summary>
        Rf64 = 0x3436_4652,

        /// <summary>
        /// The "fmt " chunk read in Little Endian.
        /// </summary>
        Format = 0x2074_6D66,

        /// <summary>
        /// The fact chunk read in Little Endian.
        /// </summary>
        Fact = 0x7463_6166,

        /// <summary>
        /// The data chunk read in Little Endian.
        /// </summary>
        Data = 0x6174_6164,

        /// <summary>
        /// The Wave List chunk read in Little Endian.
        /// </summary>
        WaveList = 0x6c76_6177,

        /// <summary>
        /// The silent chunk read in Little Endian.
        /// </summary>
        Silent = 0x746e_6c73,

        /// <summary>
        /// The cue chunk read in Little Endian.
        /// </summary>
        Cue = 0x2065_7563,

        /// <summary>
        /// The playlist chunk read in Little Endian.
        /// </summary>
        Playlist = 0x7473_6c70,

        /// <summary>
        /// The associated data list chunk read in Little Endian.
        /// </summary>
        AssociatedDataList = 0x7473_696c,

        /// <summary>
        /// The label chunk read in Little Endian.
        /// </summary>
        Label = 0x6c62_616c,

        /// <summary>
        /// The note chunk read in Little Endian.
        /// </summary>
        Note = 0x6574_6f6e,

        /// <summary>
        /// The labeled text chunk read in Little Endian.
        /// </summary>
        LabeledText = 0x7478_746c,

        /// <summary>
        /// The sampler chunk read in Little Endian.
        /// </summary>
        Sampler = 0x6c70_6d73,

        /// <summary>
        /// The instrument chunk read in Little Endian.
        /// </summary>
        Instrument = 0x7473_6e69,

        /// <summary>
        /// The JUNK chunk read in Little Endian.
        /// </summary>
        JunkLarge = 0x4b4e_554a,

        /// <summary>
        /// The junk chunk read in Little Endian.
        /// </summary>
        JunkSmall = 0x6b6e_756a,
    }
}

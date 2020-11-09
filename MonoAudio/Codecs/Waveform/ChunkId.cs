using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Represents a chunk ID for waveform file format in Little Endian.
    /// </summary>
    public enum ChunkId : uint
    {
        /// <summary>
        /// The RIFF chunk "RIFF" read in Little Endian.
        /// </summary>
        Riff = 0x4646_4952,

        /// <summary>
        /// The RF64 chunk "RF64" read in Little Endian.
        /// </summary>
        Rf64 = 0x3436_4652,

        /// <summary>
        /// The BWF chunk "bw64" read in Little Endian.
        /// </summary>
        Bw64 = 0x3436_7762,

        /// <summary>
        /// The RF64/BWF data size chunk "ds64" read in Little Endian.
        /// </summary>
        Rf64DataSize = 0x3436_7364,

        /// <summary>
        /// The format chunk "fmt " read in Little Endian.
        /// </summary>
        Format = 0x2074_6D66,

        /// <summary>
        /// The fact chunk "fact" read in Little Endian.
        /// </summary>
        Fact = 0x7463_6166,

        /// <summary>
        /// The data chunk "data" read in Little Endian.
        /// </summary>
        Data = 0x6174_6164,

        /// <summary>
        /// The Wave List chunk "wavl" read in Little Endian.
        /// </summary>
        WaveList = 0x6c76_6177,

        /// <summary>
        /// The silent chunk "slnt" read in Little Endian.
        /// </summary>
        Silent = 0x746e_6c73,

        /// <summary>
        /// The cue chunk "cue " read in Little Endian.
        /// </summary>
        Cue = 0x2065_7563,

        /// <summary>
        /// The playlist chunk "plst" read in Little Endian.
        /// </summary>
        Playlist = 0x7473_6c70,

        /// <summary>
        /// The associated data list chunk "list" read in Little Endian.
        /// </summary>
        AssociatedDataList = 0x7473_696c,

        /// <summary>
        /// The label chunk "labl" read in Little Endian.
        /// </summary>
        Label = 0x6c62_616c,

        /// <summary>
        /// The note chunk "note" read in Little Endian.
        /// </summary>
        Note = 0x6574_6f6e,

        /// <summary>
        /// The labeled text chunk "ltxt" read in Little Endian.
        /// </summary>
        LabeledText = 0x7478_746c,

        /// <summary>
        /// The sampler chunk "smpl" read in Little Endian.
        /// </summary>
        Sampler = 0x6c70_6d73,

        /// <summary>
        /// The instrument chunk "inst" read in Little Endian.
        /// </summary>
        Instrument = 0x7473_6e69,

        /// <summary>
        /// The JUNK chunk "JUNK" read in Little Endian.
        /// </summary>
        JunkLarge = 0x4b4e_554a,

        /// <summary>
        /// The junk chunk "junk" read in Little Endian.
        /// </summary>
        JunkSmall = 0x6b6e_756a,

        /// <summary>
        /// The raw XML metadata chunk "axml" read in Little Endian.
        /// </summary>
        MetadataRawAXml = 0x6c6d_7861,

        /// <summary>
        /// The compressed binary XML metadata chunk "bxml" read in Little Endian.
        /// </summary>
        MetadataCompressedBinaryXml = 0x6c6d_7862,

        /// <summary>
        /// The XML list metadata chunk "sxml" read in Little Endian.
        /// </summary>
        MetadataSubXmlList = 0x6c6d_7873,

        /// <summary>
        /// The metadata chunk of ITU-R BS.2076-2 audio definition model "chna" read in Little Endian.
        /// </summary>
        MetadataAudioDefinitionModel = 0x616e_6863,

        /// <summary>
        /// The riff metadata chunk "LIST" read in Little Endian.
        /// </summary>
        AssociatedDataListUpperCase = 0x5453_494c,

        /// <summary>
        /// The id3 metadata chunk "id3 " read in Little Endian.
        /// </summary>
        MetadataId3 = 0x2033_6469,
    }
}

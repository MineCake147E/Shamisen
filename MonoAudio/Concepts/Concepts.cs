using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Concepts;

namespace MonoAudio
{
    /// <summary>
    /// The concepts for Units.
    /// </summary>
    public static class Units
    {
        /// <summary>
        /// The <see cref="Frame"/> is a packed unit of multiple audio <see cref="Sample"/>s.<br/>
        /// A single <see cref="Frame"/> contains the same number of samples as the channel of the stream.<br/>
        /// For example, the length of frames in the 2ch <see cref="AudioEncoding.IeeeFloat"/> audio data in bytes is <c>sizeof(float) * 4</c>, which is 16.<br/>
        /// </summary>
        [Concept(ConceptKind.Unit)]
        public static string Frame { get; }

        /// <summary>
        /// The <see cref="Sample"/> is a smallest unit of the multi-channel discrete(digital) audio data.<br/>
        /// The <see cref="Sample"/> besides channel-by-channel, so the length of frame in bytes is the size of the sample itself, multiplied by the number of channels.
        /// </summary>
        [Concept(ConceptKind.Unit)]
        public static string Sample { get; }
    }
}

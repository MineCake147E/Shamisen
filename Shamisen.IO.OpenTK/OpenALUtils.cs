using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Audio.OpenAL;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Provides utility methods for OpenAL.
    /// </summary>
    public static class OpenALUtils
    {
        /// <summary>
        /// Converts an AL-style string list to a <see cref="List{String}"/>.
        /// </summary>
        /// <param name="alString">The AL-style string list.</param>
        /// <param name="output">The list to write the elements to.</param>
        public static unsafe void FromALStringList(byte* alString, List<string> output)
        {
            ArgumentNullException.ThrowIfNull(output);
            if (alString is null)
            {
                return;
            }
            var currentPos = alString;
            do
            {
                var currentString = Marshal.PtrToStringAnsi((nint)currentPos);
                if (string.IsNullOrEmpty(currentString))
                {
                    break;
                }
                output.Add(currentString);
                currentPos += currentString.Length;
                currentPos++;
            }
            while (true);
        }

        /// <summary>
        /// Converts the specified <see cref="IWaveFormat"/> to <see cref="Format"/>.
        /// </summary>
        /// <param name="format">The format to convert.</param>
        public static Format ConvertToFormat(this IWaveFormat format)
            => format.Channels switch
            {
                1 => format.Encoding switch
                {
                    AudioEncoding.LinearPcm => format.BitDepth switch
                    {
                        8 => Format.FormatMono8,
                        16 => Format.FormatMono16,
                        _ => default
                    },
                    AudioEncoding.IeeeFloat => format.BitDepth switch
                    {
                        32 => Format.FormatMonoFloat32,
                        64 => Format.FormatMonoDoubleExt,
                        _ => default
                    },
                    AudioEncoding.Alaw when format.BitDepth == 8 => Format.FormatMonoAlawExt,
                    AudioEncoding.Mulaw when format.BitDepth == 8 => Format.FormatMonoMulaw,
                    AudioEncoding.MsAdpcm => Format.FormatMonoMsadpcmSoft,
                    AudioEncoding.ImaAdpcm => Format.FormatImaAdpcmMono16Ext,
                    _ => default
                },
                2 => format.Encoding switch
                {
                    AudioEncoding.LinearPcm => format.BitDepth switch
                    {
                        8 => Format.FormatStereo8,
                        16 => Format.FormatStereo16,
                        _ => default
                    },
                    AudioEncoding.IeeeFloat => format.BitDepth switch
                    {
                        32 => Format.FormatStereoFloat32,
                        64 => Format.FormatStereoDoubleExt,
                        _ => default
                    },
                    AudioEncoding.Alaw when format.BitDepth == 8 => Format.FormatStereoAlawExt,
                    AudioEncoding.Mulaw when format.BitDepth == 8 => Format.FormatStereoMulawExt,
                    AudioEncoding.MsAdpcm => Format.FormatStereoMsadpcmSoft,
                    AudioEncoding.ImaAdpcm => Format.FormatImaAdpcmStereo16Ext,
                    _ => default
                },
                4 => format.Encoding switch
                {
                    AudioEncoding.LinearPcm => format.BitDepth switch
                    {
                        8 => Format.FormatQuad8,
                        16 => Format.FormatQuad16,
                        32 => Format.FormatQuad32,
                        _ => default
                    },
                    AudioEncoding.Mulaw when format.BitDepth == 8 => Format.FormatQuadMulaw,
                    _ => default
                },
                6 => format.Encoding switch
                {
                    AudioEncoding.LinearPcm => format.BitDepth switch
                    {
                        8 => Format.Format51chn8,
                        16 => Format.Format51chn16,
                        32 => Format.Format51chn32,
                        _ => default
                    },
                    AudioEncoding.Mulaw when format.BitDepth == 8 => Format.Format51chnMulaw,
                    _ => default
                },
                7 => format.Encoding switch
                {
                    AudioEncoding.LinearPcm => format.BitDepth switch
                    {
                        8 => Format.Format61chn8,
                        16 => Format.Format61chn16,
                        32 => Format.Format61chn32,
                        _ => default
                    },
                    AudioEncoding.Mulaw when format.BitDepth == 8 => Format.Format61chnMulaw,
                    _ => default
                },
                8 => format.Encoding switch
                {
                    AudioEncoding.LinearPcm => format.BitDepth switch
                    {
                        8 => Format.Format71chn8,
                        16 => Format.Format71chn16,
                        32 => Format.Format71chn32,
                        _ => default
                    },
                    AudioEncoding.Mulaw when format.BitDepth == 8 => Format.Format71chnMulaw,
                    _ => default
                },
                _ => default
            };
    }
}

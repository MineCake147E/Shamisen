using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamDecoder" /> struct.</summary>
    public static unsafe class FLAC__StreamDecoderTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamDecoder" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamDecoder>(), Is.EqualTo(sizeof(FLAC__StreamDecoder)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamDecoder" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamDecoder).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamDecoder" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__StreamDecoder), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(FLAC__StreamDecoder), Is.EqualTo(8));
            }
        }
    }
}
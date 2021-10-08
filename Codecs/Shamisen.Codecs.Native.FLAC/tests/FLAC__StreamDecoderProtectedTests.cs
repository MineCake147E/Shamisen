using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamDecoderProtected" /> struct.</summary>
    public static unsafe class FLAC__StreamDecoderProtectedTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamDecoderProtected" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamDecoderProtected>(), Is.EqualTo(sizeof(FLAC__StreamDecoderProtected)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamDecoderProtected" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamDecoderProtected).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamDecoderProtected" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__StreamDecoderProtected), Is.EqualTo(1));
        }
    }
}

using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamEncoderProtected" /> struct.</summary>
    public static unsafe class FLAC__StreamEncoderProtectedTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamEncoderProtected" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamEncoderProtected>(), Is.EqualTo(sizeof(FLAC__StreamEncoderProtected)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamEncoderProtected" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamEncoderProtected).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamEncoderProtected" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__StreamEncoderProtected), Is.EqualTo(1));
        }
    }
}

using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamEncoderPrivate" /> struct.</summary>
    public static unsafe class FLAC__StreamEncoderPrivateTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamEncoderPrivate" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamEncoderPrivate>(), Is.EqualTo(sizeof(FLAC__StreamEncoderPrivate)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamEncoderPrivate" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamEncoderPrivate).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamEncoderPrivate" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__StreamEncoderPrivate), Is.EqualTo(1));
        }
    }
}

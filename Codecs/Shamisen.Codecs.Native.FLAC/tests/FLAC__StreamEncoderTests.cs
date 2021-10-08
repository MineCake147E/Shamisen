using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamEncoder" /> struct.</summary>
    public static unsafe class FLAC__StreamEncoderTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamEncoder" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamEncoder>(), Is.EqualTo(sizeof(FLAC__StreamEncoder)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamEncoder" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamEncoder).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamEncoder" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__StreamEncoder), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(FLAC__StreamEncoder), Is.EqualTo(8));
            }
        }
    }
}

using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__Frame" /> struct.</summary>
    public static unsafe class FLAC__FrameTests
    {
        /// <summary>Validates that the <see cref="FLAC__Frame" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__Frame>(), Is.EqualTo(sizeof(FLAC__Frame)));
        }

        /// <summary>Validates that the <see cref="FLAC__Frame" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__Frame).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__Frame" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__Frame), Is.EqualTo(2608));
            }
            else
            {
                Assert.That(sizeof(FLAC__Frame), Is.EqualTo(2384));
            }
        }
    }
}

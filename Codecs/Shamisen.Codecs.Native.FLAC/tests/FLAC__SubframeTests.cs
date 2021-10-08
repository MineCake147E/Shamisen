using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__Subframe" /> struct.</summary>
    public static unsafe class FLAC__SubframeTests
    {
        /// <summary>Validates that the <see cref="FLAC__Subframe" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__Subframe>(), Is.EqualTo(sizeof(FLAC__Subframe)));
        }

        /// <summary>Validates that the <see cref="FLAC__Subframe" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__Subframe).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__Subframe" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__Subframe), Is.EqualTo(320));
            }
            else
            {
                Assert.That(sizeof(FLAC__Subframe), Is.EqualTo(292));
            }
        }
    }
}

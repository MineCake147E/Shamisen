using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__Subframe_Fixed" /> struct.</summary>
    public static unsafe class FLAC__Subframe_FixedTests
    {
        /// <summary>Validates that the <see cref="FLAC__Subframe_Fixed" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__Subframe_Fixed>(), Is.EqualTo(sizeof(FLAC__Subframe_Fixed)));
        }

        /// <summary>Validates that the <see cref="FLAC__Subframe_Fixed" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__Subframe_Fixed).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__Subframe_Fixed" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__Subframe_Fixed), Is.EqualTo(56));
            }
            else
            {
                Assert.That(sizeof(FLAC__Subframe_Fixed), Is.EqualTo(36));
            }
        }
    }
}

using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__Subframe_Verbatim" /> struct.</summary>
    public static unsafe class FLAC__Subframe_VerbatimTests
    {
        /// <summary>Validates that the <see cref="FLAC__Subframe_Verbatim" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__Subframe_Verbatim>(), Is.EqualTo(sizeof(FLAC__Subframe_Verbatim)));
        }

        /// <summary>Validates that the <see cref="FLAC__Subframe_Verbatim" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__Subframe_Verbatim).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__Subframe_Verbatim" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__Subframe_Verbatim), Is.EqualTo(8));
            }
            else
            {
                Assert.That(sizeof(FLAC__Subframe_Verbatim), Is.EqualTo(4));
            }
        }
    }
}

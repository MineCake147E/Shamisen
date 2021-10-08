using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__FrameFooter" /> struct.</summary>
    public static unsafe class FLAC__FrameFooterTests
    {
        /// <summary>Validates that the <see cref="FLAC__FrameFooter" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__FrameFooter>(), Is.EqualTo(sizeof(FLAC__FrameFooter)));
        }

        /// <summary>Validates that the <see cref="FLAC__FrameFooter" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__FrameFooter).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__FrameFooter" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__FrameFooter), Is.EqualTo(2));
        }
    }
}

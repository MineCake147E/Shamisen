using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__FrameHeader" /> struct.</summary>
    public static unsafe class FLAC__FrameHeaderTests
    {
        /// <summary>Validates that the <see cref="FLAC__FrameHeader" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__FrameHeader>(), Is.EqualTo(sizeof(FLAC__FrameHeader)));
        }

        /// <summary>Validates that the <see cref="FLAC__FrameHeader" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__FrameHeader).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__FrameHeader" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__FrameHeader), Is.EqualTo(40));
        }
    }
}

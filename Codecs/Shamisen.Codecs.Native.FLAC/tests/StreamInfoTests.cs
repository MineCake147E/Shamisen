using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="StreamInfo" /> struct.</summary>
    public static unsafe class StreamInfoTests
    {
        /// <summary>Validates that the <see cref="StreamInfo" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<StreamInfo>(), Is.EqualTo(sizeof(StreamInfo)));
        }

        /// <summary>Validates that the <see cref="StreamInfo" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(StreamInfo).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="StreamInfo" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(StreamInfo), Is.EqualTo(8));
        }
    }
}

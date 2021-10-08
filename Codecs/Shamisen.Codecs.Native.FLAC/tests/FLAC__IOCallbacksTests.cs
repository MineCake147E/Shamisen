using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__IOCallbacks" /> struct.</summary>
    public static unsafe class FLAC__IOCallbacksTests
    {
        /// <summary>Validates that the <see cref="FLAC__IOCallbacks" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__IOCallbacks>(), Is.EqualTo(sizeof(FLAC__IOCallbacks)));
        }

        /// <summary>Validates that the <see cref="FLAC__IOCallbacks" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__IOCallbacks).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__IOCallbacks" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__IOCallbacks), Is.EqualTo(48));
            }
            else
            {
                Assert.That(sizeof(FLAC__IOCallbacks), Is.EqualTo(24));
            }
        }
    }
}

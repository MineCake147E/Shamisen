using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__EntropyCodingMethod" /> struct.</summary>
    public static unsafe class FLAC__EntropyCodingMethodTests
    {
        /// <summary>Validates that the <see cref="FLAC__EntropyCodingMethod" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__EntropyCodingMethod>(), Is.EqualTo(sizeof(FLAC__EntropyCodingMethod)));
        }

        /// <summary>Validates that the <see cref="FLAC__EntropyCodingMethod" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__EntropyCodingMethod).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__EntropyCodingMethod" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__EntropyCodingMethod), Is.EqualTo(24));
            }
            else
            {
                Assert.That(sizeof(FLAC__EntropyCodingMethod), Is.EqualTo(12));
            }
        }
    }
}

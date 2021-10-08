using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__EntropyCodingMethod_PartitionedRice" /> struct.</summary>
    public static unsafe class FLAC__EntropyCodingMethod_PartitionedRiceTests
    {
        /// <summary>Validates that the <see cref="FLAC__EntropyCodingMethod_PartitionedRice" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__EntropyCodingMethod_PartitionedRice>(), Is.EqualTo(sizeof(FLAC__EntropyCodingMethod_PartitionedRice)));
        }

        /// <summary>Validates that the <see cref="FLAC__EntropyCodingMethod_PartitionedRice" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__EntropyCodingMethod_PartitionedRice).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__EntropyCodingMethod_PartitionedRice" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__EntropyCodingMethod_PartitionedRice), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(FLAC__EntropyCodingMethod_PartitionedRice), Is.EqualTo(8));
            }
        }
    }
}

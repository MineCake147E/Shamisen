using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__Metadata_Chain" /> struct.</summary>
    public static unsafe class FLAC__Metadata_ChainTests
    {
        /// <summary>Validates that the <see cref="FLAC__Metadata_Chain" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__Metadata_Chain>(), Is.EqualTo(sizeof(FLAC__Metadata_Chain)));
        }

        /// <summary>Validates that the <see cref="FLAC__Metadata_Chain" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__Metadata_Chain).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__Metadata_Chain" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__Metadata_Chain), Is.EqualTo(1));
        }
    }
}

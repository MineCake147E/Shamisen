using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__Metadata_SimpleIterator" /> struct.</summary>
    public static unsafe class FLAC__Metadata_SimpleIteratorTests
    {
        /// <summary>Validates that the <see cref="FLAC__Metadata_SimpleIterator" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__Metadata_SimpleIterator>(), Is.EqualTo(sizeof(FLAC__Metadata_SimpleIterator)));
        }

        /// <summary>Validates that the <see cref="FLAC__Metadata_SimpleIterator" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__Metadata_SimpleIterator).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__Metadata_SimpleIterator" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__Metadata_SimpleIterator), Is.EqualTo(1));
        }
    }
}

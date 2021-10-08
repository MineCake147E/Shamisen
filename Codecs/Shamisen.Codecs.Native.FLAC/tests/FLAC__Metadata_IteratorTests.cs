using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__Metadata_Iterator" /> struct.</summary>
    public static unsafe class FLAC__Metadata_IteratorTests
    {
        /// <summary>Validates that the <see cref="FLAC__Metadata_Iterator" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__Metadata_Iterator>(), Is.EqualTo(sizeof(FLAC__Metadata_Iterator)));
        }

        /// <summary>Validates that the <see cref="FLAC__Metadata_Iterator" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__Metadata_Iterator).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__Metadata_Iterator" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__Metadata_Iterator), Is.EqualTo(1));
        }
    }
}

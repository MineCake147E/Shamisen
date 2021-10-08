using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamMetadata_SeekPoint" /> struct.</summary>
    public static unsafe class FLAC__StreamMetadata_SeekPointTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_SeekPoint" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamMetadata_SeekPoint>(), Is.EqualTo(sizeof(FLAC__StreamMetadata_SeekPoint)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_SeekPoint" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamMetadata_SeekPoint).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_SeekPoint" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__StreamMetadata_SeekPoint), Is.EqualTo(24));
        }
    }
}

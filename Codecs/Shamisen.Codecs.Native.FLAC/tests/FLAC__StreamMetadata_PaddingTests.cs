using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamMetadata_Padding" /> struct.</summary>
    public static unsafe class FLAC__StreamMetadata_PaddingTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Padding" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamMetadata_Padding>(), Is.EqualTo(sizeof(FLAC__StreamMetadata_Padding)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Padding" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamMetadata_Padding).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Padding" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__StreamMetadata_Padding), Is.EqualTo(4));
        }
    }
}

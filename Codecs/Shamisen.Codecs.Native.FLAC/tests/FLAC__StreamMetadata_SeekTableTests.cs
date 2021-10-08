using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamMetadata_SeekTable" /> struct.</summary>
    public static unsafe class FLAC__StreamMetadata_SeekTableTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_SeekTable" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamMetadata_SeekTable>(), Is.EqualTo(sizeof(FLAC__StreamMetadata_SeekTable)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_SeekTable" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamMetadata_SeekTable).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_SeekTable" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__StreamMetadata_SeekTable), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(FLAC__StreamMetadata_SeekTable), Is.EqualTo(8));
            }
        }
    }
}

using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamMetadata_Picture" /> struct.</summary>
    public static unsafe class FLAC__StreamMetadata_PictureTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Picture" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamMetadata_Picture>(), Is.EqualTo(sizeof(FLAC__StreamMetadata_Picture)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Picture" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamMetadata_Picture).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Picture" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__StreamMetadata_Picture), Is.EqualTo(56));
            }
            else
            {
                Assert.That(sizeof(FLAC__StreamMetadata_Picture), Is.EqualTo(36));
            }
        }
    }
}

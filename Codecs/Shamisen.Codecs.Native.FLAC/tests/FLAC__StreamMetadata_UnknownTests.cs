using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamMetadata_Unknown" /> struct.</summary>
    public static unsafe class FLAC__StreamMetadata_UnknownTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Unknown" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamMetadata_Unknown>(), Is.EqualTo(sizeof(FLAC__StreamMetadata_Unknown)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Unknown" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamMetadata_Unknown).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Unknown" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__StreamMetadata_Unknown), Is.EqualTo(8));
            }
            else
            {
                Assert.That(sizeof(FLAC__StreamMetadata_Unknown), Is.EqualTo(4));
            }
        }
    }
}

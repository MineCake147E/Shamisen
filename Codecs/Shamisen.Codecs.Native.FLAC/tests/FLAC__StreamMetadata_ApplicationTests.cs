using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamMetadata_Application" /> struct.</summary>
    public static unsafe class FLAC__StreamMetadata_ApplicationTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Application" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamMetadata_Application>(), Is.EqualTo(sizeof(FLAC__StreamMetadata_Application)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Application" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamMetadata_Application).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_Application" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__StreamMetadata_Application), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(FLAC__StreamMetadata_Application), Is.EqualTo(8));
            }
        }
    }
}

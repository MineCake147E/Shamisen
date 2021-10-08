using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamMetadata_VorbisComment" /> struct.</summary>
    public static unsafe class FLAC__StreamMetadata_VorbisCommentTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_VorbisComment" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamMetadata_VorbisComment>(), Is.EqualTo(sizeof(FLAC__StreamMetadata_VorbisComment)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_VorbisComment" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamMetadata_VorbisComment).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_VorbisComment" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(FLAC__StreamMetadata_VorbisComment), Is.EqualTo(32));
            }
            else
            {
                Assert.That(sizeof(FLAC__StreamMetadata_VorbisComment), Is.EqualTo(16));
            }
        }
    }
}

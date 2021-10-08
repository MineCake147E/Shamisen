using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__StreamMetadata_CueSheet_Track" /> struct.</summary>
    public static unsafe class FLAC__StreamMetadata_CueSheet_TrackTests
    {
        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_CueSheet_Track" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__StreamMetadata_CueSheet_Track>(), Is.EqualTo(sizeof(FLAC__StreamMetadata_CueSheet_Track)));
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_CueSheet_Track" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__StreamMetadata_CueSheet_Track).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__StreamMetadata_CueSheet_Track" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__StreamMetadata_CueSheet_Track), Is.EqualTo(40));
        }
    }
}

using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="Picture" /> struct.</summary>
    public static unsafe class PictureTests
    {
        /// <summary>Validates that the <see cref="Picture" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Picture>(), Is.EqualTo(sizeof(Picture)));
        }

        /// <summary>Validates that the <see cref="Picture" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Picture).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Picture" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(Picture), Is.EqualTo(8));
        }
    }
}

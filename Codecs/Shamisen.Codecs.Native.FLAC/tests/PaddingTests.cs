using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="Padding" /> struct.</summary>
    public static unsafe class PaddingTests
    {
        /// <summary>Validates that the <see cref="Padding" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Padding>(), Is.EqualTo(sizeof(Padding)));
        }

        /// <summary>Validates that the <see cref="Padding" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Padding).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Padding" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(Padding), Is.EqualTo(8));
        }
    }
}

using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="Unknown" /> struct.</summary>
    public static unsafe class UnknownTests
    {
        /// <summary>Validates that the <see cref="Unknown" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Unknown>(), Is.EqualTo(sizeof(Unknown)));
        }

        /// <summary>Validates that the <see cref="Unknown" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Unknown).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Unknown" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(Unknown), Is.EqualTo(8));
        }
    }
}

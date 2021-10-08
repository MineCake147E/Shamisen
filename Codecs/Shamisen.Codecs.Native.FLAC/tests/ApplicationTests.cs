using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="Application" /> struct.</summary>
    public static unsafe class ApplicationTests
    {
        /// <summary>Validates that the <see cref="Application" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Application>(), Is.EqualTo(sizeof(Application)));
        }

        /// <summary>Validates that the <see cref="Application" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Application).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Application" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(Application), Is.EqualTo(8));
        }
    }
}

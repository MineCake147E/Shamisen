using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="Prototype" /> struct.</summary>
    public static unsafe class PrototypeTests
    {
        /// <summary>Validates that the <see cref="Prototype" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Prototype>(), Is.EqualTo(sizeof(Prototype)));
        }

        /// <summary>Validates that the <see cref="Prototype" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Prototype).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Prototype" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(Prototype), Is.EqualTo(24));
            }
            else
            {
                Assert.That(sizeof(Prototype), Is.EqualTo(16));
            }
        }
    }
}

using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="Chain" /> struct.</summary>
    public static unsafe class ChainTests
    {
        /// <summary>Validates that the <see cref="Chain" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Chain>(), Is.EqualTo(sizeof(Chain)));
        }

        /// <summary>Validates that the <see cref="Chain" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Chain).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Chain" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(Chain), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(Chain), Is.EqualTo(8));
            }
        }
    }
}

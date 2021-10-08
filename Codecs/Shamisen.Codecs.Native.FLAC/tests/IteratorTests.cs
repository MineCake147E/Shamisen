using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="Iterator" /> struct.</summary>
    public static unsafe class IteratorTests
    {
        /// <summary>Validates that the <see cref="Iterator" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Iterator>(), Is.EqualTo(sizeof(Iterator)));
        }

        /// <summary>Validates that the <see cref="Iterator" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Iterator).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Iterator" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(Iterator), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(Iterator), Is.EqualTo(8));
            }
        }
    }
}

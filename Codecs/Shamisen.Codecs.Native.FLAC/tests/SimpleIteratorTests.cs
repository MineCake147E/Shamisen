using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="SimpleIterator" /> struct.</summary>
    public static unsafe class SimpleIteratorTests
    {
        /// <summary>Validates that the <see cref="SimpleIterator" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<SimpleIterator>(), Is.EqualTo(sizeof(SimpleIterator)));
        }

        /// <summary>Validates that the <see cref="SimpleIterator" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(SimpleIterator).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="SimpleIterator" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(SimpleIterator), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(SimpleIterator), Is.EqualTo(8));
            }
        }
    }
}

using System;

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="Stream" /> struct.</summary>
    public static unsafe class StreamTests
    {
        /// <summary>Validates that the <see cref="Stream" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Stream>(), Is.EqualTo(sizeof(Stream)));
        }

        /// <summary>Validates that the <see cref="Stream" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Stream).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Stream" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(Stream), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(Stream), Is.EqualTo(8));
            }
        }
    }

    /// <summary>Provides validation of the <see cref="Stream" /> struct.</summary>
    public static unsafe class StreamTests
    {
        /// <summary>Validates that the <see cref="Stream" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<Stream>(), Is.EqualTo(sizeof(Stream)));
        }

        /// <summary>Validates that the <see cref="Stream" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(Stream).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="Stream" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.That(sizeof(Stream), Is.EqualTo(16));
            }
            else
            {
                Assert.That(sizeof(Stream), Is.EqualTo(8));
            }
        }
    }
}

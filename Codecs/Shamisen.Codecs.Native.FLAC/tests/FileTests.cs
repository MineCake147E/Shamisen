using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="File" /> struct.</summary>
    public static unsafe class FileTests
    {
        /// <summary>Validates that the <see cref="File" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<File>(), Is.EqualTo(sizeof(File)));
        }

        /// <summary>Validates that the <see cref="File" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(File).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="File" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(File), Is.EqualTo(8));
        }
    }

    /// <summary>Provides validation of the <see cref="File" /> struct.</summary>
    public static unsafe class FileTests
    {
        /// <summary>Validates that the <see cref="File" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<File>(), Is.EqualTo(sizeof(File)));
        }

        /// <summary>Validates that the <see cref="File" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(File).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="File" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(File), Is.EqualTo(8));
        }
    }
}

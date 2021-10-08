using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="SeekTable" /> struct.</summary>
    public static unsafe class SeekTableTests
    {
        /// <summary>Validates that the <see cref="SeekTable" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<SeekTable>(), Is.EqualTo(sizeof(SeekTable)));
        }

        /// <summary>Validates that the <see cref="SeekTable" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(SeekTable).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="SeekTable" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(SeekTable), Is.EqualTo(8));
        }
    }
}

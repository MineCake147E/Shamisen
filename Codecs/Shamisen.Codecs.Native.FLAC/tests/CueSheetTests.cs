using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="CueSheet" /> struct.</summary>
    public static unsafe class CueSheetTests
    {
        /// <summary>Validates that the <see cref="CueSheet" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<CueSheet>(), Is.EqualTo(sizeof(CueSheet)));
        }

        /// <summary>Validates that the <see cref="CueSheet" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(CueSheet).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="CueSheet" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(CueSheet), Is.EqualTo(8));
        }
    }
}

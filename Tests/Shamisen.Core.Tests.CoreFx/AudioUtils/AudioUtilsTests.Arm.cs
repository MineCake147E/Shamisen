﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Utils;

namespace Shamisen.Core.Tests.CoreFx.AudioUtilsTest
{
    public partial class AudioUtilsTests
    {
        [TestFixture(Category = "Arm")]
        public class Arm
        {
            private static IEnumerable<int> ArmSizeTestCaseGenerator() => SizeTestCaseGenerator();

            /*
            [TestCaseSource(nameof(ArmSizeTestCaseGenerator))]
            public void InterleaveStereoWorksCorrectly(int size)
            {
                PrepareStereo(size, out var a0, out var a1, out var b);
                AudioUtils.Arm.InterleaveStereoInt32(b, a0, a1);
                AssertArray(b);
            }
            */

            [TestCaseSource(nameof(ArmSizeTestCaseGenerator))]
            public void InterleaveThreeWorksCorrectly(int size)
            {
                PrepareThree(size, out int[] a0, out int[] a1, out int[] a2, out int[] b);
                if (!AudioUtils.Arm.IsSupported)
                {
                    Assert.Warn("ARM intrinsics is not supported on this machine!");
                    return;
                }
                AudioUtils.Arm.InterleaveThreeInt32(b, a0, a1, a2);
                AssertArrayForInterleave(b);
            }
        }
    }
}

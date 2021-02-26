using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Shamisen.Core.Tests.CoreFx
{
    public static class AssertExt
    {
        public static void DoesNotTakeSoLong(Action action) => DoesNotTakeSoLong(action, TimeSpan.FromSeconds(1));

        public static void DoesNotTakeSoLong(Action action, TimeSpan timeout)
        {
            CancellationTokenSource tsFail = new();
            var t = Task.Run(action, tsFail.Token);
            t.ConfigureAwait(false);
            tsFail.CancelAfter(timeout);
            Assert.DoesNotThrow(() => t.Wait(), $"Timeout({timeout}) exceeded.");
        }
    }
}

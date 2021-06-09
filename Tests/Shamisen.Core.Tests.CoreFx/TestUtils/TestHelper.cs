using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Data;

namespace Shamisen.Core.Tests.CoreFx
{
    public static class TestHelper
    {
        public const string ResourcesPath = "Shamisen.Core.Tests.CoreFx.Resources";

        public static void DoesNotTakeSoLong(Action action) => DoesNotTakeSoLong(action, TimeSpan.FromSeconds(1));

        public static void DoesNotTakeSoLong(Action action, TimeSpan timeout)
        {
            CancellationTokenSource tsFail = new();
            var t = Task.Run(action, tsFail.Token);
            t.ConfigureAwait(false);
            tsFail.CancelAfter(timeout);
            Assert.DoesNotThrow(() => t.Wait(), $"Timeout({timeout}) exceeded.");
        }

        public static DataCache<byte> GetDataCacheFromResource(string name)
        {
            var lib = Assembly.GetExecutingAssembly();
            var ms = new DataCache<byte>();
            using (var stream = lib.GetManifestResourceStream($"{ResourcesPath}.{name}"))
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);
                    _ = mem.Seek(0, SeekOrigin.Begin);
                    ms.Write(mem.GetBuffer().AsSpan().Slice(0, (int)mem.Length));
                }
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static MemoryStream GetDataStreamFromResource(string name)
        {
            var lib = Assembly.GetExecutingAssembly();
            var ms = new MemoryStream();
            using (var stream = lib.GetManifestResourceStream($"{ResourcesPath}.{name}"))
            {
                stream.CopyTo(ms);
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}

using System;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Media.Audio.CoreAudio;
using Windows.Win32.Storage.StructuredStorage;
using Windows.Win32.System.Com;

//using Windows.Win32.

namespace Shamisen.IO.WindowsCoreAudio
{
    public class Class1
    {
        public static void Main()
        {
            var hr = PInvoke.CoCreateInstance(typeof(MMDeviceEnumerator).GUID, null, CLSCTX.CLSCTX_ALL, typeof(IMMDeviceEnumerator).GUID, out var enumObject);
            hr.ThrowOnFailure();
            IMMDeviceEnumerator enumerator = (IMMDeviceEnumerator)enumObject;
            enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, out var device);
            device.Activate(typeof(IAudioClient).GUID, (uint)CLSCTX.CLSCTX_ALL, default, out void* ppInt);
        }
    }
}

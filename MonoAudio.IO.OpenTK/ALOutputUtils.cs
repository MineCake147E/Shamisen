using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Audio.OpenAL;

using ALSourceID = System.Int32;

namespace MonoAudio.IO
{
    public sealed partial class ALOutput
    {
        /// <summary>
        /// Enumerates all device names using <c>ALC_ENUMERATION_EXT</c>(falls back to <c>ALC_ENUMERATE_ALL_EXT</c>).
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> EnumrateDeviceNames()
        {
            if (Alc.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATION_EXT"))
            {
                foreach (var item in Alc.GetString(IntPtr.Zero, AlcGetStringList.DeviceSpecifier))
                {
                    yield return item;
                }
            }
            else if (Alc.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATE_ALL_EXT"))
            {
                foreach (var item in Alc.GetString(IntPtr.Zero, AlcGetStringList.AllDevicesSpecifier))
                {
                    yield return item;
                }
            }
        }
    }
}

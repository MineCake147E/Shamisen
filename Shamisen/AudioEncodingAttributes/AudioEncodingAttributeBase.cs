using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen
{
    /// <summary>
    /// Specifies the properties of audio encoding format.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public abstract class AudioEncodingAttribute : Attribute
    {
        private protected AudioEncodingAttribute() { }
    }
}

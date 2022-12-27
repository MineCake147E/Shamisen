﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base structure of an audio device.<br/>
    /// </summary>
    public interface IAudioDevice : IEquatable<IAudioDevice>
    {
        /// <summary>
        /// The name of the frontend.
        /// </summary>
        static virtual string FrontendName => string.Empty;

        /// <summary>
        /// Gets the name of this audio device.
        /// </summary>
        /// <value>
        /// The name of this audio device.
        /// </value>
        string Name { get; }
    }
}
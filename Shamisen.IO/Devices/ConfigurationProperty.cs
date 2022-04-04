using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.IO.Devices
{
    /// <summary>
    /// Provides a way to tell someone the value and priority.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ConfigurationProperty<T> : IConfigurationProperty<T>
    {
        /// <inheritdoc/>
        public ConfigurationPropertyPriority Priority { get; }
        /// <inheritdoc/>
        public T Value { get; }
    }

    /// <summary>
    /// Provides a way to tell someone the value and priority.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct MutableConfigurationProperty<T> : IMutableConfigurationProperty<T>
    {
        /// <summary>
        /// Gets or sets the priority for this setting.
        /// </summary>
        public ConfigurationPropertyPriority Priority { get; set; }

        /// <summary>
        /// Sets the priority for this setting.
        /// </summary>
        public void SetPriority(ConfigurationPropertyPriority value) => Priority = value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Sets the value.
        /// </summary>
        public void SetValue(T value) => Value = value;
    }
}

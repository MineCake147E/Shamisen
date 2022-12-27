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
        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurationProperty{T}"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="priority">The priority.</param>
        public ConfigurationProperty(T value, ConfigurationPropertyPriority priority)
        {
            Value = value;
            Priority = priority;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurationProperty{T}"/>.
        /// </summary>
        public ConfigurationProperty(MutableConfigurationProperty<T>? mutableConfigurationProperty)
        {
            var m = mutableConfigurationProperty ?? default;
            Value = m.Value;
            Priority = m.Priority;
        }

        /// <inheritdoc/>
        public ConfigurationPropertyPriority Priority { get; }
        /// <inheritdoc/>
        public T? Value { get; }
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
        /// Gets or sets the value.
        /// </summary>
        public T? Value { get; set; }

        /// <summary>
        /// Performs an explicit conversion from <see cref="MutableConfigurationProperty{T}"/> to <see cref="ConfigurationProperty{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator ConfigurationProperty<T>(MutableConfigurationProperty<T>? value) => new(value);
    }
}

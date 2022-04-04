namespace Shamisen.IO.Devices
{
    /// <summary>
    /// Defines a base infrastructure of a configuration property.
    /// </summary>
    /// <typeparam name="T">The type for <see cref="Value"/>.</typeparam>
    public interface IConfigurationProperty<out T>
    {
        /// <summary>
        /// Gets the priority for this setting.
        /// </summary>
        ConfigurationPropertyPriority Priority { get; }
        /// <summary>
        /// Gets the value.
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// Defines a base infrastructure of a mutable configuration property.<br/>
    /// Some implementation can provide setters for <see cref="IConfigurationProperty{T}.Priority"/> and <see cref="IConfigurationProperty{T}.Value"/>.
    /// </summary>
    /// <typeparam name="T">The type for <see cref="SetValue(T)"/>.</typeparam>
    public interface IMutableConfigurationProperty<T> : IConfigurationProperty<T>
    {
        /// <summary>
        /// Sets the priority for this setting.
        /// </summary>
        void SetPriority(ConfigurationPropertyPriority value);

        /// <summary>
        /// Sets the value.
        /// </summary>
        void SetValue(T value);
    }
}
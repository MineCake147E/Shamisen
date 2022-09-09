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
    /// <typeparam name="T">The type for <see cref="Value"/>.</typeparam>
    public interface IMutableConfigurationProperty<T> : IConfigurationProperty<T>
    {
        /// <summary>
        /// Gets or sets the priority for this setting.
        /// </summary>
#pragma warning disable CS0108 // メンバーは継承されたメンバーを非表示にします。キーワード new がありません
#pragma warning disable S2376 // Write-only properties should not be used
        ConfigurationPropertyPriority Priority { set; }
#pragma warning restore S2376 // Write-only properties should not be used
#pragma warning restore CS0108 // メンバーは継承されたメンバーを非表示にします。キーワード new がありません

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
#pragma warning disable CS0108 // メンバーは継承されたメンバーを非表示にします。キーワード new がありません
#pragma warning disable S2376 // Write-only properties should not be used
        T Value { set; }
#pragma warning restore S2376 // Write-only properties should not be used
#pragma warning restore CS0108 // メンバーは継承されたメンバーを非表示にします。キーワード new がありません
    }

    /// <summary>
    /// Contains some utility functions for <see cref="IConfigurationProperty{T}"/> and <see cref="IMutableConfigurationProperty{T}"/>.
    /// </summary>
    public static class ConfigurationUtils
    {
        /// <summary>
        /// Sets the <see cref="IMutableConfigurationProperty{T}.Value"/> and <see cref="IMutableConfigurationProperty{T}.Priority"/> at the same time.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The value.</param>
        /// <param name="priority">The priority.</param>
        public static void Set<TProperty, T>(this TProperty property, T value, ConfigurationPropertyPriority priority = ConfigurationPropertyPriority.RequiredExact) where TProperty : IMutableConfigurationProperty<T>
            => (property.Value, property.Priority) = (value, priority);
    }
}
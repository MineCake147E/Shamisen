using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.IO.Devices
{
    /// <summary>
    /// Defines the values for priority of settings.
    /// </summary>
    public enum ConfigurationPropertyPriority
    {
        /// <summary>
        /// The initializer can ignore the setting if it's impossible to obey.
        /// </summary>
        Optional,
        /// <summary>
        /// The initializer can compromise the setting if it's impossible to obey.
        /// </summary>
        DesiredBetterThanOrEqualsTo,
        /// <summary>
        /// The initializer must cancel the initialization if it's impossible to obey.
        /// </summary>
        RequiredBetterThanOrEqualsTo,
        /// <summary>
        /// The initializer must cancel the initialization if it's impossible to obey.
        /// </summary>
        RequiredExact
    }
}

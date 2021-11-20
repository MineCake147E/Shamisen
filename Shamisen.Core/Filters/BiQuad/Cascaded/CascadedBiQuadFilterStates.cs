using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Filters.BiQuad.Cascaded
{
    /// <summary>
    /// Contains states of <see cref="CascadedBiQuadFilter"/>.
    /// </summary>
    internal sealed class CascadedBiQuadFilterStates
    {
        /// <summary>
        /// access: parameters[ch + <see cref="Channels"/> * (order + <see cref="Orders"/> * kind)]
        /// kind id: {0 => b0, 1 => b1, 2 => b2, 3 => a1, 4 => a2}
        /// </summary>
        internal float[] parameters;

        /// <summary>
        /// access: states[ch + <see cref="Channels"/> * (order + <see cref="Orders"/> * kind)]
        /// kind id: {0 => z0, 1 => z1}
        /// </summary>
        internal float[] states;

        /// <summary>
        /// Initializes a new Instance of <see cref="CascadedBiQuadFilterStates"/>.
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="orders"></param>
        /// <param name="parameters"></param>
        /// <param name="states"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public CascadedBiQuadFilterStates(int channels, int orders, float[] parameters, float[] states)
        {
            Channels = channels;
            Orders = orders;
            if (orders % 4 != 0) throw new ArgumentOutOfRangeException(nameof(orders), $"The number of {nameof(orders)} must be aligned with 4!");
            this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            this.states = states ?? throw new ArgumentNullException(nameof(states));
            if (parameters.LongLength < 5L * channels * orders)
                throw new ArgumentException($"The length of {nameof(parameters)} must be at least as large as {5L * channels * orders} !", nameof(parameters));
            if (states.LongLength < 2L * channels * orders)
                throw new ArgumentException($"The length of {nameof(states)} must be at least as large as {2L * channels * orders} !", nameof(parameters));
        }

        private int Channels { get; }
        private int Orders { get; }

        /// <summary>
        /// This thing is supplied only for coefficients changes.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public BiQuadParameter this[int channel, int order]
        {
            get
            {
                if (channel >= Channels || channel < 0) throw new ArgumentOutOfRangeException(nameof(channel), "");
#pragma warning disable IDE0046
                if (order >= Orders || order < 0) throw new ArgumentOutOfRangeException(nameof(order), "");
#pragma warning restore IDE0046
                return new(parameters[channel + Channels * (order + Orders * 0)],
                           parameters[channel + Channels * (order + Orders * 1)],
                           parameters[channel + Channels * (order + Orders * 2)],
                           parameters[channel + Channels * (order + Orders * 3)],
                           parameters[channel + Channels * (order + Orders * 4)]);
            }
            set
            {
                if (channel >= Channels || channel < 0) throw new ArgumentOutOfRangeException(nameof(channel), "");
                if (order >= Orders || order < 0) throw new ArgumentOutOfRangeException(nameof(order), "");
                (parameters[channel + Channels * (order + Orders * 0)],
                    parameters[channel + Channels * (order + Orders * 1)],
                    parameters[channel + Channels * (order + Orders * 2)],
                    parameters[channel + Channels * (order + Orders * 3)],
                    parameters[channel + Channels * (order + Orders * 4)]) = value;
            }
        }
    }
}

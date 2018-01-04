﻿using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.CAP;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.Capacitor"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        ModelBaseParameters mbp;
        BaseParameters bp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            if (!bp.CAPcapac.Given)
                mbp = provider.GetParameters<ModelBaseParameters>(1);
        }
        
        /// <summary>
        /// Execute the behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            if (!bp.CAPcapac.Given)
            {
                if (mbp == null)
                    throw new CircuitException("No model specified");

                double width = bp.CAPwidth.Given ? bp.CAPwidth.Value : mbp.CAPdefWidth.Value;
                bp.CAPcapac.Value = mbp.CAPcj *
                    (bp.CAPwidth - mbp.CAPnarrow) *
                    (bp.CAPlength - mbp.CAPnarrow) +
                    mbp.CAPcjsw * 2 * (
                    (bp.CAPlength - mbp.CAPnarrow) +
                    (bp.CAPwidth - mbp.CAPnarrow));
            }
        }
    }
}

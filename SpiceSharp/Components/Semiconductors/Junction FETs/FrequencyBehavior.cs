﻿using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharp.Components.JFETs
{
    /// <summary>
    /// Frequency behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="Biasing" />
    /// <seealso cref="IFrequencyBehavior" />
    [BehaviorFor(typeof(JFET), typeof(IFrequencyBehavior), 2)]
    public class FrequencyBehavior : Biasing,
        IFrequencyBehavior
    {
        private readonly int _drainNode, _gateNode, _sourceNode, _drainPrimeNode, _sourcePrimeNode;

        /// <summary>
        /// Gets the gate-source capacitance.
        /// </summary>
        [ParameterName("capgs"), ParameterInfo("Capacitance G-S")]
        public double CapGs { get; private set; }

        /// <summary>
        /// Gets the gate-drain capacitance.
        /// </summary>
        [ParameterName("capgd"), ParameterInfo("Capacitance G-D")]
        public double CapGd { get; private set; }

        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Gets the complex state.
        /// </summary>
        /// <value>
        /// The complex state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        /// <summary>
        /// Gets the internal drain node.
        /// </summary>
        /// <value>
        /// The internal drain node.
        /// </value>
        protected new IVariable<Complex> DrainPrime { get; }

        /// <summary>
        /// Gets the internal source node.
        /// </summary>
        /// <value>
        /// The internal source node.
        /// </value>
        protected new IVariable<Complex> SourcePrime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public FrequencyBehavior(IComponentBindingContext context)
            : base(context)
        {
            ComplexState = context.GetState<IComplexSimulationState>();

            DrainPrime = ComplexState.GetSharedVariable(context.Nodes[0]);
            _drainNode = ComplexState.Map[DrainPrime];
            _gateNode = ComplexState.Map[ComplexState.GetSharedVariable(context.Nodes[1])];
            SourcePrime = ComplexState.GetSharedVariable(context.Nodes[2]);
            _sourceNode = ComplexState.Map[SourcePrime];

            if (ModelParameters.DrainResistance > 0)
                DrainPrime = ComplexState.CreatePrivateVariable(Name.Combine("drain"), Units.Volt);
            _drainPrimeNode = ComplexState.Map[DrainPrime];

            if (ModelParameters.SourceResistance > 0)
                SourcePrime = ComplexState.CreatePrivateVariable(Name.Combine("source"), Units.Volt);
            _sourcePrimeNode = ComplexState.Map[SourcePrime];

            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_drainNode, _drainNode),
                new MatrixLocation(_gateNode, _gateNode),
                new MatrixLocation(_sourceNode, _sourceNode),
                new MatrixLocation(_drainPrimeNode, _drainPrimeNode),
                new MatrixLocation(_sourcePrimeNode, _sourcePrimeNode),
                new MatrixLocation(_drainNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _sourcePrimeNode),
                new MatrixLocation(_sourceNode, _sourcePrimeNode),
                new MatrixLocation(_drainPrimeNode, _drainNode),
                new MatrixLocation(_drainPrimeNode, _gateNode),
                new MatrixLocation(_drainPrimeNode, _sourcePrimeNode),
                new MatrixLocation(_sourcePrimeNode, _gateNode),
                new MatrixLocation(_sourcePrimeNode, _sourceNode),
                new MatrixLocation(_sourcePrimeNode, _drainPrimeNode));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            var vgs = Vgs;
            var vgd = Vgd;

            // Calculate charge storage elements
            var czgs = TempCapGs * Parameters.Area;
            var czgd = TempCapGd * Parameters.Area;
            var twop = TempGatePotential + TempGatePotential;
            var czgsf2 = czgs / ModelTemperature.F2;
            var czgdf2 = czgd / ModelTemperature.F2;
            if (vgs < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgs / TempGatePotential);
                CapGs = czgs / sarg;
            }
            else
                CapGs = czgsf2 * (ModelTemperature.F3 + vgs / twop);

            if (vgd < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgd / TempGatePotential);
                CapGd = czgd / sarg;
            }
            else
                CapGd = czgdf2 * (ModelTemperature.F3 + vgd / twop);
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            var omega = ComplexState.Laplace.Imaginary;

            var gdpr = ModelParameters.DrainConductance * Parameters.Area;
            var gspr = ModelParameters.SourceConductance * Parameters.Area;
            var gm = Gm;
            var gds = Gds;
            var ggs = Ggs;
            var xgs = CapGs * omega;
            var ggd = Ggd;
            var xgd = CapGd * omega;

            ComplexElements.Add(
                gdpr,
                new Complex(ggd + ggs, xgd + xgs),
                gspr,
                new Complex(gdpr + gds + ggd, xgd),
                new Complex(gspr + gds + gm + ggs, xgs),
                -gdpr,
                -new Complex(ggd, xgd),
                -new Complex(ggs, xgs),
                -gspr,
                -gdpr,
                new Complex(-ggd + gm, -xgd),
                (-gds - gm),
                -new Complex(ggs + gm, xgs),
                -gspr,
                -gds);
        }
    }
}

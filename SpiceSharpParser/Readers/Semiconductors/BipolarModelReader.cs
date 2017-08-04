﻿using System;
using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read bipolar transistor models
    /// </summary>
    public class BipolarModelReader : Reader
    {
        private bool npn = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isNpn">True if it is parsing npn transistor, false for pnp transistors</param>
        public BipolarModelReader(bool isNpn)
        {
            npn = isNpn;
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            BipolarModel model = new BipolarModel(name.ReadIdentifier());
            if (npn)
                model.BJTSetNPN();
            else
                model.BJTSetPNP();
            model.ReadParameters(parameters);
            netlist.Circuit.Components.Add(model);
            return true;
        }
    }
}
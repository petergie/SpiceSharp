﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Components;

using static SpiceSharp.Parser.SpiceSharpParserConstants;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class capable of reading models
    /// </summary>
    public class ModelReader : Reader
    {
        /// <summary>
        /// A list of model readers
        /// </summary>
        public Dictionary<string, Reader> ModelReaders { get; } = new Dictionary<string, Reader>();

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (!name.TryReadLiteral("model"))
                return false;

            // Extract the name of the model
            if (parameters.Count < 2)
                throw new ParseException(name, "Invalid model declaration");
            if (!(parameters[0] is Token))
                throw new ParseException(name, "Invalid model name");
            Token modelname = parameters[0] as Token;
            parameters.RemoveAt(0);

            // We have two options for the model: bracketted or not
            // - .MODEL MNAME TYPE(PAR1=VAL1 PAR2=VAL2 ...)
            // - .MODEL MNAME TYPE PAR1=VAL1 PAR2=VAL2 ...
            string modeltype;
            if (parameters[0] is Token)
            {
                modeltype = parameters[0].ReadWord().ToLower();
                parameters.RemoveAt(0);
            }
            else if (parameters[0] is BracketToken)
            {
                var b = parameters[0] as BracketToken;
                modeltype = parameters[0].ReadWord().ToLower();
                parameters = b.Parameters;
            }
            else
                throw new ParseException(parameters[0], "Invalid model declaration");

            // Find the right type in our
            if (!ModelReaders.ContainsKey(modeltype))
                throw new ParseException(parameters[0], "Unrecognized model type");
            return ModelReaders[modeltype].Read(modelname, parameters, netlist);
        }
    }
}
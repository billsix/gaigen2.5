// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

// Copyright 2008-2010, Daniel Fontijne, University of Amsterdam -- fontijne@science.uva.nl

using System;
using System.Collections.Generic;
using System.Text;


namespace G25.CG.Shared
{

    /// <summary>
    /// This class contains functions to write 'shortcuts' to functions.
    /// For example, is there is a function gp(x, y), then the code
    /// generated by this class makes it possible to write x.gp(y).
    /// </summary>
    public class Shortcut
    {

        /// <summary>
        /// Writes getters and setters for the SMV coordinates..
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Not used yet.</param>
        /// <param name="FT">Float point type of 'SMV'.</param>
        /// <param name="smv">The specialized multivector for which the struct should be written.</param>
        public static void WriteFunctionShortcuts(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.VariableType type)
        {

            // put this code in csj shared
            // how to get back from csj shared to c#
            // or also put the operator stuff in csj shared?
            // Get some generic code to loop over all functions in S.m_functions
            // then do something when first type is this
            foreach (G25.fgs fgs in S.m_functions)
            {
                if (fgs.GetSupportedByPlugin() && (fgs.NbArguments >= 1))
                {
                    bool computeMultivectorValue = false;
                    G25.CG.Shared.FuncArgInfo[] FAI = null;
                    try
                    {
                        FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(S, fgs, fgs.NbArguments, FT, "not set", computeMultivectorValue);
                    }
                    catch (Exception ex)
                    {
                        FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(S, fgs, fgs.NbArguments, FT, "not set", computeMultivectorValue);
                    }

                    if (FAI[0].TypeName.Equals(type.GetName()))
                    {
                        // generate shortcut!
                        SB.AppendLine("// shortcut to " + fgs.ToString());
                    }

                    //Console.WriteLine("func name: " + F.m_outputName + " -- first arg type: " + F.GetArgumentTypeName(0, ""));
                    // use this to write decl: (get all FAI except the first)
                    //                            public static void WriteDeclaration(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd,
                    //          bool inline, bool staticFunc, string returnType, string functionName,
                    //        FuncArgInfo returnArgument, FuncArgInfo[] arguments)
                    // then just forward the call to Namespace.function
                    // also write the operator
                    // take care of the special operators


/*                    if (F.IsConverter(S))
                    { // is 'F' a converter (underscore constructor)?

                    }
                    else
                    {

                    }*/
                }
            }
        } // WriteFunctionShortcuts()
            


    } // end of class Shortcut
} // end of namepace G25.CG.Shared

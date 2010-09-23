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
    /// 
    /// Currently only for C# and Java, but C++ could also use it.
    /// </summary>
    public class Shortcut
    {

        /// <summary>
        /// Writes all shortcuts for 'type'.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for namespace.</param>
        /// <param name="cgd">Not used yet.</param>
        /// <param name="FT">Float point type of 'type'.</param>
        /// <param name="type">The type for which the function should be written.</param>
        public static void WriteFunctionShortcuts(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.VariableType type)
        {
            foreach (G25.fgs fgs in S.m_functions)
            {
                if (fgs.GetSupportedByPlugin() && (fgs.NbArguments >= 1) && (Array.IndexOf(fgs.FloatNames, FT.type) >= 0))
                {

                    bool computeMultivectorValue = false;
                    G25.CG.Shared.FuncArgInfo[] FAI = null;
                    try
                    {
                        FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(S, fgs, fgs.NbArguments, FT, "not set", computeMultivectorValue);
                    }
                    catch (Exception ex)
                    {
                        if ((type is G25.GMV) && (FT == S.m_floatTypes[0])) // only warn once
                            Console.WriteLine("Warning: cannot generate shortcut to " + fgs.ToString());
                        continue;
                    }

                    if (FAI[0].TypeName.Equals(type.GetName()))
                    {
                        WriteFunctionShortcut(SB, S, cgd, FT, type, fgs, FAI);
                    }
                }
            }
        } // end of function WriteFunctionShortcuts()
            

        private static FuncArgInfo[] getTail(FuncArgInfo[] FAI) {

            if (FAI.Length == 0) return null;
            else {
                FuncArgInfo[] tailFAI = new FuncArgInfo[FAI.Length-1];
                Array.Copy(FAI, 1, tailFAI, 0, tailFAI.Length); 
                return tailFAI;
            }
        }

        private static string getShortcutCall(Specification S, G25.fgs fgs, FuncArgInfo[] tailFAI)
        {
            StringBuilder SB = new StringBuilder();

            SB.Append(S.m_namespace);
            SB.Append(".");
            SB.Append(fgs.OutputName);
            SB.Append("(this");

            foreach (G25.CG.Shared.FuncArgInfo fai in tailFAI)
            {
                SB.Append(", ");
                SB.Append(fai.Name);
            }
            SB.Append(")");
            return SB.ToString();
        }


        /// <summary>
        /// Writes all shortcuts for 'type'.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Not used yet.</param>
        /// <param name="FT">Float point type of 'SMV'.</param>
        /// <param name="smv">The specialized multivector for which the struct should be written.</param>
        public static void WriteFunctionShortcut(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.VariableType type,
            G25.fgs fgs, FuncArgInfo[] FAI)
        {
            int nbTabs = 1;

            FuncArgInfo[] tailFAI = getTail(FAI);

            string shortcutCall = getShortcutCall(S, fgs, tailFAI);

            SB.AppendLine("");

            // output comment
            new Comment("shortcut to " + shortcutCall).Write(SB, S, nbTabs);

            bool inline = false;
            bool staticFunc = false;
            string returnType = FT.GetMangledName(S, fgs.ReturnTypeName);
            FuncArgInfo returnArgument = null;

            SB.Append('\t', nbTabs);
            Functions.WriteDeclaration(SB, S, cgd,
                inline, staticFunc, returnType, fgs.OutputName,
                returnArgument, tailFAI);
            SB.AppendLine(" {");

            SB.Append('\t', nbTabs+1);
            SB.Append("return ");
            SB.Append(shortcutCall);
            SB.AppendLine(";");

            SB.Append('\t', nbTabs);
            SB.AppendLine("}");

        }


    } // end of class Shortcut
} // end of namepace G25.CG.Shared

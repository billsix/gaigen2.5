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

namespace G25.CG.Shared.Func
{
    /// <summary>
    /// Generates code for dividing a variable by a scalar.
    /// To divide by a SMV/GMV, use gp(X, inverse(Y))
    /// 
    /// The function name should be <c>"div"</c>.
    /// </summary>
    public class Div : G25.CG.Shared.BaseFunctionGenerator
    {

        protected const int NB_ARGS = 2;
        protected bool m_gmvFunc; ///< is this a function over GMVs?
        protected Dictionary<string, string> m_funcName = new Dictionary<string, string>();  ///< generated function name with full mangling, etc
        protected G25.SMV m_smv = null; ///< if function over SMV, type goes here
        RefGA.Multivector m_returnValue = null;

        /// <summary>
        /// Checks if this FunctionGenerator can implement a certain function.
        /// </summary>
        /// <param name="S">The specification of the algebra.</param>
        /// <param name="F">The function to be implemented.</param>
        /// <returns>true if 'F' can be implemented</returns>
        public override bool CanImplement(Specification S, G25.fgs F)
        {
            return ((F.Name == "div") && (F.MatchNbArguments(2) &&
                S.IsFloatType(F.ArgumentTypeNames[1])));
        }

        /// <summary>
        /// If this FunctionGenerator can implement 'F', then this function should complete the (possible)
        /// blanks in 'F'. This means:
        ///  - Fill in F.m_returnTypeName if it is empty
        ///  - Fill in F.m_argumentTypeNames (and m_argumentVariableNames) if it is empty.
        /// </summary>
        public override void CompleteFGS()
        {
            FloatType FT = m_specification.GetFloatType(m_fgs.FloatNames[0]);

            // fill in ArgumentTypeNames
            if (m_fgs.ArgumentTypeNames.Length == 0)
                m_fgs.m_argumentTypeNames = new String[] { m_gmv.Name, FT.type };

            // init argument pointers from the completed typenames (language sensitive);
            m_fgs.InitArgumentPtrFromTypeNames(m_specification);

            // get all function info
            bool computeMultivectorValue = true;
            G25.CG.Shared.FuncArgInfo[] tmpFAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(m_specification, m_fgs, NB_ARGS, FT, m_specification.m_GMV.Name, computeMultivectorValue);

            m_gmvFunc = !tmpFAI[0].IsScalarOrSMV();

            // compute intermediate results, set return type
            if (m_gmvFunc) m_fgs.m_returnTypeName = m_gmv.Name; // div(gmv, scalar) = gmv
            else
            {
                m_smv = tmpFAI[0].Type as G25.SMV;

                RefGA.Multivector inputValue = tmpFAI[0].MultivectorValue[0];
                RefGA.Multivector divValue = RefGA.Symbolic.UnaryScalarOp.Inverse(new RefGA.Multivector(tmpFAI[1].Name));
                m_returnValue = RefGA.Multivector.gp(inputValue, divValue);

                // get name of return type
                if (m_fgs.m_returnTypeName.Length == 0)
                    m_fgs.m_returnTypeName = G25.CG.Shared.SpecializedReturnType.GetReturnType(m_specification, m_cgd, m_fgs, FT, m_returnValue).GetName();
            }
        }

        /// <summary>
        /// Should write the declaration/definitions of 'F' to StringBuffer 'SB', taking into account parameters specified in specification 'S'.
        /// </summary>
        public override void WriteFunction()
        {
            foreach (string floatName in m_fgs.FloatNames)
            {
                FloatType FT = m_specification.GetFloatType(floatName);

                bool computeMultivectorValue = true;
                G25.CG.Shared.FuncArgInfo[] FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(m_specification, m_fgs, NB_ARGS, FT, m_gmv.Name, computeMultivectorValue);

                // generate comment
                Comment comment = new Comment(
                    m_fgs.AddUserComment("Returns " + FAI[0].Name + " / " + FAI[1].Name));

                // if scalar or specialized: generate specialized function
                if (m_gmvFunc)
                {
                    m_funcName[FT.type] = G25.CG.Shared.GmvCASNparts.WriteDivFunction(m_specification, m_cgd, FT, m_G25M, FAI, m_fgs, comment, G25.CG.Shared.CANSparts.DIVCODETYPE.DIV);
                }
                else
                {// write simple specialized function:
                    // get symbolic result

                    // setup instructions
                    System.Collections.Generic.List<G25.CG.Shared.Instruction> I = new System.Collections.Generic.List<G25.CG.Shared.Instruction>();
                    {
                        int nbTabs = 1;
                        bool mustCast = false;
                        //bool unitPtr = !(FAI[0].Type is G25.FloatType);
                        I.Add(new G25.CG.Shared.ReturnInstruction(nbTabs, m_specification.GetType(m_fgs.m_returnTypeName), FT, mustCast, m_returnValue));

                    }

                    // because of lack of overloading, function names include names of argument types
                    G25.fgs CF = G25.CG.Shared.Util.AppendTypenameToFuncName(m_specification, FT, m_fgs, FAI);

                    m_funcName[FT.type] = CF.OutputName;

                    bool staticFunc = Functions.OutputStaticFunctions(m_specification);
                    G25.CG.Shared.Functions.WriteFunction(m_specification, m_cgd, CF, m_specification.m_inlineFunctions, staticFunc, CF.OutputName, FAI, I, comment);
                }
            }
        } // end of WriteFunction

        // used for testing:
        protected Dictionary<string, string> m_randomScalarFuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_randomBladeFuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_subtractGmvFuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_gpGmvFloatFuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_randomSmvFuncName = new Dictionary<string, string>();

        /// <summary>
        /// This function checks the dependencies for the _testing_ code of this function. If dependencies are
        /// missing, the function adds the required functions (this is done simply by asking for them . . .).
        /// </summary>
        public override void CheckTestingDepencies()
        {
            //bool returnTrueName = true;
            foreach (string floatName in m_fgs.FloatNames)
            {
                FloatType FT = m_specification.GetFloatType(floatName);

                m_randomScalarFuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "random_" + FT.type, new String[0], FT.type, FT, null);
                m_gpGmvFloatFuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "gp", new String[] { m_specification.m_GMV.Name, FT.type }, m_specification.m_GMV.Name, FT, null);
                m_subtractGmvFuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "subtract", new String[] { m_specification.m_GMV.Name, m_specification.m_GMV.Name }, m_specification.m_GMV.Name, FT, null);

                if (m_gmvFunc)
                {
                    m_randomBladeFuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "random_blade", new String[0], m_specification.m_GMV.Name, FT, null);
                }
                else if (m_smv != null)
                {
                    string defaultReturnTypeName = null;

                    m_randomSmvFuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "random_" + m_smv.Name, new String[0], defaultReturnTypeName, FT, null);
                }
            }
        }

        /// <summary>
        /// Writes the testing function for 'F' to 'm_defSB'.
        /// The generated function returns success (1) or failure (0).
        /// </summary>
        /// <returns>The list of name name of the int() function which tests the function.</returns>
        public override List<string> WriteTestFunction()
        {
            StringBuilder defSB = (m_specification.m_inlineFunctions) ? m_cgd.m_inlineDefSB : m_cgd.m_defSB;

            List<string> testFuncNames = new List<string>();


            foreach (string floatName in m_fgs.FloatNames)
            {
                FloatType FT = m_specification.GetFloatType(floatName);

                string testFuncName = Util.GetTestingFunctionName(m_specification, m_cgd, m_funcName[FT.type]);

                if (m_gmvFunc) // GMV test
                {
                    testFuncNames.Add(testFuncName);
                    System.Collections.Hashtable argTable = new System.Collections.Hashtable();
                    argTable["S"] = m_specification;
                    argTable["FT"] = FT;
                    argTable["gmv"] = m_specification.m_GMV;
                    argTable["gmvName"] = FT.GetMangledName(m_specification, m_specification.m_GMV.Name);
                    argTable["testFuncName"] = testFuncName;
                    argTable["targetFuncName"] = m_funcName[FT.type];

                    argTable["randomScalarFuncName"] = m_randomScalarFuncName[FT.type];
                    argTable["randomBladeFuncName"] = m_randomBladeFuncName[FT.type];
                    argTable["gpFuncName"] = m_gpGmvFloatFuncName[FT.type];
                    argTable["subtractGmvFuncName"] = m_subtractGmvFuncName[FT.type];

                    m_cgd.m_cog.EmitTemplate(defSB, "testDivGMV", argTable);
                }
                else if ((m_smv != null) && m_smv.CanConvertToGmv(m_specification))
                { // SMV test
                    testFuncNames.Add(testFuncName);
                    System.Collections.Hashtable argTable = new System.Collections.Hashtable();
                    argTable["S"] = m_specification;
                    argTable["FT"] = FT;
                    argTable["gmvName"] = FT.GetMangledName(m_specification, m_specification.m_GMV.Name);
                    argTable["testFuncName"] = testFuncName;
                    argTable["targetFuncName"] = m_funcName[FT.type];

                    argTable["smv"] = m_smv;
                    argTable["smvName"] = FT.GetMangledName(m_specification, m_smv.Name);
                    argTable["resultSmvName"] = FT.GetMangledName(m_specification, m_fgs.m_returnTypeName);

                    argTable["randomScalarFuncName"] = m_randomScalarFuncName[FT.type];
                    argTable["randomSmvFuncName"] = m_randomSmvFuncName[FT.type];
                    argTable["gpFuncName"] = m_gpGmvFloatFuncName[FT.type];
                    argTable["subtractGmvFuncName"] = m_subtractGmvFuncName[FT.type];

                    m_cgd.m_cog.EmitTemplate(defSB, "testDivSMV", argTable);
                }
            }

            return testFuncNames;
        } // end of WriteTestFunction()

    } // end of class Div
} // end of namespace G25.CG.Shared.Func


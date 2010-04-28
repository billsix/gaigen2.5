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
    /// Generates code for the (inverse) Hadamard product (coordinate-wise multiplication
    /// of two multivectors).
    /// 
    /// The function should be called <c>"hp"</c> (for regular product) or <c>"ihp"</c> (for inverse product).
    /// </summary>
    public class HP : G25.CG.Shared.BaseFunctionGenerator
    {

        // constants, intermediate results
        protected const int NB_ARGS = 2;
        protected bool m_gmvFunc; ///< is this a function over GMVs?
        protected RefGA.Multivector m_returnValue; ///< returned value (symbolic multivector)
        protected Dictionary<string, string> m_funcName = new Dictionary<string, string>();  ///< generated function name with full mangling, etc
        protected G25.SMV m_smv1 = null; ///< if function over SMV, type goes here
        protected G25.SMV m_smv2 = null; ///< if function over SMV, type goes here


        /// <returns>true when <c>F.Name == "ihp"</c>.</returns>
        public static bool IsInverse(G25.fgs F)
        {
            return F.Name == "ihp";
        }

        /// <summary>
        /// Checks if this FunctionGenerator can implement a certain function.
        /// </summary>
        /// <param name="S">The specification of the algebra.</param>
        /// <param name="F">The function to be implemented.</param>
        /// <returns>true if 'F' can be implemented</returns>
        public override bool CanImplement(Specification S, G25.fgs F)
        {
            return (((F.Name == "hp") || (F.Name == "ihp")) && (F.MatchNbArguments(NB_ARGS)) &&
                G25.CG.Shared.Functions.NotMixScalarGmv(S, F, NB_ARGS, S.m_GMV.Name) &&
                G25.CG.Shared.Functions.NotMixSmvGmv(S, F, NB_ARGS, S.m_GMV.Name) &&
                G25.CG.Shared.Functions.NotUseOm(S, F, NB_ARGS, S.m_GMV.Name));
        }

        /// <summary>
        /// If this FunctionGenerator can implement 'F', then this function should complete the (possible)
        /// blanks in 'F'. This means:
        ///  - Fill in F.m_returnTypeName if it is empty
        ///  - Fill in F.m_argumentTypeNames (and m_argumentVariableNames) if it is empty.
        /// </summary>
        public override void CompleteFGS()
        {
            // fill in ArgumentTypeNames
            if (m_fgs.ArgumentTypeNames.Length == 0)
                m_fgs.m_argumentTypeNames = new String[] { m_gmv.Name, m_gmv.Name };

            // init argument pointers from the completed typenames (language sensitive);
            m_fgs.InitArgumentPtrFromTypeNames(m_specification);

            // get all function info
            FloatType FT = m_specification.GetFloatType(m_fgs.FloatNames[0]);
            bool computeMultivectorValue = true;
            G25.CG.Shared.FuncArgInfo[] tmpFAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(m_specification, m_fgs, NB_ARGS, FT, m_specification.m_GMV.Name, computeMultivectorValue);

            m_gmvFunc = !(tmpFAI[0].IsScalarOrSMV() && tmpFAI[1].IsScalarOrSMV());
            m_smv1 = tmpFAI[0].Type as G25.SMV;
            m_smv2 = tmpFAI[1].Type as G25.SMV;


            // compute intermediate results, set return type
            if (m_gmvFunc) m_fgs.m_returnTypeName = m_gmv.Name; // gmv * gmv = gmv
            else
            {
                // compute return value
                if (IsInverse(m_fgs))
                {
                    m_returnValue = RefGA.Multivector.ihp(tmpFAI[0].MultivectorValue[0], tmpFAI[1].MultivectorValue[0]);
                }
                else
                {
                    m_returnValue = RefGA.Multivector.hp(tmpFAI[0].MultivectorValue[0], tmpFAI[1].MultivectorValue[0]);
                }

                bool getReturnType = (m_fgs.m_returnTypeName.Length == 0);

                // get name of return type
                if (getReturnType)
                    m_fgs.m_returnTypeName = G25.CG.Shared.SpecializedReturnType.GetReturnType(m_specification, m_cgd, m_fgs, FT, m_returnValue).GetName();

                // This code corrects the result for the signs of the basis blade, and loops until the return type converges.
                // The reason for this code is that the hadamard product is not well-defined when the basis blades of the operands and the result do not match.
                while (true) {
                    G25.VariableType returnType = m_specification.GetType(m_fgs.m_returnTypeName);
                    if (returnType is G25.SMV)
                    {
                        G25.SMV returnSMV = returnType as G25.SMV;
                        RefGA.Multivector unitReturnSmv = returnSMV.ToMultivectorValue();
                        RefGA.Multivector correctedReturnValue = RefGA.Multivector.ihp(m_returnValue, unitReturnSmv);
                        if (getReturnType)
                        {
                            string newReturnTypeName = G25.CG.Shared.SpecializedReturnType.GetReturnType(m_specification, m_cgd, m_fgs, FT, m_returnValue).GetName();
                            if (newReturnTypeName == m_fgs.m_returnTypeName)
                            {
                                m_returnValue = correctedReturnValue;
                                break;
                            }
                            else m_fgs.m_returnTypeName = newReturnTypeName;
                        }
                        else {
                            m_returnValue = correctedReturnValue;
                            break;
                        }
                    }
                    else break;
                }
            }
        }



        /// <summary>
        /// Writes the declaration/definitions of 'F' to StringBuffer 'SB', taking into account parameters specified in specification 'S'.
        /// </summary>
        public override void WriteFunction()
        {
            foreach (string floatName in m_fgs.FloatNames)
            {
                FloatType FT = m_specification.GetFloatType(floatName);

                bool computeMultivectorValue = true;
                G25.CG.Shared.FuncArgInfo[] FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(m_specification, m_fgs, NB_ARGS, FT, m_gmv.Name, computeMultivectorValue);

                // comment
                string comment = "/** " + m_fgs.AddUserComment("Returns Hadamard product (coordinate-wise multiplication) of " + FAI[0].TypeName + " and " + FAI[1].TypeName + ".") + " */";

                // if scalar or specialized: generate specialized function
                if (m_gmvFunc)
                {
                    G25.CG.Shared.CANSparts.ADD_SUB_HP_TYPE productType = (IsInverse(m_fgs))
                            ? G25.CG.Shared.CANSparts.ADD_SUB_HP_TYPE.IHP
                            : G25.CG.Shared.CANSparts.ADD_SUB_HP_TYPE.HP;
                    m_funcName[FT.type] = G25.CG.Shared.GmvCASNparts.WriteAddSubHpFunction(m_specification, m_cgd, FT, FAI, m_fgs, comment, productType);
                }
                else
                { // write simple specialized function:
                    // because of lack of overloading, function names include names of argument types
                    G25.fgs CF = G25.CG.Shared.Util.AppendTypenameToFuncName(m_specification, FT, m_fgs, FAI);

                    m_funcName[FT.type] = CF.OutputName;

                    // write out the function:
                    G25.CG.Shared.Functions.WriteSpecializedFunction(m_specification, m_cgd, CF, FT, FAI, m_returnValue, comment);
                }
            }
        } // end of WriteFunction

        // used for testing:
        protected Dictionary<string, string> m_randomScalarFuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_randomVersorFuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_subtractGmvFuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_randomSmv1FuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_randomSmv2FuncName = new Dictionary<string, string>();
        protected Dictionary<string, string> m_hpGmvFuncName = new Dictionary<string, string>();

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
                m_subtractGmvFuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "subtract", new String[] { m_specification.m_GMV.Name, m_specification.m_GMV.Name }, m_specification.m_GMV.Name, FT, null);

                if (m_gmvFunc)
                {
                    m_randomVersorFuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "random_versor", new String[0], m_specification.m_GMV.Name, FT, null);
                }
                else if ((m_smv1 != null) && (m_smv2 != null))
                {
                    string defaultReturnTypeName = null;

                    m_randomSmv1FuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "random_" + m_smv1.Name, new String[0], defaultReturnTypeName, FT, null);
                    m_randomSmv2FuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, "random_" + m_smv2.Name, new String[0], defaultReturnTypeName, FT, null);
                    m_hpGmvFuncName[FT.type] = G25.CG.Shared.Dependencies.GetDependency(m_specification, m_cgd, m_fgs.Name, new String[] { m_specification.m_GMV.Name, m_specification.m_GMV.Name }, m_specification.m_GMV.Name, FT, null);
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
                    argTable["randomVersorFuncName"] = m_randomVersorFuncName[FT.type];
                    argTable["subtractGmvFuncName"] = m_subtractGmvFuncName[FT.type];

                    m_cgd.m_cog.EmitTemplate(defSB, "testHadamardProductGMV", argTable);
                }
                else if ((m_smv1 != null) && (m_smv2 != null) && m_smv1.CanConvertToGmv(m_specification) && m_smv2.CanConvertToGmv(m_specification))
                { // SMV test
                    testFuncNames.Add(testFuncName);
                    System.Collections.Hashtable argTable = new System.Collections.Hashtable();
                    argTable["S"] = m_specification;
                    argTable["FT"] = FT;
                    argTable["gmv"] = m_specification.m_GMV;
                    argTable["gmvName"] = FT.GetMangledName(m_specification, m_specification.m_GMV.Name);
                    argTable["testFuncName"] = testFuncName;
                    argTable["targetFuncName"] = m_funcName[FT.type];

                    argTable["smv1"] = m_smv1;
                    argTable["smv1Name"] = FT.GetMangledName(m_specification, m_smv1.Name);
                    argTable["smv2"] = m_smv2;
                    argTable["smv2Name"] = FT.GetMangledName(m_specification, m_smv2.Name);
                    argTable["resultSmv"] = m_specification.GetType(m_fgs.m_returnTypeName);
                    argTable["resultSmvName"] = FT.GetMangledName(m_specification, m_fgs.m_returnTypeName);
                    argTable["inverseHadamard"] = IsInverse(m_fgs);

                    argTable["randomScalarFuncName"] = m_randomScalarFuncName[FT.type];
                    argTable["randomSmv1FuncName"] = m_randomSmv1FuncName[FT.type];
                    argTable["randomSmv2FuncName"] = m_randomSmv2FuncName[FT.type];
                    argTable["hpGmvFuncName"] = m_hpGmvFuncName[FT.type];
                    argTable["subtractGmvFuncName"] = m_subtractGmvFuncName[FT.type];

                    m_cgd.m_cog.EmitTemplate(defSB, "testHadamardProductSMV", argTable);
                }
            }

            return testFuncNames;
        } // end of WriteTestFunction()



    } // end of class HP
} // end of namespace G25.CG.Shared.Func


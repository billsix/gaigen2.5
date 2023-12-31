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

namespace G25.CG.CSJ
{
    public class SMV
    {
        /// <summary>
        /// Writes comments of a GMV class to 'SB'.
        /// </summary>
        /// <param name="SB">Where the comment goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT">Float point type of 'GMV'.</param>
        /// <param name="smv">The specialized multivector for which the comment should be written.</param>
        public static void WriteComment(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SMV smv)
        {
            bool emitCoordIndices = true;
            G25.CG.Shared.ClassComments.GetSmvComment(S, cgd, FT, smv, emitCoordIndices).Write(SB, S, 0);
        }

        /// <summary>
        /// Writes the SMV class to 'SB' (including comments).
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Not used yet.</param>
        /// <param name="FT">Float point type of 'SMV'.</param>
        /// <param name="smv">The specialized multivector for which the struct should be written.</param>
        public static void WriteMemberVariables(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SMV smv)
        {
            int nbTabs = 1;
            string accessModifier = Keywords.PackageProtectedAccessModifier(S);

            // individual coordinates or one array?
            if (S.m_coordStorage == COORD_STORAGE.VARIABLES)
            {
                for (int i = 0; i < smv.NbNonConstBasisBlade; i++)
                {
                    string comment = "The " + smv.NonConstBasisBlade(i).ToString(S.m_basisVectorNames) + " coordinate.";
                    new G25.CG.Shared.Comment(comment).Write(SB, S, nbTabs);
                    SB.AppendLine("\t" + accessModifier + " " + FT.type + " m_" + smv.GetCoordLangID(i, S) + ";");
                }
            }
            else
            {
                if (smv.NbNonConstBasisBlade > 0)
                {
                    // emit: float c[3]; // e1, e2, e3
                    string comment = " The coordinates (stored in an array).";
                    new G25.CG.Shared.Comment(comment).Write(SB, S, nbTabs);

                    SB.Append("\t" + accessModifier + " " + FT.type + "[] m_c = new " +  FT.type + "[" + smv.NbNonConstBasisBlade + "]; // ");
                    for (int i = 0; i < smv.NbNonConstBasisBlade; i++)
                    {
                        if (i > 0) SB.Append(", ");
                        SB.Append(smv.NonConstBasisBlade(i).ToString(S.m_basisVectorNames));
                    }
                    SB.AppendLine("");
                }
            }
        }


        /// <summary>
        /// Returns a symbolic name for the coordinate 'idx' of 'smv' which can be used in the
        /// output code.
        /// </summary>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="smv">The specialized multivector for which the coordinate indices should be written.</param>
        /// <param name="idx">The specialized multivector for which the coordinate indices should be written.</param>
        /// <param name="FT"></param>
        /// <returns>The symbol for the define for coordinate index 'idx' of 'smv'.</returns>
        public static string GetCoordIndexDefine(Specification S, FloatType FT, G25.SMV smv, int idx)
        {
            return smv.GetCoordLangID(idx, S, COORD_STORAGE.VARIABLES).ToUpper();
        }

        /// <summary>
        /// Writes the <c>defines</c> for indices of the smv struct to 'SB'. For example,  <c>define VECTOR_E1 0</c>.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names.</param>
        /// <param name="FT"></param>
        /// <param name="smv">The specialized multivector for which the coordinate indices should be written.</param>
        public static void WriteSMVcoordIndices(StringBuilder SB, Specification S, FloatType FT, G25.SMV smv)
        {
            string className = FT.GetMangledName(S, smv.Name);

            int nbTabs = 1;
            new G25.CG.Shared.Comment("Array indices of " + className + " coordinates.").Write(SB, S, nbTabs);


            string AccessModifier = Keywords.ConstAccessModifier(S);

            for (int i = 0; i < smv.NbNonConstBasisBlade; i++)
            {
                SB.AppendLine();
                new G25.CG.Shared.Comment("index of coordinate for " + smv.NonConstBasisBlade(i).ToString(S.m_basisVectorNames) + " in " + className).Write(SB, S, nbTabs);
                SB.AppendLine("\tpublic " + AccessModifier + " int " + GetCoordIndexDefine(S, FT, smv, i) + " = " + i + ";");
            }


        }

        /// <summary>
        /// Writes the <c>defines</c> for indices of the smv struct to 'SB'. For example,  <c>define VECTOR_E1 0</c>.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names.</param>
        /// <param name="FT"></param>
        /// <param name="smv">The specialized multivector for which the coordinate indices should be written.</param>
        public static void WriteCoordinateOrder(StringBuilder SB, Specification S, FloatType FT, G25.SMV smv)
        {

            string AccessModifier = Keywords.ConstAccessModifier(S);
            string typeName = G25.CG.Shared.SmvUtil.COORDINATE_ORDER_ENUM;
            string constantName = G25.CG.Shared.SmvUtil.GetCoordinateOrderConstant(S, smv);

            SB.AppendLine();

            int nbTabs = 1;
            new G25.CG.Shared.Comment("The order of coordinates (this is the type of the first argument of coordinate-handling functions.").Write(SB, S, nbTabs);

            string enumAccessModifier = (S.OutputCSharp()) ? "public" : "private";

            SB.AppendLine("\t" + enumAccessModifier + " enum " + typeName + " {");
            SB.AppendLine("\t\t" + constantName);
            SB.AppendLine("\t};");
            SB.AppendLine("\tpublic " + AccessModifier + " " + typeName + " " + constantName + " = " + typeName + "." + constantName + ";");


        }

/*        /// <summary>
        /// Writes the function to get the array of coordinates.
        /// Does nothing when coordinate storage is not array based or when the type has no variable coordinates.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names.</param>
        /// <param name="FT"></param>
        /// <param name="smv">The specialized multivector for which the coordinate indices should be written.</param>
        public static void WriteGetCoordinates(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SMV smv)
        {
            if (S.m_coordStorage != COORD_STORAGE.ARRAY) return;
            if (smv.NbNonConstBasisBlade == 0) return;

            //string typeName = G25.CG.Shared.SmvUtil.COORDINATE_ORDER_ENUM;
            string constantName = G25.CG.Shared.SmvUtil.GetCoordinateOrderConstant(S, smv);

            cgd.m_cog.EmitTemplate(SB, "SMVgetCoords", "FT=", FT, "COORD_TYPE_STRING=", constantName);
        }*/

        /// <summary>
        /// Writes the implementation of the multivector interface.
        /// </summary>
        /// <param name="SB"></param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteMultivectorInterface(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SMV smv)
        {
            string gmvName = FT.GetMangledName(S, S.m_GMV.Name);
            cgd.m_cog.EmitTemplate(SB, "SMVmvInterfaceImpl", "gmvName=", gmvName);
        }

        /// <summary>
        /// Writes constructors.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Not used yet.</param>
        /// <param name="FT">Float point type of 'SMV'.</param>
        /// <param name="smv">The specialized multivector for which the struct should be written.</param>
        public static void WriteConstructors(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SMV smv)
        {
            cgd.m_cog.EmitTemplate(SB, "SMVconstructors",
                "S=", S,
                "smv=", smv,
                "className=", FT.GetMangledName(S, smv.Name),
                "gmvClassName=", FT.GetMangledName(S, S.m_GMV.Name),
                "FT=", FT);
        }

        /// <summary>
        /// Writes constructors.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Not used yet.</param>
        /// <param name="FT">Float point type of 'SMV'.</param>
        /// <param name="smv">The specialized multivector for which the struct should be written.</param>
        public static void WriteSetFunctions(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SMV smv)
        {
            WriteSetZero(S, cgd, FT, smv);
            WriteSetScalar(S, cgd, FT, smv);
            WriteSetCoords(S, cgd, FT, smv);
            WriteSetArray(S, cgd, FT, smv);
            WriteSetCopy(S, cgd, FT, smv);
            WriteSetCopyCrossFloat(S, cgd, FT, smv);
            WriteGMVtoSMVcopy(S, cgd, FT, smv);
        }

        /// <summary>
        /// Writes a function to set an SMV struct to zero, for all floating point types.
        /// </summary>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        private static void WriteSetZero(Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT, G25.SMV smv)
        {
            //if (smv.NbNonConstBasisBlade == 0) return;

            cgd.m_defSB.AppendLine("");

            string funcName = GMV.GetSetFuncName(S);
            bool mustCast = false;

            string returnVarName = null;
            string dstName = G25.CG.Shared.SmvUtil.THIS;
            bool dstPtr = false;

            bool staticFunc = false;
            G25.CG.Shared.Functions.WriteAssignmentFunction(S, cgd,
                    S.m_inlineSet, staticFunc, "void", returnVarName, funcName, null, null, FT, mustCast, smv, dstName, dstPtr, new RefGA.Multivector(0.0));
        } // end of WriteSetZero()

        /// <summary>
        /// Writes a function to set an SMV struct to a scalar coordinate, for all floating point types which have a non-constant scalar coordinate.
        /// </summary>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteSetScalar(Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT, G25.SMV smv)
        {
            //if (smv.GetElementIdx(RefGA.BasisBlade.ONE) < 0) return; // if no scalar coordinate, continue

            cgd.m_defSB.AppendLine("");

            //string className = FT.GetMangledName(S, smv.Name);
            string funcName = GMV.GetSetFuncName(S);
            bool mustCast = false;

            System.Collections.ArrayList L = new System.Collections.ArrayList();
            const int NB_COORDS = 1;
            string[] argTypename = new String[NB_COORDS];
            string[] argName = new String[NB_COORDS];
            {
                RefGA.BasisBlade B = RefGA.BasisBlade.ONE;
                argTypename[0] = FT.type;
                argName[0] = "scalarVal";
                L.Add(new RefGA.BasisBlade(B.bitmap, B.scale, argName[0]));
            }
            RefGA.Multivector mvValue = new RefGA.Multivector(L);

            G25.fgs F = new G25.fgs(funcName, funcName, "", argTypename, argName, new String[] { FT.type }, null, null, null); // null, null = metricName, comment, options
            F.InitArgumentPtrFromTypeNames(S);
            bool computeMultivectorValue = false;
            G25.CG.Shared.FuncArgInfo[] FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(S, F, NB_COORDS, FT, null, computeMultivectorValue);

            string dstName = G25.CG.Shared.SmvUtil.THIS;
            bool dstPtr = false;

            bool staticFunc = false;
            G25.CG.Shared.Functions.WriteAssignmentFunction(S, cgd,
                S.m_inlineSet, staticFunc, "void", null, funcName, null, FAI, FT, mustCast, smv, dstName, dstPtr, mvValue);
        }

        /// <summary>
        /// Writes a function to set an SMV class to specified coordinates, for all floating point types.
        /// </summary>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteSetCoords(Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT, G25.SMV smv)
        {
            //if (smv.NbNonConstBasisBlade == 0) return;

            cgd.m_defSB.AppendLine("");

            //string className = FT.GetMangledName(S, smv.Name);
            string funcName = GMV.GetSetFuncName(S);
            bool mustCast = false;

            System.Collections.ArrayList L = new System.Collections.ArrayList();
            int NB_ARGS = 1 + smv.NbNonConstBasisBlade;
            string[] argTypename = new String[NB_ARGS];
            string[] argName = new String[NB_ARGS];
            argTypename[0] = G25.CG.Shared.SmvUtil.COORDINATE_ORDER_ENUM;
            argName[0] = "co";
            for (int i = 0; i < smv.NbNonConstBasisBlade; i++)
            {
                RefGA.BasisBlade B = smv.NonConstBasisBlade(i);
                argTypename[i + 1] = FT.type;
                string coordStr = "_" + smv.GetCoordLangID(i, S, COORD_STORAGE.VARIABLES);
                argName[i + 1] = coordStr;
                L.Add(new RefGA.BasisBlade(B.bitmap, B.scale, coordStr));
            }
            RefGA.Multivector mvValue = new RefGA.Multivector(L);


            G25.fgs F = new G25.fgs(funcName, funcName, "", argTypename, argName, new String[] { FT.type }, null, null, null); // null, null, null = metricName, comment, options
            F.InitArgumentPtrFromTypeNames(S);
            bool computeMultivectorValue = false;
            G25.CG.Shared.FuncArgInfo[] FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(S, F, NB_ARGS, FT, null, computeMultivectorValue);

            string dstName = G25.CG.Shared.SmvUtil.THIS;
            bool dstPtr = false;

            bool staticFunc = false;
            G25.CG.Shared.Functions.WriteAssignmentFunction(S, cgd,
                S.m_inlineSet, staticFunc, "void", null, funcName, null, FAI, FT, mustCast, smv, dstName, dstPtr, mvValue);
        } // end of WriteSetCoords()

        /// <summary>
        /// Writes a function to set an SMV struct to an array of specified coordinates, for all floating point types.
        /// </summary>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteSetArray(Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT, G25.SMV smv)
        {
            //if (smv.NbNonConstBasisBlade == 0) return;

            cgd.m_defSB.AppendLine("");

            //string className = FT.GetMangledName(S, smv.Name);
            string funcName = GMV.GetSetFuncName(S);
            bool mustCast = false;

            string[] argTypename = new string[2] { G25.CG.Shared.SmvUtil.COORDINATE_ORDER_ENUM, FT.type };
            string[] argName = new string[2] { "co", "A" };

            System.Collections.ArrayList L = new System.Collections.ArrayList();
            for (int i = 0; i < smv.NbNonConstBasisBlade; i++)
            {
                RefGA.BasisBlade B = smv.NonConstBasisBlade(i);
                String coordStr = argName[1] + "[" + i + "]";
                L.Add(new RefGA.BasisBlade(B.bitmap, B.scale, coordStr));
            }
            RefGA.Multivector mvValue = new RefGA.Multivector(L);

            G25.fgs F = new G25.fgs(funcName, funcName, "", argTypename, argName, new string[] { FT.type }, null, null, null); // null, null, null = metricName, comment, options
            F.InitArgumentPtrFromTypeNames(S);
            F.SetArgumentArr(1, true); // second argument is an array

            bool computeMultivectorValue = false;
            int NB_ARGS = 2; // enum + one array of coordinates
            G25.CG.Shared.FuncArgInfo[] FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(S, F, NB_ARGS, FT, null, computeMultivectorValue);

            string dstName = G25.CG.Shared.SmvUtil.THIS;
            bool dstPtr = false;

            bool staticFunc = false;
            G25.CG.Shared.Functions.WriteAssignmentFunction(S, cgd,
                S.m_inlineSet, staticFunc, "void", null, funcName, null, FAI, FT, mustCast, smv, dstName, dstPtr, mvValue);
        } // end of WriteSetArray()

        /// <summary>
        /// Writes a function to copy the value of one SMV struct to another, for all floating point types.
        /// </summary>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteSetCopy(Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT, G25.SMV smv)
        {
            cgd.m_defSB.AppendLine("");

            //string className = FT.GetMangledName(S, smv.Name);
            string funcName = GMV.GetSetFuncName(S);
            bool mustCast = false;

            G25.fgs F = new G25.fgs(funcName, funcName, "", new string[] { smv.Name }, null, new string[] { FT.type }, null, null, null); // null, null, null = metricName, comment, options
            F.InitArgumentPtrFromTypeNames(S);
            bool computeMultivectorValue = false;
            int nbArgs = 1;
            G25.CG.Shared.FuncArgInfo[] FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(S, F, nbArgs, FT, null, computeMultivectorValue);


            RefGA.Multivector value = G25.CG.Shared.Symbolic.SMVtoSymbolicMultivector(S, smv, FAI[0].Name, FAI[0].Pointer);

            string dstName = G25.CG.Shared.SmvUtil.THIS;
            bool dstPtr = false;

            bool staticFunc = false;
            G25.CG.Shared.Functions.WriteAssignmentFunction(S, cgd,
                S.m_inlineSet, staticFunc, "void", null, funcName, null, FAI, FT, mustCast, smv, dstName, dstPtr, value);
        } // end of WriteSetCopy()

        /// <summary>
        /// Writes a function to copy the value of one SMV struct to another with a different floating point type, for all combinations of floating point types.
        /// </summary>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteSetCopyCrossFloat(Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType dstFT, G25.SMV smv)
        {

            foreach (G25.FloatType srcFT in S.m_floatTypes)
            {
                if (srcFT.type == dstFT.type) continue;

                cgd.m_defSB.AppendLine("");

                //string srcClassName = srcFT.GetMangledName(smv.Name);
                //string dstClassName = dstFT.GetMangledName(S, smv.Name);
                string funcName = GMV.GetSetFuncName(S);
                bool mustCast = dstFT.MustCastIfAssigned(S, srcFT);

                G25.fgs F = new G25.fgs(funcName, funcName, "", new String[] { smv.Name }, null, new String[] { srcFT.type }, null, null, null); // null, null, null = metricName, comment, options
                F.InitArgumentPtrFromTypeNames(S);
                bool computeMultivectorValue = false;
                int nbArgs = 1;
                G25.CG.Shared.FuncArgInfo[] FAI = G25.CG.Shared.FuncArgInfo.GetAllFuncArgInfo(S, F, nbArgs, srcFT, null, computeMultivectorValue);

                RefGA.Multivector value = G25.CG.Shared.Symbolic.SMVtoSymbolicMultivector(S, smv, FAI[0].Name, FAI[0].Pointer);

                string dstName = G25.CG.Shared.SmvUtil.THIS;
                bool dstPtr = false;

                bool staticFunc = false;
                G25.CG.Shared.Functions.WriteAssignmentFunction(S, cgd,
                    S.m_inlineSet, staticFunc, "void", null, funcName, null, FAI, dstFT, mustCast, smv, dstName, dstPtr, value);
            }
        } // end of WriteSetCopyCrossFloat()

        /// <summary>
        /// Writes functions to copy GMVs to SMVs
        /// </summary>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteGMVtoSMVcopy(Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT, G25.SMV smv)
        {
            StringBuilder defSB = cgd.m_defSB;

            G25.GMV gmv = S.m_GMV;
            string srcClassName = FT.GetMangledName(S, gmv.Name);

            //string dstClassName = FT.GetMangledName(S, smv.Name);

            bool dstPtr = false;
            string[] smvAccessStr = G25.CG.Shared.CodeUtil.GetAccessStr(S, smv, G25.CG.Shared.SmvUtil.THIS, dstPtr);

            string funcName = GMV.GetSetFuncName(S);

            string FINAL = (S.OutputJava()) ? "final " : "";
            string funcDecl = "\tpublic " + FINAL + "void " + funcName + "(" + FINAL + srcClassName + " src)";

            defSB.Append(funcDecl);
            {
                defSB.AppendLine(" {");

                // get a dictionary which tells you for each basis blade of 'smv' where it is in 'gmv'
                // A dictionary from <smv group, smv element> to <gmv group, gmv element>
                Dictionary<Tuple<int, int>, Tuple<int, int>> D = G25.MV.GetCoordMap(smv, gmv);

                // what is the highest group of the 'gmv' that must be (partially) copied to the 'smv'
                int highestGroup = -1;
                foreach (KeyValuePair<Tuple<int, int>, Tuple<int, int>> KVP in D)
                    if (KVP.Value.Value1 > highestGroup) highestGroup = KVP.Value.Value1;

                // generate code for each group
                for (int g = 0; g <= highestGroup; g++)
                {
                    // determine if group 'g' is to be copied to smv:
                    bool groupIsUsedBySMV = false;
                    foreach (KeyValuePair<Tuple<int, int>, Tuple<int, int>> KVP in D)
                    {
                        // KVP.Key = SMV<group, element>
                        // KVP.Value = GMV<group, element>
                        if (KVP.Value.Value1 == g)
                        {
                            if (!smv.IsCoordinateConstant(KVP.Key.Value2))
                            {
                                groupIsUsedBySMV = true;
                                break;
                            }
                        }
                    }

                    // if group is present in GMV:
                    if (groupIsUsedBySMV)
                    {
                        defSB.AppendLine("\t\tif (src.c()[" + g + "] != null) {");
                        defSB.AppendLine("\t\t\t" + FT.type + "[] ptr = src.c()[" + g + "];");
                        bool mustCast = false;
                        bool srcPtr = true;
                        int nbTabs = 3;
                        RefGA.Multivector[] value = G25.CG.Shared.Symbolic.GMVtoSymbolicMultivector(S, gmv, "ptr", srcPtr, g);
                        bool writeZeros = false;
                        string str = G25.CG.Shared.CodeUtil.GenerateSMVassignmentCode(S, FT, mustCast, smv, G25.CG.Shared.SmvUtil.THIS, dstPtr, value[g], nbTabs, writeZeros);
                        defSB.Append(str);
                        defSB.AppendLine("\t\t}");

                        defSB.AppendLine("\t\telse {");
                        foreach (KeyValuePair<Tuple<int, int>, Tuple<int, int>> KVP in D)
                        {
                            if ((KVP.Value.Value1 == g) && (!smv.IsCoordinateConstant(KVP.Key.Value2)))
                            {
                                // translate KVP.Key.Value2 to non-const idx, because the accessStrs are only about non-const blades blades!
                                int bladeIdx = smv.BladeIdxToNonConstBladeIdx(KVP.Key.Value2);
                                defSB.AppendLine("\t\t\t" + smvAccessStr[bladeIdx] + " = " + FT.DoubleToString(S, 0.0) + ";");
                            }
                        }
                        defSB.AppendLine("\t\t}");
                    }
                }
                defSB.AppendLine("\t}");
            }
        } // end of WriteGMVtoSMVcopy()

        /// <summary>
        /// Writes code for abs largest coordinate
        /// </summary>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteLargestCoordinateFunctions(Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT, G25.SMV smv)
        {
            StringBuilder defSB = cgd.m_defSB;
            defSB.AppendLine("");

            const string smvName = G25.CG.Shared.SmvUtil.THIS;
            const bool ptr = false;

            string fabsFunc = G25.CG.Shared.CodeUtil.OpNameToLangString(S, FT, RefGA.Symbolic.UnaryScalarOp.ABS);

            string[] AS = G25.CG.Shared.CodeUtil.GetAccessStr(S, smv, smvName, ptr);

            RefGA.BasisBlade maxBasisBlade = smv.AbsoluteLargestConstantBasisBlade();

            //string className = FT.GetMangledName(S, smv.Name);

            for (int _returnBitmap = 0; _returnBitmap <= 1; _returnBitmap++)
            {
                bool returnBitmap = (_returnBitmap != 0);

                // write comment
                int nbTabs = 1;
                if (returnBitmap)
                    new G25.CG.Shared.Comment("Returns the absolute largest coordinate,\nand the corresponding basis blade bitmap.").Write(defSB, S, nbTabs);
                else new G25.CG.Shared.Comment("Returns the absolute largest coordinate.").Write(defSB, S, nbTabs);

                string funcName = Util.GetFunctionName(S, ((returnBitmap) ? "largestBasisBlade" : "largestCoordinate"));

                string funcDecl;
                if ((S.OutputCSharp()) && returnBitmap)
                {
                    funcDecl = FT.type + " " + funcName + "(int bm) ";
                }
                else if ((S.OutputJava()) && returnBitmap)
                {
                    funcDecl = FT.type + "[] " + funcName + "() ";
                }
                else
                {
                    funcDecl = FT.type + " " + funcName + "()";
                }

                string FINAL = (S.OutputJava()) ? "final " : "";
                defSB.Append("\tpublic " + FINAL + funcDecl);
                {
                    defSB.AppendLine(" {");

                    if ((S.OutputJava()) && returnBitmap)
                        defSB.AppendLine("\t\tint bm;");

                    int startIdx = 0;
                    if (maxBasisBlade != null)
                    {
                        defSB.AppendLine("\t\t" + FT.type + " maxValue = " + FT.DoubleToString(S, Math.Abs(maxBasisBlade.scale)) + ";");
                        if (returnBitmap)
                            defSB.AppendLine("\t\tbm = " + maxBasisBlade.bitmap + ";");
                    }
                    else
                    {
                        defSB.AppendLine("\t\t" + FT.type + " maxValue = " + fabsFunc + "(" + AS[0] + ");");
                        if (returnBitmap)
                            defSB.AppendLine("\t\tbm = 0;");
                        startIdx = 1;
                    }

                    for (int c = startIdx; c < smv.NbNonConstBasisBlade; c++)
                    {
                        defSB.Append("\t\tif (" + fabsFunc + "(" + AS[c] + ") > maxValue) { maxValue = " + fabsFunc + "(" + AS[c] + "); ");
                        if (returnBitmap) defSB.Append("bm = " + smv.NonConstBasisBlade(c).bitmap + "; ");
                        defSB.AppendLine("}");
                    }

                    if ((S.OutputJava()) && returnBitmap)
                    {
                        defSB.AppendLine("\t\treturn new " + FT.type + "[]{maxValue, (" + FT.type + ")bm};");
                    }
                    else
                    {
                        defSB.AppendLine("\t\treturn maxValue;");
                    }
                    defSB.AppendLine("\t}");
                }
            }
        } // end of WriteLargestCoordinateFunctions()

        /// <summary>
        /// Writes getters and setters for the SMV coordinates..
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Not used yet.</param>
        /// <param name="FT">Float point type of 'SMV'.</param>
        /// <param name="smv">The specialized multivector for which the struct should be written.</param>
        public static void WriteGetSetCoord(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SMV smv)
        {
            int nbTabs = 1;
            string className = FT.GetMangledName(S, smv.Name);

            // for variable coordinates:
            for (int i = 0; i < smv.NbNonConstBasisBlade; i++)
            {
                RefGA.BasisBlade B = smv.NonConstBasisBlade(i);
                string name = smv.NonConstBasisBlade(i).ToLangString(S.m_basisVectorNames);
                string accessName = G25.CG.Shared.SmvUtil.GetCoordAccessString(S, smv, i);

                // get
                string getComment = "Returns the " + B.ToString(S.m_basisVectorNames) + " coordinate.";
                new G25.CG.Shared.Comment(getComment).Write(SB, S, nbTabs);
                SB.AppendLine("\t" + Keywords.PublicAccessModifier(S) + " " + FT.type + " " + G25.CG.Shared.Main.GETTER_PREFIX + name + "() { return " + accessName + ";}");

                // set
                string setComment = "Sets the " + B.ToString(S.m_basisVectorNames) + " coordinate.";
                new G25.CG.Shared.Comment(setComment).Write(SB, S, nbTabs);
                SB.AppendLine("\t" + Keywords.PublicAccessModifier(S) + " void " + G25.CG.Shared.Main.SETTER_PREFIX + name + "(" + FT.type + " " + name + ") { " + accessName + " = " + name + ";}");
            }

            // for constant coordinates:
            for (int i = 0; i < smv.NbConstBasisBlade; i++)
            {
                RefGA.BasisBlade B = smv.ConstBasisBlade(i);
                // get
                string getComment = "Returns the " + B.ToString(S.m_basisVectorNames) + " coordinate.";
                new G25.CG.Shared.Comment(getComment).Write(SB, S, nbTabs);
                SB.AppendLine("\t" + Keywords.PublicAccessModifier(S) + " " + FT.type + " " + G25.CG.Shared.Main.GETTER_PREFIX + B.ToLangString(S.m_basisVectorNames) + "() { return " + FT.DoubleToString(S, smv.ConstBasisBladeValue(i)) + ";}");
            }

            // write a getter for the scalar which returns 0 if no scalar coordinate is present
            if (smv.GetElementIdx(RefGA.BasisBlade.ONE) < 0)
            {
                RefGA.BasisBlade B = RefGA.BasisBlade.ONE;
                string getComment = "Returns the scalar coordinate (which is always 0).";
                new G25.CG.Shared.Comment(getComment).Write(SB, S, nbTabs);
                SB.AppendLine("\t" + Keywords.PublicAccessModifier(S) + " " + FT.type + " " + G25.CG.Shared.Main.GETTER_PREFIX + B.ToLangString(S.m_basisVectorNames) + "() { return " + FT.DoubleToString(S, 0.0) + ";}");
            }

            // getter for the coordinates (stored in array)
            if ((S.m_coordStorage == COORD_STORAGE.ARRAY) && (smv.NbNonConstBasisBlade > 0))
            {
                string constantName = G25.CG.Shared.SmvUtil.GetCoordinateOrderConstant(S, smv);
                string COORD_ORDER = "coordOrder";

                new G25.CG.Shared.Comment("Returns array of coordinates.").
                    SetParamComment(COORD_ORDER, "pass the value '" + className + "." + constantName + "'").
                    Write(SB, S, nbTabs);
                SB.AppendLine("\t" + Keywords.PublicAccessModifier(S) + " " + FT.type + "[] c(" + G25.CG.Shared.SmvUtil.COORDINATE_ORDER_ENUM + " " + COORD_ORDER + ") { return m_c;}");
            }
        }

    } // end of class SMV
} // end of namespace G25.CG.CSJ

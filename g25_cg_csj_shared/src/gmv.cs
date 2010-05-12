﻿// This program is free software; you can redistribute it and/or
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
    public class GMV
    {
        public const string SMV_TYPE = "SmvType";
        public const string GROUP_BITMAP = "GroupBitmap";
        private const string ALLOCATE_GROUPS_CSHARP = "AllocateGroups";
        private const string ALLOCATE_GROUPS_JAVA = "allocateGroups";
        private const string SET_RESERVE_GROUP_CSHARP = "ReserveGroup_";
        private const string SET_RESERVE_GROUP_JAVA = "reserveGroups_";

        public static string GetAllocateGroupsString(Specification S)
        {
            return (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP)
                ? ALLOCATE_GROUPS_CSHARP
                : ALLOCATE_GROUPS_JAVA;
        }

        public static string GetReserveGroupString(Specification S, int groupIdx)
        {
            return ((S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP)
                ? SET_RESERVE_GROUP_CSHARP
                : SET_RESERVE_GROUP_JAVA) + groupIdx;
        }

        /// <summary>
        /// Writes functions to set the GMV types to zero.
        /// </summary>
        /// <param name="SB">Where the output goes.</param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteSetZero(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT)
        {
            G25.GMV gmv = S.m_GMV;

            int nbTabs = 1;
            G25.CG.Shared.Util.WriteFunctionComment(SB, S, nbTabs, "sets this to 0.", null, null);

            string className = FT.GetMangledName(S, gmv.Name);
            string funcName = (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP) ? "Set" : "set";

            string funcDecl = "\tpublic void " + funcName + "()";

            SB.Append(funcDecl);
            SB.AppendLine(" {");
            for (int g = 0; g < gmv.NbGroups; g++)
                SB.AppendLine("\t\tm_c[(int)" + GroupBitmap.GetGroupBitmapCode(g) + "] = null;");

            if (S.m_reportUsage)
            {
                SB.AppendLine("\t\tm_t = " + G25.CG.CSJ.GMV.SMV_TYPE + "." + G25.CG.Shared.ReportUsage.GetSpecializedConstantName(S, gmv.Name) + ";");
            }

            SB.AppendLine("\t}");
        }

        /// <summary>
        /// Writes functions to set the GMV types to scalar value.
        /// </summary>
        /// <param name="SB">Where the output goes.</param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteSetScalar(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT)
        {
            int nbTabs = 1;
            G25.CG.Shared.Util.WriteFunctionComment(SB, S, nbTabs, "sets this to scalar value.", null, null);

            G25.GMV gmv = S.m_GMV;
            string className = FT.GetMangledName(S, gmv.Name);
            string funcName = (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP) ? "Set" : "set";

            string funcDecl = "\tpublic void " + funcName + "(" + FT.type + " val)";

            SB.Append(funcDecl);
            SB.AppendLine(" {");
            SB.AppendLine("\t\t" + GetAllocateGroupsString(S) + "(GroupBitmap.GROUP_" + (1 << gmv.GetGroupIdx(RefGA.BasisBlade.ONE)) + ");");
            SB.AppendLine("\t\tm_c[0][0] = val;");

            if (S.m_reportUsage)
            {
                SB.AppendLine("\t\tm_t = " + G25.CG.CSJ.GMV.SMV_TYPE + "." + G25.CG.Shared.ReportUsage.GetSpecializedConstantName(S, FT.type) + ";");
            }

            SB.AppendLine("\t}");
        }

        /// <summary>
        /// Writes functions to set the GMV types by compressed array value.
        /// </summary>
        /// <param name="SB">Where the output goes.</param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteSetCompressedArray(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT)
        {
            int nbTabs = 1;
            List<Tuple<string, string>> paramComments = new List<Tuple<string, string>> {
                new Tuple<string, string>("gu", "bitwise or of the GROUPs and GRADEs which are present in 'arr'."),
                new Tuple<string, string>("arr", "compressed coordinates.")
            };
            G25.CG.Shared.Util.WriteFunctionComment(SB, S, nbTabs, "sets this coordinates in 'arr'.", paramComments, null);

            G25.GMV gmv = S.m_GMV;

            string className = FT.GetMangledName(S, gmv.Name);
            string funcName = (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP) ? "Set" : "set";

            string groupBitmapStr = (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP) ? GROUP_BITMAP : "int";

            string funcDecl = "\tpublic void " + funcName + "(" + groupBitmapStr + " gu, " + FT.type + "[] arr)";

            SB.Append(funcDecl);
            SB.AppendLine(" {");

            SB.AppendLine("\t\t" + GetAllocateGroupsString(S) + "(gu);");
            if (S.m_reportUsage)
            {
                SB.AppendLine("\t\tm_t = " + G25.CG.CSJ.GMV.SMV_TYPE + "." + G25.CG.Shared.ReportUsage.GetSpecializedConstantName(S, gmv.Name) + ";");
            }

            SB.AppendLine("\t\tint idx = 0;");

            for (int g = 0; g < S.m_GMV.NbGroups; g++)
            {
                SB.AppendLine("\t\tif ((gu & " + GroupBitmap.GetGroupBitmapCode(g) + ") != 0) {");
                SB.AppendLine("\t\t\tfor (int i = 0; i < " + S.m_GMV.Group(g).Length + "; i++)");
                SB.AppendLine("\t\t\t\tm_c[" + g + "][i] = arr[idx + i];");
                SB.AppendLine("\t\t\tidx += " + S.m_GMV.Group(g).Length + ";");
                SB.AppendLine("\t\t}");
            }


            SB.AppendLine("\t}");
        }

        /// <summary>
        /// Writes functions to set the GMV types by expanded array value.
        /// </summary>
        /// <param name="SB">Where the output goes.</param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteSetExpandedArray(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT)
        {
            int nbTabs = 1;
            List<Tuple<string, string>> paramComments = new List<Tuple<string, string>> {
                new Tuple<string, string>("arr", "coordinates.")
            };
            G25.CG.Shared.Util.WriteFunctionComment(SB, S, nbTabs, "sets this coordinates in 'arr'. \n'arr' is kept, so changes to 'arr' will be reflected in the value of this multivector. Make sure 'arr' has length " + S.m_GMV.NbGroups + " and each subarray has the length of the respective group/grade", paramComments, null);

            G25.GMV gmv = S.m_GMV;

            string className = FT.GetMangledName(S, gmv.Name);
            string funcName = (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP) ? "Set" : "set";

            string groupBitmapStr = (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP) ? GROUP_BITMAP : "int";

            string funcDecl = "\tpublic void " + funcName + "(" + FT.type + "[][] arr)";

            SB.Append(funcDecl);
            SB.AppendLine(" {");

            SB.AppendLine("\t\tm_c = arr;");
            if (S.m_reportUsage)
            {
                SB.AppendLine("\t\tm_t = " + G25.CG.CSJ.GMV.SMV_TYPE + "." + G25.CG.Shared.ReportUsage.GetSpecializedConstantName(S, gmv.Name) + ";");
            }
            SB.AppendLine("\t}");
        }

        /// <summary>
        /// Writes functions to copy GMVs from one float type to another.
        /// </summary>
        /// <param name="SB">Where the output goes.</param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="dstFT"></param>
        public static void WriteGMVtoGMVcopy(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType dstFT)
        {
            G25.GMV gmv = S.m_GMV;
            foreach (G25.FloatType srcFT in S.m_floatTypes)
            {
                string srcClassName = srcFT.GetMangledName(S, gmv.Name);

                string dstClassName = dstFT.GetMangledName(S, gmv.Name);

                string funcName = (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP) ? "Set" : "set";

                string funcDecl = "\tpublic void " + funcName + "(" + srcClassName + " src)";

                int nbTabs = 1;
                G25.CG.Shared.Util.WriteFunctionComment(SB, S, nbTabs, "sets this to multivector value.", null, null);

                SB.Append(funcDecl);
                SB.AppendLine(" {");

                SB.AppendLine("\t\t" + GetAllocateGroupsString(S) + "(src.gu());");

                for (int g = 0; g < gmv.NbGroups; g++)
                {
                    SB.AppendLine("\t\tif (m_c[" + g + "] != null) {");
                    if (dstFT == srcFT)
                    {
                        SB.Append("\t\t\t" + G25.CG.Shared.Util.GetCopyCode(S, dstFT, "src.m_c[" + g + "]", "m_c[" + g + "]", gmv.Group(g).Length));
                    }
                    else
                    {
                        SB.AppendLine("\t\t\tfor (int i = 0; i < " + gmv.Group(g).Length + "; i++)");
                        SB.AppendLine("\t\t\t\tm_c[" + g + "][i] = (" + dstFT.type + ")src.m_c[" + g + "][i];");
                    }

                    SB.AppendLine("\t\t}");
                }

                if (S.m_reportUsage)
                {
                    SB.AppendLine("\t\tm_t = src.m_t;");
                }

                SB.AppendLine("\t}");
            }
        }

        /// <summary>
        /// Writes functions to copy SMVs to GMVs
        /// </summary>
        /// <param name="SB">Where the output goes.</param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteSMVtoGMVcopy(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT)
        {
            int nbTabs;
            G25.GMV gmv = S.m_GMV;
            bool gmvParityPure = (S.m_GMV.MemoryAllocationMethod == G25.GMV.MEM_ALLOC_METHOD.PARITY_PURE);

            string dstClassName = FT.GetMangledName(S, gmv.Name);
            for (int s = 0; s < S.m_SMV.Count; s++)
            {
                G25.SMV smv = S.m_SMV[s];

                // do not generate converter if the GMV cannot hold the type
                if (gmvParityPure && (!smv.IsParityPure())) continue;


                string srcClassName = FT.GetMangledName(S, smv.Name);

                SB.AppendLine();

                nbTabs = 1;
                G25.CG.Shared.Util.WriteFunctionComment(SB, S, nbTabs, "sets this to " + srcClassName + " value.", null, null);

                string funcName = (S.m_outputLanguage == OUTPUT_LANGUAGE.CSHARP) ? "Set" : "set";

                string funcDecl = "\tpublic void " + funcName + "(" + srcClassName + " src)";

                SB.Append(funcDecl);

                {
                    SB.AppendLine(" {");

                    // get a dictionary which tells you for each basis blade of 'gmv' where it is in 'smv'
                    Dictionary<Tuple<int, int>, Tuple<int, int>> D = G25.MV.GetCoordMap(smv, gmv);

                    // convert SMV to symbolic Multivector:
                    bool smvPtr = false;
                    RefGA.Multivector value = G25.CG.Shared.Symbolic.SMVtoSymbolicMultivector(S, smv, "src", smvPtr);

                    // find out which groups are present
                    StringBuilder guSB = new StringBuilder();
                    int gu = 0;
                    foreach (KeyValuePair<Tuple<int, int>, Tuple<int, int>> KVP in D)
                    {
                        int bit = 1 << KVP.Value.Value1;
                        if ((gu & bit) == 0)
                        {
                            gu |= 1 << KVP.Value.Value1;
                            if (guSB.Length > 0) guSB.Append("|");
                            guSB.Append("GroupBitmap.GROUP_" + KVP.Value.Value1);
                        }
                    }

                    // generate the code to set group usage:
                    SB.AppendLine("\t\t" + GetAllocateGroupsString(S) + "(" + guSB.ToString() + ");");

                    // a helper pointer 
                    string dstArrName = "ptr";
                    SB.AppendLine("\t\t" + FT.type + "[] " + dstArrName + ";");


                    // for each used group, generate the assignment code
                    for (int g = 0; (1 << g) <= gu; g++)
                    {
                        if (((1 << g) & gu) != 0)
                        {
                            SB.AppendLine();
                            SB.AppendLine("\t\tptr = m_c[" + g + "];");

                            int dstBaseIdx = 0;
                            bool mustCast = false;
                            nbTabs = 2;
                            bool writeZeros = true;
                            string str = G25.CG.Shared.CodeUtil.GenerateGMVassignmentCode(S, FT, mustCast, gmv, dstArrName, g, dstBaseIdx, value, nbTabs, writeZeros);
                            SB.Append(str);
                        }

                    }

                    if (S.m_reportUsage)
                    {
                        SB.AppendLine("\t\tm_t = " + G25.CG.CSJ.GMV.SMV_TYPE + "." + G25.CG.Shared.ReportUsage.GetSpecializedConstantName(S, smv.Name) + ";");
                    }

                    SB.AppendLine("\t}");
                }
            } // end of loop over all SMVs
        } // end of WriteGMVtoSMVcopy()


        private static void WriteGetCoordFunction(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT, 
            string gmvTypeName, int groupIdx, int elementIdx, RefGA.BasisBlade B)
        {
            string bladeName = B.ToLangString(S.m_basisVectorNames);

            string funcName = G25.CG.Shared.Main.GETTER_PREFIX + bladeName;

            string funcDecl = "\t" + FT.type + " " + funcName + "() ";

            int nbTabs = 1;
            G25.CG.Shared.Util.WriteFunctionComment(SB, S, nbTabs, "Returns the " + bladeName + " coordinate of this " + gmvTypeName, null, null);

            SB.Append(funcDecl);
            SB.AppendLine(" {");
            SB.AppendLine("\t\treturn (m_c[" + groupIdx + "] == null) ? " +
                "m_c[" + groupIdx + "][" + elementIdx + "] : " +
                FT.DoubleToString(S, 0.0) + ";");
            SB.AppendLine("\t}");
        }

        /// <summary>
        /// Writes functions to get coordinates from the GMV
        /// </summary>
        /// <param name="SB"></param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteGetCoord(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT)
        {
            G25.GMV gmv = S.m_GMV;
            string typeName = FT.GetMangledName(S, gmv.Name);

            for (int groupIdx = 0; groupIdx < gmv.NbGroups; groupIdx++)
            {
                for (int elementIdx = 0; elementIdx < gmv.Group(groupIdx).Length; elementIdx++)
                {
                    WriteGetCoordFunction(SB, S, cgd, FT, typeName, groupIdx, elementIdx, gmv.Group(groupIdx)[elementIdx]);
                }
            }
        }

        /// <summary>
        /// Writes function for setting grade/group usage, reallocting memory
        /// </summary>
        /// <param name="SB"></param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteSetGroupUsage(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT)
        {
            string className = FT.GetMangledName(S, S.m_GMV.Name);
            cgd.m_cog.EmitTemplate(SB, "GMVsetGroupUsage", "S=", S, "FT=", FT, "className=", className, "gmv=", S.m_GMV);
        }

        /// <summary>
        /// Writes functions to reserve memory for groups.
        /// </summary>
        /// <param name="SB"></param>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        public static void WriteReserveGroup(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT)
        {
            cgd.m_cog.EmitTemplate(SB, "GMVreserveGroups", "S=", S, "FT=", FT, "gmv=", S.m_GMV);
        }

        private static void WriteSetCoordFunction(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT,
            string gmvTypeName, int groupIdx, int elementIdx, int groupSize, RefGA.BasisBlade B)
        {
            string bladeName = B.ToLangString(S.m_basisVectorNames);

            string funcName = G25.CG.Shared.Main.SETTER_PREFIX + bladeName;

            string coordName = "val";

            string funcDecl = "\t" + "void " + funcName + "(" + FT.type + " " + coordName + ") ";

            SB.AppendLine("\t/// Sets the " + bladeName + " coordinate of this " + gmvTypeName + ".");
            SB.Append(funcDecl);
            SB.AppendLine(" {");

            SB.AppendLine("\t\t" + GetReserveGroupString(S, groupIdx) + "();");

            SB.AppendLine("\t\tm_c[" + groupIdx + "][" + elementIdx + "] =  " + coordName + ";");

            SB.AppendLine("\t}");
        }

        /// <summary>
        /// Writes functions to set coordinates of the GMV
        /// </summary>
        /// <param name="S"></param>
        /// <param name="cgd">Results go here. Also intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT"></param>
        /// <param name="SB"></param>
        public static void WriteSetCoord(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, G25.FloatType FT)
        {
            G25.GMV gmv = S.m_GMV;
            string typeName = FT.GetMangledName(S, gmv.Name);

            for (int groupIdx = 0; groupIdx < gmv.NbGroups; groupIdx++)
            {
                for (int elementIdx = 0; elementIdx < gmv.Group(groupIdx).Length; elementIdx++)
                {
                    WriteSetCoordFunction(SB, S, cgd, FT, typeName, groupIdx, elementIdx, gmv.Group(groupIdx).Length, gmv.Group(groupIdx)[elementIdx]);
                }
            }

        }
                    


    } // end of class GMV
} // end of namespace G25.CG.CSJ

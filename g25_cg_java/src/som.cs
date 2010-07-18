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

namespace G25.CG.Java
{
    /// <summary>
    /// Handles code generation for specialized outermorphisms (classes, constructors, set functions, etc).
    /// </summary>
    class SOM
    {

        /// <summary>
        /// Generates a source file with the SOM class definition.
        /// </summary>
        /// <param name="S"></param>
        /// <param name="cgd"></param>
        /// <param name="som"></param>
        /// <param name="FT"></param>
        /// <returns></returns>
        public static string GenerateCode(Specification S, G25.CG.Shared.CGdata cgd, G25.SOM som, FloatType FT)
        {
            string className = FT.GetMangledName(S, som.Name);

            // get range vector type
            G25.SMV rangeVectorType = G25.CG.Shared.OMinit.GetRangeVectorType(S, FT, cgd, som);
            string rangeVectorSMVname = FT.GetMangledName(S, rangeVectorType.Name);

            // get filename, list of generated filenames
            List<string> generatedFiles = new List<string>();
            string sourceFilename = MainGenerator.GetClassOutputPath(S, className);
            generatedFiles.Add(sourceFilename);

            // get StringBuilder where all generated code goes
            StringBuilder SB = new StringBuilder();

            // get a new 'cgd' where all ouput goes to the one StringBuilder SB 
            cgd = new G25.CG.Shared.CGdata(cgd, SB, SB, SB);

            // output license, copyright
            G25.CG.Shared.Util.WriteCopyright(SB, S);
            G25.CG.Shared.Util.WriteLicense(SB, S);

            // open namespace
            G25.CG.Shared.Util.WriteOpenNamespace(SB, S);

            // write class comment
            G25.CG.CSJ.SOM.WriteComment(SB, S, cgd, FT, som);

            // open class
            G25.CG.Shared.Util.WriteOpenClass(SB, S, G25.CG.Shared.AccessModifier.AM_public, className, null, null);

            // write member variables
            G25.CG.CSJ.SOM.WriteMemberVariables(SB, S, cgd, FT, som);

            // write constructors
            G25.CG.CSJ.SOM.WriteConstructors(SB, S, cgd, FT, som, rangeVectorSMVname);

            // write set functions
            G25.CG.Shared.OMinit.WriteSetIdentity(S, cgd, FT, som);
            G25.CG.Shared.OMinit.WriteSetCopy(S, cgd, FT, som);
            G25.CG.Shared.OMinit.WriteSetVectorImages(S, cgd, FT, som);
            G25.CG.Shared.OMinit.WriteSetMatrix(S, cgd, FT, som, false); // false = !transpose
            G25.CG.Shared.OMinit.WriteSetMatrix(S, cgd, FT, som, true); // true = transpose 
            G25.CG.CSJ.SOM.WriteGOMtoSOMcopy(S, cgd, FT, som);

            // close class
            G25.CG.Shared.Util.WriteCloseClass(SB, S, className);

            // close namespace
            G25.CG.Shared.Util.WriteCloseNamespace(SB, S);

            // write all to file
            G25.CG.Shared.Util.WriteFile(sourceFilename, SB.ToString());

            return sourceFilename;
        }

#if RIEN
        public SOM(Specification S, CG.Shared.CGdata cgd)
        {
            m_specification = S;
            m_cgd = cgd;
        }

        protected Specification m_specification;
        protected CG.Shared.CGdata m_cgd;


        /// <summary>
        /// Writes comments of a SOM class to 'SB'.
        /// </summary>
        /// <param name="SB">Where the comment goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT">Float point type of 'GOM'.</param>
        /// <param name="som">The general outermorphism for which the class should be written.</param>
        public static void WriteComment(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SOM som)
        {
            SB.AppendLine("/**");
            SB.AppendLine(" * This class can hold a specialized outermorphism.");
            SB.AppendLine(" * ");

            SB.AppendLine(" * The coordinates are stored in type " + FT.type + ".");
            SB.AppendLine(" * ");

            SB.AppendLine(" * There are " + som.Domain.Length + " matrices, one for each grade.");
            SB.AppendLine(" * The columns of these matrices are the range of the outermorphism.");
            SB.AppendLine(" * Matrices are stored in row-major order. So the coordinates of rows are stored contiguously.");
            for (int g = 1; g < som.Domain.Length; g++) // start at '1' in order to skip scalar grade
            {
                SB.Append(" * Domain grade " + g + ": ");
                for (int i = 0; i < som.DomainForGrade(g).Length; i++)
                {
                    if (i > 0) SB.Append(", ");
                    SB.Append(som.DomainForGrade(g)[i].ToString(S.m_basisVectorNames));

                }

                SB.AppendLine(".");
            }
            SB.AppendLine(" * ");
            if (!som.DomainAndRangeAreEqual())
            {
                for (int g = 1; g < som.Range.Length; g++) // start at '1' in order to skip scalar grade
                {
                    SB.Append(" * Range grade " + g + ": ");
                    for (int i = 0; i < som.RangeForGrade(g).Length; i++)
                    {
                        if (i > 0) SB.Append(", ");
                        SB.Append(som.RangeForGrade(g)[i].ToString(S.m_basisVectorNames));

                    }

                    SB.AppendLine(".");
                }
            }
            else SB.AppendLine(" * The range and domain are equal.");
            SB.AppendLine(" * ");

            SB.AppendLine(" */");

        }

        /// <summary>
        /// Writes members variables of a GOM class to 'SB'.
        /// </summary>
        /// <param name="SB">Where the comment goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT">Float point type of 'GOM'.</param>
        /// <param name="som">The general outermorphism for which the class should be written.</param>
        public static void WriteMemberVariables(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SOM som)
        {
            SB.AppendLine("public:");

            for (int g = 1; g < som.Domain.Length; g++) // start at '1' in order to skip scalar grade
            {
                if (!som.EmptyGrade(g))
                {
                    SB.AppendLine("\t/// Matrix for grade " + g + "; the size is " + som.DomainForGrade(g).Length + " x " + som.RangeForGrade(g).Length);
                    SB.AppendLine("\t" + FT.type + " m_m" + g + "[" +
                        som.DomainForGrade(g).Length * som.RangeForGrade(g).Length + "];");
                }
            }

        }


        /// <summary>
        /// Writes members variables of a SOM class to 'SB'.
        /// </summary>
        /// <param name="SB">Where the comment goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT">Float point type of 'GOM'.</param>
        /// <param name="som">The specialized outermorphism for which the class should be written.</param>
        /// <param name="className"></param>
        public static void WriteFloatType(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SOM som, string className)
        {
            cgd.m_cog.EmitTemplate(SB, "classFloatType", "S=", S, "FT=", FT, "className=", className);
        }

        /// <summary>
        /// Writes constructors of a SOM class to 'SB'.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT">Float point type of 'SOM'.</param>
        /// <param name="som">The specialized outermorphism for which the class should be written.</param>
        /// <param name="className">Mangled name of SOM class.</param>
        /// <param name="rangeVectorSMVname">The name of the SMV which can represent a column of the OM.</param>
        /// <param name="gomClassName">Mangled name of GOM class.</param>
        public static void WriteConstructors(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SOM som, string className, string rangeVectorSMVname, string gomClassName)
        {
            cgd.m_cog.EmitTemplate(SB, "SOMconstructors", "S=", S, "FT=", FT, "som=", som, "className=", className, "gomClassName=", gomClassName, "rangeVectorSMVname=", rangeVectorSMVname);
        }

        /// <summary>
        /// Writes assignment operators of a GOM class to 'SB'.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT">Float point type of 'SOM'.</param>
        /// <param name="som">The specialized outermorphism for which the class should be written.</param>
        /// <param name="className">Mangled name of SOM class.</param>
        /// <param name="rangeVectorSMVname">The name of the SMV which can represent a column of the OM.</param>
        /// <param name="gomClassName">Mangled name of GOM class.</param>
        public static void WriteAssignmentOps(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SOM som, string className, string rangeVectorSMVname, string gomClassName)
        {
            cgd.m_cog.EmitTemplate(SB, "SOMassignmentOps", "S=", S, "FT=", FT, "som=", som, "className=", className, "gomClassName=", gomClassName, "rangeVectorSMVname=", rangeVectorSMVname);
        }

        /// <summary>
        /// Writes 'set()' declarations of a GOM class to 'SB'.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT">Float point type of 'SOM'.</param>
        /// <param name="som">The specialized outermorphism for which the class should be written.</param>
        /// <param name="className">Mangled name of SOM class.</param>
        /// <param name="rangeVectorSMVname">The name of the SMV which can represent a column of the OM.</param>
        /// <param name="gomClassName">Mangled name of GOM class.</param>
        public static void WriteSetDeclarations(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SOM som, string className, string rangeVectorSMVname, string gomClassName)
        {
            cgd.m_cog.EmitTemplate(SB, "SOMsetDecl", "S=", S, "FT=", FT, "som=", som, "className=", className, "gomClassName=", gomClassName, "rangeVectorSMVname=", rangeVectorSMVname);
        }

        /// <summary>
        /// Writes the definition of an SOM struct to 'SB' (including comments).
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        /// <param name="FT">Float point type of 'SOM'.</param>
        /// <param name="som">The general outermorphism for which the struct should be written.</param>
        public static void WriteSOMclass(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd, FloatType FT, G25.SOM som)
        {
            SB.AppendLine("");

            string className = FT.GetMangledName(S, som.Name);
            string gomClassName = (S.m_GOM == null) ? "" : FT.GetMangledName(S, S.m_GOM.Name);
            
            // get range vector type
            G25.SMV rangeVectorType = G25.CG.Shared.OMinit.getRangeVectorType(S, FT, cgd, som);
            string rangeVectorSMVname = FT.GetMangledName(S, rangeVectorType.Name);

            WriteComment(SB, S, cgd, FT, som);

            // typedef
            SB.AppendLine("class " + className);
            SB.AppendLine("{");

            // member vars
            WriteMemberVariables(SB, S, cgd, FT, som);

            SB.AppendLine("public:");

            // Float type
            WriteFloatType(SB, S, cgd, FT, som, className);

            // constructors
            WriteConstructors(SB, S, cgd, FT, som, className, rangeVectorSMVname, gomClassName);

            // operator=
            WriteAssignmentOps(SB, S, cgd, FT, som, className, rangeVectorSMVname, gomClassName);

            // set(...)
            WriteSetDeclarations(SB, S, cgd, FT, som, className, rangeVectorSMVname, gomClassName);


            SB.AppendLine("}; // end of " + className);

        }

        /// <summary>
        /// Writes structs for SOM to 'SB'.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteSOMclasses(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd)
        {
            // for each float type
            foreach (G25.FloatType FT in S.m_floatTypes)
            {
                // for each som
                foreach (G25.SOM som in S.m_SOM)
                {
                    WriteSOMclass(SB, S, cgd, FT, som);
                }
            }
        }

        /// <summary>
        /// Writes typenames of all SOMs to 'SB'.
        /// </summary>
        /// <param name="SB">Where the code goes.</param>
        /// <param name="S">Used for basis vector names and output language.</param>
        /// <param name="cgd">Intermediate data for code generation. Also contains plugins and cog.</param>
        public static void WriteSOMtypes(StringBuilder SB, Specification S, G25.CG.Shared.CGdata cgd)
        {
            // for each float type
            foreach (G25.FloatType FT in S.m_floatTypes)
            {
                foreach (G25.SOM som in S.m_SOM)
                {
                    string className = FT.GetMangledName(S, som.Name);
                    // typedef
                    SB.AppendLine("class " + className + ";");
                }
            }
        }

        /// <summary>
        /// Writes setIdentity, copy and init from vector and matrix for all specialized outermorphism types.
        /// </summary>
        public void WriteSetFunctions()
        {
            G25.CG.Shared.OMinit.WriteSetIdentity(m_specification, m_cgd);
            G25.CG.Shared.OMinit.WriteCopy(m_specification, m_cgd);
            G25.CG.Shared.OMinit.WriteSetVectorImages(m_specification, m_cgd);
            G25.CG.Shared.OMinit.WriteSetMatrix(m_specification, m_cgd, false); // false = !transpose
            G25.CG.Shared.OMinit.WriteSetMatrix(m_specification, m_cgd, true); // true = transpose 
        }

#endif
    } // end of class SOM
} // end of namespace G25.CG.Java

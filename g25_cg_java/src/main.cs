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

/*! \mainpage Gaigen 2.5 library (g25_cg_java) Documentation
 *
 * Gaigen 2.5 by Daniel Fontijne, University of Amsterdam.
 * 
 * Released under GPL license.
 */
namespace G25.CG.Java
{
    public class MainGenerator : G25.CG.Shared.Main, G25.CodeGenerator
    {
        /// <returns>what language this code generator generates for.</returns>
        public String Language()
        {
            return G25.XML.XML_JAVA;
        }

        /// <summary>
        /// Should generate the code according to the specification of the algebra.
        /// </summary>
        /// <param name="S">The specification of the algebra. The specification also lists the names of the files
        /// to be generated, or at least the base path.</param>
        /// <param name="plugins">The plugins which Gaigen found that support the same language as this code generator.</param>
        /// <returns>a list of filenames; the names of the files that were generated. This may be used
        /// for post processing.</returns>
        public List<string> GenerateCode(Specification S, List<CodeGeneratorPlugin> plugins)
        {
            // disable all inlining since the Java language does not support this:
            S.SetInlineNone();

            CreatePackageDirectory(S);

            CoGsharp.CoG cog = InitCog(S);

            CG.Shared.CGdata cgd = new G25.CG.Shared.CGdata(plugins, cog);
            cgd.SetDependencyPrefix("missing_function_"); // this makes sure that the user sees the function call is a missing dependency
            G25.CG.Shared.FunctionGeneratorInfo FGI = (S.m_generateTestSuite) ? new G25.CG.Shared.FunctionGeneratorInfo() : null; // the fields in this variable are set by Functions.WriteFunctions() and reused by TestSuite.GenerateCode()

            { // pregenerated code that will go into main source
                // generate code for parts of the geometric product, dual, etc (works in parallel internally)
                try
                {
                    bool declOnly = false;
                    G25.CG.Shared.PartsCode.GeneratePartsCode(S, cgd, declOnly);
                }
                catch (G25.UserException E) { cgd.AddError(E); }


                // write function (works in parallel internally)
               G25.CG.Shared.Functions.WriteFunctions(S, cgd, FGI, Functions.GetFunctionGeneratorPlugins(cgd));
            }

            List<string> generatedFiles = new List<string>();

            // generate Doxyfile
            generatedFiles.Add(G25.CG.Shared.Util.GenerateDoxyfile(S, cgd));
            // generate source files / classes for all GMV, SMV, GOM, SOM types
            generatedFiles.AddRange(GenerateClasses(S, cgd));
            // generate source
            generatedFiles.AddRange(Source.GenerateCode(S, cgd));
            // generate smv type enum
            generatedFiles.AddRange(SmvType.GenerateCode(S, cgd));
            // generate GroupBitmap class
            generatedFiles.AddRange(GroupBitmap.GenerateCode(S, cgd));
            // generate multivector interfaces
            generatedFiles.AddRange(MvInterface.GenerateCode(S, cgd));
            // generate parser
            generatedFiles.AddRange(Parser.GenerateCode(S, cgd));
            // generate report usage code
            generatedFiles.AddRange(ReportUsage.GenerateCode(S, cgd));

            // report errors and missing deps to user
            cgd.PrintErrors(S);
            cgd.PrintMissingDependencies(S);
            if ((cgd.GetNbErrors() == 0) && (cgd.GetNbMissingDependencies() == 0) && S.m_generateTestSuite)
            {
                // generate test suite
                generatedFiles.AddRange(TestSuite.GenerateCode(S, cgd, FGI));
            }



            return generatedFiles;
        }

        /// <summary>
        /// Loads all templates for the 'Java' language into 'cog'. Also loads
        /// shared templates by calling G25.CG.Shared.Util.LoadTemplates(cog);
        /// </summary>
        /// <param name="cog">Templates are loaded into this variable.</param>
        /// <param name="S">Specification. Used to know whether testing code will be generated.</param>
        public override void LoadTemplates(Specification S, CoGsharp.CoG cog)
        {
            cog.AddReference((new G25.CG.CSJ.GMV()).GetType().Assembly.Location); // add reference for g25_cg_csj

            // also load shared templates:
            G25.CG.Shared.Util.LoadTemplates(cog);

            cog.LoadTemplates(g25_cg_csj_shared.Properties.Resources.cg_csj_shared_templates , "cg_csj_shared_templates.txt");
            cog.LoadTemplates(g25_cg_csj_shared.Properties.Resources.cg_csj_shared_test_templates, "cg_csj_shared_test_templates.txt");

            cog.LoadTemplates(g25_cg_java.Properties.Resources.cg_java_templates, "cg_java_templates.txt");
            if (S.m_generateTestSuite) // only load when testing code is required
                cog.LoadTemplates(g25_cg_java.Properties.Resources.cg_java_test_templates, "cg_java_test_templates.txt");
        }

        /// <summary>
        /// Creates the directory required for the Java package S.m_namespace.
        /// </summary>
        /// <param name="S"></param>
        private static void CreatePackageDirectory(Specification S) {
            // get path 
            string packageName = G25.CG.Shared.Util.GetNamespaceName(S);
            string path = S.GetOutputDir() + System.IO.Path.DirectorySeparatorChar + packageName.Replace('.', System.IO.Path.DirectorySeparatorChar);

            // check if exists already; if so: do nothing, return
            if (System.IO.Directory.Exists(path)) return;

            // try to create the directory
            try
            {
                System.IO.Directory.CreateDirectory(path);
            }
            catch (Exception E)
            {
                throw new G25.UserException("Error creating package directory " + path + ":\n" + E.Message);
            }
        }

        /// <summary>
        /// Generates source code for all GA types (GMV, SMV, GOM, SOM)
        /// </summary>
        /// <param name="S"></param>
        /// <param name="cgd"></param>
        /// <returns>List of generated files.</returns>
        private static List<string> GenerateClasses(Specification S, G25.CG.Shared.CGdata cgd)
        {
            List<string> generatedFiles = new List<string>();

            foreach (FloatType FT in S.m_floatTypes)
            {
                generatedFiles.Add(GMV.GenerateCode(S, cgd, FT));

                foreach (G25.SMV smv in S.m_SMV)
                    generatedFiles.Add(SMV.GenerateCode(S, cgd, smv, FT));

                if (S.m_GOM != null)
                    generatedFiles.Add(GOM.GenerateCode(S, cgd, FT));

                foreach (G25.SOM som in S.m_SOM)
                    generatedFiles.Add(SOM.GenerateCode(S, cgd, som, FT));
            }

            return generatedFiles;
        }

        /// <returns>GetClassOutputPath(S, className, "java")</returns>
        internal static string GetClassOutputPath(Specification S, string className)
        {
            return GetClassOutputPath(S, className, "java");
        }

        
        /// <summary>
        /// Path for output of source file  'name' with 'extension'
        /// </summary>
        /// <param name="S"></param>
        /// <param name="name">Name of class / source file.</param>
        /// <param name="extension">Do not include the dot.</param>
        /// <returns>Output path for name.extension</returns>
        internal static string GetClassOutputPath(Specification S, string name, string extension)
        {
            string packageName = G25.CG.Shared.Util.GetNamespaceName(S);
            string path =
                packageName.Replace('.', System.IO.Path.DirectorySeparatorChar) +  // package dir
                System.IO.Path.DirectorySeparatorChar +  // slash
                name +  // class
                "." +
                extension; // extension

            return S.GetOutputPath(path);
        }



    } // end of class MainGenerator


} // end of namespace G25.CG.Java
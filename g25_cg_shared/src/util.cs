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

/*! \mainpage Gaigen 2.5 library (g25_cg_shared) Documentation
 *
 * Gaigen 2.5 by Daniel Fontijne, University of Amsterdam.
 * 
 * Released under GPL license.
 * 
 * This class library implements shared functionality for all other language-specific code generators.
 * 
 * It may turn out that most functionality ends up in this library, and that the language-specific code generators
 * are just shells around it.
 *   - G25.CG.Shared.Util implements generic utility functions, such as opening and closing comments,
 *     writing licences, loading templates, and so on.
 *   - G25.CG.Shared.Threads implements utility functions for dealing with arrays of threads.
 *   - G25.CG.Shared.CodeUtil contains generic functions for generating code, such as getting code to access the coordinates
 *     of a multivector, and generating code assign a value to a (specialized or general) multivector.
 *   - G25.CG.Shared.Symbolic implements functions for converting a G25.VariableTypes (scalar, specialized or general multivector)
 *      to RefGA.Multivectors with symbolic coordinates.
 *   - G25.CG.Shared.SpecializedReturnType contains code for finding a well-matching specialized multivector type 
 *     given a symbolic RefGA.Multivector. It is the opposite G25.CG.Shared.Symbolic.
 *   - G25.CG.Shared.BasisBlade contains some utility functions related to basis blades.
 *   - G25.CG.Shared.FuncArgInfo is a class that makes information about function arguments easily accessible.
 *   - G25.CG.Shared.Converters contains utility functions for generating multivector converters.
 *   - G25.CG.Shared.Functions contains code for emitting functions in language-specific ways.
 *   - G25.CG.Shared.Instruction is an superclass for representing high-level GA instructions which can be
 *     turned into actual functions use G25.CG.Shared.Functions.WriteFunction().
 *   - G25.CG.Shared.CGdata contains data for code generation that is passed along to code generating functions.
 *   - G25.CG.Shared.Dependencies implements functions for retrieving names of generated functions (dependencies), and 
 *      for collecting info on missing dependencies.
 *    
 * 
 * There are three classes which generate the low-level code for general multivector functions:
 *   - G25.CG.Shared.GPparts generates code for the geometric product, chopped up into little pieces (group x group -> group). 
 *      It also handles norm, as that is very much like a scalar product. It also handles versor application, use this function uses
 *      the geometric product and the norm2 function.
 *   - G25.CG.Shared.CANSparts generates code for the copying, copying+scaling, addition, subtraction and negation, chopped up into little pieces (group x group -> group).
 *      It also does grade part selection, unit and versor inverse.
 *   - G25.CG.Shared.DualParts generates code for the dual of general multivectors.
 * 
 */
namespace G25.CG.Shared
{
    
    /// <summary>
    /// Contains various utility functions, such as opening and closing comments,
    /// writing licences, loading templates, and so on.
    /// </summary>
    public class Util
    {
        public Util() {}

        /// <summary>
        /// Writes <c>text</c> to file <c>filename</c>. May throw an exception, but always closes the file.
        /// </summary>
        /// <param name="filename">Name of file where <c>text</c> should go.</param>
        /// <param name="text">Text to write to file</param>
        public static void WriteFile(string filename, string text)
        {
            // write all to file (TODO: shared code)
            System.IO.StreamWriter W = null;
            try
            {
                W = System.IO.File.CreateText(filename);
                W.Write(text);
            }
            finally
            {
                if (W != null) W.Close();
                W = null;
            }
        }


        /// <summary>
        /// Takes a string (intended use is for filenames) and converts all characters 
        /// which are not letters or digits (according to <c>Char.IsLetterOrDigit()</c>) to underscores '<c>_</c>'.
        /// </summary>
        /// <param name="filename">The filename to be converted.</param>
        /// <returns>'filename', but all characters which are not a digit or a letter are converted to '_'.</returns>
        public static String InvalidCharsToUnderscores(String filename)
        {
            StringBuilder SB = new StringBuilder();
            for (int i = 0; i < filename.Length; i++)
            {
                SB.Append(Char.IsLetterOrDigit(filename[i]) ? filename[i] : '_');
            }
            return SB.ToString();
        }

        /// <summary>
        /// Writes the license (as specified in 'S') to 'SB'. Uses opening and closing
        /// comments.
        /// </summary>
        /// <param name="S">Specification (used for license).</param>
        /// <param name="SB">Where the code goes.</param>
        public static void WriteLicense(StringBuilder SB, G25.Specification S)
        {
            WriteOpenMultilineComment(SB, S);
            SB.Append(S.GetLicense());
            SB.AppendLine("");
            WriteCloseMultilineComment(SB, S);
        }

        /// <summary>
        /// Writes the copyright notice (as specified in 'S') to 'SB'. 
        /// Uses opening and closing comments.
        /// </summary>
        /// <param name="S">Specification (used for license).</param>
        /// <param name="SB">Where the code goes.</param>
        public static void WriteCopyright(StringBuilder SB, G25.Specification S)
        {
            if (S.m_copyright.Length > 0)
            {
                WriteOpenMultilineComment(SB, S);
                SB.Append(S.m_copyright);
                WriteCloseMultilineComment(SB, S);
            }
        }
        

        public static void WriteOpenNamespace(StringBuilder SB, G25.Specification S, string namespaceName)
        {
            switch (S.m_outputLanguage)
            {
                case OUTPUT_LANGUAGE.CPP:
                    if (S.m_namespace.Length > 0)
                    {
                        SB.Append("namespace ");
                        SB.Append(namespaceName);
                        SB.AppendLine(" {");
                    }
                    break;
                default:
                    throw new Exception("G25.CG.Shared.Util.WriteOpenNamespace(): output language not supported");
            }
        }

        public static void WriteOpenNamespace(StringBuilder SB, G25.Specification S)
        {
            WriteOpenNamespace(SB, S, S.m_namespace);
        }

        public static void WriteCloseNamespace(StringBuilder SB, G25.Specification S, string namespaceName)
        {
            switch (S.m_outputLanguage)
            {
                case OUTPUT_LANGUAGE.CPP:
                    if (S.m_namespace.Length > 0)
                    {
                        SB.AppendLine("} // end of namespace " + namespaceName);
                    }
                    break;
                default:
                    throw new Exception("G25.CG.Shared.Util.WriteCloseNamespace(): output language not supported");
            }
        }

        public static void WriteCloseNamespace(StringBuilder SB, G25.Specification S)
        {
            WriteCloseNamespace(SB, S, S.m_namespace);
        }

        /// <summary>
        /// Writes the appropriate 'open multiline comment' string to <c>SB</c>.
        /// For example, if <c>S.m_outputLanguage</c> is C, CPP, CSharp or Java, then "/*\n" is written.
        /// For Python, "\"\"\"\n" is written.
        /// </summary>
        /// <param name="SB">Where the result goes.</param>
        /// <param name="S">Used for <c>S.m_outputLanguage</c>.</param>
        public static void WriteOpenMultilineComment(StringBuilder SB, G25.Specification S)
        {
            switch (S.m_outputLanguage)
            {
                case OUTPUT_LANGUAGE.C:
                case OUTPUT_LANGUAGE.CPP:
                case OUTPUT_LANGUAGE.JAVA:
                case OUTPUT_LANGUAGE.CSHARP:
                    SB.AppendLine("/*");
                    break;
                case OUTPUT_LANGUAGE.PYTHON:
                    SB.AppendLine("\"\"\"");
                    break;
                case OUTPUT_LANGUAGE.MATLAB:
                    SB.AppendLine("%{");
                    break;
                default:
                    throw new Exception("G25.CG.Shared.Util.WriteOpenMultilineComment(): output language not supported");
            }
        }

        /// <summary>
        /// Writes the appropriate 'close multiline comment' string to <c>SB</c>.
        /// For example, if <c>S.m_outputLanguage</c> is a C-like language, "*/\n" is written.
        /// For Python, "\"\"\"\\n" is written.
        /// </summary>
        /// <param name="SB">Where the result goes.</param>
        /// <param name="S">Used for <c>S.m_outputLanguage</c>.</param>
        public static void WriteCloseMultilineComment(StringBuilder SB, G25.Specification S)
        {
            switch (S.m_outputLanguage)
            {
                case OUTPUT_LANGUAGE.C:
                case OUTPUT_LANGUAGE.CPP:
                case OUTPUT_LANGUAGE.JAVA:
                case OUTPUT_LANGUAGE.CSHARP:
                    SB.AppendLine("*/");
                    break;
                case OUTPUT_LANGUAGE.PYTHON:
                    SB.AppendLine("\"\"\"");
                    break;
                case OUTPUT_LANGUAGE.MATLAB:
                    SB.AppendLine("%}");
                    break;
                default:
                    throw new Exception("G25.CG.Shared.Util.WriteOpenMultilineComment(): output language not supported");
            }
        }

        /// <summary>
        /// Writes an open include guard based on the name <c>filename</c> to the StringBuilder <c>SB</c>.
        /// For use with C and C++.
        /// </summary>
        /// <param name="SB">Where the result goes.</param>
        /// <param name="filename">Filename (the include guard is based on the filename).</param>
        public static void WriteOpenIncludeGuard(StringBuilder SB, String filename)
        {
            string safeFilename = InvalidCharsToUnderscores(filename).ToUpper();
            SB.AppendLine("#ifndef _" +  safeFilename + "_");
            SB.AppendLine("#define _" + safeFilename + "_");
        }

        /// <summary>
        /// Writes a close include guard based on the name <c>filename</c> to the StringBuilder <c>SB</c>.
        /// For use with C and C++.
        /// </summary>
        /// <param name="SB">Where the result goes.</param>
        /// <param name="filename">Filename (the include guard is based on the filename).</param>
        public static void WriteCloseIncludeGuard(StringBuilder SB, String filename)
        {
            string safeFilename = InvalidCharsToUnderscores(filename).ToUpper();
            SB.AppendLine("#endif /* _" + safeFilename + "_ */");
        }


        /// <summary>
        /// Loads globally shared templates for into 'cog'. 
        /// Specifically, loads 'cg_shared_templates.txt'
        /// which is a project resource of g25_cg_shared.
        /// </summary>
        /// <param name="cog">Templates are loaded into cog.</param>
        public static void LoadTemplates(CoGsharp.CoG cog)
        {
            cog.LoadTemplates(g25_cg_shared.Properties.Resources.cg_shared_templates, "cg_shared_templates.txt");
        }

        /// <summary>
        /// Loads shared templates for C and C++ for into 'cog'. 
        /// Specifically, loads 'cg_shared_c_templates.txt'
        /// which is a project resource of g25_cg_shared.
        /// </summary>
        /// <param name="cog">Templates are loaded into cog.</param>
        public static void LoadCTemplates(CoGsharp.CoG cog)
        {
            cog.LoadTemplates(g25_cg_shared.Properties.Resources.cg_shared_c_templates, "cg_shared_c_templates.txt");
        }

        /// <summary>
        /// Generates the Doxyfile which can be used to let Doxygen process the generate source code 
        /// to order to extract documentation from it.
        /// </summary>
        /// <param name="S">The specification, used for the output path and namespaces</param>
        /// <param name="cgd">CG data. Contains the code generator; the function uses the "doxyfile" template.</param>
        /// <returns>The full path of the generated doygen file.</returns>
        public static string GenerateDoxyfile(Specification S, G25.CG.Shared.CGdata cgd)
        { 
            // get filename
            string rawFilename = "Doxyfile";
            string doxyFilename = S.GetOutputPath(rawFilename);

            // emit template
            cgd.m_cog.ClearOutput(); // get rid of any junk other users may have left behind
            cgd.m_cog.EmitTemplate("doxyfile", "Namespace=", S.m_namespace);

            // write all to file
            System.IO.StreamWriter W = null;
            try
            {
                // open "namespace.h" for writing
                try
                {
                    W = System.IO.File.CreateText(doxyFilename);
                }
                catch (Exception )
                {
                    throw new G25.UserException("Could create output file: " + doxyFilename);
                }
                W.Write(cgd.m_cog.GetOutputAndClear());
            }
            finally
            {
                if (W != null) W.Close();
                W = null;
            }
            return doxyFilename;
        }

        /// <summary>
        /// Cuts a string up into a list of strings by cutting at newline characters.
        /// 
        /// This is used for emitting verbatim comment where you want to insert
        /// for example <c>//</c> prefixes at the start of each line.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <returns>list of strings.</returns>
        public static List<string> CutStringAtNewLine(string str)
        {
            List<string> L = new List<string>();

            // new line chars:
            char[] NL = new char[] { (char)0x0a, (char)0x0d };

            for (int i = 0; i < str.Length;)
            {
                // find next carriage return, or end of string
                int nextIdx = str.IndexOfAny(NL, i);
                if (nextIdx < 0) nextIdx = str.Length;

                // cut string into lines
                string subStr = str.Substring(i, nextIdx - i);
                L.Add(subStr); // add to list of comments

                // move 'i' to start of next line
                i = nextIdx + 1;
                if ((i < (str.Length)) && ((str[i-1] == (char)0x0d) && (str[i] == (char)0x0a))) i++; // skip over CR+LF
            }

            return L;
        }

        /// <summary>
        /// Appends the non-mangled typenames of the type of all arguments to the name of 'F'.
        /// Also appends the float type.
        /// </summary>
        /// <param name="S">Specification (used for output language).</param>
        /// <param name="FT">Float type of function.</param>
        /// <param name="funcName">The function name.</param>
        /// <param name="typeNames">Typenames of function arguments.</param>
        /// <returns>new name.</returns>
        public static string AppendTypenameToFuncName(Specification S, G25.FloatType FT, string funcName, string[] typeNames)
        {
            StringBuilder SB = new StringBuilder(funcName);

            if (!funcName.Contains(G25.Specification.DONT_MANGLE)) // for the auto-dependency system to work, we should not mangle when this string is present!
            {
                foreach (string tn in typeNames)
                {
                    SB.Append("_");
                    SB.Append(tn);
                }
            }

            return FT.GetMangledName(S, SB.ToString());
        }

        /// <summary>
        /// Appends the non-mangled typenames of the type of all arguments to the <c>m_outputName</c> in <c>F</c>.
        /// Also appends the float type.
        /// </summary>
        /// <param name="S">Specification (used for output language).</param>
        /// <param name="FT">Float type of function.</param>
        /// <param name="F">The function (it is copied, but with changed name).</param>
        /// <param name="FAI">Info on function arguments.</param>
        /// <returns>a copy of <c>F</c> with a new name.</returns>
        public static G25.fgs AppendTypenameToFuncName(Specification S, G25.FloatType FT, G25.fgs F, G25.CG.Shared.FuncArgInfo[] FAI)
        {
            if (S.m_outputLanguage != OUTPUT_LANGUAGE.C) return new G25.fgs(F, F.OutputName);
            else
            {
                string[] typeNames = new string[FAI.Length];
                for (int i = 0; i < FAI.Length; i++)
                    typeNames[i] = FAI[i].MangledTypeName;

                return new G25.fgs(F, AppendTypenameToFuncName(S, FT, F.OutputName, typeNames));
            }
        }

        /// <summary>
        /// If the output language is C, appends the non-mangled typenames of the type of all arguments to the <c>m_outputName</c> in <c>F</c>.
        /// </summary>
        /// <param name="S">Used for output language.</param>
        /// <param name="F">The function (it is copied, but with changed name).</param>
        /// <param name="FAI">Info on function arguments.</param>
        /// <returns>a copy of <c>F</c> with a new name.</returns>
        /*public static G25.fgs AppendTypenameToFuncName(G25.Specification S, G25.FloatType FT, G25.fgs F, G25.CG.Shared.FuncArgInfo[] FAI)
        {
            if (S.m_outputLanguage == OUTPUT_LANGUAGE.C)
                return AppendTypenameToFuncName(S, FT, F, FAI);
            else return new G25.fgs(F, F.OutputName);
        }*/

        private static string GetZeroCodePrefix(G25.Specification S, G25.FloatType FT)
        {
            return (S.m_outputLanguage == OUTPUT_LANGUAGE.C)
                ? (S.m_namespace + "_" + FT.type + "_")
                : S.m_namespace + "::";
        }

        /// <summary>
        /// Returns the code to set ptrName[0] to ptrName[nb-1] to 0.
        /// </summary>
        public static string GetSetToZeroCode(G25.Specification S, G25.FloatType FT, string ptrName, int nb) {
            if (nb <= Main.MAX_EXPLICIT_ZERO)
                return GetZeroCodePrefix(S, FT) + "zero_" + nb + "(" + ptrName + ");\n";
            else return GetSetToZeroCode(S, FT, ptrName, nb.ToString());
        }

        /// <summary>
        /// Returns the code to set ptrName[0] to ptrName[nb-1] to 0.
        /// </summary>
        public static string GetSetToZeroCode(G25.Specification S, G25.FloatType FT, string ptrName, string nb)
        {
            return GetZeroCodePrefix(S, FT) + "zero_N(" + ptrName + ", " + nb + ");\n";
        }

        /// <summary>
        /// Returns the code to copy an array of 'nb' floats from 'srcPtrName' to 'dstPtrName'.
        /// </summary>
        public static string GetCopyCode(G25.Specification S, G25.FloatType FT, string srcPtrName, string dstPtrName, int nb)
        {
            if (nb <= Main.MAX_EXPLICIT_ZERO)
                return GetZeroCodePrefix(S, FT) + "copy_" + nb + "(" + dstPtrName + ", " + srcPtrName + ");\n";
            else return GetCopyCode(S, FT, srcPtrName, dstPtrName, nb.ToString());
        }

        /// <summary>
        /// Returns the code to copy an array of 'nb' floats from 'srcPtrName' to 'dstPtrName'.
        /// </summary>
        public static string GetCopyCode(G25.Specification S, G25.FloatType FT, string srcPtrName, string dstPtrName, string nb)
        {
            return GetZeroCodePrefix(S, FT) + "copy_N(" + dstPtrName + ", " + srcPtrName + ", " + nb + ");\n";
        }

        /// <summary>
        /// Converts a string to a string that can be used in code.
        /// 
        /// For example <c>name="X"</c> would be changed into <c>"name=\"X\""</c>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToCode(string str)
        {
            StringBuilder SB = new StringBuilder();
            SB.Append("\"");
            int idx = 0;
            while (idx < str.Length)
            {
                int nextIdx = str.IndexOf('"', idx);
                if (nextIdx < 0)
                {
                    SB.Append(str.Substring(idx));
                    idx = str.Length;
                }
                else
                {
                    SB.Append(str.Substring(idx, nextIdx - idx));
                    SB.Append("\\\"");
                    idx = nextIdx + 1;
                }
            }
            SB.Append("\"");
            return SB.ToString();
        }

        /// <returns>A unique name for the testing function of 'funcName'.</returns>
        public static string GetTestingFunctionName(Specification S, CGdata cgd, string funcName)
        {
            if (S.m_outputLanguage == OUTPUT_LANGUAGE.C)
                return "test_" + funcName;
            else return "test_" + funcName + cgd.GetDontMangleUniqueId();
        }

        /// <summary>
        /// Returns the appropriate inline string for the output language.
        /// 
        /// Returns "" when inline is false, and the inline string + postFixStr otherwise.
        /// </summary>
        /// <param name="S"></param>
        /// <param name="inline"></param>
        /// <param name="postFixStr">Concatenated to inline string. May be null.</param>
        /// <returns>Inline string, or ""</returns>
        public static string GetInlineString(Specification S, bool inline, String postFixStr)
        {
            if (inline)
            {
                if (S.m_outputLanguage == OUTPUT_LANGUAGE.C)
                    return "";
                else if (S.m_outputLanguage == OUTPUT_LANGUAGE.CPP)
                    return "inline" + postFixStr;
                else return "inline_str_to_do" + postFixStr;
            }
            else return "";
        }



    } // end of class Util
} // end of namepace G25.CG.Shared

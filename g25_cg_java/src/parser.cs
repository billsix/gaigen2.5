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
    /// Handles code generation of lex/yacc lexer parser.
    /// </summary>
    class Parser
    {
        public static string GetRawParserSourceFilename(Specification S)
        {
            return MainGenerator.GetClassOutputPath(S, "Parser");
        }

        public static string GetRawParseExceptionSourceFilename(Specification S)
        {
            return MainGenerator.GetClassOutputPath(S, "ParseException");
        }

        public static string GetRawANTLRgrammarFilename(Specification S)
        {
            return MainGenerator.GetClassOutputPath(S, S.m_namespace, "g");
        }

        /// <summary>
        /// This is determined only by the namespace and ANTLR (it appends Parser.cs)
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string GetANTLRparserSourceFilename(Specification S)
        {
            return S.m_namespace + "Parser.java";
        }


        /// <summary>
        /// This is determined only by the namespace and ANTLR (it appends Lexer.cs)
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string GetANTLRlexerSourceFilename(Specification S)
        {
            return S.m_namespace + "Lexer.java";
        }

        public static G25.FloatType GetANTLRfloatType(Specification S)
        {
            if (!((S.m_floatTypes[0].type == "float") || (S.m_floatTypes[0].type == "double")))
            {
                return new G25.FloatType("double", "", "");
            }
            else return S.m_floatTypes[0];
        }

        public static List<string> GenerateCode(Specification S, G25.CG.Shared.CGdata cgd)
        {
            // get list of generated filenames
            List<string> generatedFiles = new List<string>();
            if (S.m_parserType == PARSER.NONE) return generatedFiles; // nothing to do

            // get parser source output path
            string parserSourceFilename = S.GetOutputPath(GetRawParserSourceFilename(S));
            if (S.m_parserType == PARSER.BUILTIN)
                generatedFiles.Add(parserSourceFilename);

            // get parser exception source output path
            string parseExceptionSourceFilename = S.GetOutputPath(GetRawParseExceptionSourceFilename(S));
            generatedFiles.Add(parseExceptionSourceFilename);

            // get grammar output path
            string rawGrammarFilename = GetRawANTLRgrammarFilename(S);
            string grammarFilename = S.GetOutputPath(rawGrammarFilename);
            if (S.m_parserType == PARSER.ANTLR) // only really generated when parser is ANTLR
                generatedFiles.Add(grammarFilename);

            // get StringBuilder where all generated code goes
            StringBuilder sourceSB = new StringBuilder(); // parser source (if any) goes here
            StringBuilder exceptionSB = new StringBuilder(); // ParseException source goes here
            StringBuilder grammarSB = new StringBuilder(); // grammar (if any) goes here


            // parser exception
            cgd.m_cog.EmitTemplate(exceptionSB, "ParseExceptionSource_Java", "S=", S);
            G25.CG.Shared.Util.WriteFile(parseExceptionSourceFilename, exceptionSB.ToString());

            // parser declarations:
            if (S.m_parserType == PARSER.BUILTIN)
            {
                // output license, copyright
                G25.CG.Shared.Util.WriteCopyright(sourceSB, S);
                G25.CG.Shared.Util.WriteLicense(sourceSB, S);
                cgd.m_cog.EmitTemplate(sourceSB, "BuiltinParserSource_CSharp_Java", "S=", S, "FT=", S.m_floatTypes[0]);
                G25.CG.Shared.Util.WriteFile(parserSourceFilename, sourceSB.ToString());
            }
            else if (S.m_parserType == PARSER.ANTLR)
            {
                cgd.m_cog.EmitTemplate(grammarSB, "ANTLRgrammar_CSharp_Java", "S=", S, "FT=", S.m_floatTypes[0]);
                G25.CG.Shared.Util.WriteFile(grammarFilename, grammarSB.ToString());
            }

            return generatedFiles;
        }

    } // end of class Parser
} // end of namespace G25.CG.Java


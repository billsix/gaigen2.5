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
using System.Threading;

namespace G25.CG.C
{

    /// <summary>
    /// Handles code generation of unit test suite.
    /// </summary>
    class TestSuite
    {
        public static string GetRawTestSuiteFilename(Specification S)
        {
            return S.m_namespace + "_test_main.c";
        }

        /// <summary>
        /// Generates a complete testing suite, including a main function.
        /// 
        /// The testing suite should test every function implemented, but depends
        /// on the code generator plugins to actually generate those tests.
        /// </summary>
        /// <param name="S">Specification of algebra.</param>
        /// <param name="cgd">Used to pass all kinds of info around.</param>
        /// <param name="FGI">Info about what FunctionGenerator to use for each FGS. Recycled from
        /// Function.WriteFunctions() for efficiency.</param>
        /// <returns></returns>
        public static List<string> GenerateCode(Specification S, G25.CG.Shared.CGdata cgd, G25.CG.Shared.FunctionGeneratorInfo FGI)
        {
            System.Console.WriteLine("Generating test suite . . .\n");
            cgd.SetDependencyPrefix(""); // we want missing dependencies to actually compile, and not generate an error based on function name

            // get list of generated filenames
            List<string> generatedFiles = new List<string>();
            string sourceFilename = S.GetOutputPath(G25.CG.C.TestSuite.GetRawTestSuiteFilename(S));
            generatedFiles.Add(sourceFilename);

            // reset code in cgd (get rid of all the code that was generated for the regular non-testsuite output)
            cgd.ResetSB();

            // get StringBuilder where all generated code goes
            StringBuilder SB = new StringBuilder();

            // #include all relevant headers
            SB.AppendLine("#include <time.h> /* used to seed random generator */");
            SB.AppendLine("#include \"" + S.GetOutputFilename(G25.CG.C.Header.GetRawHeaderFilename(S)) + "\"");
            if (S.m_parserType != PARSER.NONE)
            {
                //                        SB.AppendLine("#include \"" + S.GetOutputFilename(G25.CG.C.Parser.GetRawParserSourceFilename(S)) + "\"");
                if (S.m_parserType == PARSER.ANTLR)
                {
                    SB.AppendLine("#include \"" + S.GetOutputFilename(G25.CG.C.Parser.GetANTLRlexerHeaderFilename(S)) + "\"");
                    SB.AppendLine("#include \"" + S.GetOutputFilename(G25.CG.C.Parser.GetANTLRparserHeaderFilename(S)) + "\"");
                }
            }
            if (cgd.GetFeedback(G25.CG.C.MainGenerator.MERSENNE_TWISTER) == "true")
                SB.AppendLine("#include \"" + S.GetOutputFilename(G25.CG.C.RandomMT.GetRawMtHeaderFilename(S)) + "\"");

            // generate declarations for parts of the geometric product, dual, etc (works in parallel internally)
            try
            {
                bool declOnly = true;
                G25.CG.Shared.PartsCode.GeneratePartsCode(S, cgd, declOnly);
            }
            catch (G25.UserException E) { cgd.AddError(E); }

            // generate extra declarations
            foreach (FloatType FT in S.m_floatTypes)
            {
                // output internal compress function for all float types
                cgd.m_cog.EmitTemplate(cgd.m_defSB, "compress_decl",
                    "S=", S,
                    "FT=", FT,
                    "gmv=", S.m_GMV);
            }

            cgd.m_cog.EmitTemplate(cgd.m_defSB, "swapPointerDecl");
            

            // reset generated code (StringBuilders) of all CGDs    
            for (int i = 0; i < FGI.m_functionFGS.Count; i++)
                if (FGI.m_functionGenerators[i] != null)
                    FGI.m_functionGenerators[i].ResetCGdata();

            // figure out all dependencies
            // Note that m_errors and m_missingDependencies of each function cgd are shared with main cgd, but a mutex is used to avoid racing
            {
                // serial:
                for (int i = 0; i < FGI.m_functionFGS.Count; i++)
                {
                    if (FGI.m_functionGenerators[i] != null)
                    {
                        FGI.m_functionGenerators[i].CheckTestingDepenciesEntryPoint();
                    }
                }/*
                // in parallel:
                Thread[] depFunctionThreads = new Thread[FGI.m_functionFGS.Count];
                for (int i = 0; i < FGI.m_functionFGS.Count; i++)
                {
                    if (FGI.m_functionGenerators[i] != null)
                    {
                        depFunctionThreads[i] = new Thread(FGI.m_functionGenerators[i].CheckTestingDepenciesEntryPoint);
                    }
                }
                G25.CG.Shared.Threads.StartThreadArray(depFunctionThreads);
                G25.CG.Shared.Threads.JoinThreadArray(depFunctionThreads);*/
            }

            // get random number generator FGS for each float type
            List<string> randomNumberGenerators = new List<string>();
            List<string> randomNumberTimeSeedFuncs = new List<string>();
            foreach (FloatType FT in S.m_floatTypes)
            {
                //bool returnTrueName = true;
                string funcName = G25.CG.Shared.Dependencies.GetDependency(S, cgd, "random_" + FT.type, new String[0], FT, null);
                randomNumberGenerators.Add(funcName);
                randomNumberTimeSeedFuncs.Add(funcName + "_timeSeed");
            }

            // for parsing test, subtract is required (first float type):
            string subtractGmvFuncName = G25.CG.Shared.Dependencies.GetDependency(S, cgd, "subtract", new String[] { S.m_GMV.Name, S.m_GMV.Name }, S.m_GMV.Name, S.m_floatTypes[0], null);
            string randomVersorFuncName = G25.CG.Shared.Dependencies.GetDependency(S, cgd, "random_versor", new String[0], S.m_GMV.Name, S.m_floatTypes[0], null);

            // for metric / getter setter:
            Dictionary<string, string> randomVersorFuncNames = new Dictionary<string, string>();
            Dictionary<string, string> gpGmvFuncName = new Dictionary<string, string>();
            // geometric product of all metrics and float types:
            {
                foreach (FloatType FT in S.m_floatTypes)
                {
                    randomVersorFuncNames[FT.type] = G25.CG.Shared.Dependencies.GetDependency(S, cgd, "random_versor", new String[0], S.m_GMV.Name, FT, null);
                    foreach (Metric M in S.m_metric)
                        gpGmvFuncName[FT.type + "_" + M.m_name] = G25.CG.Shared.Dependencies.GetDependency(S, cgd, "gp", new String[] { S.m_GMV.Name, S.m_GMV.Name }, S.m_GMV.Name, FT, M.m_name);
                }
            }


            { // iteratively get all dependencies and generate their code
                int count = 0;
                List<fgs> missingFunctions = null, alreadyGeneratedMissingFunctions = new List<fgs>();
                do
                {
                    count = cgd.m_missingDependencies.Count; // we loop until the number of dependencies doesn't grow anymore
                    missingFunctions = cgd.GetMissingDependenciesList(alreadyGeneratedMissingFunctions);
                    G25.CG.Shared.Functions.WriteFunctions(S, cgd, null, Functions.GetFunctionGeneratorPlugins(cgd), missingFunctions);
                    alreadyGeneratedMissingFunctions.AddRange(missingFunctions);
                } while (count < cgd.m_missingDependencies.Count);
            }

            // write code for all dependencies to output
            SB.AppendLine("// Missing dependencies declarations:");
            SB.Append(cgd.m_declSB);
            SB.AppendLine("// Missing dependencies inline definitions:");
            SB.Append(cgd.m_inlineDefSB);
            SB.AppendLine("// Missing dependencies definitions:");
            SB.Append(cgd.m_defSB);

            cgd.ResetSB();

            List<string> testFunctionNames = new List<string>(); // list of names of bool functionName(void) goes here

            { // write all 'other' test code
                // metric
                testFunctionNames.AddRange(WriteMetricTests(S, cgd, gpGmvFuncName));

                // converters
                // todo . . .

                // get / set coordinates
                testFunctionNames.AddRange(WriteGetterSetterTests(S, cgd, randomNumberGenerators, randomVersorFuncNames));

                // toString & parser (if available)
                if (S.m_parserType != PARSER.NONE)
                    testFunctionNames.Add(WriteParserTest(S, cgd, randomNumberGenerators[0], randomVersorFuncName, subtractGmvFuncName));


            }


            { // write all test functions
                // old, serial code:
                /*for (int i = 0; i < FGI.m_functionFGS.Count; i++)
                {
                    if (FGI.m_functionGenerators[i] != null)
                    {
                        FGI.m_functionGenerators[i].WriteTestFunctionEntryPoint();
                    }
                }*/
                { // in parallel:
                    Thread[] testFunctionThreads = new Thread[FGI.m_functionFGS.Count];
                    for (int i = 0; i < FGI.m_functionFGS.Count; i++)
                    {
                        if (FGI.m_functionGenerators[i] != null)
                        {
                            testFunctionThreads[i] = new Thread(FGI.m_functionGenerators[i].WriteTestFunctionEntryPoint);
                        }
                    }
                    G25.CG.Shared.Threads.StartThreadArray(testFunctionThreads);
                    G25.CG.Shared.Threads.JoinThreadArray(testFunctionThreads);
                }

                // collect all the results from the threads:
                for (int f = 0; f < FGI.m_functionFGS.Count; f++)
                {
                    if (FGI.m_functionGenerators[f] != null)
                    {
                        if (FGI.m_functionCgd[f].m_generatedTestFunctions != null)
                            testFunctionNames.AddRange(FGI.m_functionCgd[f].m_generatedTestFunctions);

                        cgd.m_declSB.Append(FGI.m_functionCgd[f].m_declSB);
                        cgd.m_defSB.Append(FGI.m_functionCgd[f].m_defSB);
                        cgd.m_inlineDefSB.Append(FGI.m_functionCgd[f].m_inlineDefSB);
                    }
                }
            }

            // write code for all testing code to output
            SB.AppendLine("// Testing code declarations:");
            SB.Append(cgd.m_declSB);
            SB.AppendLine("// Testing code inline definitions:");
            SB.Append(cgd.m_inlineDefSB);
            SB.AppendLine("// Testing code definitions:");
            SB.Append(cgd.m_defSB);

            // write main function
            cgd.m_cog.EmitTemplate(SB, "testSuiteMain",
                "S=", S,
                "testFunctionNames=", testFunctionNames.ToArray(),
                "randomNumberSeedFunctionNames=", randomNumberTimeSeedFuncs.ToArray());

            // write all to file
            G25.CG.Shared.Util.WriteFile(sourceFilename, SB.ToString());

            return generatedFiles;
        } // end of GenerateCode()

        /// <summary>
        /// Writes test code for multivector parser and toString() to <c>cgd.m_defSB</c>.
        /// </summary>
        /// <param name="S">Specification of algebra.</param>
        /// <param name="cgd">Code generation data (used for <c>m_cog</c> and <c>m_defSB</c>.</param>
        /// <param name="randomNumberGeneratorFuncName">Function for random number generator (main float type).</param>
        /// <param name="randomVersorFuncName">Function for random versor (main float type).</param>
        /// <param name="subtractGmvFuncName">Function for subtracting general multivectors (main float type).</param>
        /// <returns></returns>
        public static string WriteParserTest(G25.Specification S, G25.CG.Shared.CGdata cgd, 
            string randomNumberGeneratorFuncName, 
            string randomVersorFuncName, 
            string subtractGmvFuncName)
        {
            string testFuncName = "test_parse_" + S.m_GMV.Name;

            cgd.m_cog.EmitTemplate(cgd.m_defSB, "testParse",
                "S=", S,
                "FT=", S.m_floatTypes[0],
                "gmvName=", S.m_floatTypes[0].GetMangledName(S, S.m_GMV.Name),
                "testFuncName=", testFuncName,
                "targetFuncName=", "parse_" + S.m_GMV.Name,
                "randomScalarFuncName=", randomNumberGeneratorFuncName,
                "randomVersorFuncName=", randomVersorFuncName,
                "subtractGmvFuncName=", subtractGmvFuncName
                );
            return testFuncName;
        } // end of WriteParserTest()

        public static List<string> WriteMetricTests(G25.Specification S, G25.CG.Shared.CGdata cgd,
            Dictionary<string, string> gpGmvFuncName) // , string gpFuncName
        {
            List<string> testFunctionNames = new List<string>();

            foreach (FloatType FT in S.m_floatTypes)
            {
                //string testStr = FT.DoubleToString(S, FT.PrecisionEpsilon());

                foreach (Metric M in S.m_metric)
                {
                    string gmvName = FT.GetMangledName(S, S.m_GMV.Name);
                    string testFuncName = "test_metric_" + M.m_name + "_" + gmvName;

                    cgd.m_cog.EmitTemplate(cgd.m_defSB, "testMetric",
                        "S=", S,
                        "FT=", FT,
                        "M=", M, 
                        "gmv=", S.m_GMV,
                        "gmvName=", gmvName,
                        "testFuncName=", testFuncName,
                        "gpGmvFuncName=", gpGmvFuncName[FT.type + "_" + M.m_name]
                        );
                    testFunctionNames.Add(testFuncName);
                }
            }

            return testFunctionNames;
        } // WriteMetricTests()

        public static List<string> WriteGetterSetterTests(G25.Specification S, G25.CG.Shared.CGdata cgd,
            List<string> randomNumberGeneratorFuncName, 
            Dictionary<string, string> randomVersorFuncName) // , string gpFuncName
        {
            List<string> testFunctionNames = new List<string>();
            int FTidx = 0;
            //G25.GMV gmv = S.m_GMV;

            foreach (FloatType FT in S.m_floatTypes)
            {
                //string testStr = FT.DoubleToString(S, FT.PrecisionEpsilon());

                string gmvName = FT.GetMangledName(S, S.m_GMV.Name);
                string testFuncName = "test_getter_setter_" + gmvName;

                cgd.m_cog.EmitTemplate(cgd.m_defSB, "testGetterSetter",
                    "S=", S,
                    "FT=", FT,
                    "gmv=", S.m_GMV,
                    "gmvName=", gmvName,
                    "testFuncName=", testFuncName,
                    "randomScalarFuncName=", randomNumberGeneratorFuncName[FTidx],
                    "randomVersorFuncName=", randomVersorFuncName[FT.type]
                    );
                testFunctionNames.Add(testFuncName);


                FTidx++;
            }

            return testFunctionNames;
        } // WriteGetterSetterTests()

    } // end of class TestSuite
} // end of namespace G25.CG.C


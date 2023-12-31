﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace g25_cg_cpp.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("g25_cg_cpp.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // arguments: 
        ///// Namespace = name of algebra.
        ///// Gaigen = name and version of Gaigen
        ///// License = full text of license.
        ///${CODEBLOCK doxyMainPage}
        ////*! \mainpage &lt;%Namespace%&gt; documentation
        /// *
        /// * &lt;%Namespace%&gt; implementation generated by &lt;%Gaigen%&gt;. 
        /// * 
        /// * 
        /// * License: 
        ///&lt;%License%&gt;  
        /// * 
        /// * \section intro_sec Introduction
        /// *
        /// * Todo
        /// * 
        /// */
        ///${ENDCODEBLOCK}
        ///
        ///
        ///// S = Specification of algebra.
        ///${CODEBLOCK basicInfo}
        ///
        ////// The dimension of the space:
        ///extern const int &lt;%S.m_namespace%&gt;_sp [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string cg_cpp_templates {
            get {
                return ResourceManager.GetString("cg_cpp_templates", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // S = Specification
        ///// testFunctionNames = array of string of names of test functions
        ///// randomNumberSeedFunctionNames = array of string of names of random number generator seed-with-current-time functions
        ///${CODEBLOCK testSuiteMain}
        ///int main(int argc, char *argv[]) {
        ///	int retVal = 0;
        ///	// The number of tests will be proportional to this value.
        ///	// This should become a command-line option
        ///	const int NB_TESTS_SCALER = 16384;
        ///	
        ///	// seed random number generators with current time
        ///&lt;%foreach(string fun [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string cg_cpp_test_templates {
            get {
                return ResourceManager.GetString("cg_cpp_test_templates", resourceCulture);
            }
        }
    }
}

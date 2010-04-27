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

namespace G25
{
    namespace CG
    {
        namespace Python
        {
            public class MainGenerator : G25.CodeGenerator
            {
                /// <returns>what language this code generator generates for.</returns>
                public String Language()
                {
                    return G25.Specification.XML_PYTHON;
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
                    return new List<string>();
                }
            }
        } // end of namespace 'Python'
    } // end of namespace CG
} // end of namespace G25
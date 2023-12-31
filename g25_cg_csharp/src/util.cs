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

namespace G25.CG.CSharp
{

    /// <summary>
    /// Utility class for C# code generation
    /// </summary>
    class Util
    {
        /// <summary>
        /// Writes generic 'using' statements (at start of .cs source file).
        /// </summary>
        public static void WriteGenericUsing(StringBuilder SB, Specification S)
        {
            SB.AppendLine("using System;");
        }

    } // end of class Util

} // end of namespace G25.CG.CSharp 

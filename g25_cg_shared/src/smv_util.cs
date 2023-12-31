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

using RefGA.Symbolic;

namespace G25.CG.Shared
{
    
    /// <summary>
    /// Contains various utility functions for generating SMV code.
    /// </summary>
    public class SmvUtil
    {

        public const string THIS = "this";
        public const string COORDINATE_ORDER_ENUM = "CoordinateOrder";

        /// <summary>
        /// Returns the name of the constant as first argument to functions which have coordinate arguments.
        /// </summary>
        /// <param name="S"></param>
        /// <param name="smv"></param>
        public static string GetCoordinateOrderConstant(Specification S, G25.SMV smv)
        {
            StringBuilder SB = new StringBuilder("coord");
            string wedgeSymbol = ""; // no symbol for wedge
            for (int i = 0; i < smv.NbNonConstBasisBlade; i++)
            {
                SB.Append("_");
                SB.Append(smv.NonConstBasisBlade(i).ToLangString(S.m_basisVectorNames, wedgeSymbol));
            }
            return SB.ToString();
        }

        public static Dictionary<string, int> GetSpecializedTypeDictionary(Specification S)
        {
            Dictionary<string, int> D = new Dictionary<string, int>();

            int idx = 0;

            // gmv
            D[S.m_GMV.Name] = idx;
            idx++;

            // float types
            foreach (FloatType FT in S.m_floatTypes)
            {
                D[FT.GetName()] = idx;
                idx++;
            }

            // specialized types
            foreach (G25.SMV smv in S.m_SMV)
            {
                D[smv.GetName()] = idx;
                idx++;
            }

            return D;

        }

        /// <summary>
        /// Returns string to access coordinates number 'coordIdx'.
        /// </summary>
        /// <param name="S"></param>
        /// <param name="smv"></param>
        /// <param name="coordIdx"></param>
        /// <returns></returns>
        public static string GetCoordAccessString(Specification S, G25.SMV smv, int coordIdx)
        {
            if (S.m_coordStorage == COORD_STORAGE.VARIABLES)
            {
                return "m_" + smv.GetCoordLangID(coordIdx, S);
            }
            else
            {
                return "m_c[" + coordIdx + "]";
            }
        }



    } // end of class SmvUtil
} // end of namepace G25.CG.Shared

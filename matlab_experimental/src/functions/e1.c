/* This file is part of GAMatlab.
Copyright (C) 2003  Industrial Research Limited

GAMatlab is free software; you can redistribute it and/or modify it
under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation; either version 2.1 of the License, or (at
your option) any later version.

GAMatlab is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation,
Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA */

/* 
DO NOT EDIT THIS FILE! It is auto generated and will be overwritten
the next time the generator is run
*/

#include "mexFunction01.c"

mxArray* calc(void) {
	mv X;
	mv_setZero(&X);
	mv_set_e1(&X, 1.0);
	return createMxArrayFromGA(&X);
}



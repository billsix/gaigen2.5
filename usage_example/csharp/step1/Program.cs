﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using c3ga_ns;

namespace c3ga_csharp_example_step1
{
    class Program
    {
        /*
        This small example creates a line and a plane from points.
        It then computes the intersection point of the line and the plane.

        In this step, all GA variables are stored in 'mv' (the multivector type).
        */
        static void Main(string[] args)
        {
            // get five points
            mv linePt1 = c3ga.cgaPoint(1.0f, 0.0f, 0.0f);
            mv linePt2 = c3ga.cgaPoint(1.0f, 1.0f, 0.0f);
            mv planePt1 = c3ga.cgaPoint(1.0f, 2.0f, 0.0f);
            mv planePt2 = c3ga.cgaPoint(1.0f, 2.0f, 1.0f);
            mv planePt3 = c3ga.cgaPoint(0.0f, 2.0f, 1.0f);

            // output text the can be copy-pasted into GAViewer
            Console.WriteLine("linePt1 = " + linePt1 + ",");
            Console.WriteLine("linePt2 = " + linePt2 + ",");
            Console.WriteLine("planePt1 = " + planePt1 + ",");
            Console.WriteLine("planePt2 = " + planePt2 + ",");
            Console.WriteLine("planePt3 = " + planePt3 + ",");

            // create line and plane out of points
            mv L = linePt1 ^ linePt2 ^ c3ga.ni;
            mv P = planePt1 ^ planePt2 ^ planePt3 ^ c3ga.ni;

            // output text the can be copy-pasted into GAViewer
            Console.WriteLine("L = " + L + ",");
            Console.WriteLine("P = " + P + ",");

            // compute intersection of line and plane
            mv intersection = c3ga.lc(c3ga.dual(L), P);

            Console.WriteLine("intersection = " + intersection + ",");
        }
    }
}

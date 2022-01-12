using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;

namespace RDR
{
    class Geometry
    {
        static public Vec3 RandomDirection()
        {
            // Generates a random 3D unit vector (direction) with a uniform spherical distribution
            //  Algo from http://stackoverflow.com/questions/5408276/python-uniform-spherical-distribution
            Random random = new Random();
            double phi = random.NextDouble() * 2.0f * Math.PI; // (0, 2 PI))
            double costheta = random.NextDouble() * 2f - 1f; // (-1, 1)
            double theta = Math.Acos(costheta);
            double x = Math.Sin(theta) * Math.Cos(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(theta);

            return new Vec3((float)x, (float)y, (float)z);
        }
    }
}

using System;
using System.Numerics;

namespace cl_showpos.Utils; 

public static class Utils { 
    private static float RoundDown(float i, double decimalPlaces) {
        var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
        return (float) (Math.Truncate((decimal) i * power) / power);
    }
    
    public static string ToString(this Vector3 vec, int precision, bool floor = false) {
        float x = vec.X, y = vec.Y, z = vec.Z;
        
        if (floor) {
            x = RoundDown(x, precision);
            y = RoundDown(y, precision);
            z = RoundDown(z, precision);
        }
        
        return $"{x.ToString($"F{precision}")} " +
               $"{y.ToString($"F{precision}")} " +
               $"{z.ToString($"F{precision}")}";
    }
}
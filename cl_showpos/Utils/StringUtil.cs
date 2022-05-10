using System.Numerics;

namespace cl_showpos.Utils; 

public static class StringUtil {
    public static string ToString(this Vector3 vec, int precision) {
        return $"{vec.X.ToString($"F{precision}")} " +
               $"{vec.Y.ToString($"F{precision}")} " +
               $"{vec.Z.ToString($"F{precision}")}";
    }
}
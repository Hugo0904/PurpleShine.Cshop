using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FalseApple.Core.Libraries.Mouse;

namespace FalseApple.Core.Libraries
{
    public static class NativeMethods
    {
        //
    }

    public static class SafeNativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern Int32 SendInput(Int32 cInputs, ref INPUT pInputs, Int32 cbSize);
    }

    public static class UnsafeNativeMethods
    {
        //
    }
}

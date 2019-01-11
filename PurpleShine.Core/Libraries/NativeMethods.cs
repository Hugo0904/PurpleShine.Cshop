using System;
using System.Runtime.InteropServices;
using System.Security;
using static PurpleShine.Core.Libraries.Mouse;

namespace PurpleShine.Core.Libraries
{
    public static class NativeMethods
    {
        //
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    public static class SafeNativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendInput(int cInputs, ref INPUT pInputs, int cbSize);

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        public static extern int CoInternetSetFeatureEnabled(int FeatureEntry,
                                                  [MarshalAs(UnmanagedType.U4)] int dwFlags,
                                                  bool fEnable);

        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetProcessWorkingSetSize32(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetProcessWorkingSetSize64(IntPtr pProcess, long dwMinimumWorkingSetSize, long dwMaximumWorkingSetSize);

        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetCurrentProcess();
    }

    public static class UnsafeNativeMethods
    {
        //
    }
}

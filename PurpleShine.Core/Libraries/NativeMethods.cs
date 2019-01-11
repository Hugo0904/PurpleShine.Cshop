using System;
using System.Runtime.InteropServices;
using System.Security;
using static PurpleShine.Core.Libraries.Mouse;

namespace PurpleShine.Core.Libraries
{
    /// <summary>
    /// DLL 引用類別
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

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

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow(this object o);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindow", SetLastError = true)]
        internal static extern IntPtr GetNextWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.U4)] int wFlag);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        internal static extern int GetSystemMetrics(int which);

        [DllImport("user32.dll")]
        internal static extern void SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int X, int Y, int width, int height, uint flags);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern bool DwmIsCompositionEnabled();
    }

    public static class UnsafeNativeMethods
    {
        //
    }
}

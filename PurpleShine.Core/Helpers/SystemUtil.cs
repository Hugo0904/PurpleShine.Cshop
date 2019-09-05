using System;
using System.Management;
using PurpleShine.Core.Libraries;

namespace PurpleShine.Core.Helpers
{
    public static class SystemUtil
    {
        /// <summary>
        /// 利用縮小至工作列原理釋放內存
        /// </summary>
        public static void ReleaseMemory()
        {
            GC.Collect();
            //GC.WaitForPendingFinalizers();
            IntPtr pHandle = SafeNativeMethods.GetCurrentProcess();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SafeNativeMethods.SetProcessWorkingSetSize32(pHandle, -1, -1);
            }
            else
            {
                SafeNativeMethods.SetProcessWorkingSetSize64(pHandle, -1, -1);
            }

        }

        /// <summary>
        /// 取得機器唯一識別碼
        /// 使用CPU編號+主機板編號+WINDOW安裝編號
        /// </summary>
        /// <returns></returns>
        public static string GetMachineID()
        {
            string machineId = "";
            using (ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_Processor"))
            using (ManagementObjectCollection mbsList = mbs.Get())
            {
                foreach (ManagementObject mo in mbsList)
                {
                    machineId += mo["ProcessorID"].ToString();
                }
            }

            using (ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_BaseBoard"))
            using (ManagementObjectCollection mbsList = mbs.Get())
            {
                foreach (ManagementObject mo in mbsList)
                {
                    machineId += mo["SerialNumber"].ToString();
                }
            }

            using (ManagementObject os = new ManagementObject("Win32_OperatingSystem=@"))
            {
                machineId += os["SerialNumber"].ToString();
            }

            return Security.MD5(machineId + IPGet.IPV4());
        }
    }
}

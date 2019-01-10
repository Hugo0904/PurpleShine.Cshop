using System;
using System.Diagnostics;
using System.Reflection;

namespace StackFalse.Core.Expansion
{
    public static class SystemExpansion
    {
        /// <summary>
        /// 元件版本
        /// Assembly.GetExecutingAssembly()
        /// </summary>
        public static string GetVersion(this Assembly @this)
        {
            Assembly assembly = @this;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        /// <summary>
        /// 移除該物件內某個事件所有監聽者
        /// </summary>
        /// <param name="this">要移除的物件</param>
        /// <param name="eventName">事件名稱</param>
        public static void ClearEvent(this object @this, string eventName)
        {
            FieldInfo _Field = @this.GetType().GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (_Field.IsNonNull())
            {
                object _FieldValue = _Field.GetValue(@this);
                if (_FieldValue.IsNonNull() && _FieldValue is Delegate)
                {
                    Delegate _ObjectDelegate = (Delegate)_FieldValue;
                    foreach (Delegate del in _ObjectDelegate.GetInvocationList())
                    {
                        @this.GetType().GetEvent(eventName).RemoveEventHandler(@this, del);
                    }
                }
            }
        }
    }
}

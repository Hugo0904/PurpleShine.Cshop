using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using PurpleShine.Core.Expansions;

namespace PurpleShine.Core.Libraries
{
    [Serializable]
    public class SerializableIO
    {
        #region Singleton Pattern
        public static SerializableIO Instance { get; }

        static SerializableIO()
        {
            Instance = new SerializableIO();
        }
        #endregion

        private readonly string filePath = Environment.CurrentDirectory + @"/Serializa";

        public List<Setting> Settings { get; } = new List<Setting>();

        private SerializableIO()
        {
            LoadSettings();
        }

        #region setting
        /// <summary>
        /// 取得指定設定檔
        /// </summary>
        /// <param name="setting_name">檔案名稱</param>
        /// <returns>T class</returns>
        public T GetSetting<T>() where T : class
        {
            var _setting = from a in Settings where typeof(T) == a.GetType() select a;

            if (_setting.Any())
                return (T) Convert.ChangeType(_setting.First(), typeof(T));
            else
                return null;
        }

        /// <summary>
        /// 載入所有設定檔
        /// </summary>
        public void LoadSettings()
        {
            if (File.Exists(filePath))
            {
                #region read settings
                using (FileStream oFileStream = new FileStream(filePath, FileMode.Open))
                {
                    if (oFileStream.Length > 0)
                    {
                        BinaryFormatter myBinaryFormatter = new BinaryFormatter();
                        Settings.Clear();
                        List<Setting> _list = ((SerializableIO) myBinaryFormatter.Deserialize(oFileStream)).Settings;
                        if (_list.IsNonNull() && _list.Any())
                            Settings.AddRange(_list);
                    }
                    oFileStream.Flush();
                }
                #endregion
            }
            else
            {
               File.Create(filePath).Close();
            }
        }

        /// <summary>
        /// 儲存所有設定檔案
        /// </summary>
        public void SaveSettings()
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter myBinaryFormatter = new BinaryFormatter();
                myBinaryFormatter.Serialize(fileStream, this);
            }
        }

        /// <summary>
        /// 儲存指定Setting檔至file
        /// </summary>
        /// <param name="s">設定檔</param>
        public void SaveSetting(Setting s)
        {
           if ( ! Settings.Exists(i=>i.GetType() == s.GetType()))
            {
                Settings.Add(s);
            }
            SaveSettings();
        }
        #endregion
    }

    [Serializable]
    public class Setting
    {

    }

    /// <summary>
    /// Setting的擴充方法
    /// </summary>
    public static class SettingExpansion
    {
        // 儲存檔案
        public static void Save(this Setting setting)
        {
            SerializableIO.Instance.SaveSetting(setting);
        }
    }
}

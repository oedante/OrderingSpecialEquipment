using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OrderingSpecialEquipment.Utils
{
    public static class SettingsHelper
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OrderingSpecialEquipment",
            "settings.dat");

        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("OrderingSpecialEquipmentSettings");

        public static void SaveSetting(string key, string value)
        {
            try
            {
                var settings = LoadAllSettings();
                settings[key] = value;
                SaveAllSettings(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настройки: {ex.Message}");
            }
        }

        public static string LoadSetting(string key, string defaultValue = "")
        {
            try
            {
                var settings = LoadAllSettings();
                return settings.ContainsKey(key) ? settings[key] : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static System.Collections.Generic.Dictionary<string, string> LoadAllSettings()
        {
            if (!File.Exists(SettingsPath))
                return new System.Collections.Generic.Dictionary<string, string>();

            try
            {
                var encryptedData = File.ReadAllBytes(SettingsPath);
                var data = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);
                var json = Encoding.UTF8.GetString(data);
                return System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(json)
                    ?? new System.Collections.Generic.Dictionary<string, string>();
            }
            catch
            {
                return new System.Collections.Generic.Dictionary<string, string>();
            }
        }

        private static void SaveAllSettings(System.Collections.Generic.Dictionary<string, string> settings)
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = System.Text.Json.JsonSerializer.Serialize(settings);
            var data = Encoding.UTF8.GetBytes(json);
            var encryptedData = ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(SettingsPath, encryptedData);
        }
    }
}
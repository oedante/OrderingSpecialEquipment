using Microsoft.Win32;
using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace OrderingSpecialEquipment.Helpers
{
    /// <summary>
    /// Класс для безопасного шифрования/дешифрования строки подключения
    /// Использует DPAPI для защиты ключа шифрования
    /// </summary>
    public static class ConnectionSecurityHelper
    {
        private const string RegistryPath = @"SOFTWARE\OrderingSpecialEquipment";
        private const string EncryptedKey = "EncryptedConnection";
        private const string MachineKey = "MachineKey";

        /// <summary>
        /// Сохраняет зашифрованную строку подключения в реестр
        /// </summary>
        public static void SaveConnectionString(string connectionString)
        {
            try
            {
                // Генерируем случайный ключ для шифрования
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.GenerateKey();

                    // Шифруем строку подключения
                    byte[] encryptedData = EncryptStringToBytes(connectionString, aes.Key, aes.IV);

                    // Шифруем ключ с использованием DPAPI (привязан к текущей машине)
                    byte[] encryptedKey = ProtectedData.Protect(aes.Key, null, DataProtectionScope.LocalMachine);

                    // Сохраняем в реестр
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                    {
                        key.SetValue(EncryptedKey, Convert.ToBase64String(encryptedData));
                        key.SetValue(MachineKey, Convert.ToBase64String(encryptedKey));
                        key.SetValue("IV", Convert.ToBase64String(aes.IV));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения строки подключения: {ex.Message}");
                throw new SecurityException("Не удалось сохранить защищенные данные", ex);
            }
        }

        /// <summary>
        /// Загружает и расшифровывает строку подключения из реестра
        /// </summary>
        public static string LoadConnectionString()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key == null)
                        throw new SecurityException("Настройки подключения не найдены");

                    // Загружаем зашифрованные данные
                    string encryptedDataStr = key.GetValue(EncryptedKey) as string;
                    string encryptedKeyStr = key.GetValue(MachineKey) as string;
                    string ivStr = key.GetValue("IV") as string;

                    if (string.IsNullOrEmpty(encryptedDataStr) ||
                        string.IsNullOrEmpty(encryptedKeyStr) ||
                        string.IsNullOrEmpty(ivStr))
                        throw new SecurityException("Повреждены защищенные данные");

                    // Расшифровываем ключ
                    byte[] encryptedKey = Convert.FromBase64String(encryptedKeyStr);
                    byte[] key = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.LocalMachine);

                    // Расшифровываем данные
                    byte[] encryptedData = Convert.FromBase64String(encryptedDataStr);
                    byte[] iv = Convert.FromBase64String(ivStr);

                    return DecryptStringFromBytes(encryptedData, key, iv);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки строки подключения: {ex.Message}");
                throw new SecurityException("Не удалось загрузить защищенные данные", ex);
            }
        }

        /// <summary>
        /// Проверяет наличие сохраненной строки подключения
        /// </summary>
        public static bool HasSavedConnection()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                return key != null &&
                       key.GetValue(EncryptedKey) != null &&
                       key.GetValue(MachineKey) != null;
            }
        }

        /// <summary>
        /// Удаляет сохраненные данные подключения
        /// </summary>
        public static void ClearSavedConnection()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKey(RegistryPath, false);
            }
            catch
            {
                // Игнорируем ошибки при удалении
            }
        }

        #region Вспомогательные методы шифрования

        private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
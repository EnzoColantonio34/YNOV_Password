using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YNOV_Password.Services
{
    public class EncryptionService
    {
        private readonly byte[] _key;
        
        public EncryptionService(string masterPassword)
        {
            // Génère une clé de 256 bits à partir du mot de passe maître
            using var sha256 = SHA256.Create();
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(masterPassword));
        }
        
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
                
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            
            swEncrypt.Write(plainText);
            swEncrypt.Close();
            
            var encrypted = msEncrypt.ToArray();
            
            // Combine IV + données chiffrées et encode en Base64
            var result = new byte[aes.IV.Length + encrypted.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            
            return Convert.ToBase64String(result);
        }
        
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;
                
            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);
                
                using var aes = Aes.Create();
                aes.Key = _key;
                
                // Extrait IV et données chiffrées
                var iv = new byte[aes.IV.Length];
                var cipher = new byte[fullCipher.Length - iv.Length];
                
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);
                
                aes.IV = iv;
                
                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(cipher);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                
                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Erreur lors du déchiffrement: {ex.Message}");
                return cipherText; // Retourne le texte original en cas d'erreur
            }
        }
        
        /// <summary>
        /// Vérifie si une chaîne semble être un texte chiffré valide
        /// </summary>
        public bool IsEncrypted(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            try
            {
                // Vérifie si c'est du Base64 valide et de taille appropriée
                var bytes = Convert.FromBase64String(text);
                return bytes.Length > 16; // Au moins IV (16 bytes) + quelques données
            }
            catch
            {
                return false;
            }
        }
    }
}

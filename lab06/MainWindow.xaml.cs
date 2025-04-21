using Microsoft.Win32;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoAppWpf
{
    public partial class MainWindow : Window
    {
        private static readonly byte[] _salt = Encoding.UTF8.GetBytes("B5K_La6_5a1t!");
        private const int KeySize = 256; 
        private const int IvSize = 128;  
        private const int Iterations = 10000; 

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- AES

        private void EncryptTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string password = AesPasswordBox.Password;
                string plainText = PlainTextTextBox.Text;

                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(plainText))
                {
                    MessageBox.Show("Please enter both password and text to encrypt.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                EncryptedTextBox.Text = EncryptStringToBase64(plainText, password);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encryption failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DecryptTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string password = AesPasswordBox.Password;
                string encryptedBase64Text = EncryptedTextBox.Text;

                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(encryptedBase64Text))
                {
                    MessageBox.Show("Please enter both password and encrypted text.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                PlainTextTextBox.Text = DecryptStringFromBase64(encryptedBase64Text, password);
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show($"Decryption failed. Check password or input data. Details: {ex.Message}", "Decryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Decryption failed. Input data is not valid Base64. Details: {ex.Message}", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decryption failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- AES 

        private async void EncryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            string password = AesPasswordBox.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a password first.", "Password Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select File to Encrypt";
            if (openFileDialog.ShowDialog() == true)
            {
                string inputFile = openFileDialog.FileName;
                FilePathTextBlockAes.Text = $"Selected: {Path.GetFileName(inputFile)}";

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save Encrypted File As...";
                saveFileDialog.Filter = "Encrypted files (*.enc)|*.enc|All files (*.*)|*.*";
                saveFileDialog.FileName = Path.GetFileName(inputFile) + ".enc"; // Suggest name

                if (saveFileDialog.ShowDialog() == true)
                {
                    string outputFile = saveFileDialog.FileName;
                    try
                    {
                        // Use Task.Run to avoid blocking the UI thread for file I/O
                        await Task.Run(() => EncryptFile(inputFile, outputFile, password));
                        MessageBox.Show($"File encrypted successfully to:\n{outputFile}", "Encryption Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        FilePathTextBlockAes.Text = "Encryption complete.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"File encryption failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        FilePathTextBlockAes.Text = "Encryption failed.";
                    }
                }
                else { FilePathTextBlockAes.Text = "Save cancelled."; }
            }
            else { FilePathTextBlockAes.Text = "No file selected."; }
        }


        private async void DecryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            string password = AesPasswordBox.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a password first.", "Password Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select File to Decrypt";
            openFileDialog.Filter = "Encrypted files (*.enc)|*.enc|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string inputFile = openFileDialog.FileName;
                FilePathTextBlockAes.Text = $"Selected: {Path.GetFileName(inputFile)}";

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save Decrypted File As...";

                string suggestedName = Path.GetFileNameWithoutExtension(inputFile);
                if (suggestedName.EndsWith(".enc", StringComparison.OrdinalIgnoreCase)) 
                {
                    suggestedName = Path.GetFileNameWithoutExtension(suggestedName);
                }
                saveFileDialog.FileName = suggestedName;

                if (saveFileDialog.ShowDialog() == true)
                {
                    string outputFile = saveFileDialog.FileName;
                    try
                    {
                        // Use Task.Run for file I/O
                        await Task.Run(() => DecryptFile(inputFile, outputFile, password));
                        MessageBox.Show($"File decrypted successfully to:\n{outputFile}", "Decryption Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        FilePathTextBlockAes.Text = "Decryption complete.";
                    }
                    catch (CryptographicException ex)
                    {
                        MessageBox.Show($"File decryption failed. Check password or file integrity. Details: {ex.Message}", "Decryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        FilePathTextBlockAes.Text = "Decryption failed.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"File decryption failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        FilePathTextBlockAes.Text = "Decryption failed.";
                    }
                }
                else { FilePathTextBlockAes.Text = "Save cancelled."; }
            }
            else { FilePathTextBlockAes.Text = "No file selected."; }
        }

        //SHA3-512 

        private void HashTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string inputText = ShaInputTextBox.Text;
                if (string.IsNullOrEmpty(inputText))
                {
                    ShaOutputTextBox.Text = string.Empty;
                    return;
                }
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputText);
                byte[] hashBytes = ComputeSha3_512(inputBytes);
                ShaOutputTextBox.Text = BytesToHexString(hashBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hashing failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void HashFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select File to Hash";
            if (openFileDialog.ShowDialog() == true)
            {
                string inputFile = openFileDialog.FileName;
                FilePathTextBlockSha.Text = $"Selected: {Path.GetFileName(inputFile)}";
                ShaFileOutputTextBox.Text = "Calculating..."; 

                try
                {
                    // Use Task.Run for file I/O and potentially long computation
                    byte[] hashBytes = await Task.Run(() => ComputeSha3_512ForFile(inputFile));
                    ShaFileOutputTextBox.Text = BytesToHexString(hashBytes);
                    FilePathTextBlockSha.Text = "Hashing complete.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"File hashing failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ShaFileOutputTextBox.Text = "Error during hashing.";
                    FilePathTextBlockSha.Text = "Hashing failed.";
                }
            }
            else
            {
                FilePathTextBlockSha.Text = "No file selected.";
                ShaFileOutputTextBox.Text = string.Empty;
            }
        }

     

        private byte[] GenerateKeyBytes(string password)
        {
           
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, _salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KeySize / 8);
            }
        }

        private string EncryptStringToBase64(string plainText, string password)
        {
            byte[] key = GenerateKeyBytes(password);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] iv = new byte[IvSize / 8]; // 16 bytes for 128 bits
            byte[] encryptedBytes;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7; 

                
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                }
                aesAlg.IV = iv; 

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                   
                    msEncrypt.Write(iv, 0, iv.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                        csEncrypt.FlushFinalBlock();
                    }
                    encryptedBytes = msEncrypt.ToArray();
                }
            }
            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptStringFromBase64(string cipherTextBase64, string password)
        {
            byte[] key = GenerateKeyBytes(password);
            byte[] cipherBytesWithIv = Convert.FromBase64String(cipherTextBase64);

            if (cipherBytesWithIv.Length <= IvSize / 8)
            {
                throw new ArgumentException("Ciphertext is too short to contain IV.", nameof(cipherTextBase64));
            }

            byte[] iv = new byte[IvSize / 8];
            byte[] cipherBytes = new byte[cipherBytesWithIv.Length - iv.Length];


            Buffer.BlockCopy(cipherBytesWithIv, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(cipherBytesWithIv, iv.Length, cipherBytes, 0, cipherBytes.Length);

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv; 
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8)) 
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }


        private void EncryptFile(string inputFile, string outputFile, string password)
        {
            byte[] key = GenerateKeyBytes(password);
            byte[] iv = new byte[IvSize / 8]; 
     
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv; 
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                {
                   
                    fsOut.Write(iv, 0, iv.Length);

                    using (CryptoStream csEncrypt = new CryptoStream(fsOut, encryptor, CryptoStreamMode.Write))
                    {
                        using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                        {
                            fsIn.CopyTo(csEncrypt); 
                        }
                        
                    }
                }
            }
        }

        private void DecryptFile(string inputFile, string outputFile, string password)
        {
            byte[] key = GenerateKeyBytes(password);
            byte[] iv = new byte[IvSize / 8]; 

            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
            {
               
                int bytesRead = fsIn.Read(iv, 0, iv.Length);
                if (bytesRead < iv.Length)
                {
                    throw new EndOfStreamException("Input file is too short to contain the IV.");
                }

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv; 
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                    {
                        
                        using (CryptoStream csDecrypt = new CryptoStream(fsIn, decryptor, CryptoStreamMode.Read))
                        {
                            csDecrypt.CopyTo(fsOut); 
                        }
                    }
                }
            }
        }


        private byte[] ComputeSha3_512(byte[] data)
        {
            var digest = new Sha3Digest(512);
            digest.BlockUpdate(data, 0, data.Length);
            byte[] hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);
            return hash;
        }

        private byte[] ComputeSha3_512ForFile(string filePath)
        {
            var digest = new Sha3Digest(512);
            byte[] buffer = new byte[4096]; 
            int bytesRead;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    digest.BlockUpdate(buffer, 0, bytesRead);
                }
            }

            byte[] hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);
            return hash;
        }

        private string BytesToHexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b); 
            }
            return hex.ToString();
        }
    }
}
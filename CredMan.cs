using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace LockBox
{
    internal class CredMan
    {
        private const string user_id = "LBUser";
        private const string pass = "LBPW";
        private const string SaltKey = "LBSalt";

        public static async Task SaveCredentialsAsync(string username, string password)
        {
            try
            {
                Log("Saving credentials...");
                await SecureStorage.SetAsync(user_id, username);
                Log("Username saved.");

                var key = await CreateKey(password);
                await SecureStorage.SetAsync(pass, Convert.ToBase64String(key));
                Log("Password saved.");

                // Verify that the credentials are saved correctly
                var savedUsername = await GetUsernameAsync();
                var savedPassword = await GetPasswordAsync();

                if (savedUsername == username)
                {
                    Log("Username saved successfully.");
                }
                else
                {
                    Log("Failed to save username correctly.");
                }

                if (savedPassword == key)
                {
                    Log("Password saved successfully.");
                }
                else
                {
                    Log("Failed to save password correctly.");
                }
            }
            catch (Exception e)
            {
                Log($"Error: {e.Message}");
                Log($"Stack Trace: {e.StackTrace}");
            }
        }

        public static async Task DeleteCredentialsAsync()
        {
            try
            {
                await Task.Run(() => SecureStorage.Remove(user_id));
                await Task.Run(() => SecureStorage.Remove(pass));
                await Task.Run(() => SecureStorage.Remove(SaltKey));
                Log("Credentials deleted.");
            }
            catch (Exception e)
            {
                Log($"Error: {e.Message}");
            }
        }

        public static async Task<string?> GetUsernameAsync()
        {
            return await SecureStorage.GetAsync(user_id);
        }

        public static async Task<byte[]> GetPasswordAsync()
        {
            var passwordBase64 = await SecureStorage.GetAsync(pass);
            if (passwordBase64 == null)
            {
                return [];
            }
            var passwordBytes = Convert.FromBase64String(passwordBase64);
            return passwordBytes;
        }

        public static async Task<bool> AreCredentialsAvailableAsync()//Used to check if the user has already set up the app
        {
            var username = await SecureStorage.GetAsync(user_id);
            var password = await SecureStorage.GetAsync(pass);
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }

        public static async Task<bool> IsPasswordCorrectAsync(string password) // Used to validate the login procedure
        {
            var storedPasswordBase64 = await SecureStorage.GetAsync(pass);
            if (storedPasswordBase64 == null)
            {
                return false;
            }
            var storedPassword = Convert.FromBase64String(storedPasswordBase64);
            var inputPassword = await CreateKey(password);
            return storedPassword.SequenceEqual(inputPassword);
        }

        public static byte[] GenerateSalt(int size = 32) // Generates a random salt
        {
            var salt = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        public static bool ValidatePassword(string password)
        {
            if (password.Length < 12 || password.Length > 64)
            {
                return false;
            }

            return Regex.IsMatch(password, "[a-z]") &&
                   Regex.IsMatch(password, "[A-Z]") &&
                   Regex.IsMatch(password, "\\d") &&
                   Regex.IsMatch(password, "[!@#$%^&*()_\\-+={}\\[\\]:;\"'<>,.?/|\\\\]");
        }

        private async static Task<byte[]> CreateKey(string pw)//takes the password and generates a key
        {
            byte[] salt;

            try
            {
                salt = await GetSaltAsync();
                Log("Salt retrieved successfully.");
            }
            catch (Exception ex)
            {
                Log($"Salt retrieval failed: {ex.Message}");
                salt = GenerateSalt();
                Log("New salt generated.");
                await SaveSaltAsync(salt);
                Log("New salt saved.");
            }

            byte[] password = Encoding.UTF8.GetBytes(pw);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(32);
            Log($"Key generated: {Convert.ToBase64String(key)}");
            return key;
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
        public async static Task<byte[]> GetSaltAsync()
        {
            var saltBase64 = await SecureStorage.GetAsync(SaltKey);
            if (saltBase64 == null)
            {
                throw new Exception("Salt not found.");
            }
            return Convert.FromBase64String(saltBase64);
        }
        public static async Task<bool> IsPasswordCorrectAsync(string scannedPassword, string enteredPassword)//Used to validate the transfer procedure
        {
            var scannedKey = await CreateKey(scannedPassword);
            var enteredKey = await CreateKey(enteredPassword);
            return scannedKey.SequenceEqual(enteredKey);
        }
        public async static Task SaveSaltAsync(byte[] salt)
        {
            var saltBase64 = Convert.ToBase64String(salt);
            await SecureStorage.SetAsync(SaltKey, saltBase64);
        }
    }
}

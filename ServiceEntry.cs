using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OtpNet;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace LockBox
{
    internal class ServiceEntry
    {
        private static readonly ServiceEntry _instance = new ServiceEntry();
        public static ServiceEntry Instance => _instance;

        public static List<ServiceEntry> entries = new List<ServiceEntry>();
        public static ObservableCollection<ServiceEntryViewModel> ListItems { get; set; } = new ObservableCollection<ServiceEntryViewModel>();

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public int Db_id { get; set; }
        public string Serv_name { get; set; } = string.Empty;
        public string Serv_email { get; set; } = string.Empty;
        public string Serv_password { get; set; } = string.Empty;
        public string Serv_mfasec { get; set; } = string.Empty;
        public string Algorithm { get; set; } = "SHA1"; // Default to SHA1

        private static readonly DBHandler db = new DBHandler();

        [JsonConstructor]
        public ServiceEntry(string serv_name, string serv_email, string serv_password, string serv_mfasec, string algorithm = "SHA1")
        {
            Serv_name = serv_name;
            Serv_email = serv_email;
            Serv_password = serv_password;
            Serv_mfasec = serv_mfasec;
            Algorithm = algorithm;
        }

        private ServiceEntry() { }

        public static async Task<ServiceEntry> CreateAsync(string name, string email, string password = "", string mfasec = "", string algorithm = "SHA1") //Used for entry creation, defaults values for cases where they are not provided
        {
            var encryptedName = await Crypter.EncryptAsync(name);
            var encryptedEmail = await Crypter.EncryptAsync(email);
            var encryptedPassword = await Crypter.EncryptAsync(password);
            var encryptedMfaSec = await Crypter.EncryptAsync(mfasec);

            var entry = new ServiceEntry(encryptedName, encryptedEmail, encryptedPassword, encryptedMfaSec, algorithm);

            string? collectionName = await CredMan.GetUsernameAsync();
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException(nameof(collectionName));
            }
            int currentCount = await DBHandler.Instance.GetEntryCountAsync(collectionName);
            entry.Db_id = currentCount + 1;

            await DBHandler.Instance.CreateEntryAsync(collectionName, entry);
            await AddToLists(entry);

            // Save the updated list to the local JSON file
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "entries.json");
            await SaveToJsonFileAsync(filePath);

            return entry;
        }

        public async Task<ServiceEntry> GetDecrypted() //Used for entry decryption for the view model
        {
            return new ServiceEntry(
                await Crypter.DecryptAsync(Serv_name),
                await Crypter.DecryptAsync(Serv_email),
                await Crypter.DecryptAsync(Serv_password),
                await Crypter.DecryptAsync(Serv_mfasec),
                Algorithm
            )
            {
                Db_id = this.Db_id,
                Id = this.Id
            };
        }

        public static async Task UpdateAsync(int dbId, string name, string email, string password = "", string mfasec = "", string algorithm = "SHA1") //Updates the entry and reflects the changes
        {
            string? collectionName = await CredMan.GetUsernameAsync();
            if (collectionName is null)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Operation Failed", "OK");
                return;
            }
            var entryToUpdate = entries.FirstOrDefault(e => e.Db_id == dbId);
            if (entryToUpdate != null)
            {
                entryToUpdate.Serv_name = await Crypter.EncryptAsync(name);
                entryToUpdate.Serv_email = await Crypter.EncryptAsync(email);
                entryToUpdate.Serv_password = await Crypter.EncryptAsync(password);
                entryToUpdate.Serv_mfasec = await Crypter.EncryptAsync(mfasec);
                entryToUpdate.Algorithm = algorithm;

                await DBHandler.Instance.UpdateEntryAsync(dbId, collectionName, entryToUpdate);

                // Update the corresponding ViewModel
                var viewModel = ListItems.FirstOrDefault(vm => vm.Db_id == dbId);
                if (viewModel != null)
                {
                    var decryptedEntry = await entryToUpdate.GetDecrypted();
                    viewModel.Db_id = decryptedEntry.Db_id;
                    viewModel.Serv_name = decryptedEntry.Serv_name;
                    viewModel.Serv_email = decryptedEntry.Serv_email;
                    viewModel.Serv_password = decryptedEntry.Serv_password;
                    viewModel.Serv_mfasec = decryptedEntry.Serv_mfasec;
                    viewModel.Algorithm = decryptedEntry.Algorithm;
                    viewModel.UpdateOtpCode();
                }

                // Save the updated list to the local JSON file
                string filePath = Path.Combine(FileSystem.AppDataDirectory, "entries.json");
                await SaveToJsonFileAsync(filePath);
            }
        }

        public static async Task DeleteAsync(int dbId) //Deletes the entry and reflects the changes
        {
            string? collectionName = await CredMan.GetUsernameAsync();
            if (string.IsNullOrEmpty(collectionName))
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Operation Failed", "OK");
                return;
            }
            await DBHandler.Instance.DeleteEntryAsync(dbId, collectionName);

            // Remove the entry from the in-memory list
            var entryToRemove = entries.FirstOrDefault(e => e.Db_id == dbId);
            if (entryToRemove != null)
            {
                entries.Remove(entryToRemove);
                try
                {
                    ListItems.Remove(ListItems.FirstOrDefault(vm => vm.Db_id == dbId));
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Error deleting entry "+ex.Message);
                }
            }

            // Save the updated list to the local JSON file
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "entries.json");
            await SaveToJsonFileAsync(filePath);
        }

        private static async Task AddToLists(ServiceEntry entry) //Adds the entry to the view model list from the encrypted entry list
        {
            try
            {
                entries.Add(entry);
                var decryptedEntry = await entry.GetDecrypted();
                var viewModel = new ServiceEntryViewModel
                {
                    Db_id = entry.Db_id,
                    Serv_name = decryptedEntry.Serv_name,
                    Serv_email = decryptedEntry.Serv_email,
                    Serv_password = decryptedEntry.Serv_password,
                    Serv_mfasec = decryptedEntry.Serv_mfasec,
                    Algorithm = decryptedEntry.Algorithm,
                    OtpCode = GetCurrentCode(decryptedEntry)
                };
                ListItems.Add(viewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static async Task<List<ServiceEntry>> LoadFromDBAsync() //Loads the entries from the database or on failure, from the local JSON file
        {
            string? db_coll = await CredMan.GetUsernameAsync();
            if (string.IsNullOrEmpty(db_coll))
            {
                return new List<ServiceEntry>();
            }

            try
            {
                bool emptyDB = await DBHandler.Instance.IsCollectionEmptyAsync(db_coll);
                if (emptyDB)
                {
                    return LoadLocal();
                }

                List<ServiceEntry> online = await db.GetAllEntriesAsync(db_coll);
                entries = online; // Ensure entries list is updated with loaded entries
                ListItems.Clear();
                foreach (var entry in entries)
                {
                    var decryptedEntry = await entry.GetDecrypted();
                    var viewModel = new ServiceEntryViewModel
                    {
                        Db_id = entry.Db_id,
                        Serv_name = decryptedEntry.Serv_name,
                        Serv_email = decryptedEntry.Serv_email,
                        Serv_password = decryptedEntry.Serv_password,
                        Serv_mfasec = decryptedEntry.Serv_mfasec,
                        Algorithm = decryptedEntry.Algorithm,
                        OtpCode = GetCurrentCode(decryptedEntry)
                    };
                    ListItems.Add(viewModel);
                }
                return online;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load from online database: {ex.Message}");

                var localEntries = await LoadFromJsonFileAsync(Path.Combine(FileSystem.AppDataDirectory, "entries.json"));
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Failed to load data from the Database. You're currently seeing a local copy of the data. Please try again later.", "OK");
                return localEntries;
            }
        }

        private static List<ServiceEntry> LoadLocal() //Loads the entries from the local JSON file 
        {
            try
            {
                var localEntries = new List<ServiceEntry>();
                foreach (var item in entries)
                {
                    localEntries.Add(item);
                }
                return localEntries;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new List<ServiceEntry>();
            }
        }

        public static string GetCurrentCode(ServiceEntry entry)
        {
            try
            {
                string Secret = entry.Serv_mfasec.Replace(" ", String.Empty).ToUpper();
                if (string.IsNullOrEmpty(Secret))
                {
                    Debug.WriteLine("Secret key (Serv_mfasec) is null or empty.");
                    return "MFA Not Set";
                }

                
                Debug.WriteLine($"Generating TOTP code for entry: {Secret}, Algorithm: {entry.Algorithm}, Secret: {entry.Serv_mfasec}");

                byte[] topsec;
                try
                {
                    topsec = Base32Encoding.ToBytes(Secret);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Invalid Base32 encoding for secret key.");
                    throw new ArgumentException("Invalid Base32 encoding for secret key.", ex);
                }

                OtpHashMode mode;
                switch (entry.Algorithm.ToUpperInvariant())
                {
                    case "SHA256":
                        mode = OtpHashMode.Sha256;
                        break;
                    case "SHA512":
                        mode = OtpHashMode.Sha512;
                        break;
                    case "SHA1":
                        mode = OtpHashMode.Sha1;
                        break;
                    default:
                        Debug.WriteLine($"Unsupported algorithm: {entry.Algorithm}");
                        throw new ArgumentException($"Unsupported algorithm: {entry.Algorithm}");
                }

                Debug.WriteLine($"Using algorithm: {entry.Algorithm}");

                var totp = new Totp(topsec, mode: mode);
                var code = totp.ComputeTotp(DateTime.UtcNow);
                return code;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating TOTP code: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return "MFA Not Set";
            }
        }

        public static async Task SaveToJsonFileAsync(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            var json = JsonSerializer.Serialize(entries, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        public static async Task<List<ServiceEntry>> LoadFromJsonFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<ServiceEntry>();
            }

            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };
            var loadedEntries = JsonSerializer.Deserialize<List<ServiceEntry>>(json);

            return loadedEntries ?? new List<ServiceEntry>();
        }
        public static async Task<ServiceEntry?> GetEntryByNameAndUserAsync(string name, string user)
        {
            var entries = await LoadFromDBAsync();
            return entries.FirstOrDefault(e => e.Serv_name == name && e.Serv_email == user);
        }
    }
}

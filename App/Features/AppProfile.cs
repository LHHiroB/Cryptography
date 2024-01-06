using System;
using System.IO;
using Newtonsoft.Json;
using IOCore;
using IOCore.Libs;

namespace IOApp.Features
{
    internal class AppProfile : IOItem
    {
        private readonly static string SCOPE = nameof(AppProfile);

        private static readonly Lazy<AppProfile> lazy = new(() => new AppProfile());
        public static AppProfile Inst => lazy.Value;

        private static string _PROFILE_LOCATION => "D:\\WorkSpaceFor_Y3\\SE400 - Seminar on modern issues of Software Technology\\Cryptography";
        public static string DEFAULT_STORAGE_LOCATION => Path.Combine(_PROFILE_LOCATION, "Storage");

        private readonly string _ENCRYPT_PROFILE_PASSWORD = "275010a649c4d5690f10dc49b9418456";
        private readonly string _PROFILE_PATH = Path.Combine(_PROFILE_LOCATION, "profile");

        private string _storageLocation;
        public string StorageLocation { get => _storageLocation; set { _storageLocation = value; Save(); } }

        private string _password;
        public string Password { get => _password; set { _password = value; Save(); } }

        private AppProfile()
        {
            try
            {
                Reset();

                var bytes = File.ReadAllBytes(_PROFILE_PATH);
                var jsonStr = CryptographyUtils.Decrypt(bytes, _ENCRYPT_PROFILE_PASSWORD, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                var profile = JsonConvert.DeserializeAnonymousType(jsonStr, new { StorageLocation = string.Empty, Password = string.Empty });
                StorageLocation = profile.StorageLocation;
                Password = profile.Password;
            }
            catch
            {
                Utils.DeleteFileOrDirectory(_PROFILE_PATH);
            }
        }

        public void Save()
        {
            try
            {
                var jsonStr = JsonConvert.SerializeObject(this);
                var bytes = CryptographyUtils.EncryptToBytes(jsonStr, _ENCRYPT_PROFILE_PASSWORD, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);
                File.WriteAllBytes(_PROFILE_PATH, bytes);
            }
            catch
            {
            }
        }

        [JsonIgnore]
        public string AppEncryptedLocation => Path.Combine(StorageLocation, "Encrypted");

        [JsonIgnore]
        public string AppRecoveryLocation => Path.Combine(StorageLocation, "Recovery");

        [JsonIgnore]
        public string AppTempLocation => Path.Combine(StorageLocation, "Temp");

        public void Reset()
        {
            _storageLocation = DEFAULT_STORAGE_LOCATION;
            _password = null;
        }

        public void NotifyAll()
        {
            Notify((nameof(StorageLocation)));
            Notify((nameof(AppEncryptedLocation)));
            Notify((nameof(AppRecoveryLocation)));
            Notify((nameof(AppTempLocation)));
        }
    }
}

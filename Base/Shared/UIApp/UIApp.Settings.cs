using System;
using System.Collections.Generic;
using System.Text;
using Heleus.Base;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public interface ISettingsCredential
    {
        void AddCredential(string key, string password);
        string GetCredential(string key);
        void RemoveCredential(string key);
    }

    public interface ISettingsData
    {
        void SetData(string key, byte[] data, bool save);
        byte[] GetData(string key);
    }

    partial class UIApp
	{
        Settings _currentSettings;
        public ISettingsData SettingsData => _currentSettings;
        public ISettingsCredential SettingsCredential => _currentSettings; 

        public void SaveSettings()
        {
            _currentSettings?.SaveSettings();
        }

        public class Settings : NativeSettings, ISettingsCredential, ISettingsData
		{
            // It's stupid, but it's okay as an example
            // Should use the OS key chains or let the user decide what to do 
            // WPF: https://stackoverflow.com/questions/17741424/retrieve-credentials-from-windows-credentials-store-using-c-sharp# https://github.com/mono/monodevelop/blob/7c51ae11c323d429c10acd22169373927217198f/main/src/addins/WindowsPlatform/WindowsPlatform/WindowsSecureStoragePasswordProvider.cs
            // UWP: https://docs.microsoft.com/en-us/windows/uwp/security/credential-locker
            // MAC: https://developer.xamarin.com/api/type/MonoMac.Security.SecKeyChain/
            // IOS: https://forums.xamarin.com/discussion/122847/keychain-access-on-ios-11-simulator-requires
            // Linux: ???
            string _credentialsPass;
            readonly Dictionary<string, Credential> _credentials = new Dictionary<string, Credential>();

            readonly Dictionary<string, byte[]> _data = new Dictionary<string, byte[]>();

            class Credential
            {
                public readonly string Password;
                public readonly string Key;

                public Credential(string password, string key)
                {
                    Password = password;
                    Key = key;
                }
            }

            public Settings() : base("Settings")
            {
                if (string.IsNullOrWhiteSpace(_credentialsPass))
                {
                    var pass = Rand.NextSeed(32);
                    _credentialsPass = Convert.ToBase64String(pass);
                }
            }

            public void AddCredential(string key, string password)
            {
                if (string.IsNullOrWhiteSpace(key) || key.Contains("|"))
                    throw new ArgumentException(nameof(key));

                if (password == null)
                    throw new ArgumentException(nameof(password));

                _credentials[key] = new Credential(password, Encryption.GenerateAes256(new ArraySegment<byte>(Encoding.UTF8.GetBytes(password)), _credentialsPass).HexString);
                SaveSettings();
            }

            public string GetCredential(string key)
            {
                _credentials.TryGetValue(key, out var cred);
                return cred?.Password;
            }

            public void RemoveCredential(string key)
            {
                _credentials.Remove(key);
                SaveSettings();
            }

            void ReadCredentialChunks(ChunkReader reader)
            {
                reader.Read(nameof(_credentialsPass), ref _credentialsPass);

                string credentialKeys = null;
                reader.Read(nameof(credentialKeys), ref credentialKeys);

                if(!string.IsNullOrEmpty(credentialKeys))
                {
                    var parts = credentialKeys.Split('|');
                    foreach(var part in parts)
                    {
                        string key = null;
                        if(reader.Read("Credential" + part, ref key))
                        {
                            try
                            {
                                var pass = Encoding.UTF8.GetString(Encryption.Restore(key).Decrypt(_credentialsPass));
                                _credentials[part] = new Credential(pass, key);
                            }
                            catch { }
                        }
                    }
                }
            }

            void WriteCredentialChunks(ChunkWriter writer)
            {
                writer.Write(nameof(_credentialsPass), _credentialsPass);

                var credentialKeys = string.Join("|", _credentials.Keys);
                writer.Write(nameof(credentialKeys), credentialKeys);
                foreach(var item in _credentials)
                    writer.Write("Credential" + item.Key, item.Value.Key);
            }

            public void SetData(string key, byte[] data, bool save)
            {
                if (data != null)
                {
                    _data[key] = data;
                    if(save)
                        SaveSettings();
                }
            }

            public byte[] GetData(string key)
            {
                _data.TryGetValue(key, out var data);
                return data;
            }

            void ReadDataChunks(ChunkReader reader)
            {
                foreach(var chunk in reader.Chunks)
                {
                    byte[] data = null;
                    reader.Read(chunk, ref data);
                    if(data != null)
                        _data[chunk] = data;

                }
            }

            void WriteDataChunks(ChunkWriter writer)
            {
                foreach(var d in _data)
                {
                    writer.Write(d.Key, d.Value);
                }
            }

            protected override void RestoreSettings()
			{
                try
                {
                    var credentials = GetValue("Credentials", string.Empty);
                    if(!string.IsNullOrEmpty(credentials))
                    {
                        var data = Convert.FromBase64String(credentials);
                        ChunkReader.Read(data, ReadCredentialChunks);
                    }

                    var base64 = GetValue("Data", string.Empty);
                    if(!string.IsNullOrEmpty(base64))
                    {
                        var data = Convert.FromBase64String(base64);
                        ChunkReader.Read(data, ReadDataChunks);
                    }
                }
                catch(Exception ex)
                {
                    Log.IgnoreException(ex);
                }

				try
				{
					var settings = GetValue("SettingsData", string.Empty);
					if (!string.IsNullOrEmpty(settings))
					{
						var data = Convert.FromBase64String(settings);
                        ChunkReader.Read(data, UIAppSettings.ReadChunks, Current.RestoreUIAppSettings, Current.RestoreSettings);
					}
                }
                catch (Exception ex)
				{
                    Log.IgnoreException(ex);
				}
			}

			protected override void StoreSettings()
			{
                try
                {
                    var data = ChunkWriter.Write(WriteCredentialChunks);
                    if(data != null)
                    {
                        var credentials = Convert.ToBase64String(data);
                        SetValue("Credentials", credentials);
                    }

                    data = ChunkWriter.Write(WriteDataChunks);
                    if(data != null)
                    {
                        var base64 = Convert.ToBase64String(data);
                        SetValue("Data", base64);
                    }
                }
                catch(Exception ex)
                {
                    Log.IgnoreException(ex);
                }

                try
                {
                    var data = ChunkWriter.Write(UIAppSettings.WriteChunks, Current.StoreUIAppSettings, Current.StoreSettings);
                    if (data != null)
                    {
                        var settings = Convert.ToBase64String(data);
                        SetValue("SettingsData", settings);
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
			}
		}
    }
}

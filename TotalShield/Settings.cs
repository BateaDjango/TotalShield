using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TotalShield
{


    public static class Settings
    {
        public static string AvDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TotalShield";
        public static string AV_ListFile = AvDir + @"\Settings\AV_List.xml";
        public static string KeysFile = AvDir + @"\Settings\Keys.xml";
        public static string ReportsFile = AvDir + @"\History\Reports.xml";
        public static string PreferencesFile = AvDir + @"\Settings\Preferences.xml";
        public static string AppName = "Total Shield";
        public static string shortcut_path = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\TotalShield.lnk";

        public static string ContextShellPath = "Software\\Classes\\*\\shell";

        public static string ContextShellScanMenu = ContextShellPath + "\\TotalShieldScan";
        public static string ContextShellScanCmd = ContextShellScanMenu + "\\command";

        public static string ContextShellRunSafeMenu = ContextShellPath + "\\TotalShieldRunSafe";
        public static string ContextShellRunSafeCmd = ContextShellRunSafeMenu + "\\command";

        public async static Task<bool> IsInternetAvailable()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsStartup()
        {
            return System.IO.File.Exists(shortcut_path);
        }

        public static void AddtoStartup()
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcut_path);
            shortcut.Description = "Total Shield";
            shortcut.Arguments = "startup";
            shortcut.TargetPath = GetExecutingFileName();
            shortcut.Save();
        }

        public static void RemoveFromStartup()
        {
            System.IO.File.Delete(shortcut_path);
        }


        static readonly Dictionary<Type, string> SettingsFile = new Dictionary<Type, string>
        {
             { typeof(AV_List), AV_ListFile},
             { typeof(Keys), KeysFile},
             { typeof(AV_Reports), ReportsFile},
             { typeof(Preferences), PreferencesFile}
        };


        public static bool AddFileContext(bool isScan)
        {
            try
            {
                string thisfilename = GetExecutingFileName();

                using (RegistryKey regmenu = Registry.CurrentUser.CreateSubKey(isScan ? ContextShellScanMenu : ContextShellRunSafeMenu))
                {
                    if (regmenu != null)
                    {
                        regmenu.SetValue("", isScan ? "Scan with TotalShield" : "Run Safe (TotalShield)");
                        regmenu.SetValue("Icon", thisfilename);

                        using (RegistryKey regcmd = Registry.CurrentUser.CreateSubKey(isScan ? ContextShellScanCmd : ContextShellRunSafeCmd))
                        {
                            if (regcmd != null)
                                regcmd.SetValue("", string.Format("\"{0}\" \"%1\" \"{1}\"", thisfilename, isScan ? "0" : "1"));
                            return true;
                        }
                    }
                    return false;

                }
            }
            catch
            {
                return false;
            }

        }

        public static bool RemoveFileContext(bool isScan)
        {
            try
            {

                Registry.CurrentUser.DeleteSubKeyTree(isScan ? ContextShellScanMenu : ContextShellRunSafeMenu, false);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static string GetExecutingDirectoryName()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetExecutingFileName()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public static bool isAVEnabled(string avname)
        {
            AV_List list = GetAVList();
            foreach (AV element in list.av)
            {
                if (element.name.Equals(avname))
                {

                    return element.enabled;
                }

            }

            return true;
        }

        public static bool KeysEncrypted()
        {
            try
            {
                Keys result = (Keys)Get_Settings(typeof(Keys));
                return false;
            }
            catch
            {
                return true;
            }


        }

        public static void SaveKey(string key, bool premium)
        {
            DecryptKeys();


            Keys result = (Keys)Get_Settings(typeof(Keys));

            if (!premium)
            {
                result.key[0].value = key;
                result.key[0].active = true;
                result.key[1].active = false;
            }
            else
            {
                result.key[1].value = key;
                result.key[1].active = true;
                result.key[0].active = false;
            }

            Save_Settings(result);

            EncryptKeys();
        }

        public static int ToggleActiveKey(bool premium)
        {
            DecryptKeys();

            Keys result = (Keys)Get_Settings(typeof(Keys));

            EncryptKeys();
            bool premiumValid = result.key[1].value != null && !String.IsNullOrWhiteSpace(result.key[1].value.ToString());
            bool publicValid = result.key[0].value != null && !String.IsNullOrWhiteSpace(result.key[0].value.ToString());



            if (!premium)
            {
                if (publicValid)
                {
                    if (result.key[0].active && !premiumValid)
                        return 0;


                    result.key[0].active = !result.key[0].active;
                    result.key[1].active = !result.key[1].active;
                    Save_Settings(result);
                    EncryptKeys();

                    return result.key[0].active ? 1 : 2;

                }
                else
                {

                    return 0;
                }
            }
            else
            {
                if (premiumValid)
                {
                    if (result.key[1].active && !publicValid)
                        return 0;


                    result.key[1].active = !result.key[1].active;
                    result.key[0].active = !result.key[0].active;
                    Save_Settings(result);
                    EncryptKeys();
                    return result.key[1].active ? 1 : 2;
                }
                else
                {

                    return 0;
                }
            }

        }

        public static void Save_Settings(object obj)
        {

            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (StreamWriter writer = new StreamWriter(SettingsFile[obj.GetType()]))
            {
                try
                {
                    serializer.Serialize(writer, obj);
                }
                catch (Exception ex)
                {
                    writer.Close();
                    writer.Dispose();
                    throw ex;
                }

            }
        }

        public static object Get_Settings(Type type)
        {
            string filepath = SettingsFile[type];

            if (!System.IO.File.Exists(filepath))
            {
                var assembly = Assembly.GetExecutingAssembly();
                string parent = Directory.GetParent(filepath).Name;
                var resourceName = "TotalShield." + parent + "." + Path.GetFileName(filepath);


                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    FileInfo file = new FileInfo(filepath);
                    file.Directory.Create();
                    System.IO.File.WriteAllText(filepath, reader.ReadToEnd());
                }
            }

            XmlSerializer serializer = new XmlSerializer(type);
            using (FileStream fileStream = new FileStream(SettingsFile[type], FileMode.Open))
            {
                try
                {

                    var result = serializer.Deserialize(fileStream);
                    return result;
                }
                catch (Exception ex)
                {

                    fileStream.Close();
                    fileStream.Dispose();

                    if (type == typeof(Keys))
                        throw ex;
                    else
                    {
                        System.IO.File.Delete(filepath);
                        return Get_Settings(type);
                    }



                }
            }
        }

        public static bool isPremium()
        {

            Keys keys = GetKeys();

            return keys.key[1].active;

        }

        public static bool isPublic()
        {

            Keys keys = GetKeys();

            return keys.key[0].active;
        }



        static public Keys GetKeys()
        {
            DecryptKeys();
            Keys result = (Keys)Get_Settings(typeof(Keys));
            EncryptKeys();

            return result;
        }

        public static void EncryptKeys()
        {
            if (!KeysEncrypted())
            {
                var salt = new byte[] { 0, 1, 2, 3 };

                byte[] enc_bytes = ProtectedData.Protect(System.IO.File.ReadAllBytes(Settings.KeysFile), salt, DataProtectionScope.CurrentUser);

                System.IO.File.WriteAllBytes(Settings.KeysFile, enc_bytes);

            }
        }

        public static void DecryptKeys()
        {
            if (KeysEncrypted())
            {
                try
                {
                    var salt = new byte[] { 0, 1, 2, 3 };

                    byte[] dec_bytes = ProtectedData.Unprotect(System.IO.File.ReadAllBytes(Settings.KeysFile), salt, DataProtectionScope.CurrentUser);
                    System.IO.File.WriteAllBytes(Settings.KeysFile, dec_bytes);
                }
                catch
                {
                    System.IO.File.Delete(Settings.KeysFile);
                }
            }
        }

        public static void Set_AV(string name, bool value)
        {
            AV_List list = GetAVList();
            foreach (AV element in list.av)
            {
                if (element.name.Equals(name))
                {
                    element.enabled = value;
                    break;
                }
            }
            Save_Settings(list);
        }

        public static List<string> GetActiveAVs()
        {
            List<string> avs = new List<string>();

            AV_List list = GetAVList();
            foreach (AV element in list.av)
            {
                if (element.enabled)
                    avs.Add(element.name);


            }
            return avs;
        }


        static public AV_List GetAVList()
        {
            AV_List result = (AV_List)Get_Settings(typeof(AV_List));
            return result;
        }
    }
}

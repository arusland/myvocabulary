using System.Text.RegularExpressions;
using Microsoft.Win32;
using Shared.Extensions;

namespace MyVocabulary.Helpers
{
    public static class FileAssociateHelper
    {
        #region Constants

        private const string HKEY_CURRENT_USER_Classes = @"Software\Classes\";

        #endregion

        #region Methods

        #region Public

        public static void AssociateFile(string extension, string description, string path, string iconPath, int iconIndex)
        {
            //string user = Environment.UserDomainName + "\\"
            //+ Environment.UserName;

            //RegistrySecurity mSec = new RegistrySecurity();

            //RegistryAccessRule rule = new RegistryAccessRule(user,
            //    RegistryRights.CreateSubKey,
            //    InheritanceFlags.ObjectInherit,
            //    PropagationFlags.None,
            //    AccessControlType.Allow);
            //mSec.AddAccessRule(rule);

            //rule = new RegistryAccessRule(user,
            //    RegistryRights.SetValue,
            //    InheritanceFlags.ContainerInherit,
            //    PropagationFlags.InheritOnly,
            //    AccessControlType.Allow);

            //mSec.AddAccessRule(rule);

            //RegistryKey key = Registry.ClassesRoot.CreateSubKey(string.Format(".{0}", extension), RegistryKeyPermissionCheck.ReadSubTree, mSec);
            //RegistryKey key = Registry.ClassesRoot.CreateSubKey(string.Format(".{0}", extension));
            RegistryKey key = Registry.CurrentUser.CreateSubKey(string.Format("{0}.{1}", HKEY_CURRENT_USER_Classes, extension));
            string extKey = string.Format("{0}.FileType", extension.Upper());
            key.SetValue(null, extKey);
            key.Close();

            key = Registry.CurrentUser.CreateSubKey(string.Format("{0}{1}", HKEY_CURRENT_USER_Classes, extKey));
            key.SetValue(null, description);
            RegistryKey shellKey = key.CreateSubKey("shell");
            RegistryKey openKey = shellKey.CreateSubKey("open");
            RegistryKey commandKey = openKey.CreateSubKey("command");
            commandKey.SetValue(null, string.Format("\"{0}\" \"%1\"", path));
            commandKey.Close();
            openKey.Close();
            shellKey.Close();
            RegistryKey defIcon = key.CreateSubKey("DefaultIcon");
            defIcon.SetValue(null, string.Format("\"{0}\",{1}", iconPath, iconIndex));
            defIcon.Close();
            key.Close();
        }

        public static string GetPerformer(string extension)
        {
            string extKey = string.Format("{0}.FileType", extension.Upper());

            var key = Registry.CurrentUser.OpenSubKey(string.Format("{0}{1}", HKEY_CURRENT_USER_Classes, extKey));
            if (key != null)
            {
                RegistryKey shellKey = key.OpenSubKey(@"shell\open\command\");

                if (shellKey != null)
                {
                    string value = shellKey.GetValue(null).IfNull(() => string.Empty);

                    if (value.IsNotNullOrEmpty())
                    {
                        var m = Regex.Match(value, "\"([^\"]+)\"");

                        if (m.Success)
                        {
                            return m.Groups[1].Value.Trim();
                        }
                    }
                }
            }

            return null;
    
           // "D:\Smart Solutions Projects\Orygin\Orygin.ControlPanel.Warehouse\Output\Debug\Orygin.exe" "%1"
        }

        #endregion

        #endregion
    }
}

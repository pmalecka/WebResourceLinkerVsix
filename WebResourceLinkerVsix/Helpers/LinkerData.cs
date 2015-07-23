using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WebResourceLinkerVsix
{
    [Serializable]
    public class LinkerData
    {
        public string DiscoveryUrl { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string UniqueOrgName { get; set; }
        public string PublicUrl { get; set; }

        public List<LinkerDataItem> Mappings { get; set; }

        public LinkerData()
        {
            this.Mappings = new List<LinkerDataItem>();
        }

        public static LinkerData Get(string dataPath)
        {
            LinkerData result = null;

            if (File.Exists(dataPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(LinkerData));
                using (StreamReader sr = new StreamReader(dataPath))
                {
                    result = xs.Deserialize(sr) as LinkerData;
                }
            }

            if (result == null) { result = new LinkerData { }; }
            else
            {
                result.Password = Encryption.Decrypt(result.Password, Encryption.EncryptionKey);
            }

            return result;
        }

        public void Save(string dataPath)
        {
            this.Password = Encryption.Encrypt(this.Password, Encryption.EncryptionKey);

            XmlSerializer xs = new XmlSerializer(typeof(LinkerData));
            using (StreamWriter sw = new StreamWriter(dataPath, false))
            {
                xs.Serialize(sw, this);
            }
        }

    }
}

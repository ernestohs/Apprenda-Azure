using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure_Storage_AddIn
{
    public class ConnectionInfo
    {

        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string URI { get; set; }
        public string StorageAccountName { get; set; }

        public static ConnectionInfo Parse(string connectionInfo)
        {
            ConnectionInfo info = new ConnectionInfo();

            if (!string.IsNullOrWhiteSpace(connectionInfo))
            {
                var propertyPairs = connectionInfo.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var propertyPair in propertyPairs)
                {
                    var optionPairParts = propertyPair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (optionPairParts.Length == 2)
                    {
                        MapToProperty(info, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim());
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Unable to parse connection info which should be in the form of 'property=value&nextproperty=nextValue'. The property '{0}' was not properly constructed",
                                propertyPair));
                    }
                }
            }

            return info;
        }

        public static void MapToProperty(ConnectionInfo existingInfo, string key, string value)
        {
            if ("primarykey".Equals(key))
            {
                existingInfo.PrimaryKey = value;
                return;
            }

            if ("secondarykey".Equals(key))
            {
                existingInfo.SecondaryKey = value;
                return;
            }

            if ("uri".Equals(key))
            {
                existingInfo.URI = value;
                return;
            }

            if("storageaccountname".Equals(key))
            {
                existingInfo.StorageAccountName = value;
                return;
            }

            throw new ArgumentException(string.Format("The connection info '{0}' was not expected and is not understood.", key));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (PrimaryKey != null)
                builder.AppendFormat("PrimaryKey={0}&", PrimaryKey);

            if (SecondaryKey != null)
                builder.AppendFormat("SecondaryKey={0}&", SecondaryKey);

            if (URI != null)
                builder.AppendFormat("URI={0}&", URI);

            if (StorageAccountName != null)
                builder.AppendFormat("StorageAccountName={0}", StorageAccountName);

            return builder.ToString(0, builder.Length - 1);
        }
    }
}

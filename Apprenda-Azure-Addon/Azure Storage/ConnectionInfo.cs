using Newtonsoft.Json;

namespace Apprenda.SaaSGrid.Addons.Azure.Storage
{
    public class ConnectionInfo
    {

        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string Uri { get; set; }
        public string StorageAccountName { get; set; }
        // added in the connection info parameters here.
        public string BlobContainerName { get; set; }
        public string QueueName { get; set; }
        public string TableName { get; set; }
        public string ErrorMessage { get; set; }

        public static ConnectionInfo Parse(string json)
        {
            return JsonConvert.DeserializeObject<ConnectionInfo>(json);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

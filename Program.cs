using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace Blobs
{
    class Program
    {

        static void Main(string[] args)
        {
            string storageconnection = 
                System.Configuration.ConfigurationManager.AppSettings.Get("StorageConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageconnection);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("objective2");

            container.CreateIfNotExists();

            UploadBlob(container);

            Console.ReadKey();
        }

        static void UploadBlob(CloudBlobContainer container)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("examobjectives");

            using (var fileStream = System.IO.File.OpenRead(@"C:\Users\jasraj\Downloads\test.pdf"))
            {
                blockBlob.UploadFromStream(fileStream);
            }

        }

        static void ListAttributes(CloudBlobContainer container)
        {
            container.FetchAttributes();
            Console.WriteLine("Container name " + container.StorageUri.PrimaryUri.ToString());
            Console.WriteLine("Last modified " + container.Properties.LastModified.ToString());
        }

        static void SetMetadata(CloudBlobContainer container)
        {
            container.Metadata.Clear();
            container.Metadata.Add("author", "jasraj");
            container.Metadata["authoredOn"] = "Feb 16, 2018";
            container.SetMetadata();
        }

        static void ListMetadata(CloudBlobContainer container)
        {
            container.FetchAttributes();
            Console.WriteLine("Metadata:\n");
            foreach (var item in container.Metadata)
            {
                Console.WriteLine("Key " + item.Key);
                Console.WriteLine("Value " + item.Value + "\n\n");
            }
        }

        static void CopyBlob(CloudBlobContainer container)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("examobjectives");
            CloudBlockBlob copyToBlockBlob = container.GetBlockBlobReference("examobjectives-copy");
            copyToBlockBlob.StartCopyAsync(new Uri(blockBlob.Uri.AbsoluteUri));
        }

        static void UploadBlobSubdirectory(CloudBlobContainer container)
        {
            CloudBlobDirectory directory = container.GetDirectoryReference("parent-directory");
            CloudBlobDirectory subdirectory = directory.GetDirectoryReference("child-directory");
            CloudBlockBlob blockBlob = subdirectory.GetBlockBlobReference("newexamobjectives");

            using (var fileStream = System.IO.File.OpenRead(@"C:\Users\jasraj\Downloads\test.pdf"))
            {
                blockBlob.UploadFromStream(fileStream);
            }

        }

        static void CreateSharedAccessPolicy(CloudBlobContainer container)
        {
            SharedAccessBlobPolicy sharedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List
            };

            BlobContainerPermissions permissions = new BlobContainerPermissions();
            permissions.SharedAccessPolicies.Clear();
            permissions.SharedAccessPolicies.Add("PolicyName", sharedPolicy);
            container.SetPermissions(permissions);
        }

        static void CreateCORSPolicy(CloudBlobClient blobClient)
        {
            ServiceProperties sp = new ServiceProperties();
            sp.Cors.CorsRules.Add(new CorsRule()
            {
                AllowedMethods = CorsHttpMethods.Get,
                AllowedOrigins = new List<string>() { "http://localhost:8080/"},
                MaxAgeInSeconds = 3600,
            });
            blobClient.SetServiceProperties(sp);
        }

    }
}

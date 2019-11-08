using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using CatalogAPI.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace CatalogAPI.Helpers
{
    public class StorageAccountHelper
    {
        public string storageConnectonString;
        public string tableConnectonString;
        private CloudStorageAccount storageAccount;
        private CloudStorageAccount tableStorageAccount;
        private CloudBlobClient blobClient;
        private CloudTableClient tableClient;

        public StorageAccountHelper()
        {
        }

        public string StorageConnectonString
        {
            get { return storageConnectonString; }
            set
            {
                this.storageConnectonString = value;
                storageAccount = CloudStorageAccount.Parse(this.storageConnectonString) ;
            }
        }

        public string TableConnectonString
        {
            get { return tableConnectonString; }
            set
            {
                this.tableConnectonString = value;
                tableStorageAccount = CloudStorageAccount.Parse(this.tableConnectonString);
            }
        }

        public async Task<string> UploadFileToBlobAsync(string filePath, string contianerName)
        {
            blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(contianerName);
            await container.CreateIfNotExistsAsync();
            BlobContainerPermissions permissions = new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Container
            };
            await container.SetPermissionsAsync(permissions);

            var fileName = Path.GetFileName(filePath);
            var blob = container.GetBlockBlobReference(fileName);
            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(filePath);
            return blob.Uri.AbsoluteUri;
        }

        public async Task<CatalogEntity> SaveToTableAsync(CatalogItem item)
        {
            CatalogEntity catalogEntity = new CatalogEntity(item.Name, item.Id)
            {
                ImageUrl = item.ImageUrl,
                ReorderLevel = item.ReorderLevel,
                Quantity = item.Quantity,
                Price = item.Price,
                ManufacturingDate = item.ManufacturingDate
            };
            //tableClient = storageAccount.CreateCloudTableClient();
            tableClient = tableStorageAccount.CreateCloudTableClient();
            var catalogTable = tableClient.GetTableReference("catalog");
            await catalogTable.CreateIfNotExistsAsync();
            TableOperation operation = TableOperation.InsertOrMerge(catalogEntity);
            var tableResult = await catalogTable.ExecuteAsync(operation);
            return tableResult.Result as CatalogEntity;
        }
    }
}

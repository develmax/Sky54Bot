using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

// Namespace for Table storage types

namespace Sky54Bot.Storages
{
    public class StorageAzure: IStorageAzure
    {
        private string _connectionString;

        public StorageAzure(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CloudTable GetTable(string name)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            var table = tableClient.GetTableReference(name);

            return table;
        }

        public bool IsExistsTable(CloudTable table)
        {
            Task<bool> existsAsyncResult = null;
            Task.Run(() =>
            {
                existsAsyncResult = table.ExistsAsync();
            }).Wait();

            return existsAsyncResult.Result;
        }

        public void CreateIfNotExists(CloudTable table)
        {
            Task.Run(() => table.CreateIfNotExistsAsync()).Wait();
        }

        public T RetrieveEntity<T>(CloudTable table, string partitionKey, string rowkey)
            where T: TableEntity
        {
            TableOperation retrieveOperation =
                TableOperation.Retrieve<T>(partitionKey, rowkey);

            TableResult retrievedResult = null;
            Task<TableResult> taskRetrievedResult = null;
            
            Task.Run(() =>
            {
                taskRetrievedResult = table.ExecuteAsync(retrieveOperation);
            }).Wait();

            retrievedResult = taskRetrievedResult.Result;

            T entity = retrievedResult.Result as T;

            return entity;
        }

        public void DeleteEntity<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            TableOperation deleteOperation = TableOperation.Delete(entity);

            Task.Run(() => table.ExecuteAsync(deleteOperation)).Wait();
        }

        public void UpdateEntity<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            TableOperation updateOperation = TableOperation.Replace(entity);

            Task.Run(() => table.ExecuteAsync(updateOperation)).Wait();
        }

        public void InsertEntity<T>(CloudTable table, T entity)
            where T : TableEntity
        {
            TableOperation insertOperation = TableOperation.Insert(entity);

            Task.Run(() => table.ExecuteAsync(insertOperation)).Wait();
        }

        public IEnumerable<T> RetrieveEntities<T>(CloudTable table)
            where T : TableEntity, new()
        {
            var query = new TableQuery<T>();

            Task<TableQuerySegment<T>> taskQueryResult = null;

            Task.Run(() =>
            {
                taskQueryResult = table.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());
            }).Wait();

            return taskQueryResult.Result;
        }
    }
}
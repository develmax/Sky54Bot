using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;

namespace Sky54Bot.Storages
{
    public class StorageAzureAdapter: IStorage
    {
        private IStorageAzure _storageAzure;
        private Type _storageAzureType;
        private Dictionary<string, MethodInfo> _storageAzureTypeMethods = new Dictionary<string, MethodInfo>();

        public StorageAzureAdapter(IStorageAzure storageAzure)
        {
            _storageAzure = storageAzure;
            _storageAzureType = _storageAzure.GetType();
        }

        public object GetTable(string name)
        {
            return _storageAzure.GetTable(name);
        }

        public bool IsExistsTable(object table)
        {
            return _storageAzure.IsExistsTable((CloudTable)table);
        }

        public void CreateIfNotExists(object table)
        {
            _storageAzure.CreateIfNotExists((CloudTable)table);
        }

        private MethodInfo GetMethod(string name)
        {
            if (_storageAzureTypeMethods.ContainsKey(name))
                return _storageAzureTypeMethods[name];

            if (_storageAzureType == null)
                _storageAzureType = _storageAzure.GetType();

            var method = _storageAzureType.GetMethod(name);

            _storageAzureTypeMethods.Add(name, method);

            return method;
        }

        public T RetrieveEntity<T>(object table, string partitionKey, string rowkey)
        {
            var method = GetMethod("RetrieveEntity");
            var generic = method.MakeGenericMethod(typeof(T));
            var entity = generic.Invoke(_storageAzure, new object[]{ (CloudTable)table, partitionKey, rowkey });

            return entity is T ? (T)entity : default(T);
        }

        public void DeleteEntity<T>(object table, T entity)
        {
            var method = GetMethod("DeleteEntity");
            var generic = method.MakeGenericMethod(typeof(T));
            generic.Invoke(_storageAzure, new object[] { (CloudTable)table, entity });
        }

        public void UpdateEntity<T>(object table, T entity)
        {
            var method = GetMethod("UpdateEntity");
            var generic = method.MakeGenericMethod(typeof(T));
            generic.Invoke(_storageAzure, new object[] { (CloudTable)table, entity });
        }

        public void InsertEntity<T>(object table, T entity)
        {
            var method = GetMethod("InsertEntity");
            var generic = method.MakeGenericMethod(typeof(T));
            generic.Invoke(_storageAzure, new object[] { (CloudTable)table, entity });
        }

        public IEnumerable<T> RetrieveEntities<T>(object table)
        {
            var method = GetMethod("RetrieveEntities");
            var generic = method.MakeGenericMethod(typeof(T));

            var entities = generic.Invoke(_storageAzure, new object[] { (CloudTable)table });

            return entities is IEnumerable<T> ? (IEnumerable<T>)entities : default(IEnumerable<T>);
        }
    }
}
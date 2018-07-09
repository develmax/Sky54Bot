using System.Collections.Generic;

namespace Sky54Bot.Storages
{
    public interface IStorage
    {
        object GetTable(string name);
        bool IsExistsTable(object table);
        void CreateIfNotExists(object table);

        T RetrieveEntity<T>(object table, string partitionKey, string rowkey);

        void DeleteEntity<T>(object table, T entity);

        void UpdateEntity<T>(object table, T entity);

        void InsertEntity<T>(object table, T entity);

        IEnumerable<T> RetrieveEntities<T>(object table);
    }

    public interface IStorage<TTable, TEntity>
    {
        TTable GetTable(string name);
        bool IsExistsTable(TTable table);
        void CreateIfNotExists(TTable table);

        T RetrieveEntity<T>(TTable table, string partitionKey, string rowkey)
            where T : TEntity;

        void DeleteEntity<T>(TTable table, T entity)
            where T : TEntity;

        void UpdateEntity<T>(TTable table, T entity)
            where T : TEntity;

        void InsertEntity<T>(TTable table, T entity)
            where T : TEntity;

        IEnumerable<T> RetrieveEntities<T>(TTable table)
            where T : TEntity, new();
    }
}
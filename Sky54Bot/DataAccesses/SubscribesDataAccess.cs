using System;
using System.Collections.Generic;
using System.Linq;
using Sky54Bot.Storages;
using Sky54Bot.Storages.Entities;

namespace Sky54Bot.DataAccesses
{
    public class SubscribesDataAccess: ISubscribesDataAccess
    {
        private IStorage _storage;
        public SubscribesDataAccess(IStorage storage)
        {
            _storage = storage;
        }

        public void UnSubscribe(string chatId)
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return;

            var entity = _storage.RetrieveEntity<SubscribeEntity>(table, SubscribeEntity.Key, chatId);
            if (entity != null)
            {
                _storage.DeleteEntity(table, entity);
            }
        }

        public void Subscribe(string chatId, string name)
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            _storage.CreateIfNotExists(table);

            var entity = _storage.RetrieveEntity<SubscribeEntity>(table, SubscribeEntity.Key, chatId);

            if (entity == null)
            {
                entity = new SubscribeEntity(chatId)
                {
                    Name = name
                };

                _storage.InsertEntity(table, entity);
            }
            else
            {
                entity.Name = name;
                _storage.UpdateEntity(table, entity);
            }
        }

        public bool SubscribeStatus(string chatId)
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return false;

            var entity = _storage.RetrieveEntity<SubscribeEntity>(table, SubscribeEntity.Key, chatId);
            return entity != null;
        }

        public SubscribeEntity[] GetSubscribes()
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            return _storage.RetrieveEntities<SubscribeEntity>(table).ToArray();
        }
    }
}
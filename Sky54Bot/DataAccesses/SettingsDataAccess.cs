using System.Linq;
using Sky54Bot.Storages;
using Sky54Bot.Storages.Entities;

namespace Sky54Bot.DataAccesses
{
    public class SettingsDataAccess: ISettingsDataAccess
    {
        private IStorage _storage;
        public SettingsDataAccess(IStorage storage)
        {
            _storage = storage;
        }

        public string ReadSetting(string name)
        {
            var table = _storage.GetTable(SettingEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            var entity = _storage.RetrieveEntity<SettingEntity>(table, SettingEntity.Key, name);

            return entity?.Value;
        }

        public void WriteSetting(string name, string value)
        {
            var table = _storage.GetTable(SettingEntity.TableKey);

            _storage.CreateIfNotExists(table);

            var entity = _storage.RetrieveEntity<SettingEntity>(table, SettingEntity.Key, name);
            if (entity != null)
            {
                entity.Value = value;

                _storage.UpdateEntity(table, entity);
            }
            else
            {
                entity = new SettingEntity(name, value);

                _storage.InsertEntity(table, entity);
            }
        }

        public SettingEntity[] GetSettings()
        {
            var table = _storage.GetTable(SettingEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            return _storage.RetrieveEntities<SettingEntity>(table).ToArray();
        }
    }
}
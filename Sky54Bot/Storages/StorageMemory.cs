using System;
using System.Collections.Generic;
using System.Linq;
using Sky54Bot.Storages.Entities;

namespace Sky54Bot.Storages
{
    public class StorageMemory //: IStorage
    {
        private List<SubscribeEntity> _subscribeList = new List<SubscribeEntity>();
        private List<SettingEntity> _settingsList = new List<SettingEntity>();

        public string ReadSetting(string name)
        {
            var setting = _settingsList.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            return setting?.Value;
        }

        public void WriteSetting(string name, string value)
        {
            var setting = _settingsList.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));

            if (setting == null)
                _settingsList.Add(new SettingEntity(name, value));
            else
                setting.Value = value;
        }

        public SettingEntity[] GetSettings()
        {
            return _settingsList.ToArray();
        }

        public void UnSubscribe(string chatId)
        {
            var entity = _subscribeList.FirstOrDefault(i => string.Equals(i.ChatId, chatId, StringComparison.OrdinalIgnoreCase));

            if (entity != null)
            {
                _subscribeList.Remove(entity);
            }
        }

        public void Subscribe(string chatId, string name, string envs)
        {
            var entity = _subscribeList.FirstOrDefault(i => string.Equals(i.ChatId, chatId, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(envs))
            {
                if (entity != null)
                {
                    _subscribeList.Remove(entity);
                }
            }
            else
            {
                if (entity != null)
                {
                    entity.Envs = envs;
                }
                else
                {
                    _subscribeList.Add(new SubscribeEntity(chatId) {Name = name, Envs = envs});
                }
            }
        }

        public string[] GetChatIdsByEnv(string env)
        {
            if (_subscribeList.Count == 0) return null;

            var list = new List<string>();

            foreach (SubscribeEntity entity in _subscribeList)
            {
                if (!string.IsNullOrEmpty(entity.Envs))
                {
                    var i = entity.Envs.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in i)
                    {
                        if (string.Equals(s, "all", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s, env, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!list.Contains(entity.ChatId))
                                list.Add(entity.ChatId);
                            break;
                        }
                    }
                }
            }

            return list.Count > 0 ? list.ToArray() : null;
        }

        public string GetEnvsByChatId(string chatId)
        {
            if (_subscribeList.Count == 0) return null;

            var envs = _subscribeList.FirstOrDefault(i => i.ChatId == chatId);

            return envs?.Envs;
        }

        public SubscribeEntity[] GetSubscribes()
        {
            if (_subscribeList.Count == 0) return null;

            var envs = _subscribeList.ToArray();

            return envs;
        }
    }
}
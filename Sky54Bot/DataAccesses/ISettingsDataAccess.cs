using Sky54Bot.Storages.Entities;

namespace Sky54Bot.DataAccesses
{
    public interface ISettingsDataAccess
    {
        string ReadSetting(string name);
        void WriteSetting(string name, string value);
        SettingEntity[] GetSettings();
    }
}
using Sky54Bot.DataAccesses;

namespace Sky54Bot.Storages
{
    public class Settings : ISettings
    {
        public Settings(ISettingsDataAccess settingsDataAccess)
        {
            _settingsDataAccess = settingsDataAccess;
        }

        private ISettingsDataAccess _settingsDataAccess;

        public long? AdminId
        {
            get
            {
                var adminId = _settingsDataAccess.ReadSetting(nameof(AdminId));
                if (adminId != null && long.TryParse(adminId, out var value))
                    return value;

                return null;
            }
            set => _settingsDataAccess.WriteSetting(nameof(AdminId), value.HasValue ? value.ToString() : null);
        }

        public string AdminName
        {
            get => _settingsDataAccess.ReadSetting(nameof(AdminName));
            set => _settingsDataAccess.WriteSetting(nameof(AdminName), value);
        }
    }
}
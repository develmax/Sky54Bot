namespace Sky54Bot.DataAccesses
{
    public class DataAccess: IDataAccess
    {
        public DataAccess(
            ISettingsDataAccess settingsDataAccess,
            ISubscribesDataAccess subscribesDataAccess)
        {
            SettingsDataAccess = settingsDataAccess;
            SubscribesDataAccess = subscribesDataAccess;
        }


        public ISettingsDataAccess SettingsDataAccess { get; }
        public ISubscribesDataAccess SubscribesDataAccess { get; }
    }
}
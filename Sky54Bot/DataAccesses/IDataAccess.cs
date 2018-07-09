namespace Sky54Bot.DataAccesses
{
    public interface IDataAccess
    {
        ISettingsDataAccess SettingsDataAccess { get; }
        ISubscribesDataAccess SubscribesDataAccess { get; }
    }
}
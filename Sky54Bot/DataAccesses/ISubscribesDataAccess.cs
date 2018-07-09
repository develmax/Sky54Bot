using Sky54Bot.Storages.Entities;

namespace Sky54Bot.DataAccesses
{
    public interface ISubscribesDataAccess
    {
        void UnSubscribe(string chatId);
        void Subscribe(string chatId, string name);
        bool SubscribeStatus(string chatId);
        SubscribeEntity[] GetSubscribes();
    }
}
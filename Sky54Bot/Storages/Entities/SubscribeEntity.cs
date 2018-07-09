using Microsoft.WindowsAzure.Storage.Table;

namespace Sky54Bot.Storages.Entities
{
    public class SubscribeEntity : TableEntity
    {
        public const string Key = "Subscription";
        public const string TableKey = "Subscriptions";

        public SubscribeEntity(string chatId)
        {
            this.PartitionKey = Key;
            this.RowKey = chatId;
        }

        public SubscribeEntity() { }

        public string ChatId
        {
            get { return this.RowKey; }
        }

        public string Name { get; set; }
        public string Envs { get; set; }
    }
}
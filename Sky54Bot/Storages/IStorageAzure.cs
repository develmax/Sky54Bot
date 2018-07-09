using Microsoft.WindowsAzure.Storage.Table;

namespace Sky54Bot.Storages
{
    public interface IStorageAzure: IStorage<CloudTable, TableEntity>
    {

    }
}
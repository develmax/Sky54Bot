namespace Sky54Bot.Storages
{
    public interface ISettings
    {
        long? AdminId { get; set; }
        string AdminName { get; set; }
    }
}
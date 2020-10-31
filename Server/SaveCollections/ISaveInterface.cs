namespace KeyBoardChat.SaveCollections
{
    public interface ISaveInterface<T3> where T3 : ISaveCollection
    {
        abstract private protected T3 SaveCollection { get; set; }
    }
}

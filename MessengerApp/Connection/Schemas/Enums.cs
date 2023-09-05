namespace MessengerApp.Connection.Schemas
{
    public enum ChatType
    {
        Personal,
    }

    public enum MessageType : ushort
    {
        Text,
        Image = 1,
        File = 2
    }

    public enum FileType : byte
    {
        Image = 1,
        File = 2
    }
}

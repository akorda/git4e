namespace Git4e
{
    public interface IHashToTextConverter
    {
        string ConvertHashToText(byte[] hash);
        byte[] ConvertTextToHash(string hashText);
    }
}

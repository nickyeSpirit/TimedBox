public interface IStorage
{
    string GetString(string key, string defaultValue = "");
    void SetString(string key, string value);
    void DeleteAll();
    bool HasKey(string key);
}

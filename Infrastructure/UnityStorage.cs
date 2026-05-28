using UnityEngine;

public class UnityStorage : IStorage
{
    public string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);
    public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
    public void DeleteAll() => PlayerPrefs.DeleteAll();
    public bool HasKey(string key) => PlayerPrefs.HasKey(key);
}

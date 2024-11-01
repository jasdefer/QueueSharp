namespace QueueSharp.Logic.Helper;
internal static class DictionaryExtensions
{
    public static void Increment<TKey>(this Dictionary<TKey, int> dict, TKey key) where TKey : notnull
    {
        if (dict.TryGetValue(key, out int value))
        {
            dict[key]++;
            return;
        }
        dict.Add(key, 1);
    }

    public static void Decrement<TKey>(this Dictionary<TKey, int> dict, TKey key) where TKey : notnull
    {
        if (dict.TryGetValue(key, out int value))
        {
            dict[key]--;
            return;
        }
        dict.Add(key, -1);
    }
}

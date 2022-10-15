using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static public class HighScores
{
    private const int MAX_ITEMS = 5;

    static public List<Item> Load()
    {
        try
        {
            var score = PlayerPrefs.GetString("score");
            var data = JsonUtility.FromJson<Data>(score);
            var items = data.items.ToList();
            items.Sort(ByScore);
            return items;
        }
        catch (Exception) { }
        return new List<Item>();
    }

    static public void Add(string name, int score)
    {
        var items = Load();
        items.Add(new Item { name = name, score = score });
        items.Sort(ByScore);

        if (items.Count > MAX_ITEMS)
            items.RemoveRange(MAX_ITEMS, items.Count - MAX_ITEMS);

        Save(items);
    }

    static public void Save(List<Item> items)
    {
        PlayerPrefs.SetString("score", JsonUtility.ToJson(new Data { items = items.ToArray() }));
    }

    private static int ByScore(Item x, Item y) => y.score - x.score;

    [Serializable]
    private class Data
    {
        public Item[] items;
    }

    [Serializable]
    public class Item
    {
        public string name;
        public int score;
    }
}

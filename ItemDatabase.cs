using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public List<Item> itemPrefabs; // Список всех префабов предметов

    void Awake()
    {
        Instance = this;
    }

    public Item GetPrefabByName(string name)
    {
        return itemPrefabs.Find(item => item.gameObject.name == name);
    }
}

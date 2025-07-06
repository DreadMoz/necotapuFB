using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataBase", menuName = "DataBase")]
public class DataBase : ScriptableObject
{
    [SerializeField]
    private List<Item> itemList = new List<Item>();

    [SerializeField]
    private List<string> serverList = new List<string>();

    [SerializeField]
    private List<string> medalList = new List<string>();


    public List<Item> GetItemList()
    {
        return itemList;
    }
    public List<string> GetServerList()
    {
        return serverList;
    }
    public List<string> GetMedalList()
    {
        return medalList;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventry : MonoBehaviour
{
    public static Inventry instance;
    InventryUI inventryUI;

    public List<Item> items = new List<Item>();

private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        inventryUI = GetComponent<InventryUI>();
    }

    public void Add(Item item)
    {
        items.Add(item);
        inventryUI.UpdateUI();
    }

    public void Remove(Item item)
    {
        items.Remove(item);
        inventryUI.UpdateUI();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropItem : MonoBehaviour
{
    public Item item;

    void Start()
    {
        GetComponent<Image>().sprite = item.MyItemImage;
    }
    public void Pickup()
    {
        Inventry.instance.Add(item);
        Destroy(gameObject);
    }
}

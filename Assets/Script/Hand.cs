using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    private Item grabbingItem;
    private bool equip;
    private int slotNo;

    void Update()
    {
        this.transform.position = Input.mousePosition;
    }

    public Item GetGrabbingItem()
    {
        Item oldItem = grabbingItem;
        grabbingItem = null;
        return oldItem;
    }

    public int GetGrabbingItemNo()
    {
        return grabbingItem.MyItemNo;
    }
    public ItemType GetGrabbingItemType()
    {
        return grabbingItem.MyItemType;
    }

    public void SetGrabbingItem(Item item)
    {
        grabbingItem = item;
    }

    public bool IsHavingItem()
    {
        return grabbingItem != null;
    }

    public void setEquip(bool equipFlg)
    {
        equip = equipFlg;
    }

    public void setSlotNo(int no)
    {
        slotNo = no;
    }

    public bool IsEquip()
    {
        return equip;
    }

    public int ItemSlotNo()
    {
        return slotNo;
    }
}

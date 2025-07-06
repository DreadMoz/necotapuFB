using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "ShopItem")]
public class ShopData : ScriptableObject
{
    public List<int> weaponIDs = new List<int>();
    public List<int> glassesIDs = new List<int>();
    public List<int> hatIDs = new List<int>();
    public List<int> eventIDs = new List<int>();

    // 以下、必要に応じてアイテムを取得するためのメソッドを追加
}
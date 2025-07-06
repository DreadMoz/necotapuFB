using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField]
    private Transform slotsParent;

    private GameManager gm;     // WebGLのえらーにより動的取得に変更

    InventrySlot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("EquipmentUIのStartスタート。");
        // GameManagerのインスタンスを動的に検索して取得
        gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("GameManagerが見つかりませんでした。");
            return;
        }
        Debug.Log("setAllEquipmentsに入ります。");
        setAllEquipments();
    }
    
    // Update is called once per frame
    public void UpdateUI()
    {
    }

    private void setAllEquipments()
    {
        try
        {
            slots = slotsParent.GetComponentsInChildren<InventrySlot>();
            int[] saveEquip = gm.savedata.Equipment;

            if (saveEquip.Length < 7 || slots.Length < 7)
            {
                Debug.LogError("配列の長さが不足しています。");
                return;
            }

            for (int i = 0; i < 7; i++)
            {
                if (saveEquip[i] < gm.db.GetItemList().Count)
                {
                    slots[i].SetItem(gm.db.GetItemList()[saveEquip[i]]);
                }
                else
                {
                    Debug.LogError("不正なインデックス: " + saveEquip[i]);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("setAllEquipmentsでエラーが発生しました: " + ex.Message);
        }
    }

    public void getAllEquipments()
    {
        slots = slotsParent.GetComponentsInChildren<InventrySlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].MyItem != null)
            {
                gm.savedata.Equipment[i] = slots[i].MyItem.MyItemNo;
            }
            else
            {
                gm.savedata.Equipment[i] = 0;
            }
        }
    }
}

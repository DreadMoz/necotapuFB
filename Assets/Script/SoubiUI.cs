using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
// エディタ固有のコード
using static UnityEditor.Progress;
#endif


public class SoubiUI : MonoBehaviour
{
    public Transform slotsParent;

    [SerializeField]
    private GameManager gm;

    InventrySlot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        setAllSoubi();
    }

    // Update is called once per frame
    public void UpdateUI()
    {
    }

    private void setAllSoubi()
    {
        try
        {
            int soubiLen = 4;       // いまの装備の長さ最終的に7予定
            slots = slotsParent.GetComponentsInChildren<InventrySlot>();
            int[] saveEquip = gm.savedata.Equipment;

            if (saveEquip.Length < soubiLen || slots.Length < soubiLen)
            {
                Debug.LogError("配列の長さが不足しています。");
                return;
            }

            for (int i = 0; i < soubiLen; i++)
            {
                if (saveEquip[i] < gm.db.GetItemList().Count)
                {
                    slots[i].SetItem(gm.db.GetItemList()[saveEquip[i]], i+1);
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

    public void getAllSoubi()
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

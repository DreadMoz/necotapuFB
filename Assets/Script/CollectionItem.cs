using UnityEngine;
using UnityEngine.UI;

public class CollectionItem : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;

    [SerializeField]
    private ShopData shopdata;
    [SerializeField]
    private GameObject itemSlotPrefab; // アイテムスロットのプレハブ

    [SerializeField]
    private Item[] allItems;

    // Start is called before the first frame update
    void Start()
    {
        setAllItems();
    }
    public void setAllItems()
    {
        try
        {
            ClearChildren();
            bool[] collectItem = gm.savedata.Items;

            for (int i = 0; i < shopdata.weaponIDs.Count; i++)
            {
                GameObject itemSlot = Instantiate(itemSlotPrefab, transform);
                Image itemImage = itemSlot.transform.Find("Item").GetComponent<Image>();

                if (itemImage != null)
                {
                    if (collectItem[shopdata.weaponIDs[i]])
                    {
                        itemImage.sprite = gm.db.GetItemList()[shopdata.weaponIDs[i]].MyItemImage;
                        itemImage.color = new Color(1, 1, 1, 0.9f);
                    }
                    else
                    {
                        itemImage.color = new Color(0, 0, 0, 0);
                    }
                }
            }
            for (int i = 0; i < shopdata.glassesIDs.Count; i++)
            {
                GameObject itemSlot = Instantiate(itemSlotPrefab, transform);
                Image itemImage = itemSlot.transform.Find("Item").GetComponent<Image>();

                if (itemImage != null)
                {
                    if (collectItem[shopdata.glassesIDs[i]])
                    {
                        itemImage.sprite = gm.db.GetItemList()[shopdata.glassesIDs[i]].MyItemImage;
                        itemImage.color = new Color(1, 1, 1, 0.9f);
                    }
                    else
                    {
                        itemImage.color = new Color(0, 0, 0, 0);
                    }
                }
            }
            for (int i = 0; i < shopdata.hatIDs.Count; i++)
            {
                GameObject itemSlot = Instantiate(itemSlotPrefab, transform);
                Image itemImage = itemSlot.transform.Find("Item").GetComponent<Image>();

                if (itemImage != null)
                {
                    if (collectItem[shopdata.hatIDs[i]])
                    {
                        itemImage.sprite = gm.db.GetItemList()[shopdata.hatIDs[i]].MyItemImage;
                        itemImage.color = new Color(1, 1, 1, 0.9f);
                    }
                    else
                    {
                        itemImage.color = new Color(0, 0, 0, 0);
                    }
                }
            }
            for (int i = 0; i < shopdata.eventIDs.Count; i++)
            {
                GameObject itemSlot = Instantiate(itemSlotPrefab, transform);
                Image itemImage = itemSlot.transform.Find("Item").GetComponent<Image>();

                if (itemImage != null)
                {
                    if (collectItem[shopdata.eventIDs[i]])
                    {
                        itemImage.sprite = gm.db.GetItemList()[shopdata.eventIDs[i]].MyItemImage;
                        itemImage.color = new Color(1, 1, 1, 0.9f);
                    }
                    else
                    {
                        itemImage.color = new Color(0, 0, 0, 0);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("setAllItemsでエラーが発生しました: " + ex.Message);
        }
    }
    void ClearChildren()
    {
        // 親オブジェクトのすべての子をループし、それぞれを削除
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}

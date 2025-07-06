using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
// エディタ固有のコード
using static UnityEditor.Progress;
#endif

public class ShopList : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;

    [SerializeField]
    private GameObject shopItemPrefab;

    [SerializeField]
    private TMP_Text talk;

    [SerializeField]
    private GameObject kinoko;
    private Animator kAnimator;

    private Transform shopItemParent;

    // ここで、ShopItemParentのRectTransformを参照する
    [SerializeField]
    private RectTransform shopItemParentRectTransform;

    [SerializeField]
    private ShopData shopDatabase; // ShopDatabaseへの参照

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private Setting setting;
    public int tabNo = 0;
    int[] rankScore = { 0, 40, 80, 120, 200, 263};
    int[] openWeapon = { 3, 6, 9, 13, 17, 20};
    int[] openGlasses = { 3, 5, 8, 10, 12, 13};
    int[] openHat = { 3, 5, 6, 8, 12, 14};

    // Start is called before the first frame update
    void Start()
    {
        // このスクリプトがアタッチされているオブジェクトのTransformを取得
        shopItemParent = transform;

        kAnimator = kinoko.GetComponent<Animator>(); // kinokoのアニメーターを取得
        // 初期アイテムリストの表示
        ShowItemList(tabNo);
    }

    public void listReset()
    {
        kAnimator.SetTrigger("tab");
        scrollRect.verticalNormalizedPosition = 1.0f;
        tabNo = 0;
        ShowItemList(0);
    }

    public void ShowItemListWeapons()
    {
        setting.sayColtu(1);
        tabNo = 0;
        kAnimator.SetTrigger("tab");
        talk.text = "手にもつどうぐですよ。";
        ShowItemList(tabNo);
        scrollRect.verticalNormalizedPosition = 1.0f;
    }
    public void ShowItemListGlasses()
    {
        setting.sayColtu(1);
        tabNo = 1;
        kAnimator.SetTrigger("tab");
        talk.text = "すてきなめがねですよ。";
        ShowItemList(tabNo);
        scrollRect.verticalNormalizedPosition = 1.0f;
    }
    public void ShowItemListHats()
    {
        setting.sayColtu(1);
        tabNo = 2;
        kAnimator.SetTrigger("tab");
        talk.text = "かわいいぼうしですよ。";
        ShowItemList(tabNo);
        scrollRect.verticalNormalizedPosition = 1.0f;
    }

    public void ShowItemList(int kind)
    {
        ClearList();

        List<int> itemIDsToShow = new List<int>();
        int itemLimit = getItemLimit(kind);
        switch (kind)
        {
            case 0:
                itemIDsToShow = shopDatabase.weaponIDs;
                break;
            case 1:
                itemIDsToShow = shopDatabase.glassesIDs;
                break;
            case 2:
                itemIDsToShow = shopDatabase.hatIDs;
                break;
            default:
                break;
        }
        for (int i=0; i<itemIDsToShow.Count; i++)
        {
            if (itemLimit <= i) {
                break;
            }
            Item item = gm.db.GetItemList()[itemIDsToShow[i]]; // 実際のアイテムをIDから取得
            if (item != null)
            {
                AddItem(item);
            }
        }
        // コンテンツエリアの高さをアイテム数に基づいて設定
        float contentHeight = itemLimit * 76; // アイテムの高さ
        shopItemParentRectTransform.sizeDelta = new Vector2(shopItemParentRectTransform.sizeDelta.x, contentHeight);
    }

    public int getItemLimit(int kind)
    {
        int total = gm.savedata.getTotalMedal();
        int limitNo;
        if (rankScore[5] < total) {
            limitNo = 5;
        } else if (rankScore[4] < total) {
            limitNo = 4;
        } else if (rankScore[3] < total) {
            limitNo = 3;
        } else if (rankScore[2] < total) {
            limitNo = 2;
        } else if (rankScore[1] < total) {
            limitNo = 1;
        } else {
            limitNo = 0;
        }
        switch (kind)
        {
            case 0:
                return openWeapon[limitNo];
            case 1:
                return openGlasses[limitNo];
            case 2:
                return openHat[limitNo];
        }
        return 0;
    }

    private void ClearList()
    {
        foreach (Transform child in shopItemParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddItem(Item item)
    {
        GameObject newItem = Instantiate(shopItemPrefab, shopItemParent);
        string typeMes;

        switch (item.MyItemType)
        {
            case ItemType.Weapon:
                typeMes = "(手にもつアイテム)";
                break;
            case ItemType.Hat:
                typeMes = "(あたまにかぶるアイテム)";
                break;
            case ItemType.Glasses:
                typeMes = "(かおにつけるアイテム)";
                break;
            case ItemType.Face:
                typeMes = "(ひょうじょうがかわるよ)";
                break;
            case ItemType.NickName:
                typeMes = "(なまえのうしろにつくことば)";
                break;
            case ItemType.Body:
                typeMes = "(ネコのすがたがかわる)";
                break;
            default:
                typeMes = "？？？？？";
                break;
        }

        // ShopItem個別の板プレハブにアイテムの詳細を設定
        newItem.transform.Find("ItemIcon").GetComponent<Image>().sprite = item.MyItemImage;
        newItem.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = item.MyItemName;
        newItem.transform.Find("ID").GetComponent<TextMeshProUGUI>().text = item.MyItemNo.ToString();
        newItem.transform.Find("Type").GetComponent<TextMeshProUGUI>().text = typeMes;
        newItem.transform.Find("Memo").GetComponent<TextMeshProUGUI>().text = item.MyItemMemo;
        newItem.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = item.MyItemPrice.ToString();

        if (!gm.savedata.Items[item.MyItemNo])
        {
            newItem.transform.Find("SoldOut").GetComponent<Image>().gameObject.SetActive(false);
        }
    }
}

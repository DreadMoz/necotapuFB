using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
#if UNITY_EDITOR
// エディタ固有のコード
using static UnityEditor.Progress;
#endif

public class ShopItem : MonoBehaviour
{
    private GameObject confirmation;
    private TextMeshProUGUI itemName;
    private TextMeshProUGUI itemType;
    private TextMeshProUGUI itemPrice;
    private Image itemIcon;
    private GameObject soldOut;
    public Setting setting;

    public Confirmation conf;

    // Start is called before the first frame update
    void Start()
    {
        setting = FindObjectOfType<Setting>();
        conf = FindObjectOfType<Confirmation>();
        confirmation = GameObject.Find("Confirmation");
        itemName = confirmation.transform.Find("ItemName").GetComponentInChildren<TextMeshProUGUI>();
        itemType = confirmation.transform.Find("Type").GetComponentInChildren<TextMeshProUGUI>();
        itemPrice = confirmation.transform.Find("Price").GetComponentInChildren<TextMeshProUGUI>();
        itemIcon = confirmation.transform.Find("Icon").GetComponentInChildren<Image>();

        soldOut = transform.Find("SoldOut").GetComponent<Image>().gameObject;
    }

    public void OnClick()
    {
        // このShopItemにあるMemoテキストを取得
        string memoText = transform.Find("Memo").GetComponent<TextMeshProUGUI>().text;
        // このShopItemにあるMemoテキストを取得
        string sikaText = transform.Find("Price").GetComponent<TextMeshProUGUI>().text;

        // Commentオブジェクトを探し、そのTMP_Textコンポーネントを取得
        TMP_Text talk = GameObject.Find("Comment").GetComponent<TMP_Text>();

        conf.setIta();

        if (talk != null)
        {
            if (soldOut.activeSelf)
            {
                // クリックされたアイテムのメモをtalkテキストに表示
                talk.text = memoText + "\nいまはうりきれだよ。";
            }
            else
            {
                // クリックされたアイテムのメモをtalkテキストに表示
                talk.text = memoText + "\nねだんは" + sikaText + "ｼｰｶｰだよ。";
            }
        }
    }
    public void OnBuyButton()
    {
        if (soldOut.activeSelf)
        {
            return;
        }

        // このShopItem（板）にあるMemoテキストを取得
        string memoText = transform.Find("Memo").GetComponent<TextMeshProUGUI>().text;
        string sikaText = transform.Find("Price").GetComponent<TextMeshProUGUI>().text;

        // Commentオブジェクトを探し、そのTMP_Textコンポーネントを取得
        TMP_Text talk = GameObject.Find("Comment").GetComponent<TMP_Text>();

        conf.setIta();

        if (talk != null)
        {
            // クリックされたアイテムのメモをtalkテキストに表示
            talk.text = memoText + "\nかっていきますか？。";
        }
        setting.sayWindowSetting();

        conf.show();
        // このShopItemにあるテキストを取得
        itemName.text = transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text;
        int id = int.Parse(transform.Find("ID").GetComponent<TextMeshProUGUI>().text);
        itemPrice.text = transform.Find("Price").GetComponent<TextMeshProUGUI>().text;
        itemType.text = transform.Find("Type").GetComponent<TextMeshProUGUI>().text;

        // このShopItemにあるImageスプライトを取得して設定
        Sprite newSprite = transform.Find("ItemIcon").GetComponent<Image>().sprite;
        itemIcon.sprite = newSprite;

        conf.setItemId(id);
    }
}

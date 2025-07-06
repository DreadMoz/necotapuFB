using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Confirmation : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;

    [SerializeField]
    private GameObject housePlayer;
    private Animator pAnimator;

    [SerializeField]
    private GameObject kinoko;
    private Animator kAnimator;

    [SerializeField]
    private GameObject inventory;

    [SerializeField]
    private GameObject status;

    [SerializeField]
    private TMP_Text talk;

    [SerializeField]
    private GameObject shopList;

    private ShopList shopListReset;
    private InventryUI inventoryui;
    private StatusUI statusui;
    public Setting setting;

    private int itemId;
    private int itemPrice;

    void Awake()
    {
        hide();
        inventoryui = inventory.GetComponentInChildren<InventryUI>();
        statusui = status.GetComponentInChildren<StatusUI>();
        shopListReset = shopList.GetComponentInChildren<ShopList>();

        pAnimator = housePlayer.GetComponent<Animator>(); // Playerのアニメーターを取得
        kAnimator = kinoko.GetComponent<Animator>(); // kinokoのアニメーターを取得
    }
    void Start()
    {
        hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Buy()
    {
        itemPrice = int.Parse(transform.Find("Price").GetComponent<TextMeshProUGUI>().text);
        int blankIndex = gm.savedata.getBlankInventoryIndex();
        if (blankIndex >= 0)
        {
            int saifu = gm.savedata.Status[0];
            if (saifu >= itemPrice)
            {
                setting.sayItemGet();
                pAnimator.SetTrigger("yes");
                kAnimator.SetTrigger("buy");
                gm.savedata.Inventory[blankIndex] = itemId;
                gm.savedata.Status[st.Gold] = saifu - itemPrice;
                gm.savedata.Items[itemId] = true;
                talk.text = "まいどありがとうございます！";
                hide();

                inventoryui.setAllItems();
                statusui.dispStatus();
                shopListReset.ShowItemList(shopListReset.tabNo);

                inventoryui.turnImage(blankIndex);
            }
            else
            {
                setting.sayLessMoney();
                pAnimator.SetTrigger("down");
                kAnimator.SetTrigger("no");
                talk.text = "シーカーがたりないようです。\nタイピングをしてためてきてください。";
                hide();
            }
        }
        else
        {
            setting.sayLessMoney();
            pAnimator.SetTrigger("down");
            kAnimator.SetTrigger("no");
            talk.text = "もちものがいっぱいのようです。";
            Debug.Log("インベントリに空きが見つかりませんでした。Confirmation.Buy");
            hide();
        }
    }

    public void selectNo()
    {
        setting.sayCancel();
        pAnimator.SetTrigger("no");
        kAnimator.SetTrigger("no");
        hide();
        talk.text = "ほかのしょうひんも見ていってくださいね。";
    }

    public void Cancel()
    {
        setting.sayCancel();
        pAnimator.SetTrigger("cancel");
        kAnimator.SetTrigger("cancel");
        hide();
        talk.text = "ほかのしょうひんも見ていってくださいね。";
    }

    private void hide()
    {
        // 画面サイズを都度取得しないと途中での最大化などに対応できない
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Debug.Log("Width:" + screenWidth + "  Height:" + screenHeight);
        transform.position = new Vector2(screenWidth * 0.5f,  screenHeight * 2);
    }

    public void show()
    {
        // 画面サイズを都度取得しないと途中での最大化などに対応できない
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Debug.Log("Width:" + screenWidth + "  Height:" + screenHeight);
        transform.position = new Vector2(screenWidth * 0.5f, screenHeight * 0.5f);
    }

    public void setItemPrice()
    {
    }

    public void setItemId(int id)
    {
        itemId = id;
    }
    public void setIta()
    {
        kAnimator.SetTrigger("ita");
    }
}

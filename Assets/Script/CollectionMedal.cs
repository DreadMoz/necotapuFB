using UnityEngine;

public class CollectionMedal : MonoBehaviour
{
    private bool isWindowShown = false;

    [SerializeField]
    private GameManager gm;

    [SerializeField]
    private Practice practice;

    [SerializeField]
    private GameObject backMedal;
    [SerializeField]
    private GameObject backItem;
    [SerializeField]
    private GameObject menuMedal;
    [SerializeField]
    private GameObject menuItem;
    [SerializeField]
    private CollectionItem collectionItem;
    [SerializeField]
    private Setting setting;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void show(int windowNo)
    {
        if (gm.dragging)
        {
            return;
        }
        setting.sayWindowSetting();
        if (windowNo == 1)
        {
            backMedal.SetActive(true);
            menuMedal.SetActive(true);
            backItem.SetActive(false);
            menuItem.SetActive(false);

            setStars();
        }
        else if (windowNo == 2)
        {
            backMedal.SetActive(false);
            menuMedal.SetActive(false);
            backItem.SetActive(true);
            menuItem.SetActive(true);

            collectionItem.setAllItems();
        }

        // 画面サイズを都度取得しないと途中での最大化などに対応できない
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Debug.Log("Width:" + screenWidth + "  Height:" + screenHeight);
        transform.position = new Vector2(screenWidth * 0.5f, screenHeight * 0.5f);
        isWindowShown = true; // 表示に設定
    }
    
    public void hide()
    {
        if (!isWindowShown)
        {
            return;
        }
        setting.sayWindowClose();
        // 画面サイズを都度取得しないと途中での最大化などに対応できない
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Debug.Log("Width:" + screenWidth + "  Height:" + screenHeight);
        transform.position = new Vector2(screenWidth * 0.5f, screenHeight * 2);
        isWindowShown = false; // 非表示に設定
    }

    private void setStars()
    {
        int[] medals = gm.savedata.Medals;
        int[] topStar = practice.setTopStars();
        
        for (int no = 0; no < topStar.Length; no++)
        {
            Transform childTransform = menuMedal.transform.GetChild(no);
            CollectionStar collectionStar = childTransform.GetComponent<CollectionStar>();
            collectionStar.setSlotStar(topStar[no], medals[no*3], medals[no*3+1], medals[no*3+2]);
        }
    }
}

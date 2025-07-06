using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private GameManager gm;
    [SerializeField] private GameObject typingRoom;
    [SerializeField] private GameObject tiikawa;
    [SerializeField] private GameObject kinoko;
    [SerializeField] private GameObject inventoryButton;
    [SerializeField] private GameObject rankingButton;
    [SerializeField] private GameObject settingButton;
    [SerializeField] private GameObject collectionMedalButton;
    [SerializeField] private GameObject collectionItemButton;
    [SerializeField] private GameObject itemShop;
    [SerializeField] private ShopList shopList;
    [SerializeField] private TypingRoom typingList;
    [SerializeField] private GameObject status;
    [SerializeField] private Fade fade;
    [SerializeField] private Fade fadeDoor;
    [SerializeField] private SwitchCam switchCam;
    [SerializeField] private Practice practice;
    [SerializeField] private GameObject housePlayer;
    [SerializeField] private GameObject exitHouse;
    [SerializeField] private GameObject exitShop;
    [SerializeField] private TMP_Text talk;
    [SerializeField] private TMP_Text skyTalk;
    [SerializeField] private GameObject talkObject;
    [SerializeField] private GameObject fukidashiObject;
    [SerializeField] private GameObject inventoryFilter;
    [SerializeField] private TypingDetail typingDetail;
    [SerializeField] private ChibiCat chibiCat;
    [SerializeField] private float hitBackForce = 0.3f;
    [SerializeField] private GameObject keepOut;
    [SerializeField] private Setting setting;
    [SerializeField] private CollectionMedal collection;
    public Ranking rankingWindow;

    private Animator pAnimator;
    public Camera playerCamera; // レイキャストに使用するカメラ
    private Rigidbody rb;

    private Animator animator;
    private NavMeshAgent agent;
    private float speed = 8f;
    private int typingWindow = 0;
    private int shopWindow = 0;
    private int heijoWindow = 0;
    private int keepOutCount = 0;

    private bool goNextScene = false;    // 次のシーンに遷移するためのフラグ

    void Start()
    {
        if (!gm || !typingRoom || !inventoryButton || !rankingButton || !itemShop || !status || !fade || !fadeDoor)
        {
            Debug.LogError("Playerスクリプトで必要なオブジェクトが割り当てられていません。");
            return;
        }
        rb = GetComponent<Rigidbody>();
        pAnimator = housePlayer.GetComponent<Animator>(); // Playerのアニメーターを取得
        agent = GetComponent<NavMeshAgent>();  // ナビメッシュエージェントを取得
        agent.speed = speed;

        animator = GetComponent<Animator>();  // Playerのアニメーターを取得
        animator.SetInteger("anim", 1);       // アニメーションステートを1に設定 タイトルのアニメーションを抜ける

        keepOut.SetActive(false);
        tiikawa.SetActive(false);
        kinoko.SetActive(false);
        inventoryFilter.SetActive(false);

        if (GameManager.SceneNo == scene.World)
        {
            exitHouse.SetActive(false);
            exitShop.SetActive(false);
//            transform.position = new Vector3(286, 1, 96);
            transform.rotation = Quaternion.Euler(0, 180, 0);
            resetTypingPanel();
        
            animator.SetTrigger("Hi");    // "Hi" トリガーアニメーションを開始
            fade.StartFadeIn();
        }
        else if (GameManager.SceneNo == scene.House)
        {
            if (GameManager.TypingDataPath == null)
            {
                skyTalk.text = "メニューをえらんでね。";
            }
            else
            {
                if (GameManager.TypingTab == 2)
                {
                    gm.registerRecentTypingResult();
                }
            }
            exitHouse.SetActive(true);
            exitShop.SetActive(false);
            tiikawa.SetActive(true);

            switchCam.SwitchCamera();           // カメラ切り替え
        }
        else if (GameManager.SceneNo == scene.Heijo)
        {
            exitHouse.SetActive(false);
            exitShop.SetActive(false);


            typingRoom.SetActive(false);
            itemShop.SetActive(false);
            rankingButton.GetComponent<OpenButton>().OnButton();

            talkObject.SetActive(false);


            GameManager.SceneNo = scene.World;
            heijoWindow = -1;
        
            animator.SetTrigger("Hi");    // "Hi" トリガーアニメーションを開始
        }
    }

    // Update is called once per frame
    void Update()
    {
        // フェードイン中は操作しない
        if (!fade.IsFadeInComplete())
        {
            // 花火処理をしっかり行うため、シーン遷移後に実施
            if (GameManager.SceneNo == scene.House)
            {
                practice.calcStars();           // 表示する星を計算
                if (GameManager.TypingTab == 2)
                {
                    practice.showDetail();          // 詳細画面表示 ステージ番号が入るから星計算の後
                }
                gm.exportLocal();                      // タイピング後のデータ保存ローカル＆GSS
                GameManager.SceneNo = scene.World; // ワールドシーン状態へ
            }

            // プレイヤーの向きを固定
            transform.rotation = transform.rotation;
            return;
        }
        if (typingWindow == 1)
        {
            if (!fadeDoor.IsFadeOutComplete())
            {
                return;
            }
            typingWindow = 0;

            setting.hide();
            collection.hide();
            tiikawa.SetActive(true);
            typingRoom.SetActive(true);
            settingButton.SetActive(false);
            rankingButton.SetActive(false);
            inventoryButton.SetActive(false);
            collectionMedalButton.SetActive(false);
            collectionItemButton.SetActive(false);
            rankingButton.GetComponent<OpenButton>().forceOpen();
            fadeDoor.StartFadeIn();
            exitShop.SetActive(false);
            exitHouse.SetActive(true);

            fukidashiObject.SetActive(false);
            talkObject.SetActive(true);

            // カメラ切り替え
            switchCam.SwitchCamera();

            // NavMeshAgentの目的地をリセット
            agent.ResetPath();
            // "hi" トリガーアニメーションを開始
            pAnimator.SetTrigger("hi");

            practice.calcStars();       // 表示する星を計算
        }
        else if (typingWindow == -1)
        {
            if (!fadeDoor.IsFadeOutComplete())
            {
                return;
            }
            typingWindow = 0;

            typingRoom.SetActive(false);
            settingButton.SetActive(true);
            rankingButton.SetActive(true);
            inventoryButton.SetActive(true);
            collectionMedalButton.SetActive(true);
            collectionItemButton.SetActive(true);
            rankingButton.GetComponent<OpenButton>().OnButton();
            fadeDoor.StartFadeIn();
            exitShop.SetActive(false);
            exitHouse.SetActive(false);
            tiikawa.SetActive(false);
            resetTypingPanel();

            talkObject.SetActive(false);

            transform.position = new Vector3(287f, 1.4f, 112.3f);   // タイピングハウス前位置
            transform.rotation = Quaternion.Euler(0, 40, 0); // タイピングハウス前角度

            // カメラ切り替え
            switchCam.SwitchCamera();

            // NavMeshAgentの目的地をリセット
            agent.ResetPath();
            // "Bow" トリガーアニメーションを開始
            animator.SetTrigger("Bow");
        }
        if (shopWindow == 1)
        {
            if (!fadeDoor.IsFadeOutComplete())
            {
                return;
            }
            shopWindow = 0;

            setting.hide();
            collection.hide();
            itemShop.SetActive(true);
            rankingButton.SetActive(false);
            settingButton.SetActive(false);
            inventoryButton.SetActive(false);
            collectionMedalButton.SetActive(false);
            collectionItemButton.SetActive(false);
            inventoryButton.GetComponent<OpenButton>().forceOpen();
            fadeDoor.StartFadeIn();
            exitShop.SetActive(true);
            kinoko.SetActive(true);

            fukidashiObject.SetActive(true);
            talkObject.SetActive(false);

            // カメラ切り替え
            switchCam.SwitchCamera();

            // "hi" トリガーアニメーションを開始
            pAnimator.SetTrigger("hi");
        }
        else if (shopWindow == -1)
        {
            if (!fadeDoor.IsFadeOutComplete())
            {
                return;
            }
            shopWindow = 0;
            
            itemShop.SetActive(false);
            settingButton.SetActive(true);
            rankingButton.SetActive(true);
            inventoryButton.SetActive(true);
            collectionMedalButton.SetActive(true);
            collectionItemButton.SetActive(true);
            inventoryButton.GetComponent<OpenButton>().OnButton();
            fadeDoor.StartFadeIn();
            exitShop.SetActive(false);
            kinoko.SetActive(false);
            shopList.listReset();

            fukidashiObject.SetActive(false);

            transform.position = new Vector3(238.5f, 1, 141.4f);      // ショップ前場所
            transform.rotation = Quaternion.Euler(0, -57, 0);   // ショップ前角度

            // カメラ切り替え
            switchCam.SwitchCamera();

            // "Bow" トリガーアニメーションを開始
            animator.SetTrigger("Bow");
        }
        if (heijoWindow == -1)
        {
            if (!fade.IsFadeOutComplete())
            {
                return;
            }
            heijoWindow = 0;

            transform.position = new Vector3(259, 1.668f, 173);
            transform.rotation = Quaternion.Euler(0, -179, 0);
        }

        if (keepOutCount > 0)
        {
            keepOutCount--;
            return;
        }
        if (keepOutCount == 0)
        {
            keepOut.SetActive(false);
            // 通り抜け不可処理解除追加予定
            animator.SetBool("Walk", false);
            agent.speed = speed;
        }
        // ダメージまたは"Hi"アニメーション中またはウィンドウを開いたときはプレイヤーの位置を固定
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Damage") || animator.GetCurrentAnimatorStateInfo(0).IsName("Hi") || gm.getWindowOpen())
        {
            agent.ResetPath(); // 目的地を解除する
            agent.velocity = Vector3.zero; // 速度を0にする
            rb.velocity = Vector3.zero; // 速度を0にする
            rb.angularVelocity = Vector3.zero; // 角速度を0にする
        }
        else
        {
            // UI要素上でマウスカーソルがある場合は操作しない
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (!status.activeSelf)
            {
                // 矢印キーによる入力を取得
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                Vector3 direction = new Vector3(horizontal, 0, vertical);

                if (direction.magnitude >= 0.1f)  // 入力がある場合
                {
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0, targetAngle, 0);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 10);

                    rb.velocity = Vector3.zero; // プレイヤーの速度をリセット
                    agent.SetDestination(transform.position + direction * 5f);  // 5は前方への移動距離です
                }

                // マウスクリックによる移動処理
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                    {
                        agent.SetDestination(hit.point);
                        // 目的地に到着したら速度をリセット
                        if (agent.remainingDistance <= agent.stoppingDistance)
                        {
                            agent.velocity = Vector3.zero;
                        }
                    }
                }
                // アニメーション状態の更新
                UpdateAnimationState();
            }
        }
    }

    private void UpdateAnimationState()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    animator.SetBool("Run", false);
                }
            }
            else
            {
                animator.SetBool("Run", true);
            }
        }
    }
    
    void resetTypingPanel()
    {
        for (int i=0; i<66; i++)
        {
            if (gm.savedata.Medals[i] != 4)
            {
                GameManager.TypingTab = 2;
                typingList.panelReset(2);
                return;
            }
        }
        GameManager.TypingTab = 0;
        typingList.panelReset(0);
    }
    
    void OnCollisionEnter(Collision col)
    {
        // 衝突したオブジェクトに応じてアニメーションと目的地を設定
        if (col.gameObject.name == "TypingDoor")
        {
            if (!gm.getWindowOpen())
            {
                rb.velocity = Vector3.zero; // 速度を0にする
                rb.angularVelocity = Vector3.zero; // 角速度を0にする
                setting.sayKnock();
                skyTalk.text = "タイピング練習場へようこそ！";
                // "Hi" トリガーアニメーションを開始
                animator.SetTrigger("Hi");
                agent.destination = this.transform.position;

                fadeDoor.StartFadeOut();
                typingWindow = 1;
            }
        }
        else if (col.gameObject.name == "ShopDoor")
        {
            if (!gm.getWindowOpen())
            {
                rb.velocity = Vector3.zero; // 速度を0にする
                rb.angularVelocity = Vector3.zero; // 角速度を0にする
                setting.sayKnock();
                inventoryFilter.SetActive(true);
                talk.text = "いらっしゃいませ！\nアイテムやさんだよ";
                // "Hi" トリガーアニメーションを開始
                animator.SetTrigger("Hi");
                agent.destination = this.transform.position;

                fadeDoor.StartFadeOut();
                shopWindow = 1;
            }
        }
        // 接触している間中行いたい処理
        else if (col.gameObject.CompareTag("InvisibleFence"))
        {
            rb.velocity = Vector3.zero; // 速度を0にする
            rb.angularVelocity = Vector3.zero; // 角速度を0にする
            agent.destination = this.transform.position;
            keepOut.SetActive(true);
            keepOutCount = (int)(hitBackForce * 80);
            animator.SetBool("Walk", true);
            agent.speed = speed/4;

            // 目的地を解除し、エージェントの移動を停止します。
             agent.ResetPath(); // 目的地を解除する　

            Vector3 centerDirection = (new Vector3(268, 6, 146) - transform.position).normalized;
            // NavMeshAgentを使用して、計算した位置にワープさせる
            agent.Move(centerDirection * hitBackForce / 2);
            agent.destination = transform.position + centerDirection * hitBackForce;
        }
        else if (col.gameObject.name == "kanbanCube")
        {
            /*
            if (!goNextScene)
            {
                GameManager.eventHeijo = true;
                GameManager.TypingDataPath = "TextCustom/heijoEvent";
                GameManager.SceneNo = (int)scene.Typing;
                SceneManager.LoadScene("typingStage"); // タイピングシーンに遷移
                goNextScene = true;
            }
            */
        }
        else if (col.gameObject.name != "Terrain")
        {
            // "Damage" トリガーアニメーションを開始
            animator.SetTrigger("Damage");
            agent.destination = this.transform.position;
        }
    }

    void OnCollisionStay(Collision col)
    {
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("InvisibleFence"))
        {
        }
    }

    public void CloseTypingDoor()
    {
        setting.sayOutDoor();
        typingDetail.hide();
        fadeDoor.StartFadeOut();
        typingWindow = -1;
    }
    public void CloseShopDoor()
    {
        setting.sayOutDoor();
        gm.changeEquip(0);
        inventoryFilter.SetActive(false);
        fadeDoor.StartFadeOut();
        shopWindow = -1;
    }
}

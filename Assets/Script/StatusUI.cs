using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatusUI : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;

    [SerializeField]
    private TextMeshProUGUI TMPHeadName;
    [SerializeField]
    private TextMeshProUGUI TMPName;
    [SerializeField]
    private TextMeshProUGUI TMPGold;
    [SerializeField]
    private TextMeshProUGUI TMPServer;
    [SerializeField]
    private TextMeshProUGUI TMPWpm;
    [SerializeField]
    private TextMeshProUGUI TMPRank;


    // Start is called before the first frame update
    void Start()
    {
        dispStatus();
    }

    // Update is called once per frame
    public void UpdateUI()
    {
    }

    public void dispStatus()
    {
        TMPHeadName.text = gm.savedata.UserName + gm.getNickname(gm.savedata.Equipment[eq.NickName]);
        TMPName.text = gm.savedata.UserName + gm.getNickname(gm.savedata.Equipment[eq.NickName]);
        TMPGold.text = gm.savedata.Status[st.Gold].ToString() + " ｼｰｶｰ";
        TMPServer.text = "サーバー：" + gm.db.GetServerList()[gm.savedata.Status[st.Server]];
        TMPWpm.text = "1分間に" + gm.savedata.Status[st.Kpm] + "キー";
        TMPRank.text = "(" + gm.savedata.Status[st.Rank] + "位 / 200位)";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class Connection : MonoBehaviour
{
    [SerializeField] private GameManager gm;
    [SerializeField] private TitleSky title;

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void GetOAuth();
    [DllImport("__Internal")]
    private static extern void OAuthLogout();
    [DllImport("__Internal")]
    private static extern void LoadDataFromLocal();
    [DllImport("__Internal")]
    private static extern void SaveStatusToLocal(string data);
    [DllImport("__Internal")]
    private static extern void LoadFromGss(); 
    [DllImport("__Internal")]
    private static extern void SaveToGss(string dataPointer);
    [DllImport("__Internal")]
    private static extern void GetNecoRank();
    [DllImport("__Internal")]
    private static extern void ThroughGemini(string dataPointer);

#endif

public void getRanking()
{
#if UNITY_WEBGL && !UNITY_EDITOR
        GetNecoRank();
#endif
}

    public void enetLogin()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GetOAuth();
#else
        getDummyOAuth();
#endif
    }

    public void googleLogout()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OAuthLogout();
#else
        title.finishLogout();
#endif
    }

    public void loadLocal()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        LoadDataFromLocal(); // 拡張機能にデータを要求
#else
        getDummyLocal();
#endif
    }

    public void saveLocal(string data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveStatusToLocal(data);
#endif
    }

    public void saveGas(string dataPointer)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveToGss(dataPointer);
#endif
    }

    public void loadGas()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        LoadFromGss();
#else
        getDummyGss();
#endif
    }

    public void throughGemini(string dataPointer)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ThroughGemini(dataPointer);
#endif
    }

private void getDummyOAuth()
    {
        if (gm.enetToggle.isOn)
        {
            string jsonData = @"{
                ""email"": ""demo@e-net.jp"",
                ""firstName"": ""Demo"",
                ""lastName"": ""Robo"",
                ""picture"": ""https://lh3.googleusercontent.com/a/AAcHTtdjq-TTMMygrjVNtRA6vb15AMinz6HfsldU-_wzQYF3F2j8=s96-c"",
                ""Message"": ""ろぼさんはいいネットならの仲間だね。スタートしましょう。"",
                ""access"": ""true""
            }";
            title.finishOAuth(jsonData);
        }
        else
        {
            string jsonData = @"{
                ""email"": ""rochy2moo@gmail.com"",
                ""firstName"": ""Ryosuke"",
                ""lastName"": ""Mori"",
                ""picture"": ""https://lh3.googleusercontent.com/a/AAcHTtdjq-TTMMygrjVNtRA6vb15AMinz6HfsldU-_wzQYF3F2j8=s96-c"",
                ""Message"": ""いいネットならじゃない・・・"",
                ""access"": ""false""
            }";
            title.finishOAuth(jsonData);
        }
    }

    private void getDummyLocal()
    {
        if (!gm.exToggle.isOn)
        {
            Thread.Sleep(2000);
            title.OnRequestTimeout();       // 拡張機能なし。タイムアウトのイメージ
        }
        else
        {
            // rankingDataとstatusDataを含むダミーのJSON文字列
            string statusJson = @"{
                ""statusData"": {
                    ""Email"": ""demonstration@e-net.nara.jp"",
                    ""Ou"": ""/公立学校/低学年/OU市/OU小学校"",
                    ""LastName"": ""0603-24"",
                    ""Gold"": 999,
                    ""Stage"": 7,
                    ""Ranking"": 87,
                    ""Name"": ""moru"",
                    ""RightHand"": 3,
                    ""Glasses"": 0,
                    ""Head"": 0,
                    ""LeftHand"": 0,
                    ""CatBody"": 202,
                    ""CatFace"": 0,
                    ""NickName"": 0,
                    ""Kpm"": 555,
                    ""Inventory"": [151,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
                    ""Items"": [88,144115188075855872,8388608,0],
                    ""Medals"": [658812288346769700,658812288346769700,658812288346769700,149796,0],
                    ""Kpms"": ""1001006805804107982010"",
                    ""Settings"": [0, 80, 10, 1, 0, 0, 0, 0, 0, 0]
                }
            }";
            title.finishDataLoadExtStatus(statusJson);

            string rankingJson = @"{
                ""rankingData"": [
                    [""moriryo@e-net.nara.jp"", 1, ""ryosuke"", 1, 0, 0, 0, 201, 0, 211, 574],
                    [""mojido@e-net.nara.jp"", 2, ""jido"", 2, 0, 0, 0, 202, 0, 211, 573],
                    [""uuid3@e-net.nara.jp"", 3, ""sota"", 3, 0, 0, 1, 203, 0, 211, 573],
                    [""uuid4@e-net.nara.jp"", 4, ""yuki"", 0, 0, 0, 2, 204, 0, 212, 572],
                    [""uuid5@e-net.nara.jp"", 5, ""hayato"", 0, 0, 0, 3, 205, 0, 213, 572],
                    [""uuid6@e-net.nara.jp"", 6, ""haruki"", 0, 121, 151, 4, 205, 0, 211, 571],
                    [""uuid7@e-net.nara.jp"", 7, ""ryusei"", 4, 0, 0, 5, 208, 0, 212, 571],
                    [""uuid8@e-net.nara.jp"", 8, ""kaito"", 5, 0, 0, 1, 209, 0, 211, 570],
                    [""uuid9@e-net.nara.jp"", 9, ""kota"", 0, 121, 0, 1, 202, 0, 0, 570],
                    [""uuid10@e-net.nara.jp"", 10, ""yuma"", 1, 0, 0, 2, 207, 0, 213, 569],
                    [""uuid11@e-net.nara.jp"", 11, ""soma"", 1, 0, 0, 0, 201, 0, 214, 569],
                    [""uuid12@e-net.nara.jp"", 12, ""riku"", 0, 0, 0, 0, 202, 0, 214, 568],
                    [""uuid13@e-net.nara.jp"", 13, ""sora"", 0, 0, 0, 3, 203, 0, 212, 568],
                    [""uuid14@e-net.nara.jp"", 14, ""ryota"", 0, 0, 151, 0, 204, 0, 214, 567],
                    [""uuid15@e-net.nara.jp"", 15, ""daiki"", 0, 0, 0, 0, 205, 0, 215, 567],
                    [""uuid16@e-net.nara.jp"", 16, ""minato"", 6, 0, 0, 0, 206, 0, 212, 566],
                    [""uuid17@e-net.nara.jp"", 17, ""ren"", 1, 0, 0, 0, 208, 0, 211, 566],
                    [""uuid18@e-net.nara.jp"", 18, ""hinata"", 2, 0, 0, 0, 209, 0, 213, 565],
                    [""uuid19@e-net.nara.jp"", 19, ""kazuki"", 3, 0, 0, 1, 208, 0, 214, 565],
                    [""uuid20@e-net.nara.jp"", 20, ""takumi"", 0, 0, 0, 2, 207, 0, 213, 564],
                    [""uuid21@e-net.nara.jp"", 21, ""hiroto"", 0, 0, 0, 3, 201, 0, 215, 564],
                    [""uuid22@e-net.nara.jp"", 22, ""ryuto"", 0, 0, 0, 4, 202, 0, 212, 563],
                    [""uuid23@e-net.nara.jp"", 23, ""yuma"", 4, 0, 151, 5, 203, 0, 214, 563],
                    [""uuid24@e-net.nara.jp"", 24, ""sosuke"", 5, 0, 0, 1, 204, 0, 0, 562],
                    [""uuid25@e-net.nara.jp"", 25, ""ryu"", 0, 0, 0, 1, 205, 0, 0, 562],
                    [""uuid26@e-net.nara.jp"", 26, ""keita"", 1, 0, 0, 2, 206, 0, 0, 561],
                    [""uuid27@e-net.nara.jp"", 27, ""koki"", 1, 0, 0, 0, 208, 0, 0, 561],
                    [""uuid28@e-net.nara.jp"", 28, ""toma"", 6, 0, 0, 0, 209, 0, 0, 560],
                    [""uuid29@e-net.nara.jp"", 29, ""seiji"", 1, 0, 0, 3, 205, 0, 0, 560],
                    [""uuid30@e-net.nara.jp"", 30, ""yu"", 2, 0, 0, 0, 207, 0, 0, 559],
                    [""uuid31@e-net.nara.jp"", 31, ""hana"", 3, 0, 0, 0, 201, 0, 0, 559],
                    [""uuid32@e-net.nara.jp"", 32, ""yui"", 0, 0, 151, 0, 202, 0, 0, 558],
                    [""uuid33@e-net.nara.jp"", 33, ""rin"", 0, 0, 151, 0, 203, 0, 0, 558],
                    [""uuid34@e-net.nara.jp"", 34, ""mei"", 0, 0, 0, 0, 204, 0, 0, 557],
                    [""uuid35@e-net.nara.jp"", 35, ""mio"", 4, 0, 0, 1, 205, 0, 0, 557],
                    [""uuid36@e-net.nara.jp"", 36, ""saki"", 5, 0, 0, 2, 206, 0, 0, 556],
                    [""demonstration@e-net.nara.jp"", 37, ""moru"", 0, 0, 0, 3, 208, 0, 0, 556],
                    [""uuid38@e-net.nara.jp"", 38, ""yuna"", 1, 0, 0, 4, 209, 0, 0, 555],
                    [""uuid39@e-net.nara.jp"", 39, ""maika"", 1, 121, 151, 5, 204, 0, 0, 555],
                    [""uuid40@e-net.nara.jp"", 40, ""kokona"", 6, 0, 0, 1, 207, 0, 0, 554],
                    [""uuid41@e-net.nara.jp"", 41, ""miku"", 1, 0, 0, 1, 201, 0, 0, 554],
                    [""uuid42@e-net.nara.jp"", 42, ""nana"", 2, 121, 0, 2, 202, 0, 0, 553],
                    [""uuid43@e-net.nara.jp"", 43, ""rika"", 3, 0, 0, 0, 203, 0, 0, 553],
                    [""uuid44@e-net.nara.jp"", 44, ""yuka"", 0, 0, 0, 0, 204, 0, 0, 552],
                    [""uuid45@e-net.nara.jp"", 45, ""haruka"", 0, 0, 0, 3, 205, 0, 0, 552],
                    [""uuid46@e-net.nara.jp"", 46, ""emi"", 0, 0, 0, 0, 206, 0, 0, 551],
                    [""uuid47@e-net.nara.jp"", 47, ""risa"", 4, 0, 151, 0, 208, 0, 0, 551],
                    [""uuid48@e-net.nara.jp"", 48, ""yuri"", 5, 0, 0, 0, 209, 0, 0, 550],
                    [""uuid49@e-net.nara.jp"", 49, ""sakura"", 0, 0, 0, 0, 203, 0, 0, 550],
                    [""uuid50@e-net.nara.jp"", 50, ""rei"", 1, 0, 0, 0, 207, 0, 0, 549],
                    [""uuid51@e-net.nara.jp"", 51, ""noa"", 1, 0, 0, 1, 201, 0, 0, 549],
                    [""uuid52@e-net.nara.jp"", 52, ""mai"", 6, 0, 0, 2, 202, 0, 0, 548],
                    [""uuid53@e-net.nara.jp"", 53, ""rio"", 1, 0, 0, 3, 203, 0, 0, 548],
                    [""uuid54@e-net.nara.jp"", 54, ""meika"", 2, 0, 0, 4, 204, 0, 0, 547],
                    [""uuid55@e-net.nara.jp"", 55, ""erika"", 3, 0, 0, 5, 205, 0, 0, 547],
                    [""uuid56@e-net.nara.jp"", 56, ""airi"", 0, 0, 151, 1, 206, 0, 0, 546],
                    [""uuid57@e-net.nara.jp"", 57, ""marin"", 0, 0, 0, 1, 208, 0, 0, 546],
                    [""uuid58@e-net.nara.jp"", 58, ""aya"", 0, 0, 0, 2, 209, 0, 0, 545],
                    [""uuid59@e-net.nara.jp"", 59, ""mina"", 4, 0, 0, 0, 207, 0, 0, 545],
                    [""uuid60@e-net.nara.jp"", 60, ""yuko"", 5, 0, 0, 0, 207, 0, 0, 544],
                    [""uuid61@e-net.nara.jp"", 61, ""kaede"", 0, 0, 0, 3, 201, 0, 0, 544],
                    [""uuid62@e-net.nara.jp"", 62, ""ayumu"", 1, 0, 0, 0, 202, 0, 0, 543],
                    [""uuid63@e-net.nara.jp"", 63, ""taiga"", 1, 0, 0, 0, 203, 0, 0, 543],
                    [""uuid64@e-net.nara.jp"", 64, ""shota"", 6, 0, 0, 0, 204, 0, 0, 542],
                    [""uuid65@e-net.nara.jp"", 65, ""eito"", 1, 0, 151, 0, 205, 0, 0, 542],
                    [""uuid66@e-net.nara.jp"", 66, ""reo"", 2, 0, 151, 0, 206, 0, 0, 541],
                    [""uuid67@e-net.nara.jp"", 67, ""kensei"", 3, 0, 0, 1, 208, 0, 0, 541],
                    [""uuid68@e-net.nara.jp"", 68, ""shin"", 0, 0, 0, 2, 209, 0, 0, 540],
                    [""uuid69@e-net.nara.jp"", 69, ""manato"", 0, 0, 0, 3, 209, 0, 0, 540],
                    [""uuid70@e-net.nara.jp"", 70, ""ryoga"", 0, 0, 0, 4, 207, 0, 0, 539],
                    [""uuid71@e-net.nara.jp"", 71, ""kanata"", 4, 0, 0, 5, 201, 0, 0, 539],
                    [""uuid72@e-net.nara.jp"", 72, ""tsubasa"", 5, 121, 151, 1, 202, 0, 0, 538],
                    [""uuid73@e-net.nara.jp"", 73, ""itsuki"", 0, 0, 0, 1, 203, 0, 0, 538],
                    [""uuid74@e-net.nara.jp"", 74, ""asahi"", 1, 0, 0, 2, 204, 0, 0, 537],
                    [""uuid75@e-net.nara.jp"", 75, ""mahiro"", 1, 121, 0, 0, 205, 0, 0, 537],
                    [""uuid76@e-net.nara.jp"", 76, ""haru"", 6, 0, 0, 0, 206, 0, 0, 536],
                    [""uuid77@e-net.nara.jp"", 77, ""ikki"", 1, 0, 0, 3, 208, 0, 0, 536],
                    [""uuid78@e-net.nara.jp"", 78, ""sho"", 2, 0, 0, 0, 209, 0, 0, 535],
                    [""uuid79@e-net.nara.jp"", 79, ""yuki"", 3, 0, 0, 0, 206, 0, 0, 535],
                    [""uuid80@e-net.nara.jp"", 80, ""kyou"", 0, 0, 151, 0, 207, 0, 0, 534],
                    [""uuid81@e-net.nara.jp"", 81, ""ayaka"", 0, 0, 0, 0, 201, 0, 0, 534],
                    [""uuid82@e-net.nara.jp"", 82, ""sena"", 0, 0, 0, 0, 202, 0, 0, 533],
                    [""uuid83@e-net.nara.jp"", 83, ""himari"", 4, 0, 0, 1, 203, 0, 0, 533],
                    [""uuid84@e-net.nara.jp"", 84, ""yume"", 5, 0, 0, 2, 204, 0, 0, 532],
                    [""uuid85@e-net.nara.jp"", 85, ""aina"", 0, 0, 0, 3, 205, 0, 0, 532],
                    [""uuid86@e-net.nara.jp"", 86, ""kanon"", 1, 0, 0, 4, 206, 0, 0, 531],
                    [""uuid87@e-net.nara.jp"", 87, ""ryosuke"", 6, 0, 151, 3, 207, 0, 0, 531],
                    [""uuid88@e-net.nara.jp"", 88, ""saya"", 1, 0, 0, 5, 208, 0, 0, 530],
                    [""uuid89@e-net.nara.jp"", 89, ""kaho"", 6, 0, 0, 1, 209, 0, 0, 530],
                    [""uuid89@e-net.nara.jp"", 90, ""kaho"", 6, 0, 0, 1, 209, 0, 0, 530],
                    [""uuid89@e-net.nara.jp"", 91, ""kaho"", 6, 0, 0, 1, 209, 0, 0, 530],
                    [""uuid89@e-net.nara.jp"", 92, ""kaho"", 6, 0, 0, 1, 209, 0, 0, 530],
                    [""uuid89@e-net.nara.jp"", 93, ""kaho"", 6, 0, 0, 1, 209, 0, 0, 530],
                    [""uuid89@e-net.nara.jp"", 94, ""kaho"", 6, 0, 0, 1, 209, 0, 0, 530],
                    [""uuid89@e-net.nara.jp"", 95, ""kaho"", 6, 0, 0, 1, 209, 0, 0, 530],
                    [""uuid89@e-net.nara.jp"", 96, ""kaho"", 6, 0, 0, 1, 209, 0, 0, 530],
                    [""uuid90@e-net.nara.jp"", 97, ""riona"", 5, 0, 0, 0, 206, 0, 0, 526],
                    [""uuid98@e-net.nara.jp"", 98, ""manami"", 0, 0, 0, 0, 208, 0, 0, 525],
                    [""uuid99@e-net.nara.jp"", 99, ""sayaka"", 1, 0, 151, 0, 209, 0, 0, 525],
                    [""uuid100@e-net.nara.jp"", 100, ""nao"", 1, 0, 151, 1, 204, 0, 0, 524],
                    [""uuid101@e-net.nara.jp"", 101, ""yusuke"", 6, 0, 0, 2, 207, 0, 0, 524],
                    [""uuid102@e-net.nara.jp"", 102, ""tatsuya"", 1, 0, 0, 3, 201, 0, 0, 523],
                    [""uuid103@e-net.nara.jp"", 103, ""kazuma"", 2, 0, 0, 4, 202, 0, 0, 523],
                    [""uuid104@e-net.nara.jp"", 104, ""masato"", 3, 0, 0, 5, 203, 0, 0, 522],
                    [""uuid105@e-net.nara.jp"", 105, ""shun"", 0, 0, 0, 1, 204, 0, 0, 522],
                    [""uuid106@e-net.nara.jp"", 106, ""kyohei"", 0, 121, 151, 1, 205, 0, 0, 521],
                    [""uuid107@e-net.nara.jp"", 107, ""takuya"", 0, 0, 0, 2, 206, 0, 0, 521],
                    [""uuid108@e-net.nara.jp"", 108, ""naoki"", 4, 0, 0, 0, 208, 0, 0, 520],
                    [""uuid109@e-net.nara.jp"", 109, ""kenta"", 5, 121, 0, 0, 209, 0, 0, 520],
                    [""uuid110@e-net.nara.jp"", 110, ""jun"", 0, 0, 0, 3, 202, 0, 0, 519],
                    [""uuid111@e-net.nara.jp"", 111, ""misaki"", 1, 0, 0, 0, 207, 0, 0, 519],
                    [""uuid112@e-net.nara.jp"", 112, ""riko"", 1, 0, 0, 0, 201, 0, 0, 518],
                    [""uuid113@e-net.nara.jp"", 113, ""chinatsu"", 6, 0, 0, 0, 202, 0, 0, 518],
                    [""uuid114@e-net.nara.jp"", 114, ""kumi"", 1, 0, 151, 0, 203, 0, 0, 517],
                    [""uuid115@e-net.nara.jp"", 115, ""miyu"", 2, 0, 0, 0, 204, 0, 0, 517],
                    [""uuid116@e-net.nara.jp"", 116, ""ryou"", 3, 0, 0, 1, 205, 0, 0, 516],
                    [""uuid117@e-net.nara.jp"", 117, ""naoko"", 0, 0, 0, 2, 206, 0, 0, 516],
                    [""uuid118@e-net.nara.jp"", 118, ""keiko"", 0, 0, 0, 3, 208, 0, 0, 515],
                    [""uuid119@e-net.nara.jp"", 119, ""chie"", 0, 0, 0, 4, 209, 0, 0, 515],
                    [""uuid120@e-net.nara.jp"", 120, ""akiko"", 4, 0, 0, 5, 205, 0, 0, 514],
                    [""uuid151@e-net.nara.jp"", 151, ""asuka"", 5, 0, 0, 1, 207, 0, 0, 514],
                    [""uuid122@e-net.nara.jp"", 122, ""kaito"", 0, 0, 0, 1, 201, 0, 0, 513],
                    [""uuid123@e-net.nara.jp"", 123, ""natsuki"", 1, 0, 151, 2, 202, 0, 0, 513],
                    [""uuid124@e-net.nara.jp"", 124, ""ryohei"", 1, 0, 0, 0, 203, 0, 0, 512],
                    [""uuid125@e-net.nara.jp"", 125, ""satoshi"", 6, 0, 0, 0, 204, 0, 0, 512],
                    [""uuid126@e-net.nara.jp"", 126, ""takahiro"", 1, 0, 0, 3, 205, 0, 0, 511],
                    [""uuid127@e-net.nara.jp"", 127, ""yasuharu"", 2, 0, 0, 0, 206, 0, 0, 511],
                    [""uuid128@e-net.nara.jp"", 128, ""yoshiki"", 3, 0, 0, 0, 208, 0, 0, 510],
                    [""uuid129@e-net.nara.jp"", 129, ""yota"", 0, 0, 0, 0, 209, 0, 0, 510],
                    [""uuid130@e-net.nara.jp"", 130, ""daigo"", 0, 0, 0, 0, 202, 0, 0, 509],
                    [""uuid131@e-net.nara.jp"", 131, ""ema"", 0, 0, 0, 0, 207, 0, 0, 509],
                    [""uuid132@e-net.nara.jp"", 132, ""himawari"", 4, 0, 151, 1, 201, 0, 0, 508],
                    [""uuid133@e-net.nara.jp"", 133, ""ichika"", 5, 0, 0, 2, 202, 0, 0, 508],
                    [""uuid134@e-net.nara.jp"", 134, ""juri"", 0, 0, 0, 3, 203, 0, 0, 507],
                    [""uuid135@e-net.nara.jp"", 135, ""kairi"", 1, 0, 0, 4, 204, 0, 0, 507],
                    [""uuid136@e-net.nara.jp"", 136, ""runa"", 1, 0, 0, 5, 205, 0, 0, 506],
                    [""uuid137@e-net.nara.jp"", 137, ""mao"", 6, 0, 0, 1, 206, 0, 0, 506],
                    [""uuid138@e-net.nara.jp"", 138, ""nagisa"", 1, 0, 0, 1, 208, 0, 0, 505],
                    [""uuid139@e-net.nara.jp"", 139, ""otoha"", 2, 0, 0, 2, 209, 0, 0, 505],
                    [""uuid140@e-net.nara.jp"", 140, ""hina"", 3, 0, 0, 0, 205, 0, 0, 504],
                    [""uuid141@e-net.nara.jp"", 141, ""rena"", 0, 0, 151, 0, 207, 0, 0, 504],
                    [""uuid142@e-net.nara.jp"", 142, ""suzu"", 0, 0, 0, 3, 201, 0, 0, 503],
                    [""uuid143@e-net.nara.jp"", 143, ""saiga"", 0, 0, 0, 0, 202, 0, 0, 503],
                    [""uuid144@e-net.nara.jp"", 144, ""umi"", 4, 0, 0, 0, 203, 0, 0, 502],
                    [""uuid145@e-net.nara.jp"", 145, ""nami"", 5, 0, 151, 0, 204,0, 0, 502],
                    [""uuid146@e-net.nara.jp"", 146, ""wakana"", 0, 0, 151, 0, 205, 0, 0, 501],
                    [""uuid147@e-net.nara.jp"", 147, ""haruto"", 1, 0, 0, 0, 206, 0, 0, 501],
                    [""uuid148@e-net.nara.jp"", 148, ""yuto"", 1, 0, 0, 0, 208, 0, 0, 500],
                    [""uuid149@e-net.nara.jp"", 149, ""sota"", 1, 0, 0, 0, 209, 0, 0, 500],
                    [""uuid150@e-net.nara.jp"", 150, ""yuki"", 1, 0, 0, 0, 206, 0, 0, 499],
                    ["""", 151, ""yuki"", 0, 0, 0, 0, 201, 0, 0, 90],
                    ["""", 152, ""yuki"", 0, 0, 0, 0, 202, 0, 0, 90],
                    ["""", 153, ""yuki"", 0, 0, 0, 0, 203, 0, 0, 90],
                    ["""", 154, ""yuki"", 0, 0, 0, 0, 204, 0, 0, 90],
                    ["""", 155, ""yuki"", 0, 0, 0, 0, 205, 0, 0, 90],
                    ["""", 156, ""yuki"", 0, 0, 0, 0, 206, 0, 0, 90],
                    ["""", 157, ""yuki"", 0, 0, 0, 0, 207, 0, 0, 90],
                    ["""", 158, ""yuki"", 0, 0, 0, 0, 208, 0, 0, 90],
                    ["""", 159, ""yuki"", 0, 0, 0, 0, 207, 0, 0, 90],
                    ["""", 160, ""yuki"", 0, 0, 0, 0, 206, 0, 0, 80],
                    ["""", 161, ""yuki"", 0, 0, 0, 0, 205, 0, 0, 80],
                    ["""", 162, ""yuki"", 0, 0, 0, 0, 204, 0, 0, 80],
                    ["""", 163, ""yuki"", 0, 0, 0, 0, 203, 0, 0, 80],
                    ["""", 164, ""yuki"", 0, 0, 0, 0, 202, 0, 0, 80],
                    ["""", 165, ""yuki"", 0, 0, 0, 0, 201, 0, 0, 80],
                    ["""", 166, ""yuki"", 0, 0, 0, 0, 202, 0, 0, 80],
                    ["""", 167, ""yuki"", 0, 0, 0, 0, 203, 0, 0, 80],
                    ["""", 168, ""yuki"", 0, 0, 0, 0, 204, 0, 0, 80],
                    ["""", 169, ""yuki"", 0, 0, 0, 0, 205, 0, 0, 80],
                    ["""", 170, ""yuki"", 0, 0, 0, 0, 206, 0, 0, 70],
                    ["""", 171, ""yuki"", 0, 0, 0, 0, 207, 0, 0, 70],
                    ["""", 172, ""yuki"", 0, 0, 0, 0, 208, 0, 0, 70],
                    ["""", 173, ""yuki"", 0, 0, 0, 0, 207, 0, 0, 70],
                    ["""", 174, ""yuki"", 0, 0, 0, 0, 206, 0, 0, 70],
                    ["""", 175, ""yuki"", 0, 0, 0, 0, 205, 0, 0, 70],
                    ["""", 176, ""yuki"", 0, 0, 0, 0, 204, 0, 0, 70],
                    ["""", 177, ""yuki"", 0, 0, 0, 0, 203, 0, 0, 70],
                    ["""", 178, ""yuki"", 0, 0, 0, 0, 202, 0, 0, 70],
                    ["""", 179, ""yuki"", 0, 0, 0, 0, 201, 0, 0, 70],
                    ["""", 180, ""yuki"", 0, 0, 0, 0, 202, 0, 0, 60],
                    ["""", 181, ""yuki"", 0, 0, 0, 0, 203, 0, 0, 60],
                    ["""", 182, ""yuki"", 0, 0, 0, 0, 204, 0, 0, 60],
                    ["""", 183, ""yuki"", 0, 0, 0, 0, 205, 0, 0, 60],
                    ["""", 184, ""yuki"", 0, 0, 0, 0, 206, 0, 0, 60],
                    ["""", 185, ""yuki"", 0, 0, 0, 0, 207, 0, 0, 60],
                    ["""", 186, ""yuki"", 0, 0, 0, 0, 208, 0, 0, 60],
                    ["""", 187, ""yuki"", 0, 0, 0, 0, 207, 0, 0, 60],
                    ["""", 188, ""yuki"", 0, 0, 0, 0, 206, 0, 0, 60],
                    ["""", 189, ""yuki"", 0, 0, 0, 0, 205, 0, 0, 60],
                    ["""", 190, ""yuki190"", 0, 0, 0, 0, 204, 0, 0, 50],
                    ["""", 191, ""yuki"", 0, 0, 0, 0, 203, 0, 0, 50],
                    ["""", 192, ""yuki"", 0, 0, 0, 0, 202, 0, 0, 50],
                    ["""", 193, ""yuki"", 0, 0, 0, 0, 201, 0, 0, 50],
                    ["""", 194, ""yuki"", 0, 0, 0, 0, 202, 0, 0, 50],
                    ["""", 195, ""yuki"", 0, 0, 0, 0, 203, 0, 0, 50],
                    ["""", 196, ""yuki"", 0, 0, 0, 0, 204, 0, 0, 50],
                    ["""", 197, ""yuki"", 0, 0, 0, 0, 205, 0, 0, 50],
                    ["""", 198, ""yuki"", 0, 0, 0, 0, 206, 0, 0, 50],
                    ["""", 199, ""yuki"", 0, 0, 0, 0, 207, 0, 0, 50],
                    ["""", 200, ""yuki200"", 0, 0, 0, 0, 208, 0, 0, 50]
                    ]
                }";
            gm.finishDataLoadExtRanking(rankingJson);
        }
    }
    private void getDummyGss()
    {
        string gssData;
        if (gm.gssToggle.isOn)
        {
            // rankingDataとstatusDataを含むダミーのJSON文字列
            gssData = @"{
                ""email"": ""demo@e-net.jp"",
                ""ou"": ""/公立学校/低学年/OU市/OU小学校"",
                ""lastName"": ""0603-24"",
                ""gold"": ""65900"",
                ""stage"": ""7"",
                ""ranking"": ""87"",
                ""name"": ""moru"",
                ""rightHand"": ""6"",
                ""glasses"": ""0"",
                ""head"": ""121"",
                ""leftHand"": ""3"",
                ""catBody"": ""0"",
                ""catFace"": ""0"",
                ""nickName"": ""0"",
                ""kpm"": ""333"",
                ""kpms"": ""122333444555666777"",
                ""medals"": [""54100"", ""476371964491057500"", ""471305275021828740"", ""511767441717405440"", ""0""],
                ""items"": [""0"", ""0"", ""0"", ""0""]
            }";
        }
        else
        {
            gssData = @"{}";
        }
        title.finishDataLoadGas(gssData);
}
}
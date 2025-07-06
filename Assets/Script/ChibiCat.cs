using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ChibiCat : MonoBehaviour
{
    private Material[] materials;

    public Material[] cats;
    public Material[] emos;

    [SerializeField] private GameObject spadB;
    [SerializeField] private GameObject panB;
    [SerializeField] private GameObject driedFishB;
    [SerializeField] private GameObject meatB;

    [SerializeField] private GameObject hikingHat;
    [SerializeField] private GameObject hikingHats;
    [SerializeField] private GameObject cowboyHat;
    [SerializeField] private GameObject magicianHat;
    [SerializeField] private GameObject pajamaHat;
    [SerializeField] private GameObject pajamaHatG;
    [SerializeField] private GameObject pajamaHatP;
    [SerializeField] private GameObject pajamaHatY;
    [SerializeField] private GameObject showerHat;
    [SerializeField] private GameObject vikingHelm;
    [SerializeField] private GameObject mexicoHat;
    [SerializeField] private GameObject cakeS;
    [SerializeField] private GameObject ducks;
    [SerializeField] private GameObject pingpong;

    [SerializeField] private GameObject grassARed;
    [SerializeField] private GameObject grassABlue;
    [SerializeField] private GameObject grassABlack;
    [SerializeField] private GameObject sunglasMaruB;
    [SerializeField] private GameObject sunglasMaruG;
    [SerializeField] private GameObject sunglasMaruC;
    [SerializeField] private GameObject sunglasMaruR;
    [SerializeField] private GameObject sunglasB;
    [SerializeField] private GameObject sunglasR;
    [SerializeField] private GameObject sunglasY;
    [SerializeField] private GameObject sunglasMono;
    [SerializeField] private GameObject steampunk;
    [SerializeField] private GameObject vrGoggle;

    [SerializeField] private GameObject spadR;
    [SerializeField] private GameObject driedFishR;
    [SerializeField] private GameObject meatR;
    [SerializeField] private GameObject battonWoodR;
    [SerializeField] private GameObject whirligigR;
    [SerializeField] private GameObject panR;
    [SerializeField] private GameObject donutChocoR;
    [SerializeField] private GameObject donutStrawberryR;
    [SerializeField] private GameObject pencilR;
    [SerializeField] private GameObject eraserR;
    [SerializeField] private GameObject pingpongRacketR;
    [SerializeField] private GameObject duckPinkR;
    [SerializeField] private GameObject duckPondR;
    [SerializeField] private GameObject soccerR;
    [SerializeField] private GameObject volleyR;
    [SerializeField] private GameObject clipboardR;
    [SerializeField] private GameObject smartphoneR;
    [SerializeField] private GameObject laptopR;
    [SerializeField] private GameObject tennisR;
    [SerializeField] private GameObject tennisBatR;
    [SerializeField] private GameObject basketBallR;
    [SerializeField] private GameObject eSnowManR;
    [SerializeField] private GameObject eTreeR;


    [SerializeField] private GameObject spadL;
    [SerializeField] private GameObject driedFishL;
    [SerializeField] private GameObject meatL;
    [SerializeField] private GameObject battonWoodL;
    [SerializeField] private GameObject whirligigL;
    [SerializeField] private GameObject panL;
    [SerializeField] private GameObject donutChocoL;
    [SerializeField] private GameObject donutStrawberryL;
    [SerializeField] private GameObject pencilL;
    [SerializeField] private GameObject eraserL;
    [SerializeField] private GameObject pingpongRacketL;
    [SerializeField] private GameObject duckPinkL;
    [SerializeField] private GameObject duckPondL;
    [SerializeField] private GameObject soccerL;
    [SerializeField] private GameObject volleyL;
    [SerializeField] private GameObject clipboardL;
    [SerializeField] private GameObject smartphoneL;
    [SerializeField] private GameObject laptopL;
    [SerializeField] private GameObject tennisL;
    [SerializeField] private GameObject tennisBatL;
    [SerializeField] private GameObject basketBallL;
    [SerializeField] private GameObject eSnowManL;
    [SerializeField] private GameObject eTreeL;

    // Start is called before the first frame update
    void Awake()
    {
        materials = GetComponent<Renderer>().materials;
        releaseAllEquip(0xFF);
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Alpha1)) { setChara(1); }
        // if (Input.GetKeyDown(KeyCode.Alpha2)) { setChara(2); }
        // if (Input.GetKeyDown(KeyCode.Alpha3)) { setChara(3); }
        // if (Input.GetKeyDown(KeyCode.Alpha4)) { setChara(4); }
        // if (Input.GetKeyDown(KeyCode.Alpha5)) { setChara(5); }
        // if (Input.GetKeyDown(KeyCode.Alpha6)) { setChara(6); }
        // if (Input.GetKeyDown(KeyCode.Alpha7)) { setChara(7); }
        // if (Input.GetKeyDown(KeyCode.Alpha8)) { setChara(8); }
        // if (Input.GetKeyDown(KeyCode.Alpha9)) { setChara(9); }
        // if (Input.GetKeyDown(KeyCode.Alpha0)) { setChara(0); }
//        if (Input.GetKeyDown(KeyCode.Q)) { setEmo(0); }
//        if (Input.GetKeyDown(KeyCode.W)) { setEmo(3); }
//        if (Input.GetKeyDown(KeyCode.E)) { setEmo(19); }
//        if (Input.GetKeyDown(KeyCode.R)) { setEmo(11); }
    }

    public void setName(string name)
    {
        var textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = name; // 名前をテキストにセット
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found on " + gameObject.name);
        }
    }

    public void setChara(int no)
    {
        if (no == 0)
        {
            no = 1;
        }
        else
        {
            no -= 200;
        }
        Material[] tmp = materials;
        if (tmp != null)
        {
            tmp[0] = cats[no];
            GetComponent<Renderer>().materials = tmp;
        }
    }
    public void setEmo(int no)
    {
        Material[] tmp = materials;
        tmp[1] = emos[no];
        GetComponent<Renderer>().materials = tmp;
    }
    public void changeEquipHands(int itemIdRight, int itemIdLeft, int bagItem)
    {
        releaseHands(bagItem);
        switch (itemIdRight)     // 右手
        {
            case 1:
                spadR.SetActive(true);
                spadB.SetActive(false);
                break;
            case 2:
                driedFishR.SetActive(true);
                driedFishB.SetActive(false);
                break;
            case 3:
                meatR.SetActive(true);
                meatB.SetActive(false);
                break;
            case 4:
                battonWoodR.SetActive(true);
                break;
            case 5:
                whirligigR.SetActive(true);
                break;
            case 6:
                panR.SetActive(true);
                panB.SetActive(false);
                break;
            case 7:
                donutChocoR.SetActive(true);
                break;
            case 8:
                donutStrawberryR.SetActive(true);
                break;
            case 9:
                pingpongRacketR.SetActive(true);
                break;
            case 10:
                pencilR.SetActive(true);
                break;
            case 11:
                eraserR.SetActive(true);
                break;
            case 12:
                duckPondR.SetActive(true);
                break;
            case 13:
                duckPinkR.SetActive(true);
                break;
            case 14:
                soccerR.SetActive(true);
                break;
            case 15:
                volleyR.SetActive(true);
                break;
            case 16:
                clipboardR.SetActive(true);
                break;
            case 17:
                smartphoneR.SetActive(true);
                break;
            case 18:
                laptopR.SetActive(true);
                break;
            case 19:
                tennisR.SetActive(true);
                break;
            case 20:
                tennisBatR.SetActive(true);
                break;
            case 21:
                basketBallR.SetActive(true);
                break;
            case 241:
                eSnowManR.SetActive(true);
                break;
            case 242:
                eTreeR.SetActive(true);
                break;
        }
        switch (itemIdLeft)     // 左手
        {
            case 1:
                spadL.SetActive(true);
                spadB.SetActive(false);
                break;
            case 2:
                driedFishL.SetActive(true);
                driedFishB.SetActive(false);
                break;
            case 3:
                meatL.SetActive(true);
                meatB.SetActive(false);
                break;
            case 4:
                battonWoodL.SetActive(true);
                break;
            case 5:
                whirligigL.SetActive(true);
                break;
            case 6:
                panL.SetActive(true);
                panB.SetActive(false);
                break;
            case 7:
                donutChocoL.SetActive(true);
                break;
            case 8:
                donutStrawberryL.SetActive(true);
                break;
            case 9:
                pingpongRacketL.SetActive(true);
                break;
            case 10:
                pencilL.SetActive(true);
                break;
            case 11:
                eraserL.SetActive(true);
                break;
            case 12:
                duckPondL.SetActive(true);
                break;
            case 13:
                duckPinkL.SetActive(true);
                break;
            case 14:
                soccerL.SetActive(true);
                break;
            case 15:
                volleyL.SetActive(true);
                break;
            case 16:
                clipboardL.SetActive(true);
                break;
            case 17:
                smartphoneL.SetActive(true);
                break;
            case 18:
                laptopL.SetActive(true);
                break;
            case 19:
                tennisL.SetActive(true);
                break;
            case 20:
                tennisBatL.SetActive(true);
                break;
            case 21:
                basketBallL.SetActive(true);
                break;
            case 241:
                eSnowManL.SetActive(true);
                break;
            case 242:
                eTreeL.SetActive(true);
                break;
        }
    }

    public void changeEquipGlasses(int itemIdGrass)
    {
        releaseGlasses();
        switch (itemIdGrass)
        {
            case 121:
                grassARed.SetActive(true);
                break;
            case 122:
                grassABlue.SetActive(true);
                break;
            case 123:
                grassABlack.SetActive(true);
                break;
            case 124:
                sunglasMaruB.SetActive(true);
                break;
            case 125:
                sunglasMaruG.SetActive(true);
                break;
            case 126:
                sunglasB.SetActive(true);
                break;
            case 127:
                sunglasR.SetActive(true);
                break;
            case 128:
                sunglasY.SetActive(true);
                break;
            case 129:
                sunglasMono.SetActive(true);
                break;
            case 130:
                steampunk.SetActive(true);
                break;
            case 131:
                vrGoggle.SetActive(true);
                break;
            case 132:
                sunglasMaruC.SetActive(true);
                break;
            case 133:
                sunglasMaruR.SetActive(true);
                break;
        }
    }
    public void changeEquipHead(int itemIdHead)
    {
        releaseHead();
        switch (itemIdHead)
        {
            case 151:
                hikingHat.SetActive(true);
                break;
            case 152:
                hikingHats.SetActive(true);
                break;
            case 153:
                cowboyHat.SetActive(true);
                break;
            case 154:
                magicianHat.SetActive(true);
                break;
            case 155:
                pajamaHat.SetActive(true);
                break;
            case 156:
                showerHat.SetActive(true);
                break;
            case 157:
                vikingHelm.SetActive(true);
                break;
            case 158:
                mexicoHat.SetActive(true);
                break;
            case 159:
                cakeS.SetActive(true);
                break;
            case 160:
                ducks.SetActive(true);
                break;
            case 161:
                pingpong.SetActive(true);
                break;
            case 162:
                pajamaHatG.SetActive(true);
                break;
            case 163:
                pajamaHatP.SetActive(true);
                break;
            case 164:
                pajamaHatY.SetActive(true);
                break;
        }
    }

    public void releaseAllEquip(int bagItem = 0)
    {
        releaseHands(bagItem);
        releaseHead();
        releaseGlasses();
    }

    private void releaseHands(int bagItem)
    {
        panB.SetActive((bagItem & 0x01) == 0x01);          // インベントリにあればかばんに付ける
        spadB.SetActive((bagItem & 0x02) == 0x02);
        driedFishB.SetActive((bagItem & 0x04) == 0x04);
        meatB.SetActive((bagItem & 0x08) == 0x08);

        battonWoodR.SetActive(false);   // 右手解除
        spadR.SetActive(false);
        whirligigR.SetActive(false);
        panR.SetActive(false);
        driedFishR.SetActive(false);
        meatR.SetActive(false);
        donutChocoR.SetActive(false);
        donutStrawberryR.SetActive(false);
        pencilR.SetActive(false);
        eraserR.SetActive(false);
        pingpongRacketR.SetActive(false);
        duckPinkR.SetActive(false);
        duckPondR.SetActive(false);
        soccerR.SetActive(false);
        volleyR.SetActive(false);
        clipboardR.SetActive(false);
        smartphoneR.SetActive(false);
        laptopR.SetActive(false);
        tennisR.SetActive(false);
        tennisBatR.SetActive(false);
        basketBallR.SetActive(false);
        eSnowManR.SetActive(false);
        eTreeR.SetActive(false);


        battonWoodL.SetActive(false);   // 左手解除
        spadL.SetActive(false);
        whirligigL.SetActive(false);
        panL.SetActive(false);
        driedFishL.SetActive(false);
        meatL.SetActive(false);
        donutChocoL.SetActive(false);
        donutStrawberryL.SetActive(false);
        pencilL.SetActive(false);
        eraserL.SetActive(false);
        pingpongRacketL.SetActive(false);
        duckPinkL.SetActive(false);
        duckPondL.SetActive(false);
        soccerL.SetActive(false);
        volleyL.SetActive(false);
        clipboardL.SetActive(false);
        smartphoneL.SetActive(false);
        laptopL.SetActive(false);
        tennisL.SetActive(false);
        tennisBatL.SetActive(false);
        basketBallL.SetActive(false);
        eSnowManL.SetActive(false);
        eTreeL.SetActive(false);
    }

    private void releaseGlasses()
    {
        grassARed.SetActive(false);
        grassABlue.SetActive(false);
        grassABlack.SetActive(false);
        sunglasMaruB.SetActive(false);
        sunglasMaruG.SetActive(false);
        sunglasMaruC.SetActive(false);
        sunglasMaruR.SetActive(false);
        sunglasB.SetActive(false);
        sunglasR.SetActive(false);
        sunglasY.SetActive(false);
        sunglasMono.SetActive(false);
        steampunk.SetActive(false);
        vrGoggle.SetActive(false);
    }

    private void releaseHead()
    {
        hikingHat.SetActive(false);
        hikingHats.SetActive(false);
        cowboyHat.SetActive(false);
        magicianHat.SetActive(false);
        pajamaHat.SetActive(false);
        showerHat.SetActive(false);
        vikingHelm.SetActive(false);
        mexicoHat.SetActive(false);
        cakeS.SetActive(false);
        pingpong.SetActive(false);
        ducks.SetActive(false);
        pajamaHatG.SetActive(false);
        pajamaHatP.SetActive(false);
        pajamaHatY.SetActive(false);
    }
}

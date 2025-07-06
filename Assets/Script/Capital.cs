using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Capital : MonoBehaviour
{
    [SerializeField]
    GameManager gm;
    [SerializeField]
    GameObject capitala;
    [SerializeField]
    GameObject capitalA;
    [SerializeField]
    Slider slider;
    [SerializeField]
    TypingVoice voice;
    
    [SerializeField] private TMP_Text keyq;
    [SerializeField] private TMP_Text keyw;
    [SerializeField] private TMP_Text keye;
    [SerializeField] private TMP_Text keyr;
    [SerializeField] private TMP_Text keyt;
    [SerializeField] private TMP_Text keyy;
    [SerializeField] private TMP_Text keyu;
    [SerializeField] private TMP_Text keyi;
    [SerializeField] private TMP_Text keyo;
    [SerializeField] private TMP_Text keyp;
    [SerializeField] private TMP_Text keya;
    [SerializeField] private TMP_Text keys;
    [SerializeField] private TMP_Text keyd;
    [SerializeField] private TMP_Text keyf;
    [SerializeField] private TMP_Text keyg;
    [SerializeField] private TMP_Text keyh;
    [SerializeField] private TMP_Text keyj;
    [SerializeField] private TMP_Text keyk;
    [SerializeField] private TMP_Text keyl;
    [SerializeField] private TMP_Text keyz;
    [SerializeField] private TMP_Text keyx;
    [SerializeField] private TMP_Text keyc;
    [SerializeField] private TMP_Text keyv;
    [SerializeField] private TMP_Text keyb;
    [SerializeField] private TMP_Text keyn;
    [SerializeField] private TMP_Text keym;
    // Start is called before the first frame update
    void Start()
    {
        slider.value = gm.savedata.Settings[se.Capital];
        changeToggle();
    }

    public void changeToggle()
    {
        voice.sayColtu();
        if (slider.value == 1)
        {
            capitala.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            capitalA.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);

            keyq.text = "Q";
            keyw.text = "W";
            keye.text = "E";
            keyr.text = "R";
            keyt.text = "T";
            keyy.text = "Y";
            keyu.text = "U";
            keyi.text = "I";
            keyo.text = "O";
            keyp.text = "P";
            keya.text = "A";
            keys.text = "S";  
            keyd.text = "D";
            keyf.text = "F";
            keyg.text = "G";
            keyh.text = "H";
            keyj.text = "J";
            keyk.text = "K";
            keyl.text = "L";
            keyz.text = "Z";
            keyx.text = "X";
            keyc.text = "C";
            keyv.text = "V";
            keyb.text = "B";
            keyn.text = "N";
            keym.text = "M";
        }
        else
        {
            capitala.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            capitalA.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

            keyq.text = "q";
            keyw.text = "w";
            keye.text = "e";
            keyr.text = "r";
            keyt.text = "t";
            keyy.text = "y";
            keyu.text = "u";
            keyi.text = "i";
            keyo.text = "o";
            keyp.text = "p";
            keya.text = "a";
            keys.text = "s";  
            keyd.text = "d";
            keyf.text = "f";
            keyg.text = "g";
            keyh.text = "h";
            keyj.text = "j";
            keyk.text = "k";
            keyl.text = "l";
            keyz.text = "z";
            keyx.text = "x";
            keyc.text = "c";
            keyv.text = "v";
            keyb.text = "b";
            keyn.text = "n";
            keym.text = "m";
        }
        gm.savedata.Settings[se.Capital] = (int)slider.value;
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);    // EventSystemのフォーカスをクリア

    }
}

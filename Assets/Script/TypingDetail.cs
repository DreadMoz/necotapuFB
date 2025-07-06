using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TypingDetail : MonoBehaviour
{
    [SerializeField]
    private GameObject stage1;
    [SerializeField]
    private GameObject stage2;
    [SerializeField]
    private GameObject stage3;
    [SerializeField]
    private TextMeshProUGUI memo;

    private DetailMenu menu1;
    private DetailMenu menu2;
    private DetailMenu menu3;

    void Awake()
    {
        menu1 = stage1.GetComponent<DetailMenu>();
        menu2 = stage2.GetComponent<DetailMenu>();
        menu3 = stage3.GetComponent<DetailMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setComment(string comment)
    {
        memo.text = comment + "\nのれんしゅう";
    }

    public void show()
    {
        menu1.showStars();
        menu2.showStars();
        menu3.showStars();
        transform.position = new Vector3(252, 222, transform.position.z);
    }

    public void hide()
    {
        transform.position = new Vector3(252, 999, transform.position.z);
        menu1.resetStars();
        menu2.resetStars();
        menu3.resetStars();
    }
}

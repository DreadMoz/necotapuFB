using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectionStar : MonoBehaviour
{
    [SerializeField] private GameObject[] star;
    [SerializeField] private GameObject[] topStar;
    // Start is called before the first frame update
    public void setSlotStar(int tStar, int star1, int star2, int star3)
    {
        foreach(GameObject st in star)
        {
            st.SetActive(false);
        }
        foreach(GameObject tst in topStar)
        {
            tst.SetActive(false);
        }

        switch(tStar)
        {
            case 2:
                topStar[0].SetActive(true);
                break;
            case 3:
                topStar[0].SetActive(true);
                topStar[1].SetActive(true);
                break;
            case 4:
                topStar[0].SetActive(true);
                topStar[1].SetActive(true);
                topStar[2].SetActive(true);
                break;
        }

        switch(star1)
        {
            case 2:
                star[0].SetActive(true);
                break;
            case 3:
                star[0].SetActive(true);
                star[1].SetActive(true);
                break;
            case 4:
                star[0].SetActive(true);
                star[1].SetActive(true);
                star[2].SetActive(true);
                break;
        }
        switch(star2)
        {
            case 2:
                star[3].SetActive(true);
                break;
            case 3:
                star[3].SetActive(true);
                star[4].SetActive(true);
                break;
            case 4:
                star[3].SetActive(true);
                star[4].SetActive(true);
                star[5].SetActive(true);
                break;
        }
        switch(star3)
        {
            case 2:
                star[6].SetActive(true);
                break;
            case 3:
                star[6].SetActive(true);
                star[7].SetActive(true);
                break;
            case 4:
                star[6].SetActive(true);
                star[7].SetActive(true);
                star[8].SetActive(true);
                break;
        }
    }
}

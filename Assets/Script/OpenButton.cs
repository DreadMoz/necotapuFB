using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OpenButton : MonoBehaviour
{
    private GameObject gameManager;
    private GameManager gm;
    private bool doOpen = false;
    private bool forceShop = false;
        void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        gm = gameManager.GetComponent<GameManager>();
    }
    public void OnButton()
    {
        if (gm.dragging)
        {
            return;
        }
        doOpen = !doOpen;
    }

    public bool isOpen()
    {
        return doOpen;
    }
    public bool isForce()
    {
        if (forceShop)
        {
            forceShop = false;
            return true;
        }
        return false;
    }
    
    public void resetOpen()
    {
        doOpen = false;
    }
    public void forceOpen()
    {
        doOpen = true;
        forceShop = true;
    }

}

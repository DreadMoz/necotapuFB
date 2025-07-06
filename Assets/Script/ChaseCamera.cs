using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public GameManager gameMaster;
    public GameObject player;
    private Material skyboxMaterial;

    Vector3 chaseOffset = new Vector3(0f, 8f, -14f);

    // Use this for initialization
    void Start ()
    {
        skyboxMaterial = RenderSettings.skybox;
        skyboxMaterial.SetFloat("_Rotation", 356);
    }

    // Update is called once per frame
    void Update ()
    {
        if (gameMaster.getCameraMove() == 0)
        {
            transform.position = player.transform.position + chaseOffset;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class Hovl_DemoLasers : MonoBehaviour
{
   // public float ClipLenght = 1f;
    public AudioSource laserSoundSfx;
    public AudioSource enemyHitSfx;
    
    public GameObject FirePoint;
    public Camera Cam;
    public float MaxLength;
    public GameObject[] Prefabs;

    private Ray RayMouse;
    private Vector3 direction;
    private Quaternion rotation;

    [Header("GUI")] private float windowDpi;

    private int Prefab;
    private GameObject Instance;
    private Hovl_Laser LaserScript;
    private Hovl_Laser2 LaserScript2;

    //Double-click protection
    private float buttonSaver = 0f;

    void Start()
    {
        //AudioClip.SetActive(false);
        
        //LaserEndPoint = new Vector3(0, 0, 0);
        if (Screen.dpi < 1) windowDpi = 1;
        if (Screen.dpi < 200) windowDpi = 1;
        else windowDpi = Screen.dpi / 200f;
        //Counter(0);
    }

    void Update()
    {

    }

    // Stop shooting laser
    public void StopShootingLaser()
    {
        if (LaserScript) LaserScript.DisablePrepare();
        if (LaserScript2) LaserScript2.DisablePrepare();
        Destroy(Instance, 1);
        //enemyHitSfx.Stop();
    }

    // Start shooting laser
    public void StartShootLaser()
    {
        Destroy(Instance);
        Instance = Instantiate(Prefabs[Prefab], FirePoint.transform.position, FirePoint.transform.rotation);
        Instance.transform.parent = transform;
        LaserScript = Instance.GetComponent<Hovl_Laser>();
        LaserScript2 = Instance.GetComponent<Hovl_Laser2>();
        
    }

    //GUI Text
    void OnGUI()
    {
        GUI.Label(new Rect(10 * windowDpi, 5 * windowDpi, 400 * windowDpi, 20 * windowDpi),
            "Use the keyboard buttons A/<- and D/-> to change lazers!");
        GUI.Label(new Rect(10 * windowDpi, 20 * windowDpi, 400 * windowDpi, 20 * windowDpi),
            "Use left mouse button for shooting!");
    }

    //To change prefabs (count - prefab number)
    void Counter(int count)
    {
        Prefab += count;
        if (Prefab > Prefabs.Length - 1)
        {
            Prefab = 0;
        }
        else if (Prefab < 0)
        {
            Prefab = Prefabs.Length - 1;
        }
    }

    /*//To rotate fire point
    void RotateToMouseDirection(GameObject obj, Vector3 destination)
    {
        direction = destination - obj.transform.position;
        rotation = Quaternion.LookRotation(direction);
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1);
    }*/
}

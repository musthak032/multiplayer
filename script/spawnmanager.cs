using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnmanager : MonoBehaviour
{
    public static spawnmanager instance;
    public Transform[] spawnpoints;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform spawn in spawnpoints)
        {
            spawn.gameObject.SetActive(false);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Transform getspawnpoint()
    {
        return spawnpoints[Random.Range(0,spawnpoints.Length)];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class playerspawner : MonoBehaviour
{
    public static playerspawner instance;
    private void Awake()
    {
        instance = this;
    }
    public GameObject playerprefab;
    public GameObject deatheffect;
    public float respawntime;

    private GameObject player;

    
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            spawntheplayer();
        }
        
    }

   public void spawntheplayer()
    {
        Transform spawnpoints = spawnmanager.instance.getspawnpoint();

       player=PhotonNetwork.Instantiate(playerprefab.name, spawnpoints.position, spawnpoints.rotation);
    }
    public void die(string damager)
    {
       
        uicontroller.instance.deathtext.text = "you are killed by" + damager;
        //PhotonNetwork.Destroy(player);
        //spawntheplayer();
        matchmanager.instance.updatestatsend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
        if (player != null)
        {
            StartCoroutine(dieco());
        }
    }
    public IEnumerator dieco()
    {
        PhotonNetwork.Instantiate(deatheffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        player = null;
        uicontroller.instance.deathscreen.SetActive(true);
        yield return new WaitForSeconds(respawntime);
        uicontroller.instance.deathscreen.SetActive(false);

        if (matchmanager.instance.state == matchmanager.gamestate.playing && player == null)
        {
            spawntheplayer();
        }
    }
}

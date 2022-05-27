using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
public class uicontroller : MonoBehaviour
{
    public static uicontroller instance;

    private void Awake()
    {
        instance = this;
    }
    public TMP_Text overheatedmessage;
    public Slider weapontempslider;
    public GameObject deathscreen;
    public TMP_Text deathtext;

    public Slider healthslider;

    public TMP_Text killstext;
    public TMP_Text deathstext;

    public GameObject leaderboard;
    public leaderboardplayer leaderboardplayerdisplay;

    public GameObject EndScreen;

    public TMP_Text timertext;

    public GameObject optionscreen;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            showhideoption();
        }
        if (optionscreen.activeInHierarchy && Cursor.lockState!=CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void showhideoption()
    {
        if (!optionscreen.activeInHierarchy)
        {
            optionscreen.SetActive(true);
        }
        else
        {
            optionscreen.SetActive(false);
        }

    }
    public void returnmainmenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();

    }
    public void quitegame()
    {
        Application.Quit();

    }
}

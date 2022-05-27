using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class launcher : MonoBehaviourPunCallbacks
{
    public static launcher instance;
    private void Awake()
    {
        instance = this;
    }

    public GameObject loadingscreen;
    public GameObject menubutton;
    public TMP_Text loadingtext;
    public GameObject createroomscreen;
    public TMP_InputField roomnameinput;
    public GameObject roomscreen;
    public TMP_Text roomnametext,playernamelabel;
    public GameObject errorscreen;
    public TMP_Text errortext;

    public GameObject roombrowserscreen;
    public roombutton theroombutton;
    public GameObject nameinputscreen;
    public TMP_InputField nameinput;
    public GameObject startbutton;
    public GameObject roomtestbutton;
    public string leveltoplay;


    private List<roombutton> allroombuttons = new List<roombutton>();
    private List<TMP_Text> allplayersnames = new List<TMP_Text>();
    public static bool hassetnick;

    public string[] allMaps;
    public bool changeMapBetweenRound = true;

    // Start is called before the first frame update
    void Start()
    {
        closemenu();
        loadingscreen.SetActive(true);
        loadingtext.text = "Connecting To Network...";
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
#if UNITY_EDITOR
        roomtestbutton.SetActive(true);
#endif
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void closemenu()
    {
        loadingscreen.SetActive(false);
        menubutton.SetActive(false);
        createroomscreen.SetActive(false);
        roomscreen.SetActive(false);
        errorscreen.SetActive(false);
        roombrowserscreen.SetActive(false);
        nameinputscreen.SetActive(false);

    }

    public override void OnConnectedToMaster()
    {
      
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loadingtext.text = "Joining Lobby....";
        //base.OnConnectedToMaster();
    }
    public override void OnJoinedLobby()
    {
        closemenu();
        menubutton.SetActive(true);
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();
        if (!hassetnick)
        {
            closemenu();
            nameinputscreen.SetActive(true);
            if (PlayerPrefs.HasKey("playername"))
            {
                nameinput.text = PlayerPrefs.GetString("playername");
            }
        }
        else
        {
            PhotonNetwork.NickName= PlayerPrefs.GetString("playername");
        }
        //base.OnJoinedLobby();
    }
    public void openroomcreate()
    {

        closemenu();
        createroomscreen.SetActive(true);
    }
    public void createroom()
    {
        if (!string.IsNullOrEmpty(roomnameinput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            
            PhotonNetwork.CreateRoom(roomnameinput.text);
            closemenu();
            loadingtext.text = "CREATING ROOM...";
            loadingscreen.SetActive(true);

        }
    }
    public override void OnJoinedRoom()
    {
        closemenu();
        roomscreen.SetActive(true);

        roomnametext.text = PhotonNetwork.CurrentRoom.Name;
        listallplayers();
        if (PhotonNetwork.IsMasterClient)
        {
            startbutton.SetActive(true);
        }
        else
        {
            startbutton.SetActive(false); 

        }
        //base.OnJoinedRoom();

    }

    //afterquite
    private void listallplayers()
    {
        foreach (TMP_Text player in allplayersnames)
        {
            Destroy(player.gameObject);
        }
        allplayersnames.Clear();
        Player[] players = PhotonNetwork.PlayerList;
        for(int i = 0; i < players.Length; i++)
        {

            TMP_Text newplayerlabel = Instantiate(playernamelabel,playernamelabel.transform.parent);
            newplayerlabel.text = players[i].NickName;
            newplayerlabel.gameObject.SetActive(true);
            allplayersnames.Add(newplayerlabel);
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newplayerlabel = Instantiate(playernamelabel,playernamelabel.transform.parent);
    
        newplayerlabel.text = newPlayer.NickName;
        newplayerlabel.gameObject.SetActive(true);
        allplayersnames.Add(newplayerlabel);
        //base.OnPlayerEnteredRoom(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        listallplayers();
        //base.OnPlayerLeftRoom(otherPlayer);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errortext.text = "FAIL TO CREATE ROOM:" + message;
        closemenu();
        errorscreen.SetActive(true);
        //base.OnCreateRoomFailed(returnCode, message);
    }
    public void closeerrorscreen()
    {
        closemenu();
        menubutton.SetActive(true);
    }
    public void leaveroom()
    {

        PhotonNetwork.LeaveRoom();
        closemenu();
        loadingtext.text = "LEAVING ROOM...";
        loadingscreen.SetActive(true);
    }
    public override void OnLeftRoom()
    {
        closemenu();
        menubutton.SetActive(true);
        //base.OnLeftRoom();
    }
    public void openroombrowser()
    {

        closemenu();
        roombrowserscreen.SetActive(true);
    }
    public void closeroombrowser()
    {

        closemenu();
        menubutton.SetActive(true);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(roombutton rb in allroombuttons)
        {

            Destroy(rb.gameObject);
        }
        allroombuttons.Clear();
        theroombutton.gameObject.SetActive(false);
        for(int i = 0; i < roomList.Count;i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                roombutton newbutton = Instantiate(theroombutton, theroombutton.transform.parent);
                newbutton.setbuttondetail(roomList[i]);
                newbutton.gameObject.SetActive(true);
                allroombuttons.Add(newbutton);

            }
        }
        //base.OnRoomListUpdate(roomList);
    }

    public void joinroom(RoomInfo inputinfo)
    {
        PhotonNetwork.JoinRoom(inputinfo.Name);
        closemenu();
        loadingtext.text = "JOINING ROOM...";
        loadingscreen.SetActive(true);

    }
    public void setnickname()
    {
        if (!string.IsNullOrEmpty(nameinput.text))
        {
            PhotonNetwork.NickName = nameinput.text;
            PlayerPrefs.SetString("playername", nameinput.text);
            closemenu();
            menubutton.SetActive(true);
            hassetnick = true;
          
        }
    }

    
    public void startgame()
    {
        //PhotonNetwork.LoadLevel(leveltoplay);
        PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startbutton.SetActive(true);
        }
        else
        {
            startbutton.SetActive(false);

        }

        //base.OnMasterClientSwitched(newMasterClient);
    }

    public void quickjoin()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;


        PhotonNetwork.CreateRoom("Test",options);
        closemenu();
        loadingtext.text = "CREATE ROOM";
        loadingscreen.SetActive(true);
    }
    public void quitegame()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class matchmanager : MonoBehaviourPunCallbacks,IOnEventCallback
{
    public static matchmanager instance;
    private void Awake()
    {
        instance = this;
    }

    public enum EventCodes: byte
    {
        newplayer,
        listplayer,
        updatestat,
        nextmatch,
        timersyn
      
    }



    public List<playerinfo> allplayer = new List<playerinfo>();

    private int index;

    private List<leaderboardplayer> lboardplayer = new List<leaderboardplayer>();

    public enum gamestate
    {

        waiting,
        playing,
        ending
    }

    public int killsToWin = 3;
    public Transform mapCamPoint;
    public gamestate state = gamestate.waiting;

    public float waitAfterEnding=5f;

    public bool perpetual;

    public float matchlenght=180f;
    private float currentmatchtime;
    private float sendtimer;

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {

            SceneManager.LoadScene(0);
        }
        else
        {
            newplayersend(PhotonNetwork.NickName);
            state = gamestate.playing;
            setuptimer();

            if (!PhotonNetwork.IsMasterClient)
            {
                //uicontroller.instance.timertext.gameObject.SetActive(false);
            }


        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab)&&state!=gamestate.ending)
        {

            if (uicontroller.instance.leaderboard.activeInHierarchy)
            {

                uicontroller.instance.leaderboard.SetActive(false);

            }
            else
            {
                showleaderboard();
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (currentmatchtime > 0 && state == gamestate.playing)
            {

                currentmatchtime -= Time.deltaTime;

                if (currentmatchtime <= 0f)
                {
                    currentmatchtime = 0;
                    state = gamestate.ending;
                    
                        listplayersend();
                        stateCheck();
                    
                }
                UpdateTimerDisplay();

                sendtimer -= Time.deltaTime;
                if (sendtimer >= 0)
                {
                    sendtimer += 1f;

                    timersend();
                }
            }
        }
        //extra
        if (!PhotonNetwork.IsMasterClient)
        {
            if (currentmatchtime > 0 && state == gamestate.playing)
            {

                currentmatchtime -= Time.deltaTime;

                if (currentmatchtime <= 0f)
                {
                    currentmatchtime = 0;
                    state = gamestate.ending;

                    listplayersend();
                    stateCheck();

                }
                UpdateTimerDisplay();

                sendtimer -= Time.deltaTime;
                if (sendtimer >= 0)
                {
                    sendtimer += 1f;

                    timersend();
                }
            }
        }
        //extra
    }

    public void OnEvent(EventData photonevent)
    {

        if (photonevent.Code < 200)
        {
            EventCodes theEvent =(EventCodes)photonevent.Code;
            object[] data = (object[])photonevent.CustomData;
            //Debug.Log("receive event " + theEvent);
            switch (theEvent)
            {

                case EventCodes.newplayer:

                    newplayerreceive(data);

                    break;


                case EventCodes.listplayer:

                    listplayerreceive(data);

                    break;


                case EventCodes.updatestat:

                    updatestatreceive(data);

                    break;
                case EventCodes.nextmatch:

                    nextMatchReceive();

                    break;
                case EventCodes.timersyn:

                    timerreceive(data);

                    break;

            }
        }
    }
    public override void OnEnable()
    {
        //base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        //base.OnDisable();
    }
    public void newplayersend(string username)
    {

        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;


        PhotonNetwork.RaiseEvent((byte)EventCodes.newplayer, package, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true });
            
    }   
    public void newplayerreceive(object[] datareceive)
    {
        playerinfo player = new playerinfo((string)datareceive[0],(int)datareceive[1],(int) datareceive[2],(int) datareceive[3]);
        allplayer.Add(player);

        listplayersend();

    }   
    public void listplayersend()
    {
        object[] package = new object[allplayer.Count+1];
        package[0] = state;

        for(int i = 0; i < allplayer.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allplayer[i].name;
            piece[1] = allplayer[i].actor;
            piece[2] = allplayer[i].kills;
            piece[3] = allplayer[i].deaths;

            package[i+1] = piece;
        }
        PhotonNetwork.RaiseEvent((byte)EventCodes.listplayer, package, new RaiseEventOptions { Receivers = ReceiverGroup.All },
         new SendOptions { Reliability = true });

    }   
    public void listplayerreceive(object[] datareceive)
    {
        allplayer.Clear();

        state = (gamestate)datareceive[0];

        for(int i = 1; i < datareceive.Length; i++)
        {
            object[] piece = (object[])datareceive[i];
            playerinfo player = new playerinfo((string)piece[0], (int)piece[1],(int)piece[2], (int)piece[3]);
            allplayer.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {

                index = i-1;
            }
        }
        stateCheck();

    } 
    public void updatestatsend(int actorsending,int statetoupdate,int amounttochange)
    {
        object[] package = new object[] { actorsending, statetoupdate, amounttochange };


        PhotonNetwork.RaiseEvent((byte)EventCodes.updatestat, package, new RaiseEventOptions { Receivers = ReceiverGroup.All },
       new SendOptions { Reliability = true });
    }   
    public void updatestatreceive(object[] datareceive)
    {

        int actor = (int)datareceive[0];
        int statType = (int)datareceive[1];
        int amount = (int)datareceive[2];

        for(int i = 0; i < allplayer.Count; i++)
        {
            if (allplayer[i].actor == actor)
            {

                switch (statType)
                {

                    case 0: //kils

                        allplayer[i].kills += amount;
                        Debug.Log("player" + allplayer[i].name + " : kills" + allplayer[i].kills);
                        break;

                    case 1:
                        allplayer[i].deaths += amount;
                        Debug.Log("player" + allplayer[i].name + " : death" + allplayer[i].deaths);
                        break;



                }
                if (i == index)
                {

                    updatestatsdisplay();
                }
                if (uicontroller.instance.leaderboard.activeInHierarchy)
                {
                    showleaderboard();
                }

                break;

            }
        }
        ScoreCheck();
    }

    public void updatestatsdisplay()
    {
        if (allplayer.Count > index)
        {
            uicontroller.instance.killstext.text = "KILLS: " + allplayer[index].kills;
            uicontroller.instance.deathstext.text = "DEATHS: " + allplayer[index].deaths;
        }
        else
        {

            uicontroller.instance.killstext.text = "KILLS: 0";
            uicontroller.instance.deathstext.text = "DEATHS: 0";
        }
    }

    void showleaderboard()
    {

        uicontroller.instance.leaderboard.SetActive(true);
        foreach(leaderboardplayer lp in lboardplayer)
        {

            Destroy(lp.gameObject);

        }
        lboardplayer.Clear();

        uicontroller.instance.leaderboardplayerdisplay.gameObject.SetActive(false);

        List<playerinfo> sorted = sortplayer(allplayer);
        foreach(playerinfo player in sorted)
        {

            leaderboardplayer newplayerdisplay = Instantiate(uicontroller.instance.leaderboardplayerdisplay, uicontroller.instance.leaderboardplayerdisplay.transform.parent);

            newplayerdisplay.setdetails(player.name, player.kills, player.deaths);

            newplayerdisplay.gameObject.SetActive(true);

            lboardplayer.Add(newplayerdisplay);
        }
    }

    private List<playerinfo> sortplayer(List<playerinfo> players)
    {
        List<playerinfo> sorted = new List<playerinfo>();

        while (sorted.Count < players.Count)
        {

            int highes = -1;
            playerinfo selectedplayer = players[0];

            foreach(playerinfo player in players)
            {
                if (!sorted.Contains(player))
                {

                    if (player.kills > highes)
                    {
                        selectedplayer = player;
                        highes = player.kills;
                    }
                }
            }
            sorted.Add(selectedplayer);


        }

        return sorted;

    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        SceneManager.LoadScene(0);
    }

    void ScoreCheck()
    {
        bool winnerFound = false;
        foreach(playerinfo player in allplayer)
        {

            if (player.kills >= killsToWin && killsToWin > 0)
            {

                winnerFound = true;
                break;
            }
        }
        if (winnerFound)
        {

            if (PhotonNetwork.IsMasterClient && state != gamestate.ending)
            {
                state = gamestate.ending;
                listplayersend();
            }
        }

    }

    void stateCheck()
    {
        if (state == gamestate.ending)
        {

            endGame();
        }

    }
    void endGame()
    {
        state = gamestate.ending;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();

        }

        uicontroller.instance.EndScreen.SetActive(true);
        showleaderboard();
        //mouselock
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Camera.main.transform.position = mapCamPoint.transform.position;
        Camera.main.transform.rotation = mapCamPoint.transform.rotation;
        StartCoroutine(endCo());

    }

    private IEnumerator endCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);
        if (!perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!launcher.instance.changeMapBetweenRound)
                {
                    nextMatchSend();
                }
                else
                {
                    int newlevel = Random.Range(0, launcher.instance.allMaps.Length);

                    if (launcher.instance.allMaps[newlevel] == SceneManager.GetActiveScene().name)
                    {
                        nextMatchSend();
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(launcher.instance.allMaps[newlevel]);
                    }
                }
            }
        }

    }

    public void nextMatchSend()
    {
        PhotonNetwork.RaiseEvent((byte)EventCodes.nextmatch, null, new RaiseEventOptions { Receivers = ReceiverGroup.All },
         new SendOptions { Reliability = true });

    } 
    public void nextMatchReceive()
    {
        state = gamestate.playing;
        uicontroller.instance.EndScreen.SetActive(false);
        uicontroller.instance.leaderboard.SetActive(false);

        foreach(playerinfo player in allplayer)
        {
            player.kills = 0;
            player.deaths = 0;
            
        }
        updatestatsdisplay();
        playerspawner.instance.spawntheplayer();
        setuptimer();

    }
    public void setuptimer()
    {

        if (matchlenght > 0)
        {
            currentmatchtime = matchlenght;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {
        var timetodisplay = System.TimeSpan.FromSeconds(currentmatchtime);

        uicontroller.instance.timertext.text = timetodisplay.Minutes.ToString("00")+":"+ timetodisplay.Seconds.ToString("00");
    }

    public void timersend()
    {
        object[] package = new object[] { (int)currentmatchtime, state };
        PhotonNetwork.RaiseEvent((byte)EventCodes.timersyn, package, new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true });
    }
    public void timerreceive(object[] datareceived)
    {

        currentmatchtime = (int)datareceived[0];
        state = (gamestate)datareceived[1];
        UpdateTimerDisplay();

        uicontroller.instance.timertext.gameObject.SetActive(true);

    }
}


[System.Serializable]
public class playerinfo
{

    public string name;
    public int actor, kills, deaths;

    public playerinfo(string _name,int _actor,int _kills,int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;

    }
}

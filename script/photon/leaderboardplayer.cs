using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class leaderboardplayer : MonoBehaviour
{
    public TMP_Text playernametext,killstext,deathstext;

    public void setdetails(string name,int kills,int deaths)
    {
        playernametext.text = name;
        killstext.text = kills.ToString();
        deathstext.text = deaths.ToString();
    }
    
}

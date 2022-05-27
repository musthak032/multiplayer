using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
public class roombutton : MonoBehaviour
{
    public TMP_Text buttontext;

    private RoomInfo info;
    
    public void setbuttondetail(RoomInfo inputinfo)
    {
        info = inputinfo;
        buttontext.text = info.Name;
    }
    public void openroom()
    {
        launcher.instance.joinroom(info);
    }
}

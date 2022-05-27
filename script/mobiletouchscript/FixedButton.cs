using UnityEngine;
using UnityEngine.EventSystems;

public class FixedButton : MonoBehaviour,IPointerClickHandler
{
    [HideInInspector]
    public bool Pressed;


    public playercontroller player;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPlayer(playercontroller _player)
    {

        player = _player;
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    Pressed = true;
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    Pressed = false;
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        player.mobilejump();
    }
}
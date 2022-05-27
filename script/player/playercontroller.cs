using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class playercontroller : MonoBehaviourPunCallbacks
{
    public CharacterController charcon;       // interact world and player move
    public GameObject bulletsimpact;
    public Transform viewpoint;               // look direction
    public Transform groundcheckpoint;

    public gun[] allguns;                      //type of gun
    public float mousesensitivity = 1f;         // look direction
    public float movespeed = 5f, runspeed = 8f;  //player movespeed
    public float jumpforce = 7.5f, gravitymod;   //player jump gravity mod
                                                 // public float timebtwshot=.1f;
    public float maxheat = 10f,/*heatpershot=1f,*/coolrate = 4f, overheatcoolrate = 5f;
    public float muzzledisplaytime;

    public bool invertlook;                   // look direction

    public LayerMask groundlayers;
    public GameObject playerhitimpact;

    public int maxhealth = 100;

    private float verticalrotationstore;     // look direction
    private float activemovespeed;           //playermovespeed
    private float shotcounter;
    private float heatcounter;
    private float muzzlecounter;
    private bool isgrounded;
    private bool overheated;

    private Vector2 mouseinput;              // look direction
    private Vector3 movedir, movement;        //playermove

    private int selectedgun;                 //use for select weapon
    private int currenthealth;
    private Camera cam;
    public Animator anim;
    public GameObject playermodel;
    public Transform modelgunpoint;
    public Transform gunholder;




    public Material[] AllSkins;

    public float adzSpeed=5f;
    public Transform adzOutPoint, adzInPoint;
    public AudioSource footstepslow, footstepfast;

    //mobile

    public bool mshoot;
    
    public bool mobileinputon;

    private bool mdownshoot;

    bool adzonmobile;

    // mobile
  public  FixedJoystick joystick;
    public FixedTouchField touchfield;

    public GameObject uijoystick;

    // Start is called before the first frame update
    void Start()
    {

        if (!mobileinputon)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        cam = Camera.main;
        uicontroller.instance.weapontempslider.maxValue = maxheat;
        // switchgun();
        photonView.RPC("setgun", RpcTarget.All, selectedgun);
        currenthealth = maxhealth;

        if (mobileinputon)
        {

        }
        if (photonView.IsMine)
        {
            playermodel.SetActive(false);
            uicontroller.instance.healthslider.maxValue = maxhealth;
            uicontroller.instance.healthslider.value = currenthealth;
          
            if (mobileinputon)
            {
                uijoystick.SetActive(true);
            }
         
        }
        else
        {

            gunholder.parent = modelgunpoint;
            gunholder.localPosition = Vector3.zero;
            gunholder.localRotation = Quaternion.identity;


            //gameObject.GetComponentInChildren<mobileshoot>().gameObject.SetActive(false);
            uijoystick.SetActive(false);


        }
        uicontroller.instance.gameObject.SetActive(false);
        uicontroller.instance.gameObject.SetActive(true);

        playermodel.GetComponent<Renderer>().material = AllSkins[photonView.Owner.ActorNumber % AllSkins.Length];

        //Transform newtrans = spawnmanager.instance.getspawnpoint();
        //transform.position = newtrans.position;
        //transform.rotation = newtrans.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            playerrotation();
            playermove();
            //for pc
            gunshoot();
            if (mobileinputon)
            {
                handleoverheater();
                mobiledownshoot();
                mobileshootdown();
                mobileadz();
            }

            //mobilegunshoot();



        }



    }
    [PunRPC]
    public void dealdamage(string damager,int damageamount,int actor)
    {
        takedamage(damager,damageamount,actor);
      
    }

    public void takedamage(string damager,int damageamount,int actor)
    {
        if (photonView.IsMine)
        {

            //Debug.Log(photonView.Owner.NickName + "has been hit by" + damager);

            currenthealth -= damageamount;
            if (currenthealth <= 0)
            {
                currenthealth = 0;
                playerspawner.instance.die(damager);
                matchmanager.instance.updatestatsend(actor, 0, 1);
            }
            uicontroller.instance.healthslider.value = currenthealth;


        }
    }
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (matchmanager.instance.state == matchmanager.gamestate.playing)
            {
                cam.transform.position = viewpoint.position;
                cam.transform.rotation = viewpoint.rotation;
            }
            else
            {
                cam.transform.position = matchmanager.instance.mapCamPoint.position;
                cam.transform.rotation = matchmanager.instance.mapCamPoint.rotation;
            }
        }
    }
    // player look direction
    private void playerrotation()
    {
        //input for rotation look
        if (!mobileinputon)
        {
            mouseinput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mousesensitivity;

        }
        else
        {
            mousesensitivity = .2f;

            mouseinput = new Vector2(touchfield.TouchDist.x, touchfield.TouchDist.y) * mousesensitivity;

        }
        //new touch screen

        //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        //{
        //    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        //    {
        //        return;
        //    }
        //    mouseinput = new Vector2(Input.GetTouch(0).deltaPosition.x, Input.GetTouch(0).deltaPosition.y) * mousesensitivity;
        //}

        //horizontal rotation with player body
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseinput.x, transform.rotation.eulerAngles.z);
        verticalrotationstore += mouseinput.y;

        //clamp vertical rotation
        verticalrotationstore = Mathf.Clamp(verticalrotationstore, -60f, 60f);
        if (invertlook)
        {
            viewpoint.rotation = Quaternion.Euler(verticalrotationstore, viewpoint.rotation.eulerAngles.y, viewpoint.rotation.eulerAngles.z);

        }
        else
        {
            viewpoint.rotation = Quaternion.Euler(-verticalrotationstore, viewpoint.rotation.eulerAngles.y, viewpoint.rotation.eulerAngles.z);
        }

    }
    //  player move
    private void playermove()
    {

        // playermovement get input move button
        if (!mobileinputon)
        {
            movedir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        }
        else
        {
            movedir = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);
        }


        //input button speed

        if (Input.GetKey(KeyCode.LeftShift))
        {
            activemovespeed = runspeed;
            if (!footstepfast.isPlaying && movedir != Vector3.zero)
            {
                footstepfast.Play();
                footstepslow.Stop();
            }
        }
        else
        {
            activemovespeed = movespeed;
            if (!footstepslow.isPlaying && movedir != Vector3.zero)
            {
                footstepfast.Stop();
                footstepslow.Play();
            }
        }
        if (movedir == Vector3.zero||!isgrounded)
        {
            footstepfast.Stop();
            footstepslow.Stop();
        }
     
        float yvel = movement.y;   //store the gravity in yvel 
        //move player in certain direction 
        movement = ((transform.forward * movedir.z) + (transform.right * movedir.x)).normalized*activemovespeed;
        
  
        if (!charcon.isGrounded)//add gravity
        {
            movement.y = yvel;
        }
        // use raycaste

        isgrounded = Physics.Raycast(groundcheckpoint.position, Vector3.down, .25f, groundlayers);
        //input button for jump

        if (!mobileinputon)
        {
            if ((Input.GetButtonDown("Jump")) && isgrounded)
            {
                movement.y = jumpforce;

            }
        }
       
        movement.y += Physics.gravity.y * Time.deltaTime*gravitymod; //add gravity

        charcon.Move( movement * Time.deltaTime);
       
        anim.SetBool("grounded", isgrounded);
        anim.SetFloat("speed", movedir.magnitude);
        //lock curse
        if (!mobileinputon)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {

                if (Input.GetMouseButtonDown(0)&&!uicontroller.instance.optionscreen.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

    }
    private void gunshoot()
    {

        if (!mobileinputon)
        {
            if (allguns[selectedgun].muzzleflash.activeInHierarchy)
            {
                muzzlecounter -= Time.deltaTime;
                if (muzzlecounter <= 0)
                {
                    allguns[selectedgun].muzzleflash.SetActive(false);
                }
            }
            if (!overheated)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    shoot();
                }
                if (Input.GetMouseButton(0) && allguns[selectedgun].isautomatic)
                {
                    shotcounter -= Time.deltaTime;
                    if (shotcounter <= 0)
                    {
                        shoot();
                    }
                }
                heatcounter -= coolrate * Time.deltaTime;
            }
            else
            {
                heatcounter -= overheatcoolrate * Time.deltaTime;
                if (heatcounter <= 0)
                {

                    heatcounter = 0;
                    overheated = false;
                    uicontroller.instance.overheatedmessage.gameObject.SetActive(false);
                }
            }
            if (heatcounter < 0)
            {
                heatcounter = 0;
            }
            uicontroller.instance.weapontempslider.value = heatcounter;

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                selectedgun++;
                if (selectedgun >= allguns.Length)
                {
                    selectedgun = 0;
                }
                //switchgun();
                photonView.RPC("setgun", RpcTarget.All, selectedgun);

            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                selectedgun--;
                if (selectedgun <= 0)
                {
                    selectedgun = allguns.Length - 1;
                }
                //switchgun();
                photonView.RPC("setgun", RpcTarget.All, selectedgun);

            }

            for (int i = 0; i < allguns.Length; i++)
            {

                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedgun = i;
                    //switchgun();
                    photonView.RPC("setgun", RpcTarget.All, selectedgun);

                }
            }

            if (Input.GetMouseButton(1))
            {

                cam.fieldOfView =Mathf.Lerp(cam.fieldOfView, allguns[selectedgun].adzZoom,adzSpeed*Time.deltaTime);
                gunholder.position = Vector3.Lerp(gunholder.position, adzInPoint.position, adzSpeed * Time.deltaTime);
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adzSpeed * Time.deltaTime);
                gunholder.position = Vector3.Lerp(gunholder.position, adzOutPoint.position, adzSpeed * Time.deltaTime);

            }

            //anim.SetBool("grounded", isgrounded);
            //anim.SetFloat("speed", movedir.magnitude);
        }
    }
    private void shoot()
    {

        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        ray.origin = cam.transform.position;
        if(Physics.Raycast(ray,out RaycastHit hit))
        {

            //Debug.Log("we hit"+hit.collider.gameObject.name);
            if (hit.collider.gameObject.tag == "Player")
            {
                Debug.Log("hit" + hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(playerhitimpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("dealdamage",RpcTarget.All,photonView.Owner.NickName,allguns[selectedgun].shotdamage,PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject bulletimpactobject = Instantiate(bulletsimpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletimpactobject, 10f);
            }
        
        }
        shotcounter = allguns[selectedgun].timebtwshot;
        heatcounter += allguns[selectedgun].heatpershot;
        if (heatcounter >= maxheat)
        {
            heatcounter = maxheat;
            overheated = true;
            uicontroller.instance.overheatedmessage.gameObject.SetActive(true);

        }
       
        allguns[selectedgun].muzzleflash.SetActive(true);
        muzzlecounter = muzzledisplaytime;
        allguns[selectedgun].shotSound.Stop();
        allguns[selectedgun].shotSound.Play();

        
    }
   
    void switchgun()
    {
        foreach(gun Gun in allguns)
        {

            Gun.gameObject.SetActive(false);
        }
        allguns[selectedgun].gameObject.SetActive(true);
        allguns[selectedgun].muzzleflash.SetActive(false);

    }

    [PunRPC]
    public void setgun(int guntoswitchto)
    {
        if (guntoswitchto < allguns.Length)
        {
            selectedgun = guntoswitchto;
            switchgun();
        }
    }

    //mobile

    public void mobilejump()
    {
        if (photonView.IsMine)
        {
            if (mobileinputon)
            {
                if (isgrounded)
                {

                    movement.y = jumpforce;
                    charcon.Move(movement * Time.deltaTime);
                }
            }
        }
    }

    public void mobileshootsingleshoot()
    {
        if (mobileinputon&& !allguns[selectedgun].isautomatic)
        {
            if (allguns[selectedgun].muzzleflash.activeInHierarchy)
            {
                muzzlecounter -= Time.deltaTime;
                if (muzzlecounter <= 0)
                {
                    allguns[selectedgun].muzzleflash.SetActive(false);
                }
            }
            if (!overheated)
            {
                if (allguns[selectedgun].isautomatic)
                {
                    
                }

                shoot();
                mshoot = true;
                Invoke("offmshoot", .05f);

                heatcounter -= coolrate * Time.deltaTime;
            }
        }

        }

    void offmshoot()
    {
        mshoot = false;
    }

    void mobiledownshoot()
    {

        if(mdownshoot)
        {
            heatcounter -= overheatcoolrate * Time.deltaTime;
            if (heatcounter <= 0)
            {

                heatcounter = 0;
                overheated = false;
                uicontroller.instance.overheatedmessage.gameObject.SetActive(false);
            }
        }
        if (heatcounter < 0)
        {
            heatcounter = 0;
        }
        uicontroller.instance.weapontempslider.value = heatcounter;
    }

    public void mobileshootcontinuedown()
    {
        mdownshoot = true;

    }
    public void mobileshootcontinueup()
    {
        mdownshoot = false;
        return;
    }
    void mobileshootdown()
    {
        if (mdownshoot)
        {
            if (!overheated)
            {




                if (allguns[selectedgun].isautomatic)
                {
                    shotcounter -= Time.deltaTime;
                    if (shotcounter <= 0)
                    {
                        shoot();
                        Debug.Log("shootcontinue");

                        mshoot = true;
                        Invoke("offmshoot", .05f);
                    }
                }

                heatcounter -= coolrate * Time.deltaTime;
            }
        }
    }
   
  

    //public void mobilegunshoot()
    //{

    //    if (mobileinputon)
    //    {
    //        if (allguns[selectedgun].muzzleflash.activeInHierarchy)
    //        {
               
    //            muzzlecounter -= Time.deltaTime;
    //            if (muzzlecounter <= 0)
    //            {
    //                allguns[selectedgun].muzzleflash.SetActive(false);
    //            }
    //        }
    //        if (!overheated)
    //        {
                
                
                  
                
    //            if (allguns[selectedgun].isautomatic)
    //            {
    //                shotcounter -= Time.deltaTime;
    //                if (shotcounter <= 0)
    //                {
    //                    shoot();
    //                    Debug.Log("shootcontinue");
    //                    mdownshoot = true;
    //                    mshoot = true;
    //                    Invoke("offmshoot", .05f);
    //                }
    //            }
    //            heatcounter -= coolrate * Time.deltaTime;
    //        }
    //        //else
    //        //{
    //        //    heatcounter -= overheatcoolrate * Time.deltaTime;
    //        //    if (heatcounter <= 0)
    //        //    {

    //        //        heatcounter = 0;
    //        //        overheated = false;
    //        //        uicontroller.instance.overheatedmessage.gameObject.SetActive(false);
    //        //    }
    //        //}
    //        //if (heatcounter < 0)
    //        //{
    //        //    heatcounter = 0;
    //        //}
    //        //uicontroller.instance.weapontempslider.value = heatcounter;

    //        //if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
    //        //{
    //        //    selectedgun++;
    //        //    if (selectedgun >= allguns.Length)
    //        //    {
    //        //        selectedgun = 0;
    //        //    }
    //        //    //switchgun();
    //        //    photonView.RPC("setgun", RpcTarget.All, selectedgun);

    //        //}
    //        //else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
    //        //{
    //        //    selectedgun--;
    //        //    if (selectedgun <= 0)
    //        //    {
    //        //        selectedgun = allguns.Length - 1;
    //        //    }
    //        //    //switchgun();
    //        //    photonView.RPC("setgun", RpcTarget.All, selectedgun);

    //        //}

    //        //for (int i = 0; i < allguns.Length; i++)
    //        //{

    //        //    if (Input.GetKeyDown((i + 1).ToString()))
    //        //    {
    //        //        selectedgun = i;
    //        //        //switchgun();
    //        //        photonView.RPC("setgun", RpcTarget.All, selectedgun);

    //        //    }
    //        //}

    //    }
    //} 

   
    void handleoverheater()
    {
        if (mobileinputon&&!mshoot)
        {

            if (allguns[selectedgun].muzzleflash.activeInHierarchy)
            {
                muzzlecounter -= Time.deltaTime;
                if (muzzlecounter <= 0)
                {
                    if (!mshoot)
                    {
                        allguns[selectedgun].muzzleflash.SetActive(false);
                    }
                }
            }
            if (!overheated)
            {

                {
                    //shoot();
                }
                if (allguns[selectedgun].isautomatic)
                {
                    shotcounter -= Time.deltaTime;
                    if (shotcounter <= 0)
                    {
                        //shoot();
                    }
                }
                heatcounter -= coolrate * Time.deltaTime;
            }
            else
            {
                heatcounter -= overheatcoolrate * Time.deltaTime;
                if (heatcounter <= 0)
                {

                    heatcounter = 0;
                    overheated = false;
                    uicontroller.instance.overheatedmessage.gameObject.SetActive(false);
                }
            }
            if (heatcounter < 0)
            {
                heatcounter = 0;
            }
            uicontroller.instance.weapontempslider.value = heatcounter;
        }
    }

    public void showhideoption()
    {
        //if (photonView.IsMine)
        {
            if (mobileinputon)
            {
                if (!uicontroller.instance.optionscreen.activeInHierarchy)
                {
                    uicontroller.instance.optionscreen.SetActive(true);
                }
                else
                {
                    uicontroller.instance.optionscreen.SetActive(false);
                }
            }
        }
    }

    public void mobileadz()
    {
        if (mobileinputon)
        {

            if (adzonmobile)
            {

                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, allguns[selectedgun].adzZoom, adzSpeed * Time.deltaTime);
                gunholder.position = Vector3.Lerp(gunholder.position, adzInPoint.position, adzSpeed * Time.deltaTime);
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adzSpeed * Time.deltaTime);
                gunholder.position = Vector3.Lerp(gunholder.position, adzOutPoint.position, adzSpeed * Time.deltaTime);

            }
        }
    }
    public void adzbutton()
    {
        if (!adzonmobile)
        {
            adzonmobile = true;
        }
        else
        {
            adzonmobile = false;
        }
    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isFireButtonPressed = false;
    bool isGrappleButtonPressed = false;
    bool isDashButtonPressed = false;
    bool isGrenadeFireButtonPressed = false;

    //Other components
    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;

    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!characterMovementHandler.Object.HasInputAuthority)
            return;

        //View input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");
        //Dash input
        if (Input.GetButtonDown("Dash"))
            isDashButtonPressed = true;
        
        //Jump
        if (Input.GetButtonDown("Jump"))
            isJumpButtonPressed = true;

        //Fire
        if (Input.GetButtonDown("Fire1"))
            isFireButtonPressed = true;
        
        //Grapple
        if (Input.GetButtonDown("Grapple"))
            isGrappleButtonPressed = true;
        
        //Throw grenade
        if (Input.GetKeyDown(KeyCode.G))
            isGrenadeFireButtonPressed = true;


        //Set view
        localCameraHandler.SetViewInputVector(viewInputVector);

    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //Aim data
        networkInputData.aimForwardVector = localCameraHandler.transform.forward;

        //Move data
        networkInputData.movementInput = moveInputVector;

        //Dash data
        networkInputData.isDashPressed = isDashButtonPressed;
        
        //Jump data
        networkInputData.isJumpPressed = isJumpButtonPressed;

        //Fire data
        networkInputData.isFireButtonPressed = isFireButtonPressed;
        
        //Grapple data
        networkInputData.isGrappleButtonPressed = isGrappleButtonPressed;

        //Grenade fire data
        networkInputData.isGrenadeFireButtonPressed = isGrenadeFireButtonPressed;
        
        //Reset variables now that we have read their states
        isDashButtonPressed = false;
        isJumpButtonPressed = false;
        isFireButtonPressed = false;
        isGrappleButtonPressed = false;
        isGrenadeFireButtonPressed = false;

        return networkInputData;
    }
}

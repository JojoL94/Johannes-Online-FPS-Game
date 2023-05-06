using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    public bool isGrapple { get; set; }
    public Transform aimPoint;
    public LayerMask collisionLayers;
    public Transform ropeStartPoint;
    [Header("Animation")]
    public Animator characterAnimator;
    bool isRespawnRequested = false;

    //Other components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    HPHandler hpHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;
    
    float walkSpeed = 0;
    private Vector3 tmpGrappleDirection;
    private Vector3 grappleDirection;
    float lastTimeFired = 0;
    private LineRenderer ropeRenderer;
    private float ropeWidth = 0.1f;
    Vector3 grapplePoint;
    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        hpHandler = GetComponent<HPHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();

    }

    // Start is called before the first frame update
    void Start()
    {
        ropeRenderer = GetComponent<LineRenderer>();
        ropeRenderer.startWidth = ropeWidth;
        ropeRenderer.endWidth = ropeWidth;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (isRespawnRequested)
            {
                Respawn();
                return;
            }

            //Don't update the clients position when they are dead
            if (hpHandler.isDead)
                return;
        }

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            //Rotate the transform according to the client aim vector
            transform.forward = networkInputData.aimForwardVector;

            //Cancel out rotation on X axis as we don't want our character to tilt
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;

            
            //Move and Grapple
            if (networkInputData.isGrappleButtonPressed)
            {
                if (!isGrapple)
                {
                    FireGrappleGun(networkInputData.aimForwardVector);
                    tmpGrappleDirection = grappleDirection;
                    tmpGrappleDirection.Normalize();
                    StartCoroutine(StopGrapple());
                }
            }
            if (isGrapple)
            {
                ropeRenderer.SetPosition(0, ropeStartPoint.position);
                ropeRenderer.SetPosition(1, grapplePoint);
                networkCharacterControllerPrototypeCustom.GrapplePull(tmpGrappleDirection);
            }
            else
            {
                Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
                moveDirection.Normalize();
                networkCharacterControllerPrototypeCustom.Move(moveDirection);
            }
            
            //Dash
            if (networkInputData.isDashPressed )
            { 
                networkCharacterControllerPrototypeCustom.Dash();  
            }
            
            //Jump
            if(networkInputData.isJumpPressed)
                networkCharacterControllerPrototypeCustom.Jump();

            
            //Animation walk
            Vector2 walkVector = new Vector2(networkCharacterControllerPrototypeCustom.Velocity.x, networkCharacterControllerPrototypeCustom.Velocity.z);
            walkVector.Normalize();

            walkSpeed = Mathf.Lerp(walkSpeed, Mathf.Clamp01(walkVector.magnitude), Runner.DeltaTime * 5);

            characterAnimator.SetFloat("walkSpeed", walkSpeed);
            //Check if we've fallen off the world.
            CheckFallRespawn();
        }

    }

    void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");

                networkInGameMessages.SendInGameRPCMessage(networkPlayer.nickName.ToString(), "fell off the world");

                Respawn();
            }

        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
        networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());

        hpHandler.OnRespawned();

        isRespawnRequested = false;
    }

    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }
    

    void FireGrappleGun(Vector3 aimForwardVector)
    {
        //Limit fire rate
        if (Time.time - lastTimeFired < 0.3f)
            return;
        ropeRenderer.enabled = true;
        float hitDistance = 30;
        bool isHitGrapple = false;


        if (Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, hitDistance, Object.InputAuthority, out var hitinfo, collisionLayers, HitOptions.IncludePhysX))
        {
            grapplePoint = hitinfo.Point;
            Debug.Log("Grapple Point at: " + grapplePoint);
            grappleDirection = (grapplePoint - transform.position).normalized; 
            isGrapple = true;
            isHitGrapple = true;
        }
        if (hitinfo.Distance > 0)
            hitDistance = hitinfo.Distance;
        //Debug
        if (isHitGrapple)
            Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
        else Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.green, 1);
        
        lastTimeFired = Time.time;
    }
    
    IEnumerator StopGrapple()
    {
        yield return new WaitForSeconds(0.7f);
        ropeRenderer.enabled = false;
        isGrapple = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GrapplingHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnGrappleChanged))]
    public bool isGrapple { get; set; }

    public ParticleSystem fireParticleSystem;
    public Transform aimPoint;
    public LayerMask collisionLayers;

    float lastTimeFired = 0;

    //Other components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;

    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.isGrappleButtonPressed)
                Fire(networkInputData.aimForwardVector);
        }
    }

    void Fire(Vector3 aimForwardVector)
    {
        //Limit fire rate
        if (Time.time - lastTimeFired < 0.3f)
            return;

        StartCoroutine(FireEffectCO());
/*
        Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitinfo, collisionLayers, HitOptions.IgnoreInputAuthority);

        float hitDistance = 10;
        bool isHitGrapple = false;

        if (hitinfo.Distance > 0)
            hitDistance = hitinfo.Distance;

        if (hitinfo.GameObject.GetComponent<MeshCollider>() != null )
        {
            isHitGrapple = true;
            //Hier rein ballern was pasiert wenn ich eine Hitbox erwische
        }
*/
        float hitDistance = 10;
        bool isHitGrapple = false;
        Vector3 grapplePoint;

        if (Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, hitDistance, Object.InputAuthority, out var hitinfo, collisionLayers, HitOptions.IgnoreInputAuthority))
        {
            grapplePoint = hitinfo.Point;
            Debug.Log("Grapple Point at: " + grapplePoint);
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

    IEnumerator FireEffectCO()
    {
        isGrapple = true;

        fireParticleSystem.Play();

        yield return new WaitForSeconds(0.09f);

        isGrapple = false;
    }


    static void OnGrappleChanged(Changed<GrapplingHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFireChanged value {changed.Behaviour.isFiring}");

        bool isFiringCurrent = changed.Behaviour.isGrapple;

        //Load the old value
        changed.LoadOld();

        bool isFiringOld = changed.Behaviour.isGrapple;

        if (isFiringCurrent && !isFiringOld)
            changed.Behaviour.OnFireRemote();

    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
            fireParticleSystem.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector3 aimForwardVector;
    public NetworkBool isJumpPressed;
    public NetworkBool isDashPressed;
    public NetworkBool isFireButtonPressed;
    public NetworkBool isGrappleButtonPressed;
    public NetworkBool isGrenadeFireButtonPressed;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> m_Players = new Dictionary<ushort, Player>();
    public static float RunSpeed = 6;
    public static float WalkSpeed = 4;

    public Vector2 moveInput;
    [SerializeField] private Transform cameraProxy;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void Move()
    {
        

        Vector3 moveDir = new Vector3(moveInput.x, 0.0f, moveInput.y);

        moveDir = Vector3.ClampMagnitude(moveDir, 1);

        if (characterController == null)
        {
            Debug.Log("Character controller is null");
            characterController = GetComponent<CharacterController>();
        }
        else
        {
            characterController.Move(transform.TransformDirection(NetworkManager.Instance.tickInterval * WalkSpeed * moveDir));
        }
    }

    public static void MoveAll()
    {
        foreach (KeyValuePair<ushort, Player> p in m_Players)
        {
            p.Value.Move();
        }
    }

    [MessageHandler((ushort)ClientToServerId.moveInput)]
    private static void HandleMoveInput(ushort id, Message message)
    {
        m_Players[id].moveInput = Vector2.ClampMagnitude(message.GetVector2(), 1);
    }
}

public struct PlayerInput
{
    public Vector2 move;
    public Vector3 camera;
}

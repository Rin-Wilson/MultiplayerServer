using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> List { get; private set; } = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }

    [SerializeField] private CharacterController controller;
    [SerializeField] private float gravity;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpSpeed;
    private bool grounded;

    private bool[] inputs;

    private float _verticalVelocity;

    private void OnValidate()
    {
        if (controller == null) 
            controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        inputs = new bool[6];
        gravity = -0.01f;
        walkSpeed *= Time.fixedDeltaTime;
        runSpeed *= Time.fixedDeltaTime;
        jumpSpeed = 0.2f;
    }

    private void FixedUpdate()
    {
        Vector2 inputDirection = Vector2.zero;
        if (inputs[0]) inputDirection.y += 1;
        if (inputs[1]) inputDirection.y -= 1;

        if (inputs[2]) inputDirection.x -= 1;
        if (inputs[3]) inputDirection.x += 1;

        Move(inputDirection);
    }

    private void Move(Vector2 inputDirection)
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        moveDirection *= inputs[5] ? runSpeed : walkSpeed;

        if (Grounded())
        {
            _verticalVelocity = 0f;
            if (inputs[4])
                _verticalVelocity = jumpSpeed;
        }
        else
            _verticalVelocity += gravity;

        moveDirection.y = _verticalVelocity;
        controller.Move(moveDirection);
        if (transform.position.y < -10)
        {
            transform.position = new Vector3(0f, 1f, 0f);
        }
        SendMovement();
    }

    private bool Grounded()
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out _, 0.0001f, layerMask))
        {
            return true;
        }
        return false;
    }

    public void SetDirection(Vector3 forward)
    {
        forward.y = 0f;
        transform.forward = forward;
    }

    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.PlayerMovement);
        message.AddUShort(Id);
        message.AddVector3(transform.position);
        message.AddVector3(transform.forward);
        NetworkManager.Singleton.server.SendToAll(message);
    }

    public static void Spawn(ushort id, string username)
    {
        Player player = Instantiate(NetworkManager.Singleton.PlayerPrefab, new Vector3(), Quaternion.identity).GetComponent<Player>();
        player.name = username;
        player.Id = id;
        player.Username = username;
        player.SendSpawn();
        List.Add(player.Id, player);
    }

    public void SendSpawn(ushort toClient)
    {
        NetworkManager.Singleton.server.Send(GetSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnPlayer)), toClient);
    }
    
    public void SendSpawn()
    {
        NetworkManager.Singleton.server.SendToAll(GetSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnPlayer)));
    }

    private Message GetSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    private void OnDestroy()
    {
        List.Remove(Id);
    }

    [MessageHandler((ushort)ClientToServerId.PlayerInput)]
    private static void PlayerInput(ushort fromClientId, Message message)
    {
        Player player = List[fromClientId];
        message.GetBools(6, player.inputs);
        player.SetDirection(message.GetVector3());
    }

    [MessageHandler((ushort)ClientToServerId.PlayerName)]
    private static void PlayerName(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }
}

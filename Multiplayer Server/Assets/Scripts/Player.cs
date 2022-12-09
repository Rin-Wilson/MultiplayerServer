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

    private bool[] inputs;

    private float _verticalVelocity;

    private void OnValidate()
    {
        if (controller == null) 
            controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        gravity *= Time.fixedDeltaTime;
        walkSpeed *= Time.fixedDeltaTime;
        runSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public static void Spawn(ushort id, string username)
    {
        Player player = Instantiate(NetworkManager.Singleton.PlayerPrefab, new Vector3(), Quaternion.identity).GetComponent<Player>();
        player.name = username;
        player.Id = id;

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

    private Message GetSpawnData(Message m)
    {
        m.AddUShort(Id);
        m.AddString(Username);
        return m;
    }

    [MessageHandler((ushort)ClientToServerId.PlayerInput)]
    private static void PlayerInput(ushort fromClientId, Message message)
    {
        Player player = List[fromClientId];
        message.GetBools(6, player.inputs);

    }
}

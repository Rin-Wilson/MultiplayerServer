using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using Riptide;

public enum ServerToClientId : ushort
{
    SpawnPlayer = 1,
    PlayerMovement
}

public enum ClientToServerId : ushort
{
    PlayerName = 1,
    PlayerInput
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Destroy(value);
            }
        }
    }

    [SerializeField] private ushort _port;
    [SerializeField] private ushort _maxClients;
    [SerializeField] private GameObject playerPrefab;
    public GameObject PlayerPrefab => playerPrefab;

    public Server server { get; private set; }

    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        server = new Server();
        server.Start(_port, _maxClients);
    }

    private void FixedUpdate()
    {
        server.Update();
    }

    private void NewPlayerConnected(object sender, ServerConnectedEventArgs e)
    {
        foreach (Player player in Player.List.Values)
        {
            if (player.Id != e.Client.Id)
            {
                player.SendSpawn(e.Client.Id);
            }
        }
    }
}
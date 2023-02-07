using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;
using System;

//IDs for messages sent by the server
public enum ServerToClientId : ushort
{

}

//IDs for messages sent by clients
public enum ClientToServerId : ushort
{
    moveInput = 1
}

public class NetworkManager : MonoBehaviour
{
    //singleton
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get => instance;
        private set
        {
            if (instance == null)
                instance = value;
            else if (instance != value)
            {
                Destroy(value);
            }
        }
    }

    [SerializeField] private ushort port = 7777;
    [SerializeField] private ushort maxClients = 10;
    [SerializeField] private ushort tickRate; //ticks per second
    public uint currentTick = 0; //how many ticks since the server started
    public float tickInterval { get; private set; }
    private float tickTimer = 0.0f; //time since last tick

    public Server Server { get; private set; }

    [SerializeField] private Player playerPrefab;
    public Player PlayerPrefab => playerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        //time between each tick in seconds is 1 / tickrate (ticks per second)
        tickInterval = 1f / tickRate;

        Server = new Server();
        Server.ClientConnected += NewPlayerConnected;
        Server.ClientDisconnected += PlayerLeft;

        Server.Start(port, maxClients);
    }

    void Update()
    {
        //incrament the time elapsed since last tick by time between frames
        tickTimer += Time.deltaTime;

        //Envoke Tick function once for each tick that should have been performed in the time since the last tick
        while (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;

            Tick();
            currentTick++;
        }
    }

    private void Tick()
    {
        Server.Update();
        Player.MoveAll();
    }

    private void NewPlayerConnected(object sender, ServerConnectedEventArgs e)
    {
        //spawn player prefab
        Player newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));

        //add player to the player dictionary
        Player.m_Players.Add(e.Client.Id, newPlayer);

        Debug.Log($"client with id {e.Client.Id} connected");
    }

    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {

    }

    private void OnApplicationQuit()
    {
        Server.Stop();

        Server.ClientConnected -= NewPlayerConnected;
        Server.ClientDisconnected -= PlayerLeft;
    }
}

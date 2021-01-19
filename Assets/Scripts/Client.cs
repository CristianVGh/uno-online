using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string clientName;
    public bool isHost;
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    private List<GameClient> players = new List<GameClient>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
        {
            return false;
        }

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error" + e.Message);
        }

        return socketReady;
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    //send message
    public void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    //read messages
    private void OnIncomingData(string data)
    {
        Debug.Log("Client: " + data);
        string[] aData = data.Split('|');

        switch (aData[0])
        {
            case "SWHO":
                for (int i = 1; i < aData.Length - 1; i++)
                {
                    UserConnected(aData[i], false);
                }
                Send("CWHO|" + clientName + "|" + ((isHost) ? 1 : 0).ToString());
                break;

            case "SCNN":
                UserConnected(aData[1], false);
                break;

            case "SGEN":
                GameObject player1_location = GameObject.Find(aData[15]);
                GameObject player2_location = GameObject.Find(aData[16]);
                GameLogic.Instance.initiateDiscardPile(int.Parse(aData[17]));
                for (int i = 1; i < 15; i = i + 2)
                {
                    GameLogic.Instance.drawCard(player1_location, int.Parse(aData[i]));
                    GameLogic.Instance.drawCard(player2_location, int.Parse(aData[i + 1]));
                }
                GameLogic.Instance.SetNames(aData[18], aData[19]);
                break;

            case "SDRAW":
                GameObject cardLocation = GameObject.Find(aData[1]);
                int randomNumber = int.Parse(aData[2]);
                GameLogic.Instance.drawCard(cardLocation, randomNumber);
                GameLogic.Instance.endTurn();
                break;

            case "SCLICK":
                GameObject card = GameObject.Find(aData[1]);
                GameLogic.Instance.placeCard(card);
                GameLogic.Instance.endTurn();
                break;

            case "SWILD":
                GameObject wild_card = GameObject.Find(aData[1]);
                GameLogic.Instance.placeCard(wild_card);
                GameLogic.Instance.wildCard(aData[2], aData[3]);
                GameLogic.Instance.endTurn();
                break;

            case "SWILD4":
                GameObject player_location = GameObject.Find(aData[1]);
                for (int i = 2; i < 6; i++)
                {
                    GameLogic.Instance.drawCard(player_location, int.Parse(aData[i]));
                }
                GameObject wild_card4 = GameObject.Find(aData[6]);
                GameLogic.Instance.placeCard(wild_card4);
                GameLogic.Instance.wildCard(aData[7], aData[8]);
                break;

            case "SD2":
                GameObject draw2 = GameObject.Find(aData[1]);
                GameObject player_to_draw = GameObject.Find(aData[2]);
                GameLogic.Instance.placeCard(draw2);
                GameLogic.Instance.drawCard(player_to_draw, int.Parse(aData[3]));
                GameLogic.Instance.drawCard(player_to_draw, int.Parse(aData[4]));
                break;

            case "SSKIP":
                GameObject skip = GameObject.Find(aData[1]);
                GameLogic.Instance.placeCard(skip);
                break;
        }
    }

    private void UserConnected(string name, bool host)
    {
        GameClient c = new GameClient();
        c.name = name;

        players.Add(c);

        if (players.Count == 2)
            GameManager.Instance.StartGame();
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    private void OnDisable()
    {
        CloseSocket();
    }
    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
}

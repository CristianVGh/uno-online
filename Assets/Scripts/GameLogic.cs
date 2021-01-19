
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading;
using System;

public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance{ set; get; }
    public Button cardPrefab;
    public GameObject player1Hand;
    public GameObject player2Hand;
    public GameObject discardPlace;
    public GameObject colorPickPanel;
    public GameObject colorPickText;
    public GameObject player1Indicator;
    public GameObject player2Indicator;
    public GameObject gameOverScreen;
    public Text p1Name;
    public Text p2Name;
    public Text winText;

    private  ArrayList fullDeck;
    private  ArrayList discardPile;

    private int turn;
    private GameObject wild_card;

    private Client client;

    private void Start(){
        Instance = this;
        client = FindObjectOfType<Client>();
        wild_card = null;
        player1Indicator.SetActive(true);
        player2Indicator.SetActive(false);
        turn = 0;
        gameStart();

    }

    public void cardClicked(GameObject card)
    {
        string msg;
        if ((client.isHost && turn == 0 && card.tag.Equals("Player1")) || (!client.isHost && turn == 1 && card.tag.Equals("Player2")))
        {
            if (card.name.Contains("wild"))
            {
                wild_card = card;
                colorPickPanel.SetActive(true);
            }
            else
                if (card.name.Contains("draw"))
                {
                    if (checkCard(card))
                    {
                        msg = "CD2|" + card.name;
                        if (turn == 0)
                            msg += "|" + player2Hand.name;
                        else
                            msg += "|" + player1Hand.name;
                        int new_deck_size = fullDeck.Count;
                        for (int i = 0; i < 2; i++)
                        {
                            int randomNumber = UnityEngine.Random.Range(0, new_deck_size - 1);
                            msg += "|" + randomNumber;
                            new_deck_size--;
                        }
                        client.Send(msg);
                    }
                }
                else
                    if (card.name.Contains("skip"))
                    {
                        if (checkCard(card))
                        {
                            msg = "CSKIP|" + card.name;
                            client.Send(msg);
                        }
                                            
                    }
                    else
                        if (checkCard(card))
                        {
                            msg = "CCLICK|" + card.name;
                            client.Send(msg);
                        }
        }
    }

    private bool checkCard(GameObject card)
    {
        CardDisplay card_display = card.GetComponent<CardDisplay>();
        string name = card_display.getName();
        string color = card_display.getColor();

        string[] lastCard = getDiscardPileCard();
        if(name.Equals(lastCard[0]) || color.Equals(lastCard[1]))
            return true;

        return false;
    }

    public void placeCard(GameObject card)
    {
        CardDisplay card_display = card.GetComponent<CardDisplay>();
        string name = card_display.getName();
        string color = card_display.getColor();

        discardPile.Add(name + color);
        drawDiscardPile();
        colorPickText.SetActive(false);
        Destroy(card);

    }

    public void colorPicked()
    {
        string button = EventSystem.current.currentSelectedGameObject.name;
        string player = client.clientName;
        colorPickPanel.SetActive(false);
        string msg = "CWILD";
        if (wild_card.name.Contains("4"))
        {
            msg += "4";
            if (turn == 0)
                msg += "|" + player2Hand.name;
            else
                msg += "|" + player1Hand.name;

            int new_deck_size = fullDeck.Count;
            for (int i = 0; i < 4; i++)
            {
                int randomNumber = UnityEngine.Random.Range(0, new_deck_size - 1);
                msg += "|" + randomNumber;
                new_deck_size--;
            }
        }
        msg += "|" + wild_card.name + "|" + player;
        string color = "";
        switch (button)
        {
            case "RedButton":
                color = "red";
                break;
            case "BlueButton":
                color = "blue";
                break;
            case "YellowButton":
                color = "yellow";
                break;
            case "GreenButton":
                color = "green";
                break;
        }
        msg += "|" + color;
        client.Send(msg);
    }

    public void wildCard(string player, string color)
    {
        string last_card = discardPile[discardPile.Count - 1].ToString();
        last_card += color;
        discardPile[discardPile.Count - 1] = last_card;
        Text txt = colorPickText.GetComponent<Text>();
        txt.text = player + " has chosen " + color;
        colorPickText.SetActive(true);
    }

    private string[] getDiscardPileCard()
    {
        string[] return_card = new string[2];
        string last_card = discardPile[discardPile.Count - 1].ToString();

        return_card[0] = last_card.Substring(0, 1);
        return_card[1] = last_card.Substring(1);

        if (last_card.Contains("skip"))
        {
            return_card[0] = "skip";
            return_card[1] = last_card.Substring(4);
        }
        if (last_card.Contains("reverse")){
            return_card[0] = "reverse";
            return_card[1] = last_card.Substring(7);
        }
        if (last_card.Contains("draw2")){
            return_card[0] = "draw2";
            return_card[1] = last_card.Substring(5);
        }
        if (last_card.Contains("wild"))
            if(last_card.Contains("4"))
            {
                return_card[0] = "wild4";
                return_card[1] = last_card.Substring(5);
            }
            else
            { 
                return_card[0] = "wild";
                return_card[1] = last_card.Substring(4);
            }
        return return_card;
    }

    public void endTurn()
    {
        if (player1Hand.transform.childCount < 1)
        {
            gameOverScreen.SetActive(true);
            winText.text = p1Name.text + " WON!";
        }
        if (player2Hand.transform.childCount < 1)
        {
            gameOverScreen.SetActive(true);
            winText.text = p2Name.text + " WON!";
        }

        turn = (turn + 1) % 2;

        if (turn == 0)
        {
            player1Indicator.SetActive(true);
            player2Indicator.SetActive(false);
        }
        else
        {
            player1Indicator.SetActive(false);
            player2Indicator.SetActive(true);
        }

        
       
    }

    public void drawFromDeck()
    {
        if ((client.isHost && turn == 0) || (!client.isHost && turn == 1))
        {
            switch (turn)
            {
                case 0:
                    pickRandomCard("player1");
                    break;
                case 1:
                    pickRandomCard("player2");
                    break;
                default:
                    Debug.Log("invalid card draw");
                    break;
            }
        }
    }

    public void drawCard(GameObject playerHand, int randomNumber)
    {
        string randomCard = fullDeck[randomNumber].ToString();
        fullDeck.RemoveAt(randomNumber);

        Button drawnCard = Instantiate(cardPrefab) as Button;
        GameObject drawnCardObj = drawnCard.gameObject;
        drawnCard.onClick.AddListener(() => { cardClicked(drawnCardObj); });
        CardDisplay cd = drawnCard.GetComponent<CardDisplay>();

        if ((client.isHost && playerHand.tag.Equals("Player1")) || (!client.isHost && playerHand.tag.Equals("Player2")))
            cd.setCard(randomCard);
        else
        {
            cd.setCard(randomCard);
            cd.drawEmpty();
        }
        if(GameObject.Find(cd.getName() + cd.getColor()) == null)
        { 
            drawnCardObj.name = cd.getName() + cd.getColor();
        }
        else
        {
            drawnCardObj.name = cd.getName() + cd.getColor() + "(1)";
        }
        drawnCardObj.tag = playerHand.tag;
        drawnCard.transform.SetParent(playerHand.transform, false);
    }

    private void pickRandomCard(string player)
    {
        int randomNumber = UnityEngine.Random.Range(0, fullDeck.Count - 1);
        GameObject playerHand;

        switch (player)
        {
            case "player1":
                playerHand = player1Hand;
                break;
            case "player2":
                playerHand = player2Hand;
                break;
            default:
                playerHand = null;
                Debug.Log("Incorrect player");
                break;
        }

        string msg = "CDRAW";
        msg += "|" + playerHand.name;
        msg += "|" + randomNumber;
        client.Send(msg);
    }

    private void gameStart()
    {
        fullDeck = new ArrayList();
        discardPile = new ArrayList();

        generateFullDeck();
        generateHands();
    }

    private void generateFullDeck()
    {
        for (int i = 1; i < 10; i++)
        {
            generateAllColors(i.ToString());
            generateAllColors(i.ToString());
        }
        generateAllColors("0");
        generateAllColors("draw2");
        generateAllColors("draw2");
        generateAllColors("skip");
        generateAllColors("skip");
        generateAllColors("reverse");
        generateAllColors("reverse");

        string wild = "wild";
        string wild4 = "wild4";
        for (int i = 0; i < 4; i++)
        {
            fullDeck.Add(wild);
            fullDeck.Add(wild4);
        }
    }

    private void generateAllColors(string name)
    {
        string red = name + "red";
        fullDeck.Add(red);
        string yellow = name + "yellow";
        fullDeck.Add(yellow);
        string green = name + "green";
        fullDeck.Add(green);
        string blue = name + "blue";
        fullDeck.Add(blue);
    }

    private void generateHands()
    {
        if (client.isHost) { 
            int new_deck_size = fullDeck.Count;

            string msg = "CGEN";
            for (int i = 0; i < 14; i++)
            {
                int randomNumber = UnityEngine.Random.Range(0, new_deck_size - 1);
                msg += "|" + randomNumber;
                new_deck_size--;
            }
            int first_card = UnityEngine.Random.Range(0, new_deck_size - 1);
            msg += "|" + player1Hand.name;
            msg += "|" + player2Hand.name;
            msg += "|" + first_card;

            client.Send(msg);
       }
    }

    public void initiateDiscardPile(int card_number)
    {
        string randomCard = fullDeck[card_number].ToString();
        fullDeck.RemoveAt(card_number);
        discardPile.Add(randomCard);
        drawDiscardPile();
    }

    private void drawDiscardPile()
    {
        string lastCard = discardPile[discardPile.Count - 1].ToString();
        Card card = Resources.Load("Cards/" + lastCard) as Card;
        Image img = discardPlace.GetComponent<Image>();
        img.sprite = card.image;

    }

    public void SetNames(string p1, string p2)
    {
        p1Name.text = p1;
        p2Name.text = p2;
    }
}

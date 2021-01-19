using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Card card;
    public Image image;

    public void setCard(string name)
    {
        this.card = Resources.Load("Cards/" + name) as Card;
        image.sprite = card.image;
    }

    public string getName()
    {
        return card.name;
    }

    public string getColor()
    {
        return card.color;
    }

    public void drawEmpty()
    {
        Card backcard = Resources.Load("Cards/back") as Card;
        image.sprite = backcard.image;
    }
    
}

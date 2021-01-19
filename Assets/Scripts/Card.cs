using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class Card : ScriptableObject{
  public string name;
  public string type;
  public string color;
  public Sprite image;

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    ToiletPaper,
    HandSanitizer, // образует лужу в которой подскальзываются тележки
    Masks,
    TinCan, // оглашает, если попала в персонажа
    Twinkies, // уменьшает регенерацию стамины English Text
    Floor
}

public class Item : MonoBehaviour
{
    public ItemType ItemType;
}

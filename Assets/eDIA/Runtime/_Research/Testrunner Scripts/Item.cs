using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipSlots {
    LeftHand,
    RightHand,
    Chest,
    Legs
}

public class Item : MonoBehaviour {

    public EquipSlots EquipSlot { get; set; }

    public int Armor { get; set; }

}
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    Dictionary<EquipSlots, Item> _equippedItems = new Dictionary<EquipSlots, Item>();
    List<Item> _unequippedItems = new List<Item>();

    public void EquipItem(Item item) {
        if (_equippedItems.ContainsKey(item.EquipSlot))
            _unequippedItems.Add(_equippedItems[item.EquipSlot]);

        _equippedItems[item.EquipSlot] = item;
    }

    public Item GetItem(EquipSlots equipSlot) {
        if (_equippedItems.ContainsKey(equipSlot)) 
            return _equippedItems[equipSlot];
        else return null;
    }


    public float GetTotalArmor () {
        int totalArmor = _equippedItems.Values.Sum(t => t.Armor);
        return totalArmor;
    }
}

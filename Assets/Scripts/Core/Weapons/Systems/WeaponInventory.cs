using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ThrowableData weapon;
    public int ammoCount;

    public InventorySlot(ThrowableData weaponType, int initialAmmo = 0)
    {
        weapon = weaponType;
        ammoCount = initialAmmo;
    }

    public bool CanUse() => ammoCount > 0;
    public void Use() => ammoCount--;
    public void AddAmmo(int amount) => ammoCount += amount;
    public bool IsEmpty() => ammoCount <= 0;
}

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private int maxSlots = 3;

    public event Action OnInventoryChanged;

    public List<InventorySlot> Slots => slots;
    public int MaxSlots => maxSlots;

    public bool AddWeapon(ThrowableData weapon, int ammoAmount)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.weapon == weapon)
            {
                slot.AddAmmo(ammoAmount);
                NotifyChange();
                return true;
            }
        }

        if (slots.Count < maxSlots)
        {
            slots.Add(new InventorySlot(weapon, ammoAmount));
            NotifyChange();
            return true;
        }

        return false;
    }

    public void UseAmmo(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count)
        {
            slots[slotIndex].Use();

            if (slots[slotIndex].IsEmpty())
            {
                RemoveSlot(slotIndex);
            }
            else
            {
                NotifyChange();
            }
        }
    }

    private void RemoveSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count)
        {
            slots.RemoveAt(slotIndex);
            NotifyChange();
        }
    }

    private void NotifyChange()
    {
        OnInventoryChanged?.Invoke();
    }

    public InventorySlot GetSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
            return slots[index];
        return null;
    }

    public bool HasWeapon(ThrowableData weapon)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.weapon == weapon)
                return true;
        }
        return false;
    }

    public int GetAmmoCount(ThrowableData weapon)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.weapon == weapon)
                return slot.ammoCount;
        }
        return 0;
    }
}
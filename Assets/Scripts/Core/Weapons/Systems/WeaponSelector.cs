using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WeaponSelector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] slotImages;
    [SerializeField] private TMP_Text[] ammoTexts;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color emptyColor = Color.gray;

    private WeaponManager weaponManager;

    private void Start()
    {
        weaponManager = WeaponManager.Instance;

        if (weaponManager != null && weaponManager.Inventory != null)
        {
            weaponManager.Inventory.OnInventoryChanged += UpdateUI;
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (weaponManager != null && weaponManager.Inventory != null)
        {
            weaponManager.Inventory.OnInventoryChanged -= UpdateUI;
        }
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (weaponManager == null)
        {
            weaponManager = WeaponManager.Instance;
            if (weaponManager == null) return;
        }

        if (weaponManager.Inventory == null) return;

        List<InventorySlot> slots = weaponManager.Inventory.Slots;

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i < slots.Count)
            {
                InventorySlot slot = slots[i];
                bool isSelected = i == weaponManager.CurrentWeaponIndex;

                if (slot.weapon != null && slot.weapon.Icon != null)
                {
                    slotImages[i].sprite = slot.weapon.Icon;
                }

                slotImages[i].color = isSelected ? selectedColor : normalColor;
                ammoTexts[i].text = slot.ammoCount.ToString();
                ammoTexts[i].color = slot.ammoCount > 0 ? Color.white : Color.red;

                slotImages[i].gameObject.SetActive(true);
                ammoTexts[i].gameObject.SetActive(true);
            }
            else
            {
                slotImages[i].color = emptyColor;
                slotImages[i].sprite = null;
                ammoTexts[i].text = "Empty";
                ammoTexts[i].color = Color.gray;

                slotImages[i].gameObject.SetActive(true);
                ammoTexts[i].gameObject.SetActive(true);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitInfoManager : MonoBehaviour
{
    [SerializeField] UiManager uiManager;
    [SerializeField] Player player;
    [SerializeField] TMP_Text UnitName;
    [SerializeField] TMP_Text UnitInfo;
    [SerializeField] Image UnitImage;
    [SerializeField] Slider ActionPoints;
    [SerializeField] Slider HealthPoints;
    [SerializeField] Slider SupplyPoints;

    private void Update()
    {
        if (player.CurrentlySelectedUnit == null)
        {
            uiManager.CloseUnitInfo();
            return;
        }

        UnitName.text = player.CurrentlySelectedUnit.UnitName;
        UnitInfo.text = "Attack: " + player.CurrentlySelectedUnit.Attack + " Defence: " + player.CurrentlySelectedUnit.Defence;
        UnitImage.sprite = player.CurrentlySelectedUnit.UnitImage;
        ActionPoints.maxValue = player.CurrentlySelectedUnit.MaxActionPoints;
        ActionPoints.value = player.CurrentlySelectedUnit.CurrentActionPoints;
        HealthPoints.maxValue = player.CurrentlySelectedUnit.MaxHealth;
        HealthPoints.value = player.CurrentlySelectedUnit.CurrentHealth;
        SupplyPoints.maxValue = player.CurrentlySelectedUnit.MaxSupply;
        SupplyPoints.value = player.CurrentlySelectedUnit.CurrentSupply;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CombatUIManager : MonoBehaviour
{
    [SerializeField] CombatRunner CombatRunner;
    [SerializeField] GameObject UnitSelectionUIPrefab;
    [SerializeField] GameObject DeadUnitPrefab;
    [SerializeField] GameObject CombatSelectionUI;
    [SerializeField] GameObject CombatExcecutionUI;
    [SerializeField] GameObject DoneCombat;
    [SerializeField] GameObject OptionsSelector;
    [SerializeField] RectTransform AttactingSelectionViewPort;
    [SerializeField] RectTransform DefendingSelectionViewPort;

    [SerializeField] AiFactionCore[] aiFactionCores;

    public Attack currentAttackOrder;

    List<Unit> attackingUnits = new List<Unit>();
    List<Unit> defendingUnits = new List<Unit>();

    // Will only work with 1 AI oponenet will need way to fix eventualy 
    public void StartCombatInvolveingPlayer(Tile CombatTile, Unit MainAttacker, Unit[] supportingAttackers = null)
    {
        ClearDisplays();
        attackingUnits = new List<Unit>();
        defendingUnits = new List<Unit>();
        OptionsSelector.SetActive(true);
        DoneCombat.SetActive(false);

        RectTransform mainAttackerUI = CreateUnitDisplay(MainAttacker);
        mainAttackerUI.SetParent(AttactingSelectionViewPort);
        mainAttackerUI.localPosition = Vector3.zero;
        mainAttackerUI.localScale = Vector3.one;
        UiManager.SetRight(mainAttackerUI, 0);

        attackingUnits.Add(MainAttacker);

        if (MainAttacker.FactionID == GameSettings.PLAYERFACTIONID)
        {
            MainAttacker.CurrentTile.ChangeTileOverlayColor(-3);
        }

        RectTransform mainDefender = CreateUnitDisplay(CombatTile.CurrentGroundUnit);
        mainDefender.SetParent(DefendingSelectionViewPort);
        mainDefender.localPosition = Vector3.zero;
        mainDefender.localScale = Vector3.one;
        UiManager.SetRight(mainDefender, 0);

        defendingUnits.Add(CombatTile.CurrentGroundUnit);
        if (supportingAttackers != null)
        {
            foreach (Unit item in supportingAttackers)
            {
                RectTransform attacker = CreateUnitDisplay(item);
                attacker.parent = AttactingSelectionViewPort;
                attacker.localPosition = new Vector3(0, -75 * AttactingSelectionViewPort.transform.childCount, 0);
                attackingUnits.Add(item);
            }
        }
    }

    public void QuickCombatStart()
    {
        if (defendingUnits[0].FactionID != GameSettings.PLAYERFACTIONID)
        {
            foreach (AiFactionCore item in aiFactionCores)
            {
                if (item.factionData.FactionID == defendingUnits[0].FactionID)
                {
                    defendingUnits.AddRange(item.CreateDefence(currentAttackOrder.EndTile));
                }
            }
        }
        

        if (attackingUnits.Count <= 0 || defendingUnits.Count <= 0)
        {
            return;
        }
        // At some point add a way to show the rolls and such using the combat matchups
        CombatMatchUp[] matchups = CombatRunner.Combat(attackingUnits, defendingUnits).ToArray();

        ClearDisplays();
        foreach (Unit item in attackingUnits)
        {
            RectTransform attacker = CreateUnitDisplay(item);
            attacker.SetParent(AttactingSelectionViewPort);
            attacker.localPosition = new Vector3(0, -75 * AttactingSelectionViewPort.transform.childCount + 150, 0) ;
            UiManager.SetRight(attacker, 0);
        }
        foreach (Unit item in defendingUnits)
        {
            RectTransform Defender = CreateUnitDisplay(item);
            Defender.SetParent(DefendingSelectionViewPort);
            Defender.localPosition = new Vector3(0, -75 * DefendingSelectionViewPort.transform.childCount + 150, 0);
            UiManager.SetRight(Defender, 0);
        }

        OptionsSelector.SetActive(false);
        DoneCombat.SetActive(true);
    }

    void ClearDisplays()
    {
        foreach (Transform item in AttactingSelectionViewPort)
        {
            Destroy(item.gameObject);
        }
        foreach (Transform item in DefendingSelectionViewPort)
        {
            Destroy(item.gameObject);
        }
    }

    RectTransform CreateUnitDisplay(Unit unit)
    {
        if (unit != null && unit.CurrentHealth > 0)
        {
            RectTransform rect = Instantiate(UnitSelectionUIPrefab).GetComponent<RectTransform>();
            rect.GetChild(0).GetComponent<Image>().sprite = unit.UnitImage;
            rect.GetChild(1).GetComponent<TMP_Text>().text = unit.name;
            rect.GetChild(2).GetComponent<TMP_Text>().text = "Attack: " + unit.Attack + " Defence: " + unit.Defence;
            rect.GetChild(4).GetComponent<Slider>().maxValue = unit.MaxHealth;
            rect.GetChild(4).GetComponent<Slider>().value = unit.CurrentHealth;
            rect.GetChild(3).GetComponent<Slider>().maxValue = unit.MaxSupply;
            rect.GetChild(3).GetComponent<Slider>().value = unit.CurrentSupply;

            return rect;
        }else
        {
            RectTransform rect = Instantiate(DeadUnitPrefab).GetComponent<RectTransform>();
            rect.GetChild(0).GetComponent<Image>().sprite = unit.UnitImage;
            rect.GetChild(1).GetComponent<TMP_Text>().text = unit.name;
            return rect;
        }
    }

    public void AddUnit(Unit unit, int factionID)
    {
        if (factionID == currentAttackOrder.attackingFactionID)
        {
            if (!attackingUnits.Contains(unit))
            {
                RectTransform attacker = CreateUnitDisplay(unit);
                attacker.SetParent(AttactingSelectionViewPort);
                attacker.localPosition = new Vector3(0, -75 * AttactingSelectionViewPort.transform.childCount, 0);
                attackingUnits.Add(unit);
                attacker.localScale = Vector3.one;
                UiManager.SetRight(attacker, 0);
                unit.CurrentTile.ChangeTileOverlayColor(-3);
            }
        }
        else
        {
            if (!defendingUnits.Contains(unit))
            {
                RectTransform defender = CreateUnitDisplay(unit);
                defender.SetParent(DefendingSelectionViewPort);
                defender.localPosition = new Vector3(0, -75 * DefendingSelectionViewPort.transform.childCount, 0);
                defendingUnits.Add(unit);
                defender.localScale = Vector3.one;
                UiManager.SetRight(defender, 0);
                unit.CurrentTile.ChangeTileOverlayColor(-3);
            }
        }
    }

    public void RemoveUnit(Unit unit, int factionID)
    {
        if (factionID == currentAttackOrder.attackingFactionID)
        {
            if (attackingUnits.Contains(unit))
            {
                int unitInt = attackingUnits.IndexOf(unit);
                Destroy(AttactingSelectionViewPort.transform.GetChild(unitInt).gameObject);
                attackingUnits.RemoveAt(unitInt);
                int index = 0;
                foreach (RectTransform item in AttactingSelectionViewPort)
                {
                    item.localPosition = new Vector3(0, -75 * index, 0);
                    UiManager.SetLeft(item, 0);
                    UiManager.SetRight(item, 0);
                    index++;
                }
                unit.CurrentTile.ChangeTileOverlayColor(-2);
            }
        }else
        {
            if (defendingUnits.Contains(unit))
            {
                int unitInt = defendingUnits.IndexOf(unit);
                Destroy(DefendingSelectionViewPort.transform.GetChild(unitInt).gameObject);
                defendingUnits.RemoveAt(unitInt);
                int index = 0;
                foreach (RectTransform item in DefendingSelectionViewPort)
                {
                    item.localPosition = new Vector3(0, -75 * index, 0);
                    UiManager.SetLeft(item, 0);
                    UiManager.SetRight(item, 0);
                    index++;
                }
                unit.CurrentTile.ChangeTileOverlayColor(-2);
            }
        }
    }

    public void Done()
    {
        currentAttackOrder.Completed = true;
    }
}

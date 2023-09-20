using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "ScriptableObjects/Unit")]
public class UnitScriptableObject : ScriptableObject
{
    public string UnitName = "New Unit";
    public Sprite UnitImage;
    public int Cost = 3;
    public int MaxHealth = 100;
    public int MaxSupply = 100;
    public int MaxActionPoints = 4;
    public int Attack = 0;
    public int Defence = 0;
    public int SupportRange = 0;
    public GameObject UnitModel;
    public GameObject OrderPreview;
    public int SupplyDistributionRate = 0;
    public int SupplyRadius = 0;
}

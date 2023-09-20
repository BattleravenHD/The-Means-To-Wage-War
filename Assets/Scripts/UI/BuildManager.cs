using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildManager : MonoBehaviour
{
    [SerializeField] RectTransform BuildScrolArea;
    [SerializeField] GameObject unitBuildPrefab;
    [SerializeField] UnitScriptableObject[] Units;
    [SerializeField] GameData GameData;
    [SerializeField] TurnRunner runner;

    UiManager uiManager;
    public UnitScriptableObject unitSpawning;

    [HideInInspector] public GameObject currentOrderPreview;

    // Start is called before the first frame update
    void Start()
    {
        uiManager = GetComponent<UiManager>();

        for (int i = 0; i < Units.Length; i++)
        {
            UnitScriptableObject unit = Units[i];
            RectTransform game = Instantiate(unitBuildPrefab, BuildScrolArea).GetComponent<RectTransform>();
            game.localPosition = new Vector3(10, -75 - 170 * i, 0);
            game.GetChild(0).GetComponent<Image>().sprite = unit.UnitImage;
            game.GetChild(1).GetComponent<TMP_Text>().text = unit.name + "\nCost: " + unit.Cost + "\nAttack: " + unit.Attack + "\nDefence: " + unit.Defence;
            game.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { SelectUnitToSpawn(unit); });
        }
    }    

    public void SelectUnitToSpawn(UnitScriptableObject unit)
    {
        if (uiManager.PlayerFactionData.FactionIPC < unit.Cost)
        {
            return;
        }
        if (unitSpawning == null)
        {
            uiManager.IsPlaceing = true;
            unitSpawning = unit;
            if (currentOrderPreview != null)
            {
                Destroy(currentOrderPreview);
            }
            currentOrderPreview = Instantiate(unitSpawning.OrderPreview, Vector3.zero, Quaternion.identity);
        }
        else if (unit == unitSpawning)
        {
            uiManager.IsPlaceing = true;
            unitSpawning = null;
            if (currentOrderPreview != null)
            {
                Destroy(currentOrderPreview);
            }
        }
        else
        {
            uiManager.IsPlaceing = true;
            unitSpawning = unit;
            if (currentOrderPreview != null)
            {
                Destroy(currentOrderPreview);
            }
            currentOrderPreview = Instantiate(unitSpawning.OrderPreview, Vector3.zero, Quaternion.identity);
        }
    }

    // Will Only work for land units Needs Fixing 
    public void Spawn(Tile spawnTile)
    {
        FactionData orderSet = null;

        foreach (FactionData item in GameData.factionData)
        {
            if (item.FactionID == 0)
            {
                orderSet = item;
            }
        }
        if (orderSet == null)
        {
            throw new MissingReferenceException();
        }
        GameObject game = Instantiate(unitSpawning.OrderPreview, spawnTile.Location, Quaternion.identity);
        GameData.OrderPreviewObjects.Add(game.GetComponent<OrderPreview>());

        orderSet.SpawnOrders.Add(new SpawnUnit(unitSpawning, spawnTile, runner, GameSettings.PLAYERFACTIONID));
        unitSpawning = null;
        uiManager.IsPlaceing = false;
        if (currentOrderPreview != null)
        {
            Destroy(currentOrderPreview);
        }
        currentOrderPreview = null;
    }
}

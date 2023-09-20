using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    
    [SerializeField] Vector3 clamp;
    [SerializeField] float speed;
    [SerializeField] UiManager uiManager;
    [SerializeField] GameObject TileCursor;
    [SerializeField] GameData gameData;
    [SerializeField] FactionData factionData;

    public Unit CurrentlySelectedUnit;

    Vector2 moveDirection;
    Vector2 mousPos;
    int currentTile = 0;
    int lastTile = 0;

    Plane m_Plane;

    bool hasCreatedPossibleUnits = false;
    bool hasCreatedPossibleUnitPlacementLocations = false;
    List<Unit> possibleUnits = new List<Unit>();

    OrderPreview currentOrderPreview;
    List<Tile> unitPlacementLocations = new List<Tile>();

    // Start is called before the first frame update
    void Start()
    {
        m_Plane = new Plane(Vector3.up, 0f);
    }

    private void Update()
    {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x + moveDirection.x * speed * Time.deltaTime, 0, clamp.x), transform.position.y, Mathf.Clamp(transform.position.z + moveDirection.y * speed * Time.deltaTime, 0, clamp.z));

        GetCurrentTile();

        if (currentTile < gameData.GameTiles.Length)
        {
            TileCursor.transform.position = gameData.GameTiles[currentTile].Location;
            if (uiManager.buildManager.currentOrderPreview != null)
            {
                uiManager.buildManager.currentOrderPreview.transform.position = TileCursor.transform.position;
            }
            if (currentOrderPreview != null)
            {
                currentOrderPreview.transform.position = TileCursor.transform.position;
                if (lastTile != currentTile)
                {
                    currentOrderPreview.orders = GenerateMoveOrder();
                    currentOrderPreview.UpdateLines();
                }
            }
            if (uiManager.IsPlaceing && !hasCreatedPossibleUnitPlacementLocations)
            {
                hasCreatedPossibleUnitPlacementLocations = true;
                foreach (City city in factionData.FactionCities)
                {
                    unitPlacementLocations.Add(city.CentreTile);
                    city.CentreTile.ChangeTileOverlayColor(-2);
                    foreach (Tile tile in city.CityTiles)
                    {
                        if (tile.FactionOwnerShip == GameSettings.PLAYERFACTIONID)
                        {
                            unitPlacementLocations.Add(tile);
                            tile.ChangeTileOverlayColor(-2);
                        }
                    }
                }
            }else if (!uiManager.IsPlaceing && hasCreatedPossibleUnitPlacementLocations)
            {
                hasCreatedPossibleUnitPlacementLocations = false;
                foreach (Tile item in unitPlacementLocations)
                {
                    item.ResetTileOverLayColor();
                }
                unitPlacementLocations.Clear();
            }
            lastTile = currentTile;
        }

        CreateSuitableUnits();
    }

    void GetCurrentTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(mousPos);

        float enter = 0.0f;

        if (m_Plane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            // Gets the current tile base on the position of the cursor by raycasting onto a flat plane ang then comparing that against he idnex value the tile would have been assigned based on the world postion so there cannot be any offset to the world 
            currentTile = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp(hitPoint.z, 0, float.MaxValue) / 1.5f) * Region.regionWidth * MapGenerator.MapWidth + Mathf.RoundToInt(Mathf.Clamp(hitPoint.x - 0.86f * (Mathf.RoundToInt(Mathf.Clamp(hitPoint.z, 0, float.MaxValue) / 1.5f) % 2), 0, float.MaxValue) / 1.72f), 0, gameData.GameTiles.Length - 1);
        }
    }

    void CreateSuitableUnits()
    {
        if (uiManager.combatUIOpen && !hasCreatedPossibleUnits)
        {
            hasCreatedPossibleUnits = true;
            foreach (Unit unit in factionData.FactionUnits)
            {
                int AP = 0;
                if (unit.TryGetComponent<GroundSupportUnit>(out GroundSupportUnit supportUnit))
                {
                    if (supportUnit.CurrentActionPoints > 0)
                    {
                        AP = supportUnit.SupportRange;
                    }
                }else
                {
                    AP = unit.CurrentActionPoints;
                }
                if (unit.FactionID != GameSettings.PLAYERFACTIONID)
                {
                    continue;
                }
                Order[] paths = PathFinding.PathToCreateOrder(unit.CurrentTile, uiManager.combatUIManager.currentAttackOrder.EndTile, unit, false, true).ToArray();
                foreach (Order path in paths)
                {
                    if (path is Move)
                    {
                        AP -= unit.GetType() == typeof(GroundSupportUnit) ? 1 : unit.TileTerrianCost(path.EndTile);
                    }
                }

                if (AP >= 0)
                {
                    unit.CurrentTile.ChangeTileOverlayColor(-2);
                    possibleUnits.Add(unit);
                }
            }
        }
        else if (!uiManager.combatUIOpen && hasCreatedPossibleUnits)
        {
            foreach (Unit unit in factionData.FactionUnits)
            {
                unit.CurrentTile.ResetTileOverLayColor();
            }
            hasCreatedPossibleUnits = false;
            possibleUnits.Clear();
        }
    }

    List<Order> GenerateMoveOrder()
    {
        if (currentOrderPreview.currentTile.TileDataID == currentTile)
        {
            return null;
        }
        
        return PathFinding.PathToCreateOrder(CurrentlySelectedUnit.CurrentTile, gameData.GameTiles[currentTile], CurrentlySelectedUnit);
    }



    void OnPlayerMove(InputValue inputValue)
    {
        moveDirection = inputValue.Get<Vector2>();
    }

    void OnLeftSelect(InputValue inputValue)
    {
        if (!uiManager.combatUIOpen)
        {
            NormalLeftClick();
        }else
        {
            CombatLeftClick();
        }
    }

    void OnRightSelect(InputValue inputValue)
    {
        if (!uiManager.combatUIOpen)
        {
            NormalRightClick();
        }
        else
        {
            CombatRightClick();
        }
    }

    void NormalRightClick()
    {
        if (!uiManager.IsPlaceing)
        {
            if (currentOrderPreview != null && CurrentlySelectedUnit != null)
            {
                // Removes Old orders from the faction data then adds the new orders to both the faction data and the unit
                if (CurrentlySelectedUnit.Orders.Count > 0)
                {
                    foreach (Order item in CurrentlySelectedUnit.Orders)
                    {
                        factionData.NormalOrders.Remove(item);
                    }
                }
                CurrentlySelectedUnit.Orders = currentOrderPreview.orders;

                // If there was an order preview for the current unit (Eg you are overriding current orders) destroy it 
                if (CurrentlySelectedUnit.OrderPreview != null)
                {
                    gameData.OrderPreviewObjects.Remove(CurrentlySelectedUnit.OrderPreview);
                    Destroy(CurrentlySelectedUnit.OrderPreview.gameObject);
                }
                CurrentlySelectedUnit.OrderPreview = currentOrderPreview;
                gameData.OrderPreviewObjects.Add(currentOrderPreview);
                currentOrderPreview = null;
            }
        }
    }

    void CombatRightClick()
    {
        if (gameData.GameTiles[currentTile].CurrentGroundUnit != null && possibleUnits.Contains(gameData.GameTiles[currentTile].CurrentGroundUnit))
        {
            uiManager.combatUIManager.RemoveUnit(gameData.GameTiles[currentTile].CurrentGroundUnit, GameSettings.PLAYERFACTIONID);
        }
    }

    void NormalLeftClick()
    {
        if (!uiManager.IsPlaceing && !EventSystem.current.IsPointerOverGameObject())
        {
            if ((gameData.GameTiles[currentTile].CurrentGroundUnit != null || gameData.GameTiles[currentTile].CurrentAirUnit != null) && gameData.GameTiles[currentTile].CurrentGroundUnit.FactionID == GameSettings.PLAYERFACTIONID)
            {
                // can select a unit on a tile
                if (currentOrderPreview != null)
                {
                    Destroy(currentOrderPreview.gameObject);
                }
                if (CurrentlySelectedUnit != null)
                {
                    CurrentlySelectedUnit.UnitDeselected();
                }
                if (gameData.GameTiles[currentTile].CurrentGroundUnit != null)
                {
                    CurrentlySelectedUnit = gameData.GameTiles[currentTile].CurrentGroundUnit;
                }
                else
                {
                    CurrentlySelectedUnit = gameData.GameTiles[currentTile].CurrentAirUnit;
                }
                currentOrderPreview = Instantiate(CurrentlySelectedUnit.OrderPreviewIntantiable, Vector3.zero, Quaternion.identity).GetComponent<OrderPreview>();
                currentOrderPreview.baseUnit = CurrentlySelectedUnit;
                currentOrderPreview.currentTile = gameData.GameTiles[currentTile];
                // Add order creation 
                CurrentlySelectedUnit.UnitSelected();
                uiManager.OpenUnitInfo();
            }
            else
            {
                // no unit on tile to select
                uiManager.CloseMenu();
                uiManager.CloseUnitInfo();
                if (CurrentlySelectedUnit != null)
                {
                    CurrentlySelectedUnit.UnitDeselected();
                }
                if (currentOrderPreview != null)
                {
                    Destroy(currentOrderPreview.gameObject);
                }
                CurrentlySelectedUnit = null;
            }
        }
        else if (uiManager.IsPlaceing && unitPlacementLocations.Contains(gameData.GameTiles[currentTile]))
        {
            factionData.FactionIPC -= uiManager.buildManager.unitSpawning.Cost;
            uiManager.buildManager.Spawn(gameData.GameTiles[currentTile]);
        }
    }

    void CombatLeftClick()
    {
        if (gameData.GameTiles[currentTile].CurrentGroundUnit != null && possibleUnits.Contains(gameData.GameTiles[currentTile].CurrentGroundUnit))
        {
            uiManager.combatUIManager.AddUnit(gameData.GameTiles[currentTile].CurrentGroundUnit, GameSettings.PLAYERFACTIONID);
        }
    }

    void OnMousePosition(InputValue inputValue)
    {
        mousPos = inputValue.Get<Vector2>();
    }
}

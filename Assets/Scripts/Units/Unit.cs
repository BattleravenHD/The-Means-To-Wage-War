using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Top Level class for Units. 
public abstract class Unit : MonoBehaviour
{
    public string UnitName;         // Used for custom nameing of units as they spawn eg (145th Inf Division)
    public Sprite UnitImage;
    public int FactionID;             // Faction Int 0 = player
    public int CurrentHealth;
    public int MaxHealth;
    public int CurrentSupply;
    public int MaxSupply;
    public int CurrentActionPoints;
    public int MaxActionPoints;
    public int Attack;
    public int Defence;

    public List<Order> Orders = new List<Order>();
    public Tile CurrentTile;
    public GameObject OrderPreviewIntantiable;
    public OrderPreview OrderPreview;
    public FactionData factionDataReference;

    public UnitScriptableObject unitType;

    List<Tile> visited = new List<Tile>();

    public abstract void MoveUnit(Tile tile);
    public abstract void MoveUnit(Tile startTile, Tile endTile, float percent);
    public abstract int CreateAttack(Tile combatTile, Tile StartTile, int roll, float bonus);
    public abstract int CreateDefence(Tile combatTile, int roll, float bonus);

    public virtual int StrengthRating()
    {
        return Mathf.RoundToInt(Defence * SupplyMalice());
    }

    public virtual int TileTerrianCost(Tile tile)
    {
        if (tile == null)
        {
            return int.MaxValue;
        }
        if (tile.HasRoad)
        {
            return 1;   // Faster Movement on roads
        }
        switch (tile.tileTerrian)
        {
            case TileTerrianTypes.GrassLand:
                return 2;
            case TileTerrianTypes.Ocean:
                return 1;
            case TileTerrianTypes.City:
                return 4;
            case TileTerrianTypes.Town:
                return 3;
            case TileTerrianTypes.Hamlet:
                return 2;
            default:
                break;
        }
        return int.MaxValue;
    }

    /// <summary>
    /// Makes the Unit take damage
    /// Returns true if unit lives
    /// </summary>
    /// <returns></returns>
    public virtual bool TakeDamage(int damage)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, int.MaxValue);
        ConsumeSupply(damage/2);
        if (CurrentHealth == 0)
        {
            return false;
        }
        return true;
    }
    public virtual float SupplyMalice()
    {
        return 1;
    }

    public int AddSupply(int amount)
    {
        if (CurrentSupply + amount > MaxSupply)
        {
            int leftover = CurrentSupply + amount - MaxSupply;
            CurrentSupply = MaxSupply;
            return leftover;
        }
        CurrentSupply += amount;
        return 0;
    }

    public virtual void LoadUnitData(UnitScriptableObject data, FactionData factionData)
    {
        UnitName = data.UnitName;
        UnitImage = data.UnitImage;
        MaxHealth = data.MaxHealth;
        CurrentHealth = MaxHealth;
        MaxSupply = data.MaxSupply;
        CurrentSupply = MaxSupply;
        MaxActionPoints = data.MaxActionPoints;
        CurrentActionPoints = MaxActionPoints;
        Attack = data.Attack;
        Defence = data.Defence;
        factionDataReference = factionData;
        unitType = data;
    }

    public void AddPossibleOrders()
    {
        int currentAP = CurrentActionPoints;
        bool stop = false;
        int currentOrderIndex = 0;

        if (Orders.Count <= 0)
        {
            return;
        }

        while(currentAP > 0 && !stop)
        {
            if (currentOrderIndex < Orders.Count && currentAP - TileTerrianCost(Orders[currentOrderIndex].EndTile) >= 0)
            {
                factionDataReference.NormalOrders.Add(Orders[currentOrderIndex]);
                currentAP -= TileTerrianCost(Orders[currentOrderIndex].EndTile);
                currentOrderIndex++;
            }
            else
            {
                stop = true;
            }
        }
    }

    public void ConsumeSupply()
    {
        if (CurrentSupply >= GameSettings.TURNSUPPLYCONSUMPTION)
        {
            CurrentSupply -= GameSettings.TURNSUPPLYCONSUMPTION;
            return;
        }
        int amount = GameSettings.TURNSUPPLYCONSUMPTION;

        amount -= CurrentSupply;
        CurrentSupply = 0;
        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, int.MaxValue); 
    }

    public void ConsumeSupply(int amount)
    {
        if (CurrentSupply >= amount)
        {
            CurrentSupply -= amount;
            return;
        }

        amount -= CurrentSupply;
        CurrentSupply = 0;
        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, int.MaxValue);
    }

    public void NewTurn()
    {
        List<Order> temp = new List<Order>();

        foreach (Order item in Orders)
        {
            if (!item.Completed)
            {
                temp.Add(item);
            }
        }
        Orders = temp;

        CurrentActionPoints = MaxActionPoints;
    }

    public void UnitDeselected()
    {
        if (visited.Count > 0)
        {
            foreach (var tile in visited)
            {
                tile.ResetTileOverLayColor();
            }
        }
    }

    private void OnDestroy()
    {
        UnitDeselected();
    }

    public void UnitSelected()
    {
        if (visited.Count > 0)
        {
            foreach (var tile in visited)
            {
                tile.ResetTileOverLayColor();
            }
        }
        visited.Clear();
        List<MovementDisplay> currentPossibleMoves = new List<MovementDisplay>();

        foreach (Tile tile in CurrentTile.AjacentTiles())
        {
            if (tile != null)
            {
                if (CurrentActionPoints - TileTerrianCost(tile) >= 0)
                {
                    currentPossibleMoves.Add(new MovementDisplay(tile, CurrentActionPoints - TileTerrianCost(tile)));
                    visited.Add(tile);
                    // changes the colour to show where it can move in one turn
                    tile.ChangeTileOverlayColor(-2);
                }
            }
        }

        while (currentPossibleMoves.Count > 0)
        {
            if (currentPossibleMoves[0].actionPointsRemaining != 0)
            {
                foreach (Tile tile in currentPossibleMoves[0].tile.AjacentTiles())
                {
                    if (tile != null)
                    {
                        if (currentPossibleMoves[0].actionPointsRemaining - TileTerrianCost(tile) >= 0 && !visited.Contains(tile))
                        {
                            currentPossibleMoves.Add(new MovementDisplay(tile, currentPossibleMoves[0].actionPointsRemaining - TileTerrianCost(tile)));
                            visited.Add(tile);
                            
                            tile.ChangeTileOverlayColor(-2);
                        }
                    }
                }
            }
            currentPossibleMoves.RemoveAt(0);
        }   
    }
}

public class MovementDisplay
{
    public Tile tile;
    public int actionPointsRemaining;

    public MovementDisplay(Tile tile, int actionPointsRemaining)
    {
        this.tile = tile;
        this.actionPointsRemaining = actionPointsRemaining;
    }
}

public enum UnitType
{
    Ground,
    Air,
    Sea
}

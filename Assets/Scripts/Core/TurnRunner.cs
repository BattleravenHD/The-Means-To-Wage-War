using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnRunner : MonoBehaviour
{
    [SerializeField] GameData gameData;
    [SerializeField] UiManager uiManager;

    public void EndTurn()
    {
        StartCoroutine(RunEndTurn());
    }

    IEnumerator RunEndTurn()
    {
        List<OrderPreview> remainingItems = new List<OrderPreview>();
        foreach (AiFactionCore AI in gameData.AiFactions)
        {
            AI.StartCoroutine(AI.CalculateTurn());
            while (!AI.Ready)
            {
                yield return new WaitForSeconds(0);
            }
        }

        foreach (FactionData item in gameData.factionData)
        {
            foreach (Unit unit in item.FactionUnits)
            {
                unit.AddPossibleOrders();
            }
        }
        gameData.CollateOrders();
        foreach (Order order in gameData.CollatedOrders)
        {
            // Add a way to create notifications if an order fails 
            int orderReturn = order.CompleteOrder();
            while(!order.Completed && orderReturn >= 0)
            {
                yield return new WaitForSeconds(0);
            }
        }
        
        foreach (FactionData item in gameData.factionData)
        {
            item.UpdateUnits();
            item.UpdateOrderLists();
            item.StartCoroutine(item.SupplyStep());
            item.UpdateIPC();
        }
        foreach (OrderPreview item in gameData.OrderPreviewObjects)
        {
            item.UpdateLines();
            if (item.orders.Count <= 0)
            {
                Destroy(item.gameObject);
            }
            else
            {
                remainingItems.Add(item);
            }
        }
        gameData.OrderPreviewObjects = remainingItems;
        uiManager.NewTurn();
    }


    // Spawns a unit if it can (used by orders due to non-monobehvour)
    public Unit SpawnGroundUnit(UnitScriptableObject unit, Tile tile, int FactionID)
    {
        if (tile.CurrentGroundUnit == null)
        {
            Unit game = Instantiate(unit.UnitModel).GetComponent<Unit>(); ;
            game.transform.position = tile.Location;
            tile.CurrentGroundUnit = game;
            game.CurrentTile = tile;
            game.FactionID = FactionID;
            game.LoadUnitData(unit, (from item in gameData.factionData where (item.FactionID == FactionID) select item).First());
            game.OrderPreviewIntantiable = unit.OrderPreview;
            (from item in gameData.factionData where (item.FactionID == FactionID) select item).First().FactionUnits.Add(game);
            return game;
        }else
        {
            return null;
        }
    }
}

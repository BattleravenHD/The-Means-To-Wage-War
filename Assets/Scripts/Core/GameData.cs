using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameData : MonoBehaviour
{
    public Tile[] GameTiles = new Tile[MapGenerator.MapWidth * Region.regionWidth * MapGenerator.MapHeight * Region.regionHeight];
    public List<Order> CollatedOrders = new List<Order>();
    public List<FactionData> factionData = new List<FactionData>();
    public List<OrderPreview> OrderPreviewObjects = new List<OrderPreview>();
    public City[] Cities;
    public AiFactionCore[] AiFactions;

    // Collates the orders of all the factions (Done After AI phase)
    public void CollateOrders()
    {
        CollatedOrders = new List<Order>();
        List<List<Order>> ordersToCollate = new List<List<Order>>(); // List of Lists of all factions Orders
        foreach (FactionData faction in factionData)
        {
            if (faction.NormalOrders.Count > 0)
            {
                ordersToCollate.Add(faction.NormalOrders);
            }
        }
        int factionCurrent = UnityEngine.Random.Range(0, ordersToCollate.Count);
        int currentSpeed = UnityEngine.Random.Range(1, 11);
        while (ordersToCollate.Count > 0) // While there are factions with orders left to add
        {
            while (currentSpeed > 0) // A random number so that faster orders get added in quick sucsession
            {
                if (ordersToCollate[factionCurrent].Count > 0)
                {
                    CollatedOrders.Add(ordersToCollate[factionCurrent][0]);
                    currentSpeed -= ordersToCollate[factionCurrent][0].OrderSpeed;
                    ordersToCollate[factionCurrent].RemoveAt(0);
                }
                else
                {
                    ordersToCollate.RemoveAt(factionCurrent);
                    break;
                }
            }
            factionCurrent = UnityEngine.Random.Range(0, ordersToCollate.Count);
            currentSpeed = UnityEngine.Random.Range(1, 11);
        }
        foreach (FactionData faction in factionData) // Add the spawn orders to it
        {
            CollatedOrders.AddRange(faction.SpawnOrders);
        }
    }

    /*
    private void OnGUI()
    {
    // For Debugging tile num + placement 
        for (int i = 0; i < GameTiles.Length; i++)
        {
            Vector2 vec = Camera.main.WorldToScreenPoint(GameTiles[i].Location);
            vec.y *= -1;
            vec.y += 1100;
            vec.x -= 15;
            GUI.Label(new Rect(vec, new Vector2(60, 60)), i.ToString() + " , "+ GameTiles[i].TileDataID);
        }
    }*/
}
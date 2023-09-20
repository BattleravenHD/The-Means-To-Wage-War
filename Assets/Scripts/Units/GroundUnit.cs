using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GroundUnit : Unit
{
    // Snap Move unit
    public override void MoveUnit(Tile tile)
    {
        transform.position = tile.Location;
        CurrentTile.CurrentGroundUnit = null;
        CurrentTile = tile;
        CurrentTile.CurrentGroundUnit = this;
        CaptureTile(tile);
    }

    // Slide Move Unit
    public override void MoveUnit(Tile startTile, Tile endTile, float percent)
    {
        transform.position = Vector3.Lerp(startTile.Location, endTile.Location, percent);
    }

    void CaptureTile(Tile tile)
    {
        tile.FactionOwnerShip = FactionID;
        tile.ResetTileOverLayColor();

        if (tile.city != null)
        {
            tile.city.CaptureCity(FactionID);
            factionDataReference.FactionCities.Add(tile.city);
        }
    }
}

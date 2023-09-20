using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameSettings : MonoBehaviour
{
    // Game Setting Statics
    public static int PLAYERFACTIONID = 0;

    public static int SLOWESTORDERSPEED = 14; // slowest unit max actions + 10
    public static float UNITMOVESPEEDMULTIPLIER = 4.0f;
    public static float UNITHEALTHCOMBATEFFECTIVENESS = 0.4f; // How much damage a unit must take before its comabe effectiveness begins to tank
    public static int ACTIONCOSTREDUCTION = 4; // How Much AP is reduced through participation in combat /2 for support
    public static int TURNSUPPLYCONSUMPTION = 10;

    public static int MINCITYSPREAD = 20;
    public static int CITYPLACEMENTSEARCHAMOUNT = 50;
    public static int CITYROADLINKMAXDISTANCE = 40;
    public static int CITYIPCAMOUNT = 3;

    public static int CITYSUPPLYINCREASEAMOPUNT = 500;
    public static int CITYSUPPLYMAXDISTIBUTIONAMOUNT = 20;
    public static int CITYSUPPLYRADIUS = 3;

    public static int AIFRONTLINEMAXSIZE = 15;

    public FactionColour[] factionColours;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class FactionColour
{
    public int FactionInt;
    public Color FactionColor;
    public Vector2 UVCoord;
}

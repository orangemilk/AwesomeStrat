﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class TurnController : ISystemState
{

    public HashSet<Unit> ControlledUnits;
    public HashSet<Unit> HasNotActed;
    public int PlayerNo;

    public bool DoesControl( UnitMapHelper unit )
    {
        return unit.PlayerOwner == 0;
    }

    public TurnController( int PlayerNumber, Color color )
    {
        PlayerNo = PlayerNumber;
        var controlledUnits = BattleSystem.Instance.UnitLayer.GetComponentsInChildren<UnitMapHelper>()
           .Where( DoesControl ).ToList();

        foreach ( var unit in controlledUnits )
        {
            unit.GetComponent<UnitGraphics>()
                .UnitIndicator.material.color = color;
        }

        ControlledUnits = new HashSet<Unit>( controlledUnits
            .Select( obj => obj.GetComponent<Unit>() )
            .ToList() );
        HasNotActed = new HashSet<Unit>( ControlledUnits );
    }


    public abstract void Enter( BattleSystem state );
    public abstract void Exit( BattleSystem state );
    public abstract void Update( BattleSystem state );
}

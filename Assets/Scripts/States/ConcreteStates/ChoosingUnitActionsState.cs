﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoosingUnitActionsState : MenuState, IAbilityCreateState
{
    private Unit SelectedUnit;
    private GameTile SelectedTile { get { return sys.Map.UnitGametileMap[ SelectedUnit ]; } }

    public static BattleState Create( Unit selectedUnit)
    {
        return new CancelableState( new ChoosingUnitActionsState( selectedUnit ) );
    }

    private ChoosingUnitActionsState( Unit selectedUnit)
    {
        SelectedUnit = selectedUnit;
    }

    public override void Enter( TurnState context )
    {
        List<Button> buttons = GetButtons( SelectedUnit.Abilities, context ).ToList();
        foreach ( var button in buttons )
        {
            sys.Menu.AddButton( button );
        }

        EventSystem.current.SetSelectedGameObject( buttons.First().gameObject );
    }

    private IEnumerable<Button> GetButtons( IEnumerable<Ability> abilities, TurnState context )
    {
        foreach ( Ability ability in abilities )
        {
            Button button = AbilityButtonFactory.instance.Create( ability );
            button.onClick.AddListener( () => ability.Accept( this, context ) );
            yield return button;
        }
    }

    public void CreateState( TargetAbility ability, TurnState context )
    {
        sys.TurnState = ChooseTargetsState.Create( ability );
    }

    public void CreateState( WaitAbility ability, TurnState context )
    {
        context.GoToStateAndForget( ChoosingUnitState.Instance );
        context.UnitFinished( SelectedUnit );
    }

    public void CreateState( AreaOfEffectAbility ability, TurnState context )
    {
        throw new NotImplementedException();
    }
}
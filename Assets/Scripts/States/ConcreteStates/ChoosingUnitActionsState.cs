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

    public static BattleState Create( Unit selectedUnit )
    {
        return new CancelableState( new ChoosingUnitActionsState( selectedUnit ) );
    }

    private ChoosingUnitActionsState( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
    }

    public override void Enter( PlayerTurnController context )
    {
        List<Button> buttons = GetButtons( SelectedUnit.Abilities, context ).ToList();
        sys.Menu.AddButtons( buttons );
        EventSystem.current.SetSelectedGameObject( buttons.First().gameObject );
    }

    private IEnumerable<Button> GetButtons( IEnumerable<Ability> abilities, PlayerTurnController context )
    {
        foreach ( Ability ability in abilities )
        {
            Button button = AbilityButtonFactory.instance.Create( ability );
            button.onClick.AddListener( () => ability.Accept( this, context ) );
            yield return button;
        }
    }

    public void CreateState( TargetAbility ability, PlayerTurnController context )
    {
        if ( ability.GetInteractableUnits(
            ability.GetTargetPredicate( context ), sys.Map )
            .Count() > 0 )
            context.State = ChooseTargetsState.Create( ability );
    }

    public void CreateState( WaitAbility ability, PlayerTurnController context )
    {
        context.GoToStateAndForget( ChoosingUnitState.Instance );
        context.UnitFinished( SelectedUnit );
    }

    public void CreateState( AreaOfEffectAbility ability, PlayerTurnController context )
    {
        throw new NotImplementedException();
    }
}
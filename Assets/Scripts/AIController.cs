﻿using Assets.General.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class AIController : TurnController
{
    public AIController( int PlayerNumber, Color color ) : base( PlayerNumber, color )
    {
    }

    public class AIAbilityScorer : IAbilityGeneric<int>
    {
        public Vector2Int PositionForCheck;
        public AIController Context;

        public int Visit( WaitAbility ability )
        {
            return 1;
        }

        public int Visit( AreaOfEffectAbility ability )
        {
            return 0;
        }

        public int Visit( TargetAbility ability )
        {
            GameMap map = BattleSystem.Instance.Map;
            IEnumerable<Unit> interactableUnits =
                ability.GetInteractableUnits(
                    map.GetUnitsWithinRange( PositionForCheck, ability.Range ),
                    ability.GetTargetPredicate( Context ) );
            if ( interactableUnits.Count() == 0 )
                return 0;
            else return interactableUnits.Count();
        }
    }

    public override void Enter( BattleSystem state )
    {
        GameMap map = BattleSystem.Instance.Map;
        //Get ordering for Units
        foreach ( var myUnit in ControlledUnits )
        {
            IEnumerable<Vector2Int> unitMovement = map.GetValidMovementPositions( myUnit );
            Unit bestTarget = GetBestTarget( GetEnemies() );
            Vector2Int bestTargetPosition = map.UnitPos[ bestTarget ];
            IEnumerable<Vector2Int> tilesAround = map.GetTilesWithinAbsoluteRange(
                bestTargetPosition,
                ( ( AttackAbility )myUnit.Abilities.First( ability => ability is AttackAbility ) ).Range );
            Vector2Int attackPosition = tilesAround.First();
            IEnumerable<Vector2Int> pathToAttackPosition = MapSearcher.Search( map.UnitPos[ myUnit ], attackPosition, map );
            int pathLength = pathToAttackPosition.Count();

            if ( pathLength > myUnit.MovementRange )
                pathToAttackPosition = pathToAttackPosition.Take( myUnit.MovementRange );

            //Vector2Int bestPosition = unitMovement.Select(
            //    delegate ( Vector2Int pos )
            //    {
            //        AIAbilityScorer scorer = new AIAbilityScorer { PositionForCheck = pos, Context = this };
            //        return new { Score = myUnit.Abilities.Select( ability => ability.Accept( scorer ) ).OrderByDescending( i => i ).First(), Position = pos };
            //    } ).OrderBy( item => item.Score ).First().Position;

            BattleSystem.Instance.CreateMoveCommand( pathToAttackPosition, myUnit ).Execute();
        }
        BattleSystem.Instance.EndTurn();
    }

    private IEnumerable<Unit> GetEnemies()
    {
        return BattleSystem.Instance.Map.UnitPos
            .Where( kvp => ControlledUnits.Contains( kvp.Key ) == false )
            .Select( kvp => kvp.Key );
    }

    private Unit GetBestTarget( IEnumerable<Unit> enemyUnits )
    {
        return enemyUnits.OrderByDescending( unit => unit.HP ).First();
    }

    public override void Exit( BattleSystem state )
    {
    }

    public override void Update( BattleSystem state )
    {
    }
}

public interface IDecisionTreeNode
{
    IDecisionTreeNode MakeDecision();
}

public abstract class Decision<T> : IDecisionTreeNode
{
    public IDecisionTreeNode TrueNode;
    public IDecisionTreeNode FalseNode;
    public T TestValue;
    public abstract bool GetBranch();
    public IDecisionTreeNode MakeDecision()
    {
        if ( GetBranch() )
            return TrueNode == null ? null : TrueNode.MakeDecision();
        else
            return FalseNode == null ? null : FalseNode.MakeDecision();
    }
}

public class CanAttack : Decision<Unit>
{
    public override bool GetBranch()
    {
        //GameTile unitTile = BattleSystem.Instance.Map.UnitGametileMap[ TestValue ];
        return true;
    }
}

public abstract class ActionNode<T> : IDecisionTreeNode
{
    public T ActionValue; 
    public IDecisionTreeNode MakeDecision()
    {
        return this;
    }

    public abstract void Execute();
}

public class DebugAction : ActionNode<object>
{
    public override void Execute()
    {
        Debug.Log( "Hey it's a debug action" );
    }
}
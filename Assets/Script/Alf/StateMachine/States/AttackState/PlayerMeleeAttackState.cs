﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttackState : PlayerAttackState
{
    /*    #region CONTROL VARIABLES
        private bool isFirstAttack;
        #endregion*/

    public float attackCooldownTimer;

    public PlayerMeleeAttackState(PlayerStateMachine stateMachine, Player player, int animCode, D_PlayerStateMachine data) : base(stateMachine, player, animCode, data)
    {
        this.attackCooldownTimer = -1f;
    }

    public override void Enter()
    {
        switch(player.playerRuntimeData.GetCurrentWeaponInfo().weapon){
            case ItemData.Weapon.Iron_Sword:
                animCode = AlfAnimationHash.ATTACK_IRONSWORD;
                break;
            case ItemData.Weapon.Claymore:
                animCode = AlfAnimationHash.ATTACK_CLAYMORE;
                break;
            case ItemData.Weapon.Dragon_Slayer_Sword:
                animCode = AlfAnimationHash.ATTACK_DRAGONSLAYER;
                break;
            default:
                animCode = AlfAnimationHash.ATTACK_IRONSWORD;
                break;
        }
        
        base.Enter();
        combatData.damage = player.CalculatePlayerDamage();
        combatData.stunDamage = data.MAS_stunAmount;
        combatData.knockbackDir = data.MAS_knockbackDirection;
        combatData.knockbackImpulse = data.MAS_knockbackImpulse;
        combatData.from = player.gameObject;
        combatData.isParryDamage = false;
        combatData.facingDirection = player.facingDirection;

        ConsumeAttackBuffer();
    }

public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    protected override void DoCheck()
    {
        base.DoCheck();
    }

    protected override void ResetControlVariables()
    {
        base.ResetControlVariables();
        //isFirstAttack = false;
    }

    protected override void UpdateInputSubscription()
    {
        base.UpdateInputSubscription();

    }

    protected override void UpdatePhysicsStatusSubScription()
    {
        base.UpdatePhysicsStatusSubScription();
    }

    protected override void UpdateStatusSubscription()
    {
        base.UpdateStatusSubscription();
    }

    
    public override void ConsumeAttackBuffer()
    {
        base.ConsumeAttackBuffer();

        isMeleeAttack = false;
        player.InputHandler.ResetIsMeleeAttack();
    }

    public override void CheckHitbox()
    {
        base.CheckHitbox();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(hitbox.position, data.MAS_hitboxRadius, data.GD_whatIsEnemy);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.tag == "Enemy")
            {
                combatData.position = player.transform.position;                
                collider.gameObject.SendMessage("Damage", combatData);
            }
        }
        // consuming MeleeAttack buffer
        // player.InputHandler.ResetIsMeleeAttack();

        //isFirstAttack = !isFirstAttack;
    }

    public override void CompleteAttack()
    {
        base.CompleteAttack();

        ResetTimer();
        
        stateMachine.SwitchState(player.idleState);
    }

    //public override bool CheckEndAttack() => !isMeleeAttack;
    public override void ResetTimer(){
        attackCooldownTimer = data.MAS_attackCooldownTimer;
        if(player.playerRuntimeData.playerSlot.IsWearableEquiped(player.playerRuntimeData.playerStock, ItemData.Wearable.Coldblue_Ring)){
            attackCooldownTimer *= ItemData.WearableItemBuffData.Coldblue_Ring_attackReductionMultiplier;
        }
    }
    public override bool CanAction() => attackCooldownTimer < 0;

    public override void UpdateTimer()
    {
        UpdateAttackCooldownTimer();
    }

    protected void UpdateAttackCooldownTimer()
    {
        if (attackCooldownTimer >= 0)
            attackCooldownTimer -= Time.deltaTime;
    }

}

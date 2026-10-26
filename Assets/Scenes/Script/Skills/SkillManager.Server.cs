// SkillManager.Server.cs
using Unity.Netcode;
using UnityEngine;

public partial class SkillManager : NetworkBehaviour
{
    // ============================================================
    // 主动技能入口（客户端 RPC 调用）
    // ============================================================

    [ServerRpc]
    private void RequestUseSkillServerRpc(SkillRequest request)
    {
        SkillData skill = FindOwnedSkillById(request.skillId);
        if (skill == null)
        {
            Debug.LogWarning($"技能 ID {request.skillId} 不存在");
            return;
        }

        // 只允许主动技能
        if (skill.SkillType != SkillType.Active)
        {
            Debug.LogWarning($"{skill.SkillName} 不是主动技能，拒绝执行");
            return;
        }

        // 验证：是否拥有
        if (!state.Owns(skill))
        {
            Debug.LogWarning($"玩家没有技能 {skill.SkillName}");
            return;
        }

        // 验证：冷却
        bool? onCooldown = state.IsOnCooldown(skill, Time.time);
        if (onCooldown == null)
        {
            Debug.LogWarning("技能冷却数据异常");
            return;
        }
        else if (onCooldown == true)
        {
            Debug.Log($"技能 {skill.SkillName} 冷却中");
            return;
        }

        // 验证：MP
        if (playerStats.CurrentMP < skill.MpCost)
        {
            Debug.Log($"MP 不足");
            return;
        }

        // 验证：Direction 合法性
        if (request.direction.sqrMagnitude < 0.01f)
        {
            Debug.LogWarning($"Direction 无效，拒绝技能释放");
            return;
        }

        // 根据 CastType 构建 Context
        SkillContext context = BuildContextFromRequest(skill, request);
        if (context == null)
        {
            Debug.LogWarning($"构建技能上下文失败，拒绝技能释放");
            return;
        }
        // 汇聚到内部执行层
        ExecuteSkillInternal(skill, context);
    }

    // ============================================================
    // 被动技能入口（由 PassiveSkillManager 调用）
    // ============================================================

    public bool ServerTriggerPassiveSkill(SkillData skill, GameObject target = null, GameObject source = null, float value = 0f)
    {
        if (!IsServer) return false;
        if (skill == null) return false;
        if (skill.SkillType != SkillType.Passive)
        {
            Debug.LogWarning($"{skill.SkillName} 不是被动技能");
            return false;
        }
        if (!state.Owns(skill)) return false;

        // 被动技能不需要 MP 和冷却检查（按你的设计）
        // 如果需要的话可以加

        if(!state.TryGetLevel(skill, out int level))
        {
            Debug.LogWarning($"技能 {skill.SkillName} 等级数据异常");
            return false;
        }
        // 构建 Context
        SkillContext context = new SkillContext
        {
            caster = gameObject,
            target = target,
            source = source,
            castPosition = transform.position,
            baseValue = skill.EffectValue,
            skillLevel =  level,
            damageAmount = value
        };

        // 汇聚到内部执行层
        ExecuteSkillInternal(skill, context);
        return true;
    }

    // ============================================================
    // 内部执行层（主动/被动共用）
    // ============================================================

    private void ExecuteSkillInternal(SkillData skill, SkillContext context)
    {
        if (skill == null || context == null) return;

        // 扣除资源（主动技能才扣，被动技能如果设计有消耗则在这里统一扣，或者由调用方控制）
        if (skill.SkillType == SkillType.Active)
        {
            playerStats.ReduceMP(skill.MpCost);
            state.SetCooldown(skill, Time.time + skill.Cooldown);
        }

        // 执行效果
        foreach (var effect in skill.Effects)
        {
            if (effect != null)
                effect.Execute(context);
        }

        // 增加熟练度（主动/被动都加）
        AddSkillExp(skill, skill.ExpPerUse);

        // 广播表现
        BroadcastSkillEffectsClientRpc(skill.SkillId, context.castPosition, context.aimDirection);
    }

    // ============================================================
    // 辅助方法
    // ============================================================

    private SkillContext BuildContextFromRequest(SkillData skill, SkillRequest request)
    {
        GameObject target = null;

        switch (skill.CastType)
        {
            case SkillCastType.Self:
                target = gameObject;
                break;

            case SkillCastType.Direction:
            case SkillCastType.Target:
                target = GetTargetFromDirection(request.origin, request.direction);
                break;

            case SkillCastType.Area:
                // 预留
                break;
        }
        if(!state.TryGetLevel(skill, out int level))
        {
            Debug.LogWarning($"技能 {skill.SkillName} 等级数据异常");
            return null;
        }
        return new SkillContext
        {
            caster = gameObject,
            target = target,
            castPosition = transform.position,
            aimDirection = request.direction,
            baseValue = skill.EffectValue,
            skillLevel = level
        };
    }

    private GameObject GetTargetFromDirection(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f))
        {
            // 排除自身
            if (hit.collider.gameObject == gameObject) return null;
            return hit.collider.gameObject;
        }
        return null;
    }

    [ClientRpc]
    private void BroadcastSkillEffectsClientRpc(int skillId, Vector3 origin, Vector3 direction)
    {
        // 客户端播放特效、音效、动画
        // 不修改任何游戏状态
    }
}
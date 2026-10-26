using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public partial class SkillManager : NetworkBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (OwnedSkills?.Count == 0) return;

        foreach (var skill in OwnedSkills)
        {
            if (skill == null) continue;
            if (skill.SkillType == SkillType.Passive) continue;

            // 从 SkillBindingManager 读取 技能按键
            SkillKeyBinding binding = SkillBindingManager.Instance.GetBinding(skill.SkillId);
            KeyCode hotkey = binding != null && !binding.IsEmpty() ? binding.primaryKey : KeyCode.None;

            if (hotkey != KeyCode.None && Input.GetKeyDown(hotkey))
            {
                RequestUseSkill(skill);
            }
        }
    }

    private void RequestUseSkill(SkillData skill)
    {
        if (skill == null) return;

        // 本地预检查（为了手感）
        bool? onCooldown = state.IsOnCooldown(skill, Time.time);
        if (onCooldown == null)
        {
            Debug.LogWarning($"技能 {skill.SkillName} 不存在");
            return;
        }
        else if (onCooldown == true)
        {
            Debug.Log($"技能 {skill.SkillName} 冷却中");
            return;
        }

        if (playerStats.CurrentMP < skill.MpCost)
        {
            Debug.Log($"MP 不足");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("没有主相机");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        // 构建请求
        SkillRequest request = new SkillRequest
        {
            skillId = skill.SkillId,
            origin = ray.origin,
            direction = ray.direction
        };

        RequestUseSkillServerRpc(request);
    }
}

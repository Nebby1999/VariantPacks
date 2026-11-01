using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.MoffeinAncientWispSkills.AncientStoneWisp
{
    public class EndFistRain : BaseState
    {
        // Token: 0x0600355B RID: 13659 RVA: 0x000E0A70 File Offset: 0x000DEC70
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            base.PlayAnimation("Body", "EndRain", "EndRain.playbackRate", duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public static float baseDuration = 3f;

        private float duration;
    }
}
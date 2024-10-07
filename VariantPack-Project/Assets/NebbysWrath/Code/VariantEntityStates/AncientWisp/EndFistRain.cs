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
            this.duration = baseDuration / this.attackSpeedStat;
            base.PlayAnimation("Body", "EndRain", "EndRain.playbackRate", this.duration);
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
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
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
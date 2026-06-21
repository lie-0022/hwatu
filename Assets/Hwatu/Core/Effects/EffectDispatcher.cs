using System;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Effects
{
    /// <summary>
    /// 효과 데이터(op)를 받아 전투 상태를 변형한다. 이번 마일스톤은 5종(switch).
    /// 마일스톤2에서 op→핸들러 등록 테이블로 승격해 데이터-주도를 강화한다.
    /// </summary>
    public sealed class EffectDispatcher
    {
        public void Execute(EffectData e, IEffectContext ctx)
        {
            switch (e.Op)
            {
                case EffectOp.DealDamage: DealDamage(e, ctx); break;
                case EffectOp.GainBlock: GainBlock(e, ctx); break;
                case EffectOp.Draw: Draw(e, ctx); break;
                case EffectOp.ApplyStatus: ApplyStatus(e, ctx); break;
                case EffectOp.GainResource: GainResource(e, ctx); break;
                case EffectOp.ClearStatus: ClearStatus(e, ctx); break;
                default: throw new NotSupportedException("Unknown effect op: " + e.Op);
            }
        }

        // 피해 산정(STS 관례 — 각 modifier마다 정수 floor, 순서 고정):
        //   dmg = (기본 + radiance) → Weak면 ×3/4(floor) → Vulnerable이면 ×3/2(floor) → max(0)
        //   이후 Block을 먼저 차감하고 잔여만 HP에 적용한다.
        //   ※ Weak+Vulnerable 동시면 절단이 두 번 일어난다(Slay the Spire와 동일 동작). float·Mathf 금지(결정론 보장).
        private static void DealDamage(EffectData e, IEffectContext ctx)
        {
            int dmg = e.Amount + ctx.Source.GetStatus(StatusType.Radiance);
            if (ctx.Source.GetStatus(StatusType.Weak) > 0)
            {
                dmg = dmg * 3 / 4;
            }
            if (ctx.Target.GetStatus(StatusType.Vulnerable) > 0)
            {
                dmg = dmg * 3 / 2;
            }
            if (dmg < 0)
            {
                dmg = 0;
            }

            int block = ctx.Target.Block;
            if (dmg <= block)
            {
                ctx.Target.SetBlock(block - dmg);
                return;
            }
            ctx.Target.SetBlock(0);
            ctx.Target.SetHp(ctx.Target.Hp - (dmg - block));
        }

        private static void GainBlock(EffectData e, IEffectContext ctx)
        {
            ICombatant who = Resolve(e, ctx);
            int amount = e.Amount + who.GetStatus(StatusType.Dexterity);   // 민첩 가산
            if (amount < 0)
            {
                amount = 0;
            }
            who.SetBlock(who.Block + amount);
        }

        private static void Draw(EffectData e, IEffectContext ctx)
        {
            PileSystem.Draw(ctx.Hand, ctx.DrawPile, ctx.DiscardPile, ctx.ShuffleRng, e.Amount);
        }

        private static void ApplyStatus(EffectData e, IEffectContext ctx)
        {
            ICombatant who = Resolve(e, ctx);
            who.AddStatus(e.Status, e.Amount);
        }

        private static void GainResource(EffectData e, IEffectContext ctx)
        {
            // radiance는 StatusType.Radiance로 통합 라우팅(단일 출처 → Attack 피해가 자동 반영).
            // 그 외 자원(stakes/go/chaff)은 후순위 캐릭터용으로 이번 스코프 미사용.
            if (e.Resource == ResourceType.Radiance)
            {
                ctx.Source.AddStatus(StatusType.Radiance, e.Amount);
            }
        }

        // 대상의 특정 status를 0으로(정화). e.Status로 어떤 상태인지 지정.
        private static void ClearStatus(EffectData e, IEffectContext ctx)
        {
            ICombatant who = Resolve(e, ctx);
            int cur = who.GetStatus(e.Status);
            if (cur != 0)
            {
                who.AddStatus(e.Status, -cur);
            }
        }

        // Self면 Source(시전자), 그 외면 컨텍스트가 정한 Target.
        private static ICombatant Resolve(EffectData e, IEffectContext ctx)
            => e.Target == TargetType.Self ? ctx.Source : ctx.Target;
    }
}

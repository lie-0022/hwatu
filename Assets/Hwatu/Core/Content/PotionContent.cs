using System.Collections.Generic;
using Hwatu.Core.Combat;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Core.Content
{
    /// <summary>포션 정의 + 시드 선택. 전투 중 CombatState에 즉시 효과(self 버프 + 적 대상 전술 혼합, STS2식).</summary>
    public static class PotionContent
    {
        // ── self 버프 / 회복 ──
        public static PotionData Strength() => new PotionData("pot_str", "힘약", "광 +3.",
            s => s.Player.AddStatus(StatusType.Radiance, 3));

        public static PotionData Block() => new PotionData("pot_block", "방패약", "방어 +14.",
            s => s.Player.SetBlock(s.Player.Block + 14));

        public static PotionData Energy() => new PotionData("pot_energy", "기력약", "이번 턴 에너지 +2.",
            s => s.Player.Energy += 2);

        public static PotionData Heal() => new PotionData("pot_heal", "회복약", "HP 20 회복.",
            s => s.Player.SetHp(System.Math.Min(s.Player.MaxHp, s.Player.Hp + 20)));

        public static PotionData Valor() => new PotionData("pot_valor", "강심약", "광 +3, 방어 +8.",
            s => { s.Player.AddStatus(StatusType.Radiance, 3); s.Player.SetBlock(s.Player.Block + 8); });

        public static PotionData Elixir() => new PotionData("pot_elixir", "선약", "HP 40 회복.",
            s => s.Player.SetHp(System.Math.Min(s.Player.MaxHp, s.Player.Hp + 40)));

        // ── 적 대상 전술 ──
        public static PotionData Fire() => new PotionData("pot_fire", "화염병", "적에게 22 피해(방어 무시).",
            s => HitFirstEnemy(s, 22));

        public static PotionData WeakBrew() => new PotionData("pot_weak", "약화병", "적에게 약화 3.",
            s => StatusFirstEnemy(s, StatusType.Weak, 3));

        public static PotionData PoisonVial() => new PotionData("pot_poison", "독병", "적에게 중독 7.",
            s => StatusFirstEnemy(s, StatusType.Poison, 7));

        /// <summary>첫 생존 적에게 즉발 피해(방어·취약 무시 — STS 포션식 고정 피해).</summary>
        private static void HitFirstEnemy(CombatState s, int dmg)
        {
            for (int i = 0; i < s.Enemies.Count; i++)
            {
                if (!s.Enemies[i].IsDead)
                {
                    s.Enemies[i].SetHp(System.Math.Max(0, s.Enemies[i].Hp - dmg));
                    return;
                }
            }
        }

        /// <summary>첫 생존 적에게 status 부여.</summary>
        private static void StatusFirstEnemy(CombatState s, StatusType st, int amt)
        {
            for (int i = 0; i < s.Enemies.Count; i++)
            {
                if (!s.Enemies[i].IsDead)
                {
                    s.Enemies[i].AddStatus(st, amt);
                    return;
                }
            }
        }

        /// <summary>전체 포션 풀(보상/상점 추첨용).</summary>
        public static List<PotionData> All()
        {
            return new List<PotionData>
            {
                Strength(), Block(), Energy(), Heal(), Valor(), Elixir(),
                Fire(), WeakBrew(), PoisonVial(),
            };
        }

        /// <summary>시드로 포션 1개 선택.</summary>
        public static PotionData Pick(IRandom rng)
        {
            List<PotionData> all = All();
            return all[rng.NextInt(all.Count)];
        }
    }
}

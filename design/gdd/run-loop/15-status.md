# 15. 상태 효과 (Status Effects)

## 1. Overview
status 5종: Radiance(빛/힘) · Weak(약화) · Vulnerable(취약) · Poison(중독) · Dexterity(민첩).

## 3. Detailed Rules
- **Radiance**: `deal_damage`에 +Radiance(영구 힘). 빛타격 등 자동 반영.
- **Weak**: 가해자 공격 ×3/4(정수 floor).
- **Vulnerable**: 피해자가 받는 피해 ×3/2(정수 floor).
- **Poison(중독)**: 턴 시작 시 스택만큼 피해(**Block 무시**, 직접 HP) + 그 후 1 감소(`CombatEngine.TickPoison`).
- **Dexterity(민첩)**: `GainBlock`에 +Dexterity(영구 방어 증가, Radiance의 방어판). 예: 철벽.

## 5. Edge Cases
- Poison은 0까지만(음수 HP 없음).
- 플레이어 Poison 사망: `StartPlayerTurn`에서 Lose 전이(입력 안 받음).
- 적 Poison 사망: `EnemyTurn`에서 그 적 행동 스킵.

## 6. Dependencies
`StatusType` · `EffectDispatcher`(Weak/Vuln/Radiance) · `CombatEngine.TickPoison` · `ICombatant`.

## 8. Acceptance Criteria
- [x] Weak/Vuln/Radiance(기존) + Poison 틱·감소·사망 처리 — `PoisonTests` 2종(전체 66/66).

## 후속
Poison 부여 카드/적(화투 테마 고려) · Strength/Dexterity · 지속 효과(Power) · status UI(아이콘/스택 표시).

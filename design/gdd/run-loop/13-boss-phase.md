# 13. 보스 페이즈 (Phase AI)

## 1. Overview
보스가 HP 임계(50%) 미만으로 떨어지면 광폭화 — 더 공격적인 AiOrder(phase2)로 전환.

## 3. Detailed Rules
- `IEnemyAi.PeekNext(EnemyState self)`: self의 HP로 페이즈 분기(완전정보, 멱등).
- `PhaseAi(data, phase1, phase2, threshold=50)`: `Hp*100 <= MaxHp*threshold`면 **enrage 래치**(되돌아가지 않음) + index 리셋 → phase2 첫 수부터.
- `EnemyData.SecondPhaseOrder`(optional) + `AiKind.Phase`. `CombatFactory`가 Phase+phase2 있으면 `PhaseAi`, 아니면 `SequenceAi`.
- `SequenceAi`는 self 무시(HP 무관). `CombatEngine`은 실행 시 `Ai.PeekNext(enemy)` 사용.

## 4. 달그림자 도깨비(보스)
- moves: 강타13 / 광폭9 / 방벽12 / **월식18**.
- phase1 `[강타,광폭,방벽]` / phase2(HP<50%) `[월식18,강타,광폭]` — 방어 버리고 월식 추가.

## 5. Edge Cases
- enrage 래치: HP가 (이론상) 회복해도 phase2 유지.
- PeekNext 멱등: Advance 전 여러 번 호출해도 같은 move.

## 6. Dependencies
`IEnemyAi`/`PhaseAi`/`SequenceAi` · `EnemyData.SecondPhaseOrder`/`AiKind` · `CombatFactory` · `CombatEngine.PeekNext(enemy)` · `EnemyState`.

## 8. Acceptance Criteria
- [x] phase1/2 전환·래치·멱등 — `PhaseAiTests` 4종(전체 60/60).

## 후속
intent 광폭 색/연출 · 다단 페이즈(3+) · 페이즈 진입 1회 버프(Strength) · 잡몹 Conditional/WeightedRandom AI.

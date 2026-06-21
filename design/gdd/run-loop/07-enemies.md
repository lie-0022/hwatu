# 07. 적 콘텐츠 (Enemies)

## 1. Overview
노드 타입별 적 풀 + 시드 선택. 일반 3종 / 엘리트 1 / 보스 1.

## 3. Detailed Rules
- **일반 풀**: 잡도깨비(공7/방6) · 까마귀떼(다회 약공 2×3 / 방4) · 허수아비(Weak 2 디버프 / 공5).
- **엘리트**: 광귀(강타11 / Vulnerable 2 / 방벽10).
- **보스**: 달그림자 도깨비(강타13 / 광폭9 / 방벽12).
- **선택**: `EnemyContent.PickEnemy(NodeType, IRandom)` — 보스·엘리트 고정, 일반은 RNG로 3종 중.
- **다회공격** = `Effects`에 DealDamage 여러 개(까마귀떼 3회). **디버프** = `ApplyStatus`(target=Enemy→실제 플레이어), Weak/Vulnerable은 EffectDispatcher에서 효과 적용됨.

## 6. Dependencies
`StarterContent`(적 정의) · `EnemyContent`(선택) · `RunUI.StartCombatNextFrame`(전투 진입 시 PickEnemy) · `SequenceAi`.

## 7. Tuning Knobs
적별 HP/패턴 · 풀 구성 · 일반/엘리트 분류.

## 8. Acceptance Criteria
- [x] 노드 타입별 적 선택(보스/엘리트 고정, 일반 결정론) · 다회/디버프 동작. `EnemyContentTests` 3종 — 전체 45/45 통과.

## 후속
적 추가 · AI 다양화(랜덤/조건부) · intent 다회 표시(EnemyMoveData에 Hits 필드) · 적 스프라이트.

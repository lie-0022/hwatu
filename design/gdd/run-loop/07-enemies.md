# 07. 적 콘텐츠 (Enemies)

## 1. Overview
노드 타입별 적 풀 + 시드 선택. 일반 8종 / 엘리트 3 / 보스 3(자율 확장).

## 3. Detailed Rules
- **일반 풀(8)**: 잡도깨비(공7/방6) · 까마귀떼(다회 약공 2×3 / 방4) · 허수아비(Weak 2 / 공5) · 도깨비불(scald 중독3) · 장승 · 그슨대 · 멧돼지(씩씩대기 광+2→들이받기 공10/강화12) · 두꺼비(웅크리기 방어10→독침 공5+중독2).
- **엘리트(3)**: 광귀(강타11 / Vulnerable 2 / 방벽10) · 외눈도깨비 · 구렁이.
- **보스(3)**: 달그림자 도깨비(강타13 / 광폭9 / 방벽12, PhaseAi) · 구미호(Doom 카운트다운) · 장군(rally 자버프).
- **선택**: `EnemyContent.PickEnemy(NodeType, IRandom)` — 보스·엘리트 RNG 3종, 일반 RNG 7종.
- **다회공격** = `Effects`에 DealDamage 여러 개(까마귀떼 3회). **디버프** = `ApplyStatus`(target=Enemy→실제 플레이어), Weak/Vulnerable은 EffectDispatcher에서 효과 적용됨.

## 6. Dependencies
`StarterContent`(적 정의) · `EnemyContent`(선택) · `RunUI.StartCombatNextFrame`(전투 진입 시 PickEnemy) · `SequenceAi`.

## 7. Tuning Knobs
적별 HP/패턴 · 풀 구성 · 일반/엘리트 분류.

## 8. Acceptance Criteria
- [x] 노드 타입별 적 선택(보스/엘리트/일반 RNG) · 다회/디버프/Doom 동작. `EnemyContentTests` — 전체 133/133 통과.

## 후속
적 추가 · AI 다양화(랜덤/조건부) · intent 다회 표시(EnemyMoveData에 Hits 필드) · 적 스프라이트.

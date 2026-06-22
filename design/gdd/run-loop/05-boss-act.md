# 05. 보스 + 액트 진행

## 1. Overview
보스 전투(페이즈/인텐트)와 액트 진행. MVP는 1액트, 보스=최종.

## 2. Player Fantasy
맵 끝에서 강한 보스를 만나 한 판을 마무리(또는 다음 액트로).

## 3. Detailed Rules
- **보스**: 맵 최상위 노드, 강한 적. MVP `달그림자 도깨비`(HP 45~55, 강타13/광폭9/방벽12 sequence).
- 보스 처치 → **Victory**(MVP 액트1=최종). 후속: 보스 유물 3택1 + 다음 액트.
- **액트**: MVP 1액트. 후속 2~3 + 전환(부족분 HP 회복 + 새 맵 생성).
- **보스 페이즈(STS 원형)**: ①임계값 모드전환(Guardian) ②0HP 부활(Awakened One) ③HP% 강화(Champ) ④행동제약(Time Eater/Corrupt Heart). MVP는 단순 sequence.

## 5. Edge Cases
- 보스 전투 패배 → GameOver.
- HP 전투 간 유지(보스전 직전 행14 휴식으로 회복 가능).

## 6. Dependencies
`StarterContent.DokkaebiBoss` · `GameFlow.OnCombatEnded`(보스→Victory) · `RunUI`의 보스 적 분기(`CurrentNodeIsBoss`).

## 7. Tuning Knobs
보스 HP/패턴 · 액트 수 · 액트 전환 회복률(STS 부족분 100%, Asc5+ 75%).

## 8. Acceptance Criteria
- [x] 보스 노드 도달 → 보스 적 전투 → 승리=Victory / 패배=GameOver. (컴파일)

## 후속
보스 페이즈 패턴(PhaseTrigger 데이터화) · 액트 2~3 + ActTransition(회복+새 맵) · 보스 유물 3택1 · 인텐트 아이콘 다양화(소환/디버프 등).

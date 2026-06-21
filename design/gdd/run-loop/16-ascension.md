# 16. 어센션 (난이도)

## 1. Overview
어센션(0~5) = 난이도 모디파이어(STS Ascension). 클수록 적이 강하고 시작 HP 페널티.

## 3. Detailed Rules
- `AscensionRules.EnemyHpPercent(asc)`: `100 + 5%×asc`. `CombatFactory`가 적 HP 롤에 곱(정수 floor).
- `AscensionRules.StartHpPenalty(asc)`: `-3×asc`(플레이어 시작 최대 HP).
- `RunState.Ascension`. `CombatFactory.CreateCombat(..., ascension)`.

## 6. Dependencies
`AscensionRules` · `CombatFactory` · `RunState.Ascension`.

## 8. Acceptance Criteria
- [x] HP% 스케일·페널티·CombatFactory 적용 — `AscensionTests` 3종(전체 69/69). 난이도 선택 UI + GameFlow 연결은 후속.

## 후속
난이도 선택 UI(메인) · GameFlow StartNewRun 시 페널티 적용 + CombatFactory에 `Run.Ascension` 전달 · asc별 추가 효과(엘리트 빈도↑·보스 강화·회복 감소) · 클리어 시 다음 어센션 언락.

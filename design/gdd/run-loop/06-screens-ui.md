# 06. 화면 UI (Screens)

> 현재 구현 기록 + Play 검증 지점. 화면은 코드 생성(uGUI), 한 판 루프 화면 조율은 `RunUI`.

## 1. Overview
`RunUI`가 `GameFlow.OnPhaseChanged`를 구독해 패널(메인/캐릭/맵/보상/결과)과 전투 화면을 토글한다.

## 3. Detailed Rules
- **RunUI**: 자체 Canvas(sortOrder 0) + 5패널 동적 생성. 페이즈별 SetActive.
- **전투 GO**: `CombatController`+`CombatView` 동적 생성(초기 SetActive false). Combat 페이즈에 활성 + **코루틴으로 다음 프레임** StartCombat(런 덱·HP) → `CombatView.SetVisible(true)`·`Refresh()`.
- **보상 패널**: `RewardSystem`으로 3장 → 레어도색 버튼 → 선택 시 `RunState.AddCard` / 스킵.
- **맵**: `MapView` 노드 버튼 + 간선 선, 진입 가능 노드만 활성.

## 5. Edge Cases — ⚠️ Play로만 검증 가능 (이 브랜치 미검증)
- 전투 GO `Awake`(_autoStart=true 데모 NewCombat) → 코루틴 `StartCombat`이 덮어씀(데모 1프레임 깜빡 가능, 무해 예상).
- `CombatView.Start`(BuildUI) 타이밍 → 코루틴 `yield return null`로 1프레임 대기 후 StartCombat.
- RunUI Canvas vs 전투 GO 자체 Canvas의 sortOrder/표시 충돌 여부.
- EventSystem 단일 공유(EnsureEventSystem 중복 호출 가드).

## 6. Dependencies
`GameFlow` · `CombatController`/`CombatView` · `MapView` · `RewardSystem` · `LuminaryCards`.

## 8. Acceptance Criteria
- [ ] 메인부터 한 바퀴 화면 전환이 끊김 없이(전투 시작/종료 포함) — **Play 체감 검증 대기**.

## 후속
상점/이벤트/보물 화면 · 덱 보기 · 더미 열람 · 맵 스크롤 · 화면 전환 트랜지션.

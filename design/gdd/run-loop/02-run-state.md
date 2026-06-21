# 02. RunState + GameFlow (런 상태 & 화면 전환)

## 1. Overview
한 런의 상태(`RunState`)와 화면 전환 중추(`GameFlow`). GameFlow가 페이즈를 바꾸면 화면 View들이 이벤트를 받아 자기 화면을 토글한다.

## 2. Player Fantasy
플레이어 입장에선 "메뉴 → 캐릭터 고름 → 맵에서 길 선택 → 전투 → 보상 → 다시 맵"이 끊김 없이 이어진다.

## 3. Detailed Rules — 상태머신
```
MainMenu ──GoToCharacterSelect──▶ CharacterSelect ──StartNewRun(ch)──▶ Map
Map ──EnterNode(전투/엘리트/보스)──▶ Combat
Map ──EnterNode(휴식/상점/이벤트/보물)──▶ (MVP stub) Map
Combat ──OnCombatEnded(승, 보스아님)──▶ Reward ──OnRewardDone──▶ Map
Combat ──OnCombatEnded(승, 보스)──▶ Victory
Combat ──OnCombatEnded(패)──▶ GameOver
(언제든 HP 0 → GameOver)
```

## 4. 데이터 (`Hwatu.Core.Run`)
- `RunState`: Character, Deck(List<CardData>), MaxHp/Hp, Gold, Seed, Act, Map, CurrentNodeId(-1=진입전), RareOffset(보상 피티).
- `CharacterData`: Id/Name/StartMaxHp/StartGold/StartingDeck. `Luminary()` = 광객(HP80·골드99·덱10).
- `GameFlow`(Game): RunState 보유 + RunPhase + `OnPhaseChanged` 이벤트 + 노드/전투/보상 전이 API.

## 5. 전투 연동
- `CombatFactory.CreateCombat(deck, enemyData, seed, maxHp, hp)` — RunState.Deck/HP로 전투 조립.
- 전투 종료 → `GameFlow.OnCombatEnded(won)` → RunState.Hp 갱신(전투 결과 반영) → Reward/GameOver/Victory.

## 6. Edge Cases
- HP 0은 어느 전투에서든 GameOver(글로벌).
- 보스 노드 승리 → Victory(MVP는 액트1 보스=최종). 후속: 다음 액트 ActTransition.
- 맵 진입 전 CurrentNodeId=-1 → 행0 시작 노드만 선택 가능.

## 7. Tuning Knobs
- 시드(`GameFlow._seed`) — 결정론 재현/랜덤.
- 액트 수(MVP 1).

## 8. Acceptance Criteria
- [ ] 메인→캐릭터→맵→전투→보상→맵 전이가 이벤트로 정확히 발생.
- [ ] 전투 결과(HP/승패)가 RunState에 반영되고 다음 전투로 이어짐.
- [ ] 보스 승리=Victory, HP0=GameOver.

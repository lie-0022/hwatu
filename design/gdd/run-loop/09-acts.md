# 09. 액트 진행 (Acts)

## 1. Overview
보스 처치 후 다음 액트(회복 + 새 맵) 또는 마지막 액트면 승리.

## 3. Detailed Rules
- `FinalAct = 3`. 보스 승리 → 보상(Rare 3택1) → `OnRewardDone`: `Act >= FinalAct`이면 Victory, 아니면 `AdvanceAct`.
- `AdvanceAct`: `Act++`, 부족분 HP **100% 회복**(STS; Asc5+ 75%는 후속), 새 맵 생성(시드 `ForStream("map_act"+Act)`).

## 5. Edge Cases
- 보스 노드도 일반과 같이 Reward를 거침(보스 보상=Rare). 액트 진행 판정은 OnRewardDone에서 노드 타입으로.

## 6. Dependencies
`GameFlow.OnCombatEnded/OnRewardDone/AdvanceAct` · `RunState.Act/Heal` · `MapGenerator`.

## 8. Acceptance Criteria
- [x] 보스→보상→(다음 액트 or 승리) 전이 + 액트별 맵 재생성(컴파일). Play 체감 검증 대기.

## 후속
액트별 적/보스 풀 차등 · 바이옴 분기(STS2 알터네이트 액트) · 난이도 상승 · 액트 전환 연출.

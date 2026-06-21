# 19. 포션 (Potions)

## 1. Overview
포션 = 일회용 소모품(전투 중 즉시 효과). STS 보상 포션 피티(40%).

## 3. Detailed Rules
- `PotionData`(전투 중 `Action<PlayerState>` 효과). `PotionContent.Pick(rng)` 시드 선택.
- 3종: 힘약(광 +2)·방패약(방어 +12)·민첩약(민첩 +2).

## 6. Dependencies
`PotionData`/`PotionContent` · `PlayerState`(AddStatus/SetBlock). 보유 슬롯(RunState.Potions)·사용 UI는 후속.

## 8. Acceptance Criteria
- [x] 포션 효과·결정론 — `PotionTests` 4종(전체 104/104). 슬롯3·전투 사용·보상 등장은 후속.

## 후속
RunState.Potions 슬롯3 · 전투 중 사용 UI · 보상 포션 피티 40% · 포션 풀 확장(약화약·회복약·해독약).

# 03. 카드 보상 (Card Reward)

> 출처: [Card Rarity — Spire Codex](https://spire-codex.com/mechanics/card-rarity), [Card Rewards — Fandom](https://slay-the-spire.fandom.com/wiki/Card_Rewards).

## 1. Overview
전투 후 카드 3장을 제시해 1장 선택(또는 스킵). 레어도는 밴드 + 피티 오프셋으로 정한다.

## 2. Player Fantasy
"이번엔 뭘 넣을까?" — 덱을 키우면 희석되니, 좋은 카드만 고르고 때론 스킵(덱 압축)하는 게 전략.

## 3. Detailed Rules
- 3장 제시, 1 선택 또는 **스킵**. 한 보상 안 3장 **중복 없음**.
- 레어도 기본: 일반 Rare 3% / Uncommon 밴드 37%, 엘리트 10% / 40%, **보스 Rare 확정**.
- **피티 오프셋**(RunState.RareOffset): 시작 -5, Rare가 아니면 +1(최대 +40), Rare를 뽑으면 -5 리셋. **카드 1장 생성 직후마다** 갱신 → 3장이면 3번 조정.

## 4. Formulas
```
rareChance = max(0, baseRare + rareOffset)
roll = rng.NextInt(100)
roll < rareChance                  → Rare
roll < rareChance + uncommonBand   → Uncommon
else                               → Common
nextOffset = (rolled==Rare) ? -5 : min(40, offset+1)
```

## 5. Edge Cases
- rareChance가 음수면 0으로 바닥(일반 전투 시작 3-5=-2 → 0%).
- 풀에 해당 레어도 미사용 카드가 없으면 → 미사용 카드 아무거나 폴백.
- 보스는 RollRarity가 Rare 확정.

## 6. Dependencies
- `IRandom`, `CardData.Rarity`, `LuminaryCards.RewardPool`(7장), `RunState.RareOffset`.
- 소비처: 보상 화면(3택1 UI) → `RunState.AddCard`.

## 7. Tuning Knobs
- baseRare(일반 3 / 엘리트 10), uncommonBand(37 / 40), 오프셋 증가(+1)·캡(40)·리셋(-5).
- 카드 풀 구성(레어도별 장수).

## 8. Acceptance Criteria
- [x] 보스 Rare 확정 / 3장 중복 없음 / 같은 시드 동일 결과(결정론) / 피티 작동(Rare가 가끔, 과하지 않게).
- [x] RewardSystemTests 5종 통과(전체 42/42).

# 17. 캐릭터 (Characters)

## 1. Overview
플레이 캐릭터 2종: 광객(빛/공격) · 묵귀(독/약화). 시작 HP·덱·보상 풀이 다르다.

## 3. Detailed Rules
- `CharacterData`(Id/Name/StartMaxHp/StartGold/StartingDeck).
- **광객**: HP 80, 덱 빛타격×5/방패×4/점화×1. 보상 `LuminaryCards`(16장, radiance 빌드).
- **묵귀**: HP 70, 덱 그림자칼×5/그늘×4/옻칠×1. 보상 `InkCards`(9장, Poison/Weak 빌드).
- `CharacterPools.RewardPool(characterId)`: 캐릭터 id로 보상 풀 선택.

## 4. 묵귀 카드(11종)
그림자칼·그늘·옻칠·먹칼·그을음·암막·침습·먹구름·흑무·역병·부패. **독(Poison) 중심** + 약화/취약.

## 6. Dependencies
`CharacterData` · `InkCards`/`LuminaryCards` · `CharacterPools` · `RunState.Character`.

## 8. Acceptance Criteria
- [x] 묵귀 HP70·덱10·풀9 + 캐릭터별 풀 — `InkSpiritTests` 3종(전체 79/79). 캐릭터 선택 UI + RunUI 풀 연결은 후속.

## 후속
캐릭터 선택 UI(메인) · GameFlow/RunUI가 `CharacterPools.RewardPool(Character.Id)` 사용 · 캐릭터 고유 유물/패시브 · 3~5번째 캐릭터.

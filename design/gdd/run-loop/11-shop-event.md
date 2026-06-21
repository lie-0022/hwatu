# 11. 상점 / 이벤트 노드

## 1. Overview
상점(카드/유물 구매·카드 제거) + 이벤트(선택지). Core 경제 헬퍼 완료, UI는 후순위.

## 3. Detailed Rules
- **상점**: `RunState.TrySpend`(골드 차감) + `AddCard`/`AddRelic`/`RemoveCard`. 가격(카드 레어도별, 후속 ShopSystem로 재고·가격).
- **유물 보상**: `RewardSystem.RollRelicReward(pool)` — 보물/엘리트/보스에서 유물 획득.
- **이벤트**: 선택지 데이터 + 효과(HP/골드/카드 추가·제거) — 후속.

## 6. Dependencies
`RunState.{TrySpend, AddRelic, AddCard, RemoveCard}` · `RewardSystem.RollRelicReward` · `RelicContent.AllRelics`.

## 8. Acceptance Criteria
- [x] 경제 헬퍼(TrySpend/AddRelic/RemoveCard) + 유물 보상 롤 — `ShopEconomyTests` 4종(전체 56/56). 상점·이벤트·보상 유물 UI는 Play 게이트 이후.

## 후속
상점 UI(재고·가격·구매·제거) · 이벤트 데이터+UI(선택지) · 보물 노드 유물 보상 화면 · 엘리트/보스 유물 드롭 · 골드 표시.

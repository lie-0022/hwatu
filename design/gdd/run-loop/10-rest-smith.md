# 10. 휴식 Smith (카드 업그레이드)

## 1. Overview
휴식처에서 회복(완료) 또는 카드 업그레이드 선택.

## 3. Detailed Rules
- `CardData.Upgrade()`: DealDamage/GainBlock 수치 +3, 이름·Id에 `+`(MVP; 카드별 세부 강화는 후속).
- `RunState.UpgradeCard(index)`: 덱의 해당 카드를 업그레이드 버전으로 교체.
- 휴식 노드: **회복**(최대 HP 30%, 현재) vs **Smith**(카드 1장 강화) 택1 — UI는 후순위.

## 6. Dependencies
`CardData.Upgrade` · `RunState.UpgradeCard` · GameFlow 휴식 노드.

## 8. Acceptance Criteria
- [x] Upgrade 수치/이름(`CardUpgradeTests` 3종, 전체 52/52). 휴식 UI(회복 vs 강화 + 덱 선택)는 Play 게이트 이후.

## 후속
휴식 UI(회복/강화 선택 + 덱 카드 고르기) · 카드별 업그레이드 데이터(코스트 감소 등) · 보상 카드가 강화된 채 등장.

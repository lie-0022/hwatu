# 18. 카드 키워드 (Keywords)

## 1. Overview
카드 키워드: Exhaust(소멸) · Retain(보유). 덱빌딩 깊이를 더한다.

## 3. Detailed Rules
- **Exhaust(소멸)**: 사용 시 버림 더미 대신 소멸 더미로(전투 중 재사용 불가). `CardData.Exhaust`. 예: 백광·각성.
- **Retain(보유)**: 턴 종료 시 버리지 않고 손패 유지(`CombatEngine.EndPlayerTurn`). `CardData.Retain`. 예: 수호.
- **Innate(선천)**: 전투 첫 손패에 보장(`StartCombat`에서 셔플 후 더미 맨 위로). `CardData.Innate`. 예: 서광.
- `CardData.Upgrade`는 세 키워드를 그대로 전파.

## 6. Dependencies
`CardData.Exhaust/Retain` · `CombatEngine`(PlayCard→ExhaustPile / EndPlayerTurn→Retain 유지).

## 8. Acceptance Criteria
- [x] Exhaust→소멸 더미(`ExhaustTests`) · Retain→손패 유지(`RetainTests`) — 전체 81/81.

## 후속
Innate(전투 첫 손패 보장) · Ethereal(턴 끝 소멸) · Unplayable · 키워드 카드 텍스트 UI 표시.

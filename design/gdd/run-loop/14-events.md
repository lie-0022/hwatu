# 14. 이벤트 노드 (Events)

## 1. Overview
이벤트(物음표) 노드 = 선택지로 RunState 변경(회복/골드/최대HP 등). Core 완료, UI는 후속(J).

## 3. Detailed Rules
- `EventData`(제목/설명/선택지[]). `EventChoice`(라벨 / 결과 문구 / `Action<RunState>` 효과).
- `EventContent.Pick(rng)`: 시드로 1개 선택. `All()`: 달빛 옹달샘 · 도깨비의 거래.
- **달빛 옹달샘**: HP +12 / 골드 +25. **도깨비의 거래**: 골드 -30 → 최대 HP +8 / 거절.

## 5. Edge Cases
- 골드 부족 시 거래 선택은 `TrySpend` 실패로 효과 없음(차감도 안 함).
- HP 회복은 최대치 초과 안 함.

## 6. Dependencies
`EventData`/`EventContent` · `RunState`(Heal/TrySpend, Gold·MaxHp public set).

## 8. Acceptance Criteria
- [x] 선택지 효과·결정론 — `EventContentTests` 4종(전체 64/64). 이벤트 UI 연결은 J(Play 게이트).

## 후속
이벤트 UI(RunUI 패널) · 전투 이벤트(적 조우) · 카드 추가/제거 이벤트 · 위험 선택(HP 도박) · 이벤트 풀 확장 · 유물 보상 이벤트.

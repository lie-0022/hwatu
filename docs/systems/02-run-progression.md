# 02. 런 진행

> `Hwatu.Core.Run` + `Hwatu.Game`(GameFlow/RunUI/MapView). 설계: `design/gdd/run-loop/01·02·05·09·10·11·14·16`.

## 현황

- **GameFlow** (`RunPhase` 상태머신): MainMenu → CharacterSelect → Map → Combat → Reward/BossReward → Event/Rest/Shop → GameOver/Victory.
- **맵**: `MapGenerator` 7×15 그리드 · 분기(`NextIds`) · 보스. `MapView` 노드 버튼 → 현재 위치에서 **도달 가능한 노드만 활성** → `EnterNode`.
- **노드 7종**: Combat/Elite/Boss(전투) · Rest(회복/강화) · Treasure(유물) · Event(선택지) · Shop(카드 구매).
- **보상**: 카드 3택1(피티 오프셋) + 포션(40% 피티). `RewardSystem`.
- **액트 3**: 보스 클리어 → 다음 액트(부족분 회복). **2막+ 적 HP ×1.25씩**(EnemyContent act 오버로드).
- **경제**: 골드 `TrySpend`, 상점가 레어도별(Rare150/Uncommon75/기본50).
- **RunState**: Deck · HP · Gold · Relics · Potions · Act · Map · RareOffset · Ascension.

## STS 비교

| STS | 화투 | 상태 |
|-----|------|------|
| 맵 분기·경로 선택 | 7×15 NextIds | ✅ |
| 노드 타입(전투/엘리트/보스/휴식/상점/이벤트/보물) | 7종 | ✅ |
| 보상 3택1 + 스킵 | ✅ | |
| 액트 전환·회복 | 3액트 + act HP 스케일 | ✅ |
| 캠프파이어(휴식/강화) | 회복/강화 택1 | ✅ |

## 갭 / 계획

- 맵에서 **노드 타입 시각 구분**(전투/상점/휴식 아이콘) 명확성 점검 → [04-ui-ux](04-ui-ux.md).
- 어센션 단계화 · 엽전(보상 골드 증가) 실효화는 후속(보류).

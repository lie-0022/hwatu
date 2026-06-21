# 화투 로그라이크 덱빌더 — 게임 기획·구현 스펙

> **이 문서의 목적**: AI 코딩 에이전트(Claude Code)가 이 게임의 프로토타입을 구현할 수 있도록 만든 단일 컨텍스트 문서입니다.
> 한국어 설명 + 영어 식별자(코드용) 혼용. 레퍼런스는 *Slay the Spire 2*(이하 STS2).
> **최우선 지시**: 9장(MVP 범위)을 먼저 구현한다. 그 외는 `OUT OF SCOPE`로 미룬다.

---

## 0. AGENT NOTES (먼저 읽기)

- **장르**: 싱글플레이어 턴제 로그라이크 덱빌더 (STS2 클론, 화투 테마).
- **엔진(기본)**: Unity(C#). 단, 데이터 모델은 엔진 중립적으로 기술했으니 Godot/웹(TS)로도 이식 가능. 빠른 검증만 목적이면 웹(TypeScript) 스택이 가장 빠르게 스캐폴딩됨.
- **구현 철학**:
  1. **데이터-주도(data-driven)**: 카드/적/유물/효과는 코드가 아니라 데이터로 정의(§4). 콘텐츠 추가가 코드 수정 없이 가능해야 함.
  2. **효과는 원자 단위 조합(§5.1)**. 카드 = 효과 리스트.
  3. **시드 기반 결정론적 RNG(§3.4)**. 같은 시드 → 같은 런.
  4. **UI보다 전투 루프 먼저**. 단, 카드게임은 체감상 80%가 UI.
- **완료 정의**: §11 마일스톤 + 각 마일스톤의 "완료 기준"을 만족하면 그 단계 완료.

---

## 1. 프로젝트 개요

| 항목 | 값 |
|---|---|
| 코드네임 | `hwatu-spire` (가칭) |
| 한 줄 설명 | 화투(광·열끗·띠·피)를 자원으로 쓰는 로그라이크 덱빌더 |
| 레퍼런스 | Slay the Spire 2 (턴제, 완전정보, 노드 맵, 유물) |
| MVP 목표 | 캐릭터 1명으로 "한 판"이 끝까지 도는 수직 슬라이스 |
| MVP 성공 기준 | §11.6 (한 판 더 누르고 싶은가 / 콤보 쾌감 / 패배=내 실수) |

---

## 2. 핵심 설계 원칙 (DESIGN PILLARS — 어기지 말 것)

> STS2의 재미를 분해한 결과. 구현·밸런싱 시 이 원칙을 우선한다.

1. **콤보 폭발의 쾌감 보존**. 플레이어가 조립한 덱이 한 턴에 터지는 순간이 이 장르의 심장이다. *콤보를 과도하게 너프하지 말 것.* (STS2는 이걸 너프했다 평점 폭락.)
2. **완전 정보(perfect information)**. 적의 다음 행동(intent)과 예상 피해를 항상 공개. 드로우/버린/소멸 더미 열람 가능. → 패배가 "운"이 아니라 "내 판단"이 되게 한다.
3. **의미 있는 리스크-리워드 반복**. 보상은 항상 트레이드오프와 함께. 매 선택이 저울질이 되도록.
4. **판마다 다른 변주**. 경로·카드·적·유물 무작위. 자동조종(autopilot) 방지.
5. **발견의 여지**. 의도치 않은 시너지가 터지도록 카드 상호작용을 열어둔다.

---

## 3. 게임 루프 명세

### 3.1 런(run) 흐름
```
캐릭터 선택
 → ACT 시작: Ancient(달의 신) 축복 3택1   [MVP: 생략 가능, §12]
 → 노드 맵 이동 (전투/엘리트/이벤트/상점/휴식/보물/보스)
 → 각 노드 해결 → 보상
 → ACT 보스 처치 → HP 회복 + 보상
 → 다음 ACT 반복
HP 0 → 런 종료(사망), 처음부터.
```

### 3.2 전투 턴 순서 (상태머신 — 이 순서를 정확히 구현)
```
COMBAT_START
  - 시작 유물/파워 트리거
  - drawPile = shuffle(deck, seed)
PLAYER_TURN_START
  - energy = baseEnergy (기본 3)
  - draw(handSize)            # 기본 5
  - 턴 시작 status 처리
PLAYER_ACTION (반복)
  - 카드 1장 플레이: 코스트 차감 → effects 순차 실행 → 카드를 discard/exhaust로 이동
  - 또는 "턴 종료" 선택
PLAYER_TURN_END
  - 남은 손패 처리(기본 discard), 턴 종료 status 처리(독 등)
ENEMY_TURN
  - 각 적이 예고된 intent 실행
  - 다음 intent 계산·표시
CHECK_DEATH (적 전멸 → COMBAT_WIN / 플레이어 HP0 → DEAD)
→ PLAYER_TURN_START 로 루프
COMBAT_WIN → 보상(카드 3택1, 골드, [유물])
```

### 3.3 전투 규칙 (수치 baseline — STS2/STS1 기준)
- 기본 에너지/턴: **3**. 손패 드로우/턴: **5**.
- **Block**은 턴 시작 시 0으로 초기화(특수 status 제외). 피해는 Block 먼저 소모 후 HP.
- 카드 타입: `Attack` / `Skill` / `Power`(지속). 상태카드 `Status`(적이 덱에 삽입).
- 덱 소진 시 discardPile를 셔플해 drawPile 재생성.

### 3.4 RNG
- 단일 마스터 시드 → 하위 스트림 분리(map, combatShuffle, cardReward, events). `seededRandom(stream)`.
- 목적: 버그 재현 + 밸런스 테스트 + (후일) 시드 공유.

---

## 4. 데이터 모델 (스키마)

> JSON으로 표기(엔진 중립). Unity는 ScriptableObject, Godot는 Resource로 1:1 매핑.

### 4.1 Card
```jsonc
{
  "id": "luminary_light_strike",
  "name": "빛타격",
  "type": "Attack",            // Attack | Skill | Power | Status | Curse
  "characterClass": "luminary",// 또는 "neutral"
  "rarity": "Basic",           // Basic | Common | Uncommon | Rare
  "cost": 1,                   // 정수 또는 "X"
  "target": "enemy",           // enemy | all_enemies | self | none
  "keywords": [],              // §5.2 키워드 id 배열
  "effects": [                 // 플레이 시 순서대로 실행 (§5.1)
    { "op": "deal_damage", "amount": 6 }
  ],
  "upgrade": {                 // 강화 시 덮어쓸 필드
    "effects": [{ "op": "deal_damage", "amount": 9 }]
  },
  "enchant": null,             // §5.4
  "exhaust": false,
  "art": "placeholder"
}
```

### 4.2 Enemy + Intent
```jsonc
{
  "id": "dokkaebi_minion",
  "name": "잡도깨비",
  "maxHp": [12, 16],           // [min,max] 범위 롤
  "moves": [
    { "id": "swipe", "intent": "attack", "value": 7,
      "effects": [{ "op": "deal_damage", "amount": 7, "target": "player" }] },
    { "id": "guard", "intent": "defend",
      "effects": [{ "op": "gain_block", "amount": 6, "target": "self" }] }
  ],
  "ai": { "type": "sequence", "order": ["swipe", "swipe", "guard"] }
  // ai.type: sequence | weighted_random | conditional
}
```
- `intent` enum: `attack | attack_multi | block | buff | debuff | summon | doom | unknown`. UI는 이 값으로 아이콘 + (attack이면) 예상 피해 표시.

### 4.3 Relic
```jsonc
{
  "id": "bangseok",            // 방석: 점수 배율
  "name": "방석",
  "rarity": "Common",
  "trigger": "on_combat_start",// on_combat_start | on_turn_start | on_card_play | on_damage | passive
  "effects": [{ "op": "gain_resource", "resource": "radiance", "amount": 1 }],
  "durability": null           // 정수면 전투당 발동 횟수 제한 (STS2 내구도)
}
```

### 4.4 RunState / CombatState
```jsonc
// RunState (런 전역)
{ "seed": 123456, "character": "luminary",
  "deck": ["card_id", "..."], "relics": ["..."], "potions": ["..."],
  "hp": 80, "maxHp": 80, "gold": 99, "act": 1, "mapNode": "n_0_2" }

// CombatState (전투 중)
{ "energy": 3, "baseEnergy": 3,
  "hand": [], "drawPile": [], "discardPile": [], "exhaustPile": [],
  "block": 0,
  "statuses": { "radiance": 0, "weak": 0, "vulnerable": 0, "poison": 0 },
  "resources": { "radiance": 0, "stakes": 0, "go": 0, "chaff": 0 },
  "enemies": [ /* Enemy 인스턴스 + 현재 hp/intent */ ],
  "turn": 1 }
```

### 4.5 Map Node
```jsonc
{ "id": "n_1_3", "type": "combat",  // combat|elite|event|shop|rest|treasure|boss
  "next": ["n_2_2", "n_2_3"],       // 분기 간선
  "encounter": "pack_easy_a" }
```

---

## 5. 키워드 · 효과 명세

### 5.1 효과 원자(Effect ops) — 엔진의 실행 단위
> 카드/유물/적은 이 op들의 조합으로 표현. 새 op 추가는 신중히(확장은 데이터로).

| op | 인자 | 동작 |
|---|---|---|
| `deal_damage` | amount, target? | 대상에 피해(공격자 광/힘 + 대상 vulnerable 반영) |
| `gain_block` | amount, target? | Block 획득 |
| `draw` | n | n장 드로우 |
| `gain_energy` | n | 에너지 +n |
| `apply_status` | status, amount, target | 상태 부여(radiance/weak/vulnerable/poison 등) |
| `gain_resource` | resource, n | 캐릭터 자원 누적(radiance/stakes/go/chaff) |
| `add_card` | cardId, pile | 카드 생성(hand/draw/discard) |
| `exhaust_card` | ref | 카드 소멸 |
| `summon` | unitId, hp | 동료 소환/강화(§6 산군) |
| `mark_doom` | amount, target | 박(처형) 표식 부여(§5.3) |
| `multiply_score` | factor | 판돈/점수 배율(§6 타짜) |

### 5.2 공통 키워드
| 키워드 | id | 정의 |
|---|---|---|
| 소멸 | `exhaust` | 사용 후 이번 전투 제거(소멸 더미로) |
| 휘발 | `ethereal` | 손에 남은 채 턴 종료 시 소멸 |
| 보유 | `retain` | 턴 종료 시 버려지지 않음 |
| 다회타격 | `multi(n)` | 효과를 n회 반복 |
| 리플레이 | `replay` | 이 카드를 1회 더 실행 |

### 5.3 박(剝) = 처형 (STS2의 Doom)
- `mark_doom(n, target)`로 대상에 **박 표식** 누적. **적 턴 종료 시 표식 ≥ 현재 HP면 즉사**.
- 화투 변형 박: `피박`/`광박`/`멍박`은 "특정 조건 충족 시 표식 대량 부여 또는 배율" 트리거로 구현(§6 타짜/피바라기).

### 5.4 인챈트(Enchant) — 카드별 영구 수식자 (런 지속)
```jsonc
{ "id": "geumtae", "name": "금테 두르기",
  "apply": { "field": "effects[0].amount", "delta": 3 },  // 예: 첫 효과 +3
  "tradeoff": [{ "op": "apply_status", "status": "self_hp_loss", "amount": 1 }] }
```
- 카드당 **1개만**, 제거 불가. 카드 보상 단계의 카드에도 부여 가능. **MVP에서는 후순위(§12)**.

---

## 6. 캐릭터 (화투 매핑)

> 각 캐릭터 = "고유 자원 1축". MVP는 **1번(Luminary)만** 구현.

| id | 가칭 | 화투 축 | 고유 자원/동사 | STS2 대응 | MVP |
|---|---|---|---|---|---|
| `luminary` | 광객(光客) | 광 | `radiance` 스택 → 오광 폭발 | 아이언클래드+리젠트 | ★ 우선 |
| `ribbon` | 단(丹)을 잇는 자 | 띠 | 홍·청·초단 세트 콤보 | 사일런트 | 후순위 |
| `beast` | 산군(山君) | 열끗 | 동물 `summon`·동료 | 디펙트+네크로 | 후순위 |
| `husk` | 피바라기 | 피 | `chaff` 물량 스택·피박 | (신규) | 후순위 |
| `gambler` | 타짜 | 고스톱 | `stakes`/`go` 판돈·박 처형 | 리젠트+네크로 | 후순위 |

### 6.1 광객(Luminary) — 상세 (MVP 캐릭터)
- **자원 `radiance`(광)**: 누적 스택. **모든 Attack 피해에 +radiance**(STS 힘과 동일 동작).
- **테마 규칙**: "광 카드"(아래 5종)를 모을수록 강해지고, 임계치에서 보너스.
  - 광 3장 보유 시: 전투 시작 `radiance +2`.
  - 광 5장 보유(오광) 시: 피니셔 카드 `오광난무` 해금 또는 비용 0화.
- **시작 스탯**: HP 80, 시작 유물 `burning_lantern`(전투 종료 시 HP +6).
- **시작 덱(10장)**:
  - `빛타격` ×5 — Attack, cost1, 6 피해
  - `방패` ×4 — Skill, cost1, 5 Block
  - `점화` ×1 — Skill, cost1, `radiance +1`
- **카드 풀(MVP 예시, 보상 후보 ~12장)**:
| 이름 | 타입 | 코스트 | 효과 | 비고 |
|---|---|---|---|---|
| 송학(1월 광) | Power | 1 | radiance +2 | 광 카드 |
| 공산명월(8월 광) | Skill | 1 | Block 8, radiance +1 | 광 카드 |
| 비광(12월 광) | Attack | 1 | 9 피해, radiance +1 | 광 카드 |
| 벚꽃(3월 광) | Skill | 0 | 카드 1장 드로우, radiance +1 | 광 카드 |
| 오동(11월 광) | Power | 2 | 매 턴 radiance +1 | 광 카드 |
| 일제사격 | Attack | 2 | 4 피해 ×3 (multi) | 광 스케일 강함 |
| 정화 | Skill | 1 | weak 2 부여 | |
| 광휘폭발 | Attack | 2 | radiance×3 피해 | 피니셔 |
| 오광난무 | Attack | 3 | 전체 radiance×4 피해 | 오광 해금 |
- **플레이 패턴**: 초반 광 카드로 radiance 누적 → 중후반 `일제사격`/`광휘폭발`로 스케일 폭발.

### 6.2 나머지 4종 요약 (후순위)
- **Ribbon**: 홍단/청단/초단 색 띠를 모아 같은 색 3장 완성 시 보너스 발동. 버리기(discard)가 콤보 트리거(STS2 Sly).
- **Beast**: `summon`으로 동물 동료(멧돼지=탱킹, 사슴=공격, 나비=방어). 동료는 별도 HP, 플레이어 대신 피해 흡수.
- **Husk**: 0코스트 `피` 토큰 양산 → `chaff` 장수 비례 피해/방어. 적에 `피박` 표식.
- **Gambler**: `go`를 외쳐 `stakes`(판돈) 배율↑·리스크↑. 흔들기/폭탄으로 한 방 배율. 적에 박 처형.

---

## 7. 화투 ↔ 메커니즘 용어집 (GLOSSARY)

| 화투 용어 | 게임 메커니즘 | 코드 식별자 |
|---|---|---|
| 광(光) | 누적 파워 스택(=힘) | `radiance` |
| 오광 | 광 5종 보유 시 피니셔 해금 | `ogwang_unlock` |
| 열끗/멍 | 소환 동료 토큰 | `summon` / `companion` |
| 띠(단) | 세트 수집 콤보(홍/청/초) | `ribbon_set` (`hong/cheong/cho`) |
| 피(皮) | 물량 토큰 | `chaff` |
| 쌍피 | 피 2장 가치 토큰 | `chaff_double` |
| 고(GO) | 판돈 배율 누적(리스크) | `go` |
| 판돈 | 2차 자원(점수→환전) | `stakes` |
| 박(剝) | 처형 표식 | `doom` |
| 피박/광박/멍박 | 조건부 처형/배율 트리거 | `doom_trigger_*` |
| 흔들기/폭탄 | 배율 부스트 | `multiplier_boost` |
| 고스톱(3인)/맞고(2인) | 협동 모드(OUT OF SCOPE) | `coop` |

---

## 8. 콘텐츠 스펙 (MVP 1액트)

- **적(일반)**: 3종 — `잡도깨비`(공/방), `까마귀떼`(다회 약공), `허수아비`(디버프). HP 12~22.
- **엘리트**: 1종 — `달그림자 도깨비`(중간 보스급, HP ~55).
- **보스**: 1종 — `비(雨)의 정령` (멀티페이즈: HP 50%에서 패턴 변경, 예: 카드 봉인 디버프). HP ~120.
- **유물**: 5종 — `방석`(radiance+1/전투), `담요`(전투시작 Block 5), `엽전꾸러미`(골드+), `숫돌`(첫 공격 +3), `등잔`(시작 유물).
- **맵**: 1액트 = 노드 12~15개, 깊이 ~7층, 층마다 1~3분기. 보스 직전 휴식처 1개 고정.
- **보상**: 전투 후 카드 3택1(+스킵), 골드. 엘리트/보물 → 유물.

---

## 9. MVP 범위 (수직 슬라이스) — 먼저 만들 것

**포함(필수)**
<!-- 진행: 2026-06-22 갱신. [x]=완료 / [~]=부분 -->
- [~] 캐릭터 1명(`luminary`) + 시작 덱 ✅ / 카드 풀 ~12장 (현재 3장: 빛타격·방패·점화)
- [x] 전투 루프(§3.2) 완전 동작: 에너지/드로우/Block/HP/intent ✅ (M1, 33 EditMode 테스트)
- [ ] 적 3종 + 엘리트 1 + 보스 1 (현재 잡도깨비 1종)
- [ ] 노드 맵 1액트(분기 선택, §4.5)
- [ ] 전투 후 카드 보상(3택1) + 상점/휴식에서 카드 제거 1수단
- [ ] 유물 3~5종
- [x] 승/패 처리 + 다시하기 + 시드 RNG ✅
- [~] 최소 UI: 손패 드래그/타깃팅 ✅ · intent 표시 ✅ · 더미 열람(미완) — + 비주얼(HP바·데미지팝업)·모션 polish 완료

**검증 질문(§11.6 충족 시 MVP 성공)**

---

## 10. 권장 프로젝트 구조 (엔진 중립 예시)

```
/data            # 콘텐츠 (JSON) — 코드와 분리
  cards/luminary.json
  enemies/act1.json
  relics.json
  encounters.json
  map/act1.json
/src
  /core          # 엔진 비의존 게임 로직 (순수 함수 권장)
    combat.*      # 턴 상태머신(§3.2)
    effects.*     # op 디스패처(§5.1)
    rng.*         # 시드 RNG(§3.4)
    run.*         # RunState 진행
    map.*         # 절차적 맵 생성
  /ui            # 렌더링/입력 (엔진 의존)
  /content       # 데이터 로더(JSON→객체)
/tests           # core 로직 유닛 테스트(전투/효과/시드 결정론)
```
- **Unity 매핑**: `/data` → ScriptableObject 또는 Resources의 JSON; `/src/core` → 순수 C# 어셈블리(MonoBehaviour 비의존, 테스트 용이); `/src/ui` → MonoBehaviour/Prefab.
- **핵심 원칙**: `core`는 엔진/UI를 모른다 → 헤드리스 테스트·시드 재현 가능.

---

## 11. 개발 마일스톤 + 완료 기준

1. **전투 코어** ✅ — 1캐릭터·고정 덱·적 1종. 에너지/드로우/Block/HP/intent 동작. (M1, 33 테스트; + 전투 UI/비주얼/모션 polish 완료 — `production/session-state/active.md` 참조)
   - 완료 기준: 한 전투를 손으로 끝까지 이기고 질 수 있다. ✅
2. **효과 시스템 + 카드 다양성** (진행 중): §5.1 op 디스패처 ✅(5종), 광 자원 ✅; 카드 풀 ~12 (현재 3장, 코드 빌더).
   - 완료 기준: 데이터(JSON)만 추가해 새 카드가 동작한다. → 다음: 효과 데이터 외부화 + 카드 추가.
3. **한 판 루프**: 맵·노드·보상·보스·승패.
   - 완료 기준: 시작→보스→결과까지 끊김 없이 1런 완주.
4. **의사결정 깊이**: 유물·카드 제거·(간단) 이벤트.
   - 완료 기준: 매 런 덱이 달라지고 선택이 결과를 바꾼다.
5. **밸런스 + 플레이테스트 반복**.
   - 완료 기준: §11.6.

### 11.6 MVP 성공 판정 (셋 이상 YES면 통과)
- [ ] 같은 캐릭터 5판에서 매번 다른 선택/덱이 나온다.
- [ ] 한 판에 최소 1회 "콤보 터지는" 쾌감 순간이 있다.
- [ ] 졌을 때 "운"이 아니라 "내 실수"로 느껴진다.
- [ ] 테스터가 시키지 않아도 "한 판 더"를 누른다.

---

## 12. OUT OF SCOPE (MVP 이후)

협동(맞고/고스톱) · 캐릭터 2~5번 · 인챈트 전체 · 고대의 존재(축복) · 대체 액트(바이옴) · 어센션 난이도 · 메타 진행(연대기/에포크) · 완성 아트/사운드 · 다수 액트.

---

## 부록 A. STS2 참고 요약 (설계 근거)

- **STS2 핵심 신규**: 인챈트, 컴패니언(Osty), 고대의 존재(보스유물 대체), 대체 액트, 멀티페이즈 보스, 4인 협동, 신규 자원(별/영혼/둠/단조).
- **캐릭터=고유 자원 1축** 구조(아이언클래드=힘, 사일런트=버리기/독, 디펙트=오브, 리젠트=별, 네크로바인더=소환/둠) → 본 게임 5캐릭터의 직접 모델.
- **교훈**: STS2는 콤보/덱순환을 너프했다 평점 폭락("덱빌딩 게임에서 덱빌딩 막지 마라") → §2 원칙 1의 근거.
- 기준: STS2 얼리액세스(2026-03 출시, Godot 엔진). 수치는 패치로 변동.

## 부록 B. 출처
- 나무위키 Slay the Spire 2 — https://namu.wiki/w/Slay%20the%20Spire%202
- Wikipedia: Slay the Spire II — https://en.wikipedia.org/wiki/Slay_the_Spire_II
- slaythespire.wiki.gg — https://slaythespire.wiki.gg/wiki/Slay_the_Spire_2
- Mobalytics STS2 (캐릭터/키워드) — https://mobalytics.gg/slay-the-spire-2
- GamesRadar 리뷰/평점 보도 — https://www.gamesradar.com/slay-the-spire-2-review/
- 나무위키 화투/패 — https://namu.wiki/w/%ED%99%94%ED%88%AC/%ED%8C%A8
- 위키백과 고스톱 — https://ko.wikipedia.org/wiki/%EA%B3%A0%EC%8A%A4%ED%86%B1
- Godot vs Unity 2026 — https://dev.to/linou518/godot-vs-unity-in-2026-which-engine-should-indie-developers-choose-50g4

---

## 부록 C. 전투 UI 리서치 (Slay the Spire 참고, 2026-06-21)

> 정식 전투 UI 설계 기준. 현재 OnGUI 디버그 UI를 대체할 레이아웃·UX의 근거.

### C.1 표준 레이아웃 (STS1/STS2 공통)
```
[상단/중앙]  적들 — 각 적 위에 intent 아이콘(다음 행동) + HP바 + Block 수치 + 상태 아이콘
[하단]       손패 — 카드 가로/부채꼴 배치. 클릭 또는 드래그-드롭으로 사용·타깃팅
[좌하단]     에너지 구슬(예 3/3) · 드로우 더미(남은 수) · 플레이어 HP/Block/상태
[우하단]     버린 더미 · 소멸 더미 · "턴 종료" 버튼
```
- 중요 요소는 화면 가장자리에서 띄우고, UI 스케일 조정 지원(가독성).

### C.2 intent (완전정보의 핵심, §2.2)
- 공격: 빨간 무기 아이콘 + 숫자(예상 피해). 피해량에 따라 단검→낫 5단계로 위협도 시각화.
- 방어: 방패 / 버프·디버프: 각 아이콘 / 도주: 회오리 화살표 / 카드 오염(STS2): 회색 카드+주황 테두리.
- **예상 피해는 Block·약화(weak) 등 modifier 반영 후 숫자로** 노출(막을지 판단 가능).

### C.3 카드 표현
- 코스트(좌상단 숫자), 이름, 아트, 효과 설명, 타입별 테두리 색(Attack/Skill/Power).

### C.4 STS2 신규 UI
- 크리처 세로 배치(가독성 조정 가능), 효과를 좌(아군)/우(적) 세로 정렬로 구분.
- 예상 피해 카운터, multi-hit 합계, Heirloom 카드(완료 조건), 4인 협동 UI.

### C.5 화투 매핑
| STS 요소 | 화투 |
|---|---|
| 손패 카드 | 화투 카드(광/열끗/띠/피 비주얼) — 하단 |
| 에너지 | 그대로(또는 화투 테마 리스킨) |
| 적 + intent | 몬스터 + 공격/방어 의도 아이콘 |
| 힘(strength) 스택 | 광(radiance) 상태 스택 |
| HP/Block/상태 | 동일 |

### C.6 출처
- Interface In Game (STS2/STS1 UI): https://interfaceingame.com/games/slay-the-spire-2/
- Mega Crit STS2 Press Kit: https://www.megacrit.com/press-kits/slay-the-spire-2/
- Untapped.gg intent 가이드: https://sts2.untapped.gg/en/guides/how-to-read-enemy-intent
- STS Wiki — Intent: https://slaythespire.wiki.gg/wiki/Intent

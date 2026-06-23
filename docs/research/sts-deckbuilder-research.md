# 슬더슬1·2 리서치 — 덱빌더 설계 참고

> 화투 로그라이크 개발 시 참고용 **영구 리서치 문서**. Slay the Spire 1·2 위키·가이드 기반.
> 섹션별로 정리하며, 새 리서치는 해당 섹션에 추가한다. 출처는 각 섹션 끝.
> 작업(강화·AI·밸런싱·유물·몬스터) 전 해당 섹션을 먼저 참고할 것.

## 📑 목차 (섹션 인덱스 — 필요할 때 바로 찾기)
| § | 주제 | 하위 항목 |
|---|---|---|
| **1** | 카드 강화(Upgrade) | 1.1 규칙 · 1.2 타입별 패턴 · 1.3 화투 op별 규칙 ✅ |
| **2** | 카드 밸런싱(코스트·효율) | 2.1 baseline(6/E) · 2.2 화투 기준표 ✅ |
| **3** | 유물(다양성·트리거) | 3.1 분류 · 3.2 트리거 다양성 · 3.3 화투 구현(전투시작/턴시작/카드플레이) ✅ |
| **4** | 적 AI / 의도(Intent) | 4.1 핵심 · 4.2 화투 적용(Sequence/Weighted) ✅ · 4.3 고도화(PeekNext+ConditionalAi, 외눈 적용) ✅ |
| **5** | 몬스터(체력·수·패턴) | 5.1 Act1 기준 · 5.2 화투 기준 · 5.3 밸런스 시뮬(엘리트/보스) ✅ · 5.4 멀티+SimAi버그 ✅ |

> ✅ = 화투에 구현/적용 완료. 미표시 = 리서치만(후속 가능 — 예: §3.2 피격시/HP임계 트리거, §4.3 보스 ConditionalAi).

---

## 1. 카드 강화 (Upgrade)

### 1.1 핵심 규칙 (STS1/2)
- 카드는 **1회만 강화**(예외: Searing Blow 무한 — 첫 +4, 이후 +1씩 증가). 이름에 `+`, 초록색.
- 강화 = **효과 수치↑ 또는 코스트↓** (둘 중 하나가 보통, 동시는 드묾).
- **Status/저주 카드는 플레이어가 강화 불가**.
- 강화처: 휴식처 단조, 이벤트(Upgrade Shrine 등), 유물(Egg류 자동), 전투 중(Armaments/Apotheosis).

### 1.2 타입별 강화 패턴 (구체 예시)
| 카드 | 강화 전 → 후 | 증가 |
|---|---|---|
| Defend(방어) | 블록 5 → 8 | +3 |
| Shrug It Off | 블록 8 → 11 | +3 |
| Vigilance | 블록 8 → 12 | +4 |
| Power Through | 블록 15 → 20 | +5 |
| Strike(공격) | 6 → 9 | +3 |
| Bash | 8 → 10 + 취약 2→3 | +2/+1 |
| Searing Blow | 무한(+4, 이후 +1씩) | 가변 |

- **공격**: 데미지 +3 보통, 강한 카드는 +4~5, 일부는 코스트 감소(2→1).
- **방어/스킬**: 블록 +3~5, 또는 부가효과(드로우·힘) 강화.
- **파워**: 효과 강화(힘/집중↑) 또는 코스트↓.
- **상태이상**: 독 +3~5, 약화/취약 +1, 힘/민첩 +1.
- **다회타격**: 타격 횟수↑(예: Twin Strike) 또는 타격당 데미지↑ — **둘 중 하나만**(과강화 방지).

### 1.3 화투 적용 규칙 (op별 — `CardData.Upgrade()`)
| op | 강화량 | 근거 |
|---|---|---|
| DealDamage(단일) | +3 | Strike 패턴 |
| DealDamage(다회, 2타+) | 타격당 +1 | 다회 과강화 방지 |
| GainBlock | +3 | Defend 패턴 |
| ApplyStatus: Poison | +3 | 독 강화 큼 |
| ApplyStatus: Weak/Vulnerable | +1 | 디버프 보수적 |
| ApplyStatus: Radiance/Dexterity/Thorns/Regen | +2 | 스케일 자원 |
| Draw | +1 | 드로우 보수적 |
| GainResource(광) | +1 | 자원 보수적 |
| ConsumeRadiance/MultiplyPoison(배수) | +1 | 배수형 |

> 출처: [Upgrade — STS Wiki](https://slaythespire.wiki.gg/wiki/Upgrade) · [Block — STS Wiki](https://slaythespire.wiki.gg/wiki/Block) · [Upgrade Guide 2026 — GameHelper](https://www.gamehelper.io/games/slay-the-spire/articles/slay-the-spire-the-ultimate-guide-to-upgrading-cards-in-2026)

---

## 2. 카드 밸런싱 (코스트·효율)

### 2.1 baseline (STS1)
- **Strike**: 6 데미지 / 1에너지. **Defend**: 6 블록 / 1에너지. → 기본 = **6 데미지·6 블록 per 에너지**.
- 효율 = 임팩트 / 자원. 예: 12뎀·2E(6/E) vs 8뎀·1E(8/E) → 후자가 효율↑.
- Common 공격: ~8~9뎀·1E(baseline +α). Uncommon/Rare는 부가효과(상태이상·드로우)로 가치 보강.
- **코스트 감소 강화가 매우 강력**(0코 Defend ≫ 1코 Defend). 데미지가 공격적으로 스케일 → 턴당 전체 에너지를 한 장에 몰지 않게 분산.

### 2.2 화투 적용 기준
| 분류 | 코스트 | 기준치 |
|---|---|---|
| 공격(단일) | 1코 | 6~9뎀(+자원) |
| 공격(강타) | 2코 | 12~16뎀 |
| 공격(피니셔/소멸) | 2~3코 | 20~25뎀 |
| 방어 | 1코 | 5~8블록 |
| 방어(대형) | 2코 | 14블록 |
| 0코 카드 | 0코 | 효과 작게(드로우1·자원1·소형타격) |
| 상태이상 | 1코 | 독 4·약화2 등(누적 가치라 보수적) |

- 현재 카드 풀 점검: 백호 일섬(9뎀·1E)·연격(3×3·2E)·거수일격(10뎀+위엄2·2E)은 기준 부합. 광객/묵귀도 대체로 정합.

> 출처: [Energy — Fandom](https://slay-the-spire.fandom.com/wiki/Energy) · [Card Rewards — Spire Builds](https://www.spirebuilds.com/guides/understanding-card-rewards)

## 3. 유물 (다양성·효과 트리거)

### 3.1 분류 (STS1/2)
- 등급: **Common/Uncommon/Rare/Boss/Event/Shop** (+ Starter 시작 유물). STS2는 Ancient 추가(6등급).
- 출처 제한: Boss=보스만, Event=이벤트만, Shop=상점만, Starter=캐릭터 기본.
- 등급 확률 보통 **3:2:1**(Common50/Uncommon33/Rare17). 상자는 49/42/9.
- 총량: STS1 100+, STS2 287개 → **다양성이 정체성**.

### 3.2 효과 트리거 다양성 (현재 화투는 '전투 시작'만 — 핵심 개선점)
- 트리거 종류: 전투시작 / **턴 시작·종료** / **공격 시·피격 시** / **카드 플레이 시**(타입별) / **HP 임계** / **처치 시** / 상점·맵·휴식 / 패시브 상시.
- 화투 현재: `RelicData.ApplyCombatStart(PlayerState)`만 → **턴/피격/플레이 트리거 훅 확장 필요**(다양성 핵심).
- **Boss 유물 = 강력+페널티**(예: 에너지+1 + 손해) — 트레이드오프 설계.

> 출처: [Relics — Fandom](https://slay-the-spire.fandom.com/wiki/Relics) · [Relic Tier List — Unduel](https://unduel.com/slay-the-spire/tier-list/the-best-common-uncommon-and-rare-relics)

### 3.3 화투 트리거 확장 구현 (2026-06)
- **1차 구현**: `RelicData.onTurnStart` 훅 추가 → 전투시작 외 **'매 턴 시작'** 트리거 도입. `EchoHide`(산울림 가죽, 매턴 방어4)가 첫 적용.
- **순환 의존 회피**: RelicData(Run)가 PlayerState(Combat)를 참조하므로 CombatState가 RelicData를 직접 들면 Run↔Combat 순환. → `CombatState.OnTurnStartHooks`(`List<Action<PlayerState>>`)로 우회. CombatFactory가 `relic.ApplyTurnStart` 메서드 그룹을 주입, CombatEngine.StartPlayerTurn에서 실행.
- **후속 트리거 후보**: 피격 시(EffectDispatcher.DealDamage 훅)·카드 플레이 시(타입별)·HP 임계·처치 시. 같은 Action-주입 패턴으로 확장 가능.

## 4. 적 AI / 의도(Intent) 패턴

### 4.1 핵심 (STS1/2)
- 적은 **스크립트(상태기계)**로 행동. 의도(Intent)를 머리 위에 미리 표시(완전정보).
- Act1 단순(고정/교대), **Act2·3은 가중치 move 풀**(확률 선택) — "보통 한 행동을 선호하나 확정은 아님".
- **연속 제한(repetition limit)**: 같은 move를 연속 N회 초과 금지(예: 공격 2연타 후 강제 전환). 예측 가능성↓·패턴화 방지.
- 예: Cultist = 의식(버프) 1회 → 이후 공격 반복. Jaw Worm = 가중치(물기/방어버프/포효).

### 4.2 화투 적용
- 기존: `SequenceAi`(고정 순서 순환), `PhaseAi`(HP 임계 페이즈 전환).
- 추가: **`WeightedAi`** — AiOrder를 가중치 풀(중복 ID = 높은 확률)로 보고, 시드 RNG로 선택 + **연속 제한(기본 2)**. Act2+ 일반 적에 적용해 예측성↓.
- 의도 미리보기(`PeekNext`)는 한 번 정한 move를 **캐시**해 완전정보(미리보기=실제 실행) 유지. `Advance`에서 캐시 비우고 연속 카운트 갱신.

> 출처: [Intent — STS Wiki](https://slaythespire.wiki.gg/wiki/Intent) · [Enemy Patterns & AI — A20 Mastery](https://oboe.com/learn/slay-the-spire-ascension-20-mastery-rgvy3n/enemy-patterns-and-ai-4) · [Intent — Fandom](https://slay-the-spire.fandom.com/wiki/Intent)

### 4.3 화투 AI 고도화 설계 (후속 — 구현 가이드)
- **현 한계**: `IEnemyAi.PeekNext(EnemyState self)`가 self만 받아 **적이 플레이어 상태(Block/status/HP)를 못 본다** → 조건부 반응 AI 불가. 현재는 Sequence(고정)·Weighted(가중치+연속제한)·Phase(HP 임계)뿐.
- **개선안(시그니처 확장)**: `PeekNext(EnemyState self, PlayerState player)`로 변경. 수정 대상 = `IEnemyAi`·`SequenceAi`·`WeightedAi`·`PhaseAi` + `CombatEngine.EnemyTurn`(호출부) + `EnemyState.RefreshIntent`. 완전정보(미리보기=실행) 유지 위해 캐시 패턴은 WeightedAi와 동일.
- **`ConditionalAi`(`EnemyAiKind.Conditional`, 미구현)**: 플레이어/자기 조건별 move 선택. 예 —
  - 플레이어 Block↑(예: ≥10)이면 디버프(약화·취약)로 방어 무력화
  - 플레이어 Block 0이면 강타
  - 자기 HP<50%면 방어/광폭(Phase와 결합 가능)
  - STS 레퍼런스: Gremlin Nob(스킬 플레이 시 힘↑)·Lagavulin(수면→각성)·Cultist(의식 1회 후 공격 반복) 식 반응형.
- **데이터 스키마**: EnemyData에 조건→move 매핑(예: `[{cond:"player_block>=10", move:"debuff"}, ...]`) 추가하거나, 적별 ConditionalAi 서브클래스로 구현.

## 5. 몬스터 (체력·수·패턴·특징)

### 5.1 Act1 기준 (STS1)
- **일반 인카운터 구성**: ①단일 중강몹 / ②약몹 무리(다수) / ③2 중강몹 페어.
- HP: 약몹 10~20, 중강 40~50대. 엘리트 ~90~110(Lagavulin 109~111). 보스 ~250.
- **엘리트 풀(Act1)**: Gremlin Nob(스킬 쓰면 힘↑·취약), Lagavulin(3턴 수면→각성 강타), Tri-Sentries(다수+Dazed 덱오염).
- 각 몬스터 **고유 기믹**: 수면/각성, 조건부 버프(스킬 트리거), 덱 오염, 다수 협공, 가시(피격 반사).

### 5.2 화투 적용 기준
- 인카운터: 일반 1~3마리(`PickEnemies` 구현됨). 약몹 무리 vs 단일 강몹 다양화.
- HP 가이드(act1): 약몹 18~28 · 중강 38~50 · 엘리트 90~120 · 보스 100~125 (현재 밸런스와 정합 — `docs/systems/05-balance-sim.md`).
- 몹별 고유 기믹: 현재 Doom(구미호)·PhaseAi(보스)·가중치 AI(멧돼지·두꺼비). 추가 — **가시 적(밤송이도깨비, 피격 반사)**, 수면/각성, 조건부 버프.

> 출처: [Monsters — STS Wiki](https://slaythespire.wiki.gg/wiki/Monsters) · [Elites — STS Wiki](https://slaythespire.wiki.gg/wiki/Elites) · [Bosses — STS Wiki](https://slaythespire.wiki.gg/wiki/Bosses)

### 5.3 화투 밸런스 시뮬 결과 (시작덱 기준 · BalanceSim + execute_code)
> 광객(HP80)·묵귀(HP70)·백호(HP80) **시작덱**으로 각 적 30~40회 자동 전투(SimAi) 승률. 시작덱 기준이라 보스 승률이 낮은 건 정상(덱빌딩 전제).

| 적 | 조정 전 | 조정 후 |
|---|---|---|
| 일반(잡도깨비~밤송이) | 100% (잔여 60~73) | 유지(시작덱이라 쉬운 게 정상) |
| 엘리트 광귀 | 100% (잔여 47~57) | **97~100% (잔여 9~14)** |
| 엘리트 외눈 | 100% (잔여 35~62) | **85~97% (잔여 20~26)** |
| 엘리트 구렁이 | 92~100% (잔여 19~45) | **72~92% (잔여 7~13)** |
| 보스 달그림자/구미호/장군 | 16~53 / 0~16 / 6~30% | 유지(덱빌딩 전제) |

- **엘리트 조정 내역**: HP 52~70 → 90~110(§5.1 기준), 주공격 데미지↑(광귀 14→15·외눈 17→21·구렁이 12→16), 광귀 공격빈도↑(방벽→공격). 시작덱으로도 100%였던 엘리트를 빠듯한 승부로.
- **보스**: 시작덱 0~53%는 정상(보상 카드로 덱 강화 후 상승). 구미호 0~16%는 다소 빡빡하나 최종 보스 위엄으로 유지.
- **재현**: `BalanceSim.Simulate(deck, enemy, hp, trials)`를 `execute_code`(codedom)로 호출. 결정론 시드라 재현 가능.

### 5.4 멀티 몬스터(약몹 무리) 시뮬 + SimAi 버그 (2026-06)
- **SimAi 버그 발견·수정**: 시뮬 자동 플레이어가 항상 적 index 0만 공격(`PlayCard(pick, 0)`) → 다중 적 전투가 **전부 0%**(밸런스가 아니라 시뮬 AI 한계). 살아있는 적 순차 타겟(`FirstAliveEnemy`)으로 수정.
- **`BalanceSim.SimulateMulti`** 오버로드로 다중 적 시뮬 가능.
- 수정 후 결과(시작덱, 30회):

  | 인카운터 | 광객 | 묵귀 | 백호 |
  |---|---|---|---|
  | 잡도깨비 ×2 | 100% | 100% | 100% |
  | 잡도깨비 ×3 | 70% | 43% | 30% |
  | 까마귀+도깨비불 | 100% | 100% | 100% |

- **판단**: 2마리는 쉬움(정상), 3마리는 도전적(빡빡). 백호가 다중 적에 약함(단일 위엄 스케일이라 분산 비효율 — 캐릭터 특성). 시작덱 기준 적정이라 멀티 수치 추가 조정 불필요.

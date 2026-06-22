# 한 판 루프 — 마스터 설계 (Overview)

> STS2 리서치 기반(2026-06-22). 출처: 맵=`sts_map_oracle`(원본 복제 Rust)·wiki.gg, 보상=Spire Codex·Fandom, 런구조=wiki.gg. 세부 리서치는 본 폴더의 기능별 문서에 인용.

## 0. 목표 (사용자 정의)
메인화면 → **캐릭터 선택** → **맵** → 노드 선택 → **전투** → **카드 보상 3택1·덱빌딩** → 맵 → 다음 스테이지 → **보스**. 전체를 STS2처럼.

## 1. 런 상태머신 (화면/페이즈 전환)
```
MainMenu
 → CharacterSelect            (캐릭터 1 선택 → 시작 HP/덱/유물/골드 세팅, 시드 생성)
 → MapView                    (맵 표시, 진입 가능 노드 하이라이트)
   → NodeResolve              (노드 타입별: Combat/Elite/Event/Shop/Rest/Treasure/Boss)
     → Combat → RewardScreen  (골드 + 카드 3택1 + (확률)포션/유물) → MapView
     → Boss   → BossReward    (보스 유물 3택1) → ActTransition(회복) → MapView(새 맵)
 → ... 반복 ...
 → Victory (최종 보스 격파) / GameOver (HP 0, 글로벌 전이)
```
- **이동 규칙**: 현재 노드에서 **간선으로 연결된 다음 층 노드만** 선택(자유 점프 금지, 단방향).
- **런 종료 조건은 둘뿐**: 전투 중 HP 0(패배) / 최종 보스 격파(승리).
- HP는 전투 간 유지(자동 회복 없음). 휴식처·액트 전환에서만 회복.

## 2. 시스템 분해 (기능별 문서)
| # | 문서 | 시스템 | 레이어 |
|---|---|---|---|
| 01 | `01-map-generation.md` | 절차적 맵 생성(7×15·6경로) | Core |
| 02 | `02-run-state.md` | RunState(덱·HP·골드·맵위치·액트) + GameFlow 상태머신 | Core+Game |
| 03 | `03-card-reward.md` | 카드 보상 3택1 + 밴드+오프셋 레어도 | Core+Game |
| 04 | `04-character.md` | 캐릭터 정의(4축) + 선택 화면 | Core+Game |
| 05 | `05-boss-act.md` | 보스(페이즈·인텐트) + 액트 진행 | Core+Game |
| 06 | `06-screens-ui.md` | 메인메뉴·맵·보상 화면 + 전투 통합 | Game |
| 07 | `07-rest-shop-event.md` | 휴식/상점/이벤트(후순위) | Core+Game |

## 3. 맵 핵심 (→ 01 상세)
- 그리드 **7×15**, 경로 **6**가닥. 액트 1장 = 그리드 하나(시드 독립 생성).
- **고정 층**: 행0=전투, 행8=보물, 행14=휴식, 그 위=보스 단일 수렴.
- **노드 타입 확률**(고정 아닌 칸): 엘리트 8%(난이도↑×1.6)·휴식 12%·이벤트 22%·상점 5%·나머지 전투.
- **배치 제약**: 행≤4 휴식/엘리트 금지, 행≥13 휴식 금지, 부모/형제 같은 특수타입 금지.
- **생성 알고리즘**: 6경로를 바닥→꼭대기로 그으며 (a)조상충돌 회피 (b)좌우 교차 금지(간선 단조성). 경로 안 닿은 노드 제거. → 방 타입 배정.

## 4. 보상 핵심 (→ 03 상세)
- 전투 후 **카드 3택1 + 스킵**(스킵=덱 압축 전략). 3장 중복 없음.
- **레어도 = 밴드 + 피티 오프셋**: 일반 Rare 3%/Uncommon 37%, 엘리트 10%/40%, 보스 Rare 확정. 오프셋 시작 −5%, Rare 아니면 +1%(캡 +40%), Rare 뽑으면 −5% 리셋. 카드 1장 생성마다 갱신.
- 골드: 일반 10–20 / 엘리트 25–35 / 보스 95–105. 포션 피티 40%(±10). 유물: 엘리트 확정·보물·보스.

## 5. 캐릭터/보스 핵심 (→ 04·05 상세)
- 캐릭터 차이 **4축**: 시작 HP(66~80) / 시작 유물 / 시작 덱(10~12) / 고유 자원. 시작 골드 99·기본 에너지 3은 공통.
- MVP 캐릭터 = **광객(Luminary)** — 고유 자원 `Radiance(광)`. (SPEC §6.1)
- 보스: 인텐트 예고 + 페이즈 패턴(임계값 모드전환 / 0HP 부활 / HP% 강화 / 행동제약). 처치 후 보스 유물 3택1 + 거의 풀회복 → 다음 액트.

## 6. 데이터 모델 (Core, `Hwatu.Core.Run`)
```
RunState   { CharacterData Character; List<CardData> Deck; int Hp/MaxHp/Gold;
             ulong Seed; int Act; MapGraph Map; int CurrentNodeId; int RareOffset; ... }
MapGraph   { List<MapNode> Nodes; int Width=7; int Height=15 }
MapNode    { int Id; int Row,Col; NodeType Type; List<int> NextIds; bool OnPath }
NodeType   { Combat, Elite, Rest, Shop, Treasure, Event, Boss }
CharacterData { string Id,Name; int StartMaxHp,StartGold,BaseEnergy;
                List<CardData> StartingDeck; ResourceType UniqueResource }
RewardOption  { CardData Card; ... }  // 3택1
```

## 7. 아키텍처
- **`Hwatu.Core`**(엔진 비의존): MapGenerator·RewardSystem·RunState 진행 로직 → **헤드리스 테스트**(맵 연결성·보상 확률·결정론).
- **`Hwatu.Game`**(UI): GameFlow(상태머신 MonoBehaviour) + 화면 View(MainMenu/CharSelect/Map/Reward) + 기존 CombatView 통합.
- **씬 구조**: 단일 `GamePlay` 씬 + 화면 패널 전환(GameFlow가 활성 화면 토글). 메인메뉴만 분리 검토.
- 전투 연동: `RunState.Deck` → 전투 시작 시 `CombatState` 생성(셔플 드로우). 전투 결과(HP/승패) → RunState 반영.

## 8. 구현 로드맵 (이 브랜치, 순서)
1. **RunState + GameFlow 골격** — 페이즈 상태머신, 화면 전환 stub.
2. **맵 생성(Core) + 테스트** — MapGenerator, 연결성/결정론 검증.
3. **맵 UI** — 노드/간선 렌더, 선택 입력.
4. **전투 통합** — 맵 노드 → 전투(런 덱) → 결과 → 맵 복귀.
5. **카드 보상(Core+UI)** — 레어도 롤 + 3택1 화면 + 덱 추가.
6. **캐릭터 선택 + 메인메뉴**.
7. **보스 + 액트 진행** — 보스 노드/전투, 액트 전환.
8. (후순위) 휴식/상점/이벤트/유물.

## 9. MVP 스코프 (이번 브랜치 목표)
**메인 → 캐릭터(광객) → 맵 → 전투 → 보상 3택1 → 맵 진행 → 보스 → 승리**의 한 바퀴가 끊김 없이 도는 것.
- 포함: 맵 생성·이동, 전투 통합, 카드 보상(레어도), 캐릭터 1, 보스 1, 액트 1.
- 후속: 다수 캐릭터, 휴식/상점/이벤트 노드 내용, 유물, 다수 액트, 카드 풀 확장.

## 10. 화투 테마 매핑 (골격은 STS, 명칭/비주얼만 교체)
- 노드: 전투=피(피 몹) / 엘리트=광 도깨비 / 보물=열끗·단 / 상점=초장 / 이벤트=흔들기 / 휴식=쉼 / 보스=각 월(月) 대장.
- 카드 풀: 광객의 빛 계열(빛타격·방패·점화) + 보상으로 확장.

## 11. 구현 현황 (2026-06-22 자율 확장, feature/run-loop)
설계(MVP)를 넘어 콘텐츠·메커닉·UI를 자율 확장. EditMode **133/133 통과**.
- **루프**: 메인→캐릭터→맵(분기 7×15)→전투→보상3택1+포션→이벤트/휴식/상점/보물→보스(페이즈+Doom)→액트전환(FinalAct=3 승리). 노드 7종 전부 UI 작동.
- **전투**: 턴 상태머신, 효과 op 6종(deal_damage/gain_block/draw/apply_status/gain_resource/clear_status), 키워드 4(Exhaust/Retain/Innate/Ethereal), Doom 카운트다운(예고→발동).
- **status 5**: Radiance(빛=힘)·Weak·Vulnerable·Poison(턴틱)·Dexterity(방어+).
- **적 13**: 일반7(잡도깨비·까마귀떼·허수아비·도깨비불·장승·그슨대·멧돼지)·엘리트3(광귀·외눈도깨비·구렁이)·보스3(달그림자·구미호[Doom]·장군, PhaseAi HP광폭).
- **카드 39**: 광객 풀 23장(빛/공격/방어/Exhaust/Innate/Ethereal/여명/광파), 묵귀 풀 16장(독/약화/고독/부식). 캐릭터별 풀(CharacterPools).
- **캐릭터 2**: 광객(HP80, radiance)·묵귀(HP70, poison). 시작 덱·보상 풀 차등.
- **경제/메타**: 골드 TrySpend·유물 11종(전투 시작 효과; +떡/호리병/먹)·카드 업그레이드(+3, 이름 "+")·포션 7(+강심약/선약)·이벤트 2·어센션(HP스케일)·전투 로그.
- **UI(uGUI 자율)**: 캐릭터 2택·맵 HUD/분기 선택·전투(인텐트 색/Doom 카운트)·보상 3택1·이벤트 선택·휴식(회복/강화 카드택1)·상점 구매·덱 보기 모달·전투 포션 버튼.
- **기능 문서**: `08~19`(유물·액트·휴식·상점·밸런스·보스페이즈·이벤트·status·어센션·캐릭터·키워드·포션).
- **남은 Play 게이트**(본인 체감만): 조작감·난이도 밸런싱·시각 미세 튜닝(코드/기능은 자율 검증 완료).

## 출처
- 맵: [sts_map_oracle (원본 복제)](https://github.com/Ru5ty0ne/sts_map_oracle) · [Map Generation wiki.gg](https://slaythespire.wiki.gg/wiki/Map_Generation)
- 보상: [Card Rarity — Spire Codex](https://spire-codex.com/mechanics/card-rarity) · [Card Rewards — Fandom](https://slay-the-spire.fandom.com/wiki/Card_Rewards)
- 런구조: [Mechanics](https://slaythespire.wiki.gg/wiki/Mechanics) · [Acts](https://slaythespire.wiki.gg/wiki/Acts) · [Bosses](https://slaythespire.wiki.gg/wiki/Bosses) · [STS2 공식](https://www.megacrit.com/news/2026-02-19-release-date-trailer/)

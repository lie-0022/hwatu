# 슬더슬1·2 리서치 — 덱빌더 설계 참고

> 화투 로그라이크 개발 시 참고용 **영구 리서치 문서**. Slay the Spire 1·2 위키·가이드 기반.
> 섹션별로 정리하며, 새 리서치는 해당 섹션에 추가한다. 출처는 각 섹션 끝.
> 작업(강화·AI·밸런싱·유물·몬스터) 전 해당 섹션을 먼저 참고할 것.

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

## 2. 카드 밸런싱 (코스트·효율) — [리서치 예정]

## 3. 유물 (다양성·효과 트리거) — [리서치 예정]

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

## 5. 몬스터 (체력·수·패턴·특징) — [리서치 예정]

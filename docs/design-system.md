# 디자인 시스템 — 화투 (먹·금 테마)

> UI 일관성의 **단일 출처(SSOT)**. 코드 토큰은 `Assets/Hwatu/Game/DesignTokens.cs`.
> 모든 UI는 색·크기를 하드코딩하지 말고 이 토큰을 참조한다. 신규 UI는 "토큰 먼저, 없으면 토큰 추가" 순서.

## 1. 색 팔레트
| 토큰 | 용도 | Hex |
|---|---|---|
| `Ink` | 배경(먹빛) | #1A1A1C |
| `Panel` / `PanelHi` | 패널·카드 / hover·선택 | #29282B / #383640 |
| `Paper` | 한지 백색(밝은 면) | #F5F5F0 |
| `Gold` | 금 강조·테두리 | #D4AF37 |
| `Spirit` | 영혼불 청백(영적 연출) | #7FD8E8 |
| `TextMain` / `TextDim` | 본문 / 보조 | — |

### 의미 색 (status·속성 — 아이콘/칩/텍스트 통일)
| 토큰 | 대상 | 색감 |
|---|---|---|
| `Danger` | 공격·위험·HP | 적 |
| `Defense` | 방어·민첩 | 청 |
| `Radiance` | 광(광객) | 금빛 |
| `Majesty` | 위엄(백호) | 금 |
| `Poison` | 독(묵귀) | 독녹 |
| `Weak` | 약화 | 보라 |
| `Vulnerable` | 취약 | 주황 |
| `Heal` | 회복·재생 | 녹 |
| `Thorns` | 가시 | 갈 |

> 코드에서 status 색은 `DesignTokens.StatusColor(StatusType)` 한 곳에서 받는다(분기 중복 금지).

## 2. 타이포 / 간격
- 폰트 크기: `FontTitle 40` · `FontHeading 28` · `FontBody 20` · `FontSmall 16` · `FontNumber 32`(HP·데미지).
- 간격: `SpaceXs 4` · `SpaceSm 8` · `SpaceMd 16` · `SpaceLg 24`.

## 3. 아이콘 규격 (들쑥날쑥 방지 — **3규격만**)
| 규격 | 크기 | 용도 |
|---|---|---|
| `IconSm` | 24 | status 칩·인라인(숫자 옆) |
| `IconMd` | 40 | 유물·적 인텐트 |
| `IconLg` | 64 | 보상·캐릭터 등 강조 |

- 소스: **Iconify `game-icons` 세트**(`https://api.iconify.design/game-icons/{name}.svg`) — 판타지/화투 톤 일치, 단색 SVG라 토큰 색으로 틴트 가능.
- 세트 **혼용 금지**(game-icons로 통일)해 선·디테일 밀도를 맞춘다.

## 4. 아이콘 매핑 (컨셉 일치 — game-icons)
| 대상 | Iconify 이름 | 색 |
|---|---|---|
| 광(Radiance) | `game-icons:sun` | Radiance |
| 위엄(Majesty) | `game-icons:tiger-head` | Majesty |
| 독(Poison) | `game-icons:poison-bottle` | Poison |
| 약화(Weak) | `game-icons:broken-bone` | Weak |
| 취약(Vulnerable) | `game-icons:cracked-shield` | Vulnerable |
| 재생(Regen) | `game-icons:health-normal` | Heal |
| 가시(Thorns) | `game-icons:thorns` | Thorns |
| 민첩(Dexterity) | `game-icons:run` | Defense |
| 공격 인텐트 | `game-icons:bloody-sword` | Danger |
| 방어 인텐트 | `game-icons:shield` | Defense |
| HP | `game-icons:heart` | Danger |
| 골드 | `game-icons:coins` | Gold |
| 에너지 | `game-icons:lightning-arc` | Spirit |

> 위 이름은 후보(적용 시 Iconify에서 실제 존재 확인 후 확정). 새 매핑은 이 표에 추가해 휘발 방지.

## 5. 사용 규칙
1. 색은 `DesignTokens.X` 참조. status는 `StatusColor(type)`.
2. 아이콘은 3규격 크기 고정 + game-icons 세트 통일.
3. 텍스트는 `Font*` 스케일 중 하나만(임의 크기 금지).
4. 토큰에 없는 값이 필요하면 **토큰을 먼저 추가**하고 참조(하드코딩 금지).

# 자연어로 Unity 게임 만들기 — 제로부터 따라하기 (PRO 플랜)

> **이 문서 사용법**
> 컴퓨터에 아무것도 없는 상태에서, **Claude 데스크톱 앱 안의 Code 탭**으로 Unity 게임을
> 자연어(한국어)만으로 개발하기 위한 전체 세팅 순서다.
> Claude **PRO** 플랜 기준으로 작성했다.
>
> **따라하는 방법 2가지**
> 1. 사람이 위에서부터 한 줄씩 따라한다.
> 2. **이 .md 파일 전체를 복사해서, 설치를 끝낸 Claude Code(또는 데스크톱 앱 Code 탭)에 붙여넣는다.**
>    그러면 Claude가 아래 "Claude에게" 지시를 읽고 단계별 코치 역할을 한다.

---

## 🤖 Claude에게 (이 문서를 통째로 받았다면 먼저 읽을 것)

너는 지금부터 **Unity 자연어 개발 환경 세팅 코치**다. 다음 규칙을 지켜라.

1. **한 번에 한 Phase(또는 한 Step)씩만** 안내한다. 절대 전체를 한꺼번에 쏟지 마라.
2. 각 Step마다 (1) 사용자가 할 일 → (2) **이 Step의 검증 1줄**("~가 보이면 성공")을 준다.
3. 사용자가 "다음" / "됐어" / "OK" 라고 하면 그때 다음 Step으로 간다.
4. 막히면 해당 Step의 트러블슈팅(문서 맨 아래)을 먼저 확인하고, 콘솔/에러 메시지를 그대로 달라고 요청한다.
5. 너는 PowerShell 명령 실행, 파일 복사, 폴더 구조 확인 같은 **로컬 작업을 직접 도울 수 있다.** 단, 결제·앱 설치·Unity 클릭·Play 체감 검증은 사용자가 한다.
6. 목표는 마지막 **Phase 6 — 첫 자연어 개발**까지 사용자를 데려가는 것이다.

---

## 0. 준비물 한눈에 (전체 체크리스트)

```
[ ] Claude PRO 구독 (claude.ai)
[ ] Claude 데스크톱 앱 (Windows)
[ ] Git for Windows           ← Code 탭 + 훅 동작에 필수
[ ] uv (파이썬 러너)           ← Unity MCP 서버 구동
[ ] Unity Hub + Unity 6.3 LTS
[ ] Unity 새 프로젝트
[ ] .claude 폴더 + CLAUDE.md   ← "우리 세팅"(에이전트/스킬/훅) — 친구한테 폴더째 받기
[ ] Unity MCP 패키지 + Claude Code 클라이언트 설정
[ ] 연결 확인 → 첫 명령
```

소요 시간: 다운로드 포함 약 40~60분.

---

## Phase 1 — 결제 & 데스크톱 앱

### Step 1-1. PRO 결제
- [claude.ai](https://claude.ai) 가입/로그인 → 계정 → **Upgrade** → **Pro($20/월)** 결제.
- 이 계정으로 이후 데스크톱 앱에 로그인한다.
- 참고: Unity MCP 작업은 토큰을 많이 쓴다. PRO는 한도가 빨리 차므로 "한 기능 = 한 채팅"으로 쪼개 쓰는 게 좋다(뒤에서 다시 설명).
- **검증**: 계정 화면에 "Pro" 배지가 보이면 성공.

### Step 1-2. Claude 데스크톱 앱 설치 (Windows)
- [claude.com/download](https://claude.com/download) 에서 **Windows(x64)** 설치.
  - (ARM 노트북이면 ARM64 설치본을 받는다.)
- 앱은 탭이 3개다: **Chat / Cowork / Code**. 우리가 쓸 건 **Code** 탭.
- **검증**: 앱이 실행되고 상단/사이드에 Chat·Cowork·Code 탭이 보이면 성공.

### Step 1-3. Git for Windows 설치 (필수)
- Code 탭은 내부적으로 Git Bash를 쓴다. **이게 없으면 Code 탭도, 우리 세팅의 훅(.sh)도 안 돈다.**
- [git-scm.com/downloads/win](https://git-scm.com/downloads/win) 에서 설치(옵션 전부 기본값).
- **설치 후 Claude 데스크톱 앱을 완전히 종료했다가 다시 켠다.**
- **검증**: PowerShell에서 `git --version` 이 버전을 출력하면 성공.

### Step 1-4. 로그인 + Code 탭 진입
- 데스크톱 앱 실행 → **PRO 계정으로 로그인** → **Code 탭** 클릭.
- Code 탭에서 "대화 1개 = 세션 1개"이고, 각 세션은 **자기 프로젝트 폴더 + 채팅 + 코드 변경**을 따로 가진다.
- **검증**: Code 탭에서 새 세션을 만들 수 있고, 채팅 입력창이 보이면 성공.

> 여기까지가 "Claude Code를 데스크톱 앱에서 쓰는" 기본 환경이다.

---

## Phase 2 — 사전 도구 (uv)

PowerShell을 열고(시작 → "PowerShell" → 엔터) 아래 한 줄을 붙여넣는다.

```powershell
irm https://astral.sh/uv/install.ps1 | iex
```

- `uv`는 Unity MCP 서버(파이썬)를 실행하는 러너다.
- Python 3.10+ 이 필요하지만, 없으면 **Unity MCP 셋업 위저드가 안내**해준다(uv가 파이썬을 관리하기도 한다).
- 설치 후 **PowerShell 창을 새로 연다**(PATH 갱신).
- **검증**: 새 PowerShell에서 `uv --version` 이 버전을 출력하면 성공.

> Node.js / npm 은 필요 없다.

---

## Phase 3 — Unity 설치 + 프로젝트 생성

### Step 3-1. Unity Hub + 에디터
- [unity.com/download](https://unity.com/download) 에서 **Unity Hub** 설치.
- Hub → **Installs → Install Editor → Unity 6.3 LTS**(이 프로젝트는 `6000.3.10f1`) 설치.
  - 모듈: **Windows Build Support** 정도면 충분(나중에 추가 가능).
- Hub에서 무료 **Personal** 라이선스 활성화.
- **검증**: Hub의 Installs 목록에 6.3 에디터가 뜨면 성공.

### Step 3-2. 새 프로젝트
- Hub → **Projects → New project → Universal 2D**(또는 원하는 템플릿) → 위치 지정 → **Create**.
- 이 **프로젝트 폴더 경로를 기억**한다(예: `C:\Users\내이름\내게임`). 뒤에서 계속 쓴다.
- **검증**: Unity 에디터가 열리고 빈 씬이 보이면 성공.

> ⚠️ Unity **공식 MCP는 무료(Personal) 라이선스에서 외부 연결이 막힌다.** 그래서 무료로 잘 도는
> **CoplayDev unity-mcp**를 쓴다(Phase 5). 이게 이 프로젝트가 실제로 쓰는 방식이다.

---

## Phase 4 — "우리 세팅"(에이전트 + 스킬 + 훅) 이식

여기가 핵심이다. 이 작업을 하면 친구의 새 프로젝트도 **이 프로젝트와 동일한
47개 전문 에이전트 + 40여 개 스킬 + 자동화 훅 + 컨벤션 문서**를 그대로 갖춘다.

### 무엇을 이식하나 (이 프로젝트 `.claude/` 구성)
| 폴더/파일 | 내용 | 개수 |
|---|---|---|
| `.claude/agents/` | 전문 서브에이전트(unity-csharp, unity-architect, game-designer …). godot 계열은 `_disabled/`에 격리 | 47개 |
| `.claude/skills/` | 워크플로 스킬(team-combat, design-system, balance-check, code-review …) | 40여 개 |
| `.claude/hooks/` | 자동화 훅(session-start, auto-commit, validate-commit, detect-gaps …, 전부 `.sh`) | 9개 |
| `.claude/rules/` | 코드 규칙(ai-code, engine-code, data-files …) | 5개 |
| `.claude/docs/` | 컨벤션/워크플로 문서(naming, coding-standards, mcp-capabilities …) | 다수 |
| `.claude/settings.json` | 훅 등록 + 권한(allow/deny) + statusLine | 1개 |
| `CLAUDE.md` | 마스터 설정(게임 컨셉/기술 스택/규약을 `@`로 docs 참조) | 1개 |

> 이 47개 에이전트 내용을 맨손으로 다시 만들 수는 없다. **폴더째 복사**가 정답이다.

### Step 4-1. 폴더 받기 → 새 프로젝트에 복사
1. 친구(이 프로젝트 보유자)에게서 **`.claude/` 폴더 통째 + 루트의 `CLAUDE.md`** 를 받는다(zip 권장).
2. Phase 3에서 만든 **Unity 프로젝트 폴더 루트**(`내게임\`)에 그대로 붙여넣는다.
   - 결과: `내게임\.claude\…` 와 `내게임\CLAUDE.md` 가 존재.
- **검증**: 새 프로젝트 폴더 안에 `.claude` 폴더와 `CLAUDE.md` 가 보이면 성공.

### Step 4-2. 새 프로젝트를 git 저장소로 초기화
- 우리 훅 중 `auto-commit.sh`(작업 끝나면 자동 커밋)·`validate-commit.sh` 가 git을 전제로 한다.
- 프로젝트 폴더에서 PowerShell:
  ```powershell
  cd C:\Users\내이름\내게임
  git init
  ```
- (선택) Unity용 `.gitignore` 추가 — 이건 Code 탭에서 Claude에게 "Unity .gitignore 만들어줘"라고 시키면 된다.
- **검증**: 폴더에 `.git` 이 생기면 성공.

### Step 4-3. Code 탭에서 이 프로젝트 폴더 열기
- 데스크톱 앱 **Code 탭 → 새 세션 → 프로젝트 폴더로 `내게임` 선택**.
- Claude Code는 폴더 루트의 `CLAUDE.md` 와 `.claude/` 를 **자동으로 읽는다**(에이전트·스킬·훅·규약이 즉시 활성화).
- **검증**: 세션을 시작하고 "지금 이 프로젝트의 CLAUDE.md 요약해줘"라고 물었을 때, 화투/게임 스튜디오 설정을 인식하면 성공.

### Step 4-4. CLAUDE.md 를 "친구 게임"에 맞게 고치기
- 받은 `CLAUDE.md` 는 **화투 게임 전용**(컨셉·경로 등)이다. 새 게임에 맞게 바꿔야 한다.
- Code 탭에서 그냥 자연어로 시킨다:
  > "CLAUDE.md의 게임 컨셉을 '[내 게임 한 줄 설명]'으로 바꾸고, 화투 특화 내용은 새 게임에 맞게 정리해줘. 기술 스택(Unity 6.3 / URP 2D)·컨벤션·워크플로는 유지해."
- **검증**: CLAUDE.md 상단 게임 컨셉이 내 게임으로 바뀌면 성공.

---

## Phase 5 — Unity MCP 연결 (자연어 ↔ Unity 다리)

전제: Unity 2021.3+ (여기선 6.3), Python 3.10+, uv(Phase 2 완료).

### Step 5-1. Unity MCP 패키지 설치
- Unity 에디터 → **Window → Package Manager**.
- 좌측 상단 **`+` → Add package from git URL…** → 아래 붙여넣고 **Add**:
  ```
  https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
  ```
- **검증**: Package Manager 목록에 "MCP for Unity"가 보이면 성공.

### Step 5-2. 셋업 위저드 → Claude Code 설정
- 임포트가 끝나면 **셋업 위저드가 자동으로 뜬다.**
  1. **Python / uv** 항목이 **초록색**이 될 때까지 위저드 안내를 따른다(uv는 Phase 2에서 깔았으니 대개 바로 초록).
  2. 의존성이 초록이면, 감지된 MCP 클라이언트 목록이 나온다. **"Claude Code"를 선택 → Configure Selected.**
     - 메뉴로도 가능: **Window → MCP for Unity → Configure All Detected Clients.**
- **참고**: Claude Code는 HTTP 연결을 지원해 설정 후 자동 연결된다. (데스크톱 앱 Code 탭은 Claude Code 엔진이라 이 설정을 공유한다.)
- **검증**: 위저드에서 Claude Code 항목이 "Configured/Connected" 상태가 되면 성공.

### Step 5-3. 재시작 + 연결 확인
1. **Unity 에디터는 켜 둔 채**, **데스크톱 앱 Code 탭 세션을 재시작**(MCP는 세션 시작 시 붙는다).
2. Code 탭에서 입력:
   > "Unity 콘솔에 'hello' 로그 하나 찍어줘" 또는 "지금 씬 계층 구조 보여줘"
3. Claude가 실제 Unity 정보를 읽거나 콘솔에 로그를 남기면 **다리 연결 성공.**
- **검증**: Unity Console 또는 Hierarchy에 Claude가 만든 변화가 실제로 나타나면 성공.

---

## Phase 6 — 첫 자연어 개발 🎮

이제 한국어로 시키면 된다. 예시:

- "바닥 평면이랑 그 위에 빨간 큐브 하나 만들어줘"
- "WASD로 큐브 움직이는 스크립트 짜서 붙여줘"
- "플레이 모드 켜서 에러 없는지 확인해줘"
- "이 게임 핵심 루프를 game-designer 에이전트로 설계해줘" (← 이식한 에이전트 활용)

역할 분담(이 프로젝트의 협업 방식 그대로):
- **사람** = PM/결정자. 방향 결정 + 검토/승인 + **Play 체감 테스트**(재미·조작감) + 외부 자산(그림·소리·모델) 임포트.
- **AI** = 풀스택 개발자. 코드·씬·컴포넌트·프리팹·커밋을 직접 처리.

---

## 🔧 막히면 (트러블슈팅)

| 증상 | 원인 / 해결 |
|---|---|
| **Claude가 Unity를 못 본다 / MCP가 끊겼다** | **Unity 에디터가 켜져 있고 창이 포커스(맨 앞)인지 확인.** Unity가 닫히거나 백그라운드로 가면 MCP relay가 끊긴다. 창을 한 번 클릭해 활성화하면 도메인 리로드/연결이 복구된다. (실제로 이 가이드를 만든 세션에서도 Unity가 포커스를 잃자 MCP 도구가 끊겼다.) |
| **Code 탭이 안 열리거나 명령이 안 돈다** | Git for Windows 설치 후 **앱을 재시작**했는지 확인(Step 1-3). `git --version` 확인. |
| **위저드에서 Python/uv가 빨간색** | PowerShell에서 `uv --version` 확인. 안 되면 Phase 2 재실행 후 새 창에서 위저드 다시. |
| **훅(자동 커밋 등)이 안 돈다** | `.sh` 훅은 Git Bash가 필요(Git for Windows). 그리고 프로젝트가 `git init` 됐는지 확인(Step 4-2). |
| **PRO 사용량 한도가 빨리 찬다** | "한 기능 = 한 채팅(세션)"으로 쪼갠다. 긴 대화는 `/compact` 또는 새 세션. 무거운 작업이 잦으면 Max로 업그레이드 검토. |
| **에이전트/스킬이 안 보인다** | `.claude/` 폴더가 **프로젝트 루트**에 있고, Code 탭에서 그 폴더를 열었는지 확인(Step 4-1, 4-3). |
| **공식 Unity MCP를 쓰려다 막힘** | Personal 라이선스는 공식 MCP 외부 연결이 막힌다. **CoplayDev unity-mcp**를 쓴다(Phase 5). |

---

## 📎 부록 — "우리 세팅"이 정확히 무엇인가

이 프로젝트(= 친구가 받는 `.claude` + `CLAUDE.md`)의 구성 요약:

- **에이전트 47개**: `unity-csharp`(C# 구현), `unity-architect`(구조/아키텍처), `unity-debugger`(에러/성능), `game-designer`, `producer`, `qa-tester` … 도메인별 전문가. godot 4종은 비활성화 격리.
- **스킬 40여 개**: `team-combat`·`team-ui` 같은 팀 오케스트레이션, `design-system`(GDD 작성), `balance-check`, `code-review`, `architecture-decision`(ADR), `project-stage-detect` 등.
- **훅 9개(자동화)**: `session-start`(세션 시작 시 git 상태 표시), `auto-commit`(작업 끝나면 자동 커밋), `validate-commit`/`validate-push`(위험 명령 차단), `detect-gaps`, `pre-compact`, `log-agent`, `session-stop`.
- **settings.json**: 위 훅 등록 + 권한 정책(`rm -rf`·`git reset --hard`·`.env` 접근 등 차단) + statusLine.
- **docs 컨벤션**: 네이밍/코딩 표준, B타입 워크플로(1인 PM + AI), MCP 능력 경계, 에디터 핸드오프 양식 등. → CLAUDE.md가 `@`로 불러온다.

> 새 게임에서는 `CLAUDE.md`의 **게임 컨셉/경로만** 갈아끼우면 되고(Step 4-4),
> 에이전트·스킬·훅·컨벤션은 그대로 재사용된다.

---

### 빠른 순서 요약
`PRO 결제 → 데스크톱 앱 + Git → uv → Unity 6.3 + 새 프로젝트 → .claude/CLAUDE.md 복사 + git init → Code 탭에서 폴더 열기 → CLAUDE.md 내 게임으로 수정 → Unity MCP 패키지 + Claude Code 설정 → Unity 켠 채 세션 재시작 → "씬 보여줘"로 확인 → 개발 시작`

# Task Execute

Execute a task from the FlappyKookaburra task board autonomously.

## Input

Task ID: $ARGUMENTS (e.g., T001, T014)

## Process

### 1. Load Task Spec
Read the task specification from `../flappykookaburra-docs/tasks/`:
- Phase 1: `tasks/phase-1/T001-*.md` through `T005-*.md`
- Phase 2: `tasks/phase-2/T006-*.md` through `T009-*.md`
- Phase 3: `tasks/phase-3/T010-*.md` through `T015-*.md`
- Phase 4: `tasks/phase-4/T016-*.md` through `T020-*.md`

### 2. Check Dependencies
Read `../flappykookaburra-docs/TASK_BOARD.md` and verify all "Depends On" tasks are marked DONE. If any are not, report which dependencies are missing and stop.

### 3. Understand Context
- Read `../flappykookaburra-docs/architecture/system-overview.md` for system architecture
- Read existing code files that will be modified
- Read related files to understand patterns in use
- Read `.claude/CLAUDE.md` for project conventions

### 4. Plan Implementation
Before writing any code:
- List all files to create/modify
- Identify the build sequence (what to implement first)
- Note any decisions that need user input

### 5. Execute
Implement the task following FlappyKookaburra conventions:
- Singleton pattern for managers (see CLAUDE.md)
- Events for system communication (Action delegates)
- ScriptableObjects for all tunable values
- ObjectPool for spawned objects
- [RequireComponent] for mandatory dependencies
- Cache GetComponent in Awake
- Animator.StringToHash for parameter IDs
- CompareTag() for tag checks
- Private fields with underscore prefix
- One class per file, PascalCase naming

### 6. Verify
- Open Unity Editor — no compile errors
- Press Play — no runtime exceptions
- Check Inspector — all references assigned
- Test the specific mechanic described in acceptance criteria

### 7. Report
Output a summary:
- What was implemented
- Files created/modified
- Any decisions made
- What to test manually
- Suggested next steps

## Important
- Always read the full task spec before starting
- Follow the acceptance criteria exactly
- Don't modify files outside the task's scope
- Create a git branch: `feat/{task-id}-{short-name}` (e.g., `feat/T001-unity-project-setup`)
- Commit format: `[Phase X] TXXX: Brief description`

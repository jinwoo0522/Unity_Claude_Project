# PLAN.md

## 발견된 결함
- **Player_Move 수평 이동 게이트 누락** (중요): Step 5는 "스킬 중 `Player_Move` 이동은 게이트로 정지 전제"라 했으나, Step 4에 `PlayerMove()`의 수평 이동 차단 게이트가 빠져 계획이 모순됨. Golem 마우스 스킬(루트모션) 중 WASD 입력 시 입력 이동과 루트모션 이동이 중복 적용되어 미끄러짐·간섭 물리 버그 발생.
  - 조치: Step 4에 `PlayerMove()` 수평 이동 차단(`skill.IsSkilling` 시 수평 0, 중력 유지) 게이트 추가, Step 5 리스크 문구 정합화.

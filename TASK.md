## Golem_Player 리팩토링하기

C:\Unity\Golem_VS_Magician_Agents_1\Assets\Prefabs\Player\Golem_Player.prefab : 골렘프리펩
C:\Unity\Golem_VS_Magician_Agents_1\Assets\Prefabs\Player\Elf_Player.prefab : 엘프 프리펩
C:\Unity\Golem_VS_Magician_Agents_1\Assets\Resources\Data\AnimData\AnimData.cs : 애니메이션 데이터

Elf_Player를 만들면서 플레이어의 모든 기능을 리펙토링 했습니다.
스킬 부분은 아직 리펙토링 하지 않았습니다.

Elf_Player는 상태머신과 기능을 전부 분리하여 작성했습니다.
Golem_Player는 Elf_Player를 보고 기본적인 움직임만 다시 리펙토링 해주세요
Golem_Player가 사용하고있던 컴포넌트 대부분이 삭제되어 있을겁니다.

애니메이터의 상태들도 전부 가공하고 Data도 작성해주세요

상체 움직임도 구현이 가능하다면 해주세요
상체는 아마 새로운 상태를 만들고 트랜지션을 추가해야할것입니다.
하체움직임의 경우에는 중복되는 것을 사용하고 필요하면 생성해주세요 

아마 착지 애니메이션이 존재하지 않을것인데 일단 상태만 만들고 공석으로 나둬도 됩니다.
최대한 작성된 상태들을 보고 참고해서 만드는 것을 추천합니다.
최대한 작성된 레거시 코드를 건들이지 않는 선에서 추가해주세요





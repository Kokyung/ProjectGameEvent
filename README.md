# Project GameEvent
게임이벤트 구조 사용성 개선 작업

## 핵심 아이디어의 시작
- https://www.youtube.com/watch?v=lgA8KirhLEU
- https://www.youtube.com/watch?v=6vmRwLYWNRo

## 기본 개념 설명

**보통 컨텐츠 내 이벤트와 UI 및 조작 로직은 종속되어 있습니다.**

때문에 Object.FindObjectOfType 메서드나 Monobehaviour.GetComponent 메서드를 사용하여 해당 컴포넌트를 찾아 이벤트 호출 함수를 등록하거나 호출하는것이 일반적입니다.

하지만 그렇게 구현할 경우 코드가 지저분해지거나 상기 메서드 사용으로 인한 성능 저하를 유발하게 됩니다.

그리고 게임 디자이너가 직접 Scene 구성을 하는 경우 이벤트 작동을 위해 프로그래머가 다시 참여해야하는 등 추가적인 작업이 필요해지게 됩니다.

Game Event Architecture는 그러한 이슈를 해결하는 방법입니다.

**Game Event는 이벤트 등록과 호출 주체를 Scriptable Object가 되어 수행함으로서 종속성을 분리시킵니다.**

Game Event Scriptable Object에 등록할 이벤트는 Game Event Listener 컴포넌트를 통해 등록하고

이벤트 호출은 Game Event Scriptable Object를 참조하여 GameEvent.Raise를 호출하므로서 수행됩니다.

프로젝트에 포함된 데모는 종속성 분리 관점에서 기존 방식과 Game Event의 차이를 설명합니다.

## 부가 기능

* Game Event Explorer
  * 사용시 Game Event 에셋이 많아졌을때 관리용으로 제작
  * **위치: 에디터 상단 메뉴 BKK/Game Event/Game Event Explorer**

* Game Event Generator
  * 커스텀 타입을 파라미터로 갖는 게임이벤트를 생성할때 사용
  * 해당 윈도우 없이 GameEvent<> 클래스를 상속 받아서 직접 작성하는 것도 가능.
  * **위치: 에디터 상단 메뉴 BKK/Game Event/Game Event Generator**

## * 앞으로 할 것
  - 디버그 로그 On Off 기능 추가

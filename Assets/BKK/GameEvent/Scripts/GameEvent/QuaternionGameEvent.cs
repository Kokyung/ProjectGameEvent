// 게임 이벤트 생성 메뉴에 의해 생성되었습니다.
using UnityEngine;

namespace BKK.GameEventArchitecture
{
    [CreateAssetMenu(menuName = "BKK/Game Event Architecture/Quaternion Game Event", fileName = "New Quaternion Game Event", order = 100)]
    public class QuaternionGameEvent : GameEvent<Quaternion>
    {
    }
}

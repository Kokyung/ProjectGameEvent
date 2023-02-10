// 게임 이벤트 생성 메뉴에 의해 생성되었습니다.
using UnityEngine;

namespace BKK.GameEventArchitecture
{
    [CreateAssetMenu(menuName = "BKK/Game Event Architecture/Texture Game Event", fileName = "New Texture Game Event", order = 100)]
    public class TextureGameEvent : GameEvent<Texture>
    {
    }
}

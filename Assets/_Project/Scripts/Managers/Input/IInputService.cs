using UnityEngine;

namespace Suhdo.Managers.Input
{
    public interface IInputService
    {
        Vector2 MoveDirection { get; }
        bool IsMoving { get; }
    }
}

using Reflex.Attributes;
using Suhdo.Managers.Input;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

namespace Suhdo.UI.JoystickUIController
{
	public class JoystickUIController : MonoBehaviour
	{
		[SerializeField] private OnScreenStick _screenStick;
		
		private IInputService _inputService;

		[Inject]
		public void Construct(IInputService inputService)
		{
			_inputService = inputService;
		}

		private void OnEnable()
		{
			
		}

		private void Update()
		{
			if (_inputService.IsMoving)
			{
				Debug.Log(_inputService.MoveDirection);
			}
		}
	}
}
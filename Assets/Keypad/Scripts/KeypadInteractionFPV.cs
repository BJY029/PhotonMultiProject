using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavKeypad
{
	public class KeypadInteractionFPV : MonoBehaviour
	{
		//발사할 Ray 거리
		private float RayDistance = 5f;
		[SerializeField] //탐지할 레이어
		private LayerMask keypadButtonLayer;

		public Camera cam; //사용할 카메라

		public GameObject parentKeyPad;

		private void Update()
		{
			//상호작용 중이 아니라면 Ray를 감지하지 않는다.
			if (!parentKeyPad.GetComponent<SwitchToKeypad>().isInteraction) return;
			//카메라 방향으로 Ray를 발사한다.
			var ray = cam.ScreenPointToRay(Input.mousePosition);
			//디버깅 용 Ray를 그리는 함수
			Debug.DrawRay(ray.origin, ray.direction, Color.red);
			//좌클릭이 발생한 경우
			if (Input.GetMouseButtonDown(0))
			{
				//해당 조건에 맞게 Ray를 발생
				if (Physics.Raycast(ray, out var hit, RayDistance, keypadButtonLayer))
				{
					//만약 충돌한 collider가 키패드 버튼이면
					if (hit.collider.TryGetComponent(out KeypadButton keypadButton))
					{
						//해당 버튼을 누르는 효과를 지닌 함수 호출
						keypadButton.PressButton();
					}
				}
			}
		}
	}
}
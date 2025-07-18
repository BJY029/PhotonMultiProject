using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace NavKeypad
{
    public class Keypad : MonoBehaviourPun
    {
        //각 상황에 발생시킬 이벤트들
        [Header("Events")]
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;
        [Header("Combination Code (9 Numbers Max)")]
        [SerializeField] private int keypadCombo = 12345;

        //이벤트 연결
        public UnityEvent OnAccessGranted => onAccessGranted;
        public UnityEvent OnAccessDenied => onAccessDenied;

        //성공 및 실패시 출력할 텍스트 정의
        [Header("Settings")]
        [SerializeField] private string accessGrantedText = "Granted";
        [SerializeField] private string accessDeniedText = "Denied";

        //출력될 텍스트 시간 및 UI 설정
        [Header("Visuals")]
        [SerializeField] private float displayResultTime = 1f;
        [Range(0, 5)]
        [SerializeField] private float screenIntensity = 2.5f;
        [Header("Colors")]
        [SerializeField] private Color screenNormalColor = new Color(0.98f, 0.50f, 0.032f, 1f); //orangy
        [SerializeField] private Color screenDeniedColor = new Color(1f, 0f, 0f, 1f); //red
        [SerializeField] private Color screenGrantedColor = new Color(0f, 0.62f, 0.07f); //greenish
        [Header("SoundFx")]
        [SerializeField] private AudioClip buttonClickedSfx;
        [SerializeField] private AudioClip accessDeniedSfx;
        [SerializeField] private AudioClip accessGrantedSfx;
        [Header("Component References")]
        [SerializeField] private Renderer panelMesh;
        [SerializeField] private TMP_Text keypadDisplayText;
        [SerializeField] private AudioSource audioSource;


        private string currentInput;
        private bool displayingResult = false;
        private bool accessWasGranted = false;

        //RPC를 통해서 해당 키패드의 내용들을 초기화한다.
        private void Awake()
        {
			photonView.RPC(nameof(RPC_ClearInput), RpcTarget.All);
			panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);
        }

        public void setKeypadCombo(int value)
        {
            keypadCombo = value;
        }

        //특정 입력이 발생되었을 때 호출될 함수
        //Gets value from pressedbutton
        public void AddInput(string input)
        {
            audioSource.PlayOneShot(buttonClickedSfx);
            if (displayingResult || accessWasGranted) return;
            switch (input)
            {
                //enter 키가 입력된 경우
                case "enter":
                    CheckCombo(); //답을 확인
                    break;
                default: //다른 키인 경우, 최대 9자리 까지만 입력 받는다.
                    if (currentInput != null && currentInput.Length == 9) // 9 max passcode size 
                    {
                        return;
                    }
                    currentInput += input;
                    keypadDisplayText.text = currentInput;
                    break;
            }

        }
        //답을 확인하고 그에 맞는 이벤트들을 실행하는 함수
        public void CheckCombo()
        {
            if (int.TryParse(currentInput, out var currentKombo))
            {
                bool granted = currentKombo == keypadCombo;
                if (!displayingResult)
                {
                    StartCoroutine(DisplayResultRoutine(granted));
                }
            }
            else
            {
                Debug.LogWarning("Couldn't process input for some reason..");
            }

        }

        //답 여부에 따라 실행될 이벤트를 관리하는 코루틴
        //mainly for animations 
        private IEnumerator DisplayResultRoutine(bool granted)
        {
            displayingResult = true;

            if (granted) //답이 맞을 경우, RPC로 ACCESSGRAnted 함수를 실행
                photonView.RPC(nameof(RPC_AccessGranted), RpcTarget.All);
            else //답이 틀릴 경우 RPC로 다음 함수 실행
                photonView.RPC(nameof(RPC_AccessDenied), RpcTarget.All);

            yield return new WaitForSeconds(displayResultTime);
            displayingResult = false;
            if (granted) yield break; //답이 맞을 경우 그대로 둔다.
            photonView.RPC(nameof(RPC_ClearInput), RpcTarget.All); //답이 틀린 경우 적힌 값들 초기화
            panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);

        }

        [PunRPC] //답이 맞는 경우 호출되는 RPC 함수
        private void RPC_AccessDenied()
        {
            keypadDisplayText.text = accessDeniedText;
            onAccessDenied?.Invoke(); //연결된 이벤트를 실행한다. 해당 이벤트에는 관련 애니메이션이 들어가있다.
            panelMesh.material.SetVector("_EmissionColor", screenDeniedColor * screenIntensity);
            audioSource.PlayOneShot(accessDeniedSfx);
        }

        [PunRPC] //텍스트를 초기화 하는 RPC 함수
        private void RPC_ClearInput()
        {
            currentInput = "";
            keypadDisplayText.text = currentInput;
        }

        [PunRPC]//답이 틀릴 경우 호출되는 RPC 함수
        private void RPC_AccessGranted()
        {
            accessWasGranted = true;
            keypadDisplayText.text = accessGrantedText;
            onAccessGranted?.Invoke(); //관련된 애니메이션을 실행시키는 이벤트 invoke
            panelMesh.material.SetVector("_EmissionColor", screenGrantedColor * screenIntensity);
            audioSource.PlayOneShot(accessGrantedSfx);
        }

    }
}
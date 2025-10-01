using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CommandType { Move }
public struct Command { public CommandType commandType; public Vector3 direction; }

public class EkkoController : MonoBehaviour
{
    [Header("플레이어 설정")]
    public float moveSpeed = 15.0f;

    [Header("시간 역행 설정")]
    public float historyDuration = 3.0f;
    public float recordInterval = 0.1f;
    public float rewindMoveSpeed = 25.0f;

    [Header("Material 설정")]
    public Material normalMaterial;
    public Material rewindMaterial;

    [Header("UI 설정")]
    public Text commandCountText;
    public Button playButton;
    public Button recordButton;
    public Button ekkoButton;

    private Queue<Command> commandQueue = new Queue<Command>();
    private Queue<Vector3> positionHistory = new Queue<Vector3>();

    private Renderer playerRenderer;
    private bool isReplaying = false;
    private bool isRewinding = false;
    private bool isRecording = false;
    private Text recordButtonText;

    void Start()
    {
        playerRenderer = GetComponent<Renderer>();
        playerRenderer.material = normalMaterial;
        StartCoroutine(RecordPositionRoutine());
        if (recordButton != null) recordButtonText = recordButton.GetComponentInChildren<Text>();
        recordButton.onClick.AddListener(ToggleRecording);
        playButton.onClick.AddListener(StartReplay);
        ekkoButton.onClick.AddListener(TriggerRewind);
        UpdateUI();
    }

    void Update()
    {
        if (isRewinding || isReplaying) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isReplaying) TriggerRewind();
            return;
        }
        if (isRecording) HandleMoveInput();
    }

    // ========================================================================
    // ★★★★★ 여기가 수정된 최종 '빨리 감기' 로직입니다 ★★★★★
    // ========================================================================
    private IEnumerator ReplayCommandsRoutine()
    {
        isReplaying = true;
        UpdateUI();

        int fastForwardCount = 0;

        while (commandQueue.Count > 0)
        {
            // R키를 누르면 빨리 감기 카운트 설정
            if (Input.GetKeyDown(KeyCode.R) && fastForwardCount <= 0)
            {
                Debug.Log("R키 입력! 약 2초간 빨리 감기를 시작합니다.");
                float fastForwardDuration = 2.0f;
                float singleCommandDuration = 0.2f;
                fastForwardCount = Mathf.CeilToInt(fastForwardDuration / singleCommandDuration);
            }

            Command currentCommand = commandQueue.Dequeue();
            UpdateUI(); // 명령을 꺼내자마자 UI를 업데이트해서 숫자가 바로 줄어듦

            if (currentCommand.commandType == CommandType.Move)
            {
                float moveDuration = 0.2f;

                // 빨리 감기 모드일 경우: 애니메이션 없이 즉시 이동!
                if (fastForwardCount > 0)
                {
                    playerRenderer.material = rewindMaterial;
                    // 이동 결과를 한 번에 적용
                    transform.Translate(currentCommand.direction * moveSpeed * moveDuration);
                    fastForwardCount--;
                    yield return null; // 화면이 멈추지 않도록 한 프레임만 대기
                }
                // 일반 재생 모드일 경우: 부드럽게 이동
                else
                {
                    playerRenderer.material = normalMaterial;
                    float elapsedTime = 0f;
                    while (elapsedTime < moveDuration)
                    {
                        // 부드러운 이동 중에도 R키 입력 감지
                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            float fastForwardDuration = 2.0f;
                            fastForwardCount = Mathf.CeilToInt(fastForwardDuration / moveDuration);
                            break; // 현재 이동을 중단하고 다음부터 빨리 감기 시작
                        }

                        transform.Translate(currentCommand.direction * moveSpeed * Time.deltaTime);
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                }
            }
        }

        playerRenderer.material = normalMaterial;
        isReplaying = false;
        UpdateUI();
    }

    // (이하 다른 함수들은 이전과 동일합니다)
    public void TriggerRewind() { if (positionHistory.Count > 0 && !isRewinding && !isReplaying) { isRecording = false; StartCoroutine(RewindRoutine()); } }
    private void HandleMoveInput() { float moveX = Input.GetAxisRaw("Horizontal"); float moveZ = Input.GetAxisRaw("Vertical"); if (moveX != 0 || moveZ != 0) { Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized; commandQueue.Enqueue(new Command { commandType = CommandType.Move, direction = moveDirection }); UpdateUI(); } }
    public void ToggleRecording() { isRecording = !isRecording; UpdateUI(); }
    public void StartReplay() { if (commandQueue.Count > 0 && !isReplaying) { isRecording = false; StartCoroutine(ReplayCommandsRoutine()); } }
    private void UpdateUI() { if (commandCountText != null) commandCountText.text = $"명령 수: {commandQueue.Count}"; if (recordButtonText != null) recordButtonText.text = isRecording ? "녹화 중지" : "녹화 시작"; bool isBusy = isReplaying || isReplaying; if (playButton != null) playButton.interactable = commandQueue.Count > 0 && !isRecording && !isBusy; if (ekkoButton != null) ekkoButton.interactable = positionHistory.Count > 0 && !isBusy; if (recordButton != null) recordButton.interactable = !isBusy; }
    private IEnumerator RecordPositionRoutine() { while (true) { if (!isRewinding) { positionHistory.Enqueue(transform.position); int maxHistorySize = Mathf.RoundToInt(historyDuration / recordInterval); while (positionHistory.Count > maxHistorySize) positionHistory.Dequeue(); } yield return new WaitForSeconds(recordInterval); } }
    private IEnumerator RewindRoutine() { isRewinding = true; UpdateUI(); playerRenderer.material = rewindMaterial; Stack<Vector3> rewindStack = new Stack<Vector3>(); foreach (Vector3 pos in positionHistory) rewindStack.Push(pos); positionHistory.Clear(); while (rewindStack.Count > 0) { Vector3 targetPosition = rewindStack.Pop(); while (Vector3.Distance(transform.position, targetPosition) > 0.01f) { transform.position = Vector3.MoveTowards(transform.position, targetPosition, rewindMoveSpeed * Time.deltaTime); yield return null; } } playerRenderer.material = normalMaterial; isReplaying = false; UpdateUI(); }
}
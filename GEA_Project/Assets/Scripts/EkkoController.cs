using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CommandType { Move }
public struct Command { public CommandType commandType; public Vector3 direction; }

public class EkkoController : MonoBehaviour
{
    [Header("�÷��̾� ����")]
    public float moveSpeed = 15.0f;

    [Header("�ð� ���� ����")]
    public float historyDuration = 3.0f;
    public float recordInterval = 0.1f;
    public float rewindMoveSpeed = 25.0f;

    [Header("Material ����")]
    public Material normalMaterial;
    public Material rewindMaterial;

    [Header("UI ����")]
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
    // �ڡڡڡڡ� ���Ⱑ ������ ���� '���� ����' �����Դϴ� �ڡڡڡڡ�
    // ========================================================================
    private IEnumerator ReplayCommandsRoutine()
    {
        isReplaying = true;
        UpdateUI();

        int fastForwardCount = 0;

        while (commandQueue.Count > 0)
        {
            // RŰ�� ������ ���� ���� ī��Ʈ ����
            if (Input.GetKeyDown(KeyCode.R) && fastForwardCount <= 0)
            {
                Debug.Log("RŰ �Է�! �� 2�ʰ� ���� ���⸦ �����մϴ�.");
                float fastForwardDuration = 2.0f;
                float singleCommandDuration = 0.2f;
                fastForwardCount = Mathf.CeilToInt(fastForwardDuration / singleCommandDuration);
            }

            Command currentCommand = commandQueue.Dequeue();
            UpdateUI(); // ����� �����ڸ��� UI�� ������Ʈ�ؼ� ���ڰ� �ٷ� �پ��

            if (currentCommand.commandType == CommandType.Move)
            {
                float moveDuration = 0.2f;

                // ���� ���� ����� ���: �ִϸ��̼� ���� ��� �̵�!
                if (fastForwardCount > 0)
                {
                    playerRenderer.material = rewindMaterial;
                    // �̵� ����� �� ���� ����
                    transform.Translate(currentCommand.direction * moveSpeed * moveDuration);
                    fastForwardCount--;
                    yield return null; // ȭ���� ������ �ʵ��� �� �����Ӹ� ���
                }
                // �Ϲ� ��� ����� ���: �ε巴�� �̵�
                else
                {
                    playerRenderer.material = normalMaterial;
                    float elapsedTime = 0f;
                    while (elapsedTime < moveDuration)
                    {
                        // �ε巯�� �̵� �߿��� RŰ �Է� ����
                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            float fastForwardDuration = 2.0f;
                            fastForwardCount = Mathf.CeilToInt(fastForwardDuration / moveDuration);
                            break; // ���� �̵��� �ߴ��ϰ� �������� ���� ���� ����
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

    // (���� �ٸ� �Լ����� ������ �����մϴ�)
    public void TriggerRewind() { if (positionHistory.Count > 0 && !isRewinding && !isReplaying) { isRecording = false; StartCoroutine(RewindRoutine()); } }
    private void HandleMoveInput() { float moveX = Input.GetAxisRaw("Horizontal"); float moveZ = Input.GetAxisRaw("Vertical"); if (moveX != 0 || moveZ != 0) { Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized; commandQueue.Enqueue(new Command { commandType = CommandType.Move, direction = moveDirection }); UpdateUI(); } }
    public void ToggleRecording() { isRecording = !isRecording; UpdateUI(); }
    public void StartReplay() { if (commandQueue.Count > 0 && !isReplaying) { isRecording = false; StartCoroutine(ReplayCommandsRoutine()); } }
    private void UpdateUI() { if (commandCountText != null) commandCountText.text = $"��� ��: {commandQueue.Count}"; if (recordButtonText != null) recordButtonText.text = isRecording ? "��ȭ ����" : "��ȭ ����"; bool isBusy = isReplaying || isReplaying; if (playButton != null) playButton.interactable = commandQueue.Count > 0 && !isRecording && !isBusy; if (ekkoButton != null) ekkoButton.interactable = positionHistory.Count > 0 && !isBusy; if (recordButton != null) recordButton.interactable = !isBusy; }
    private IEnumerator RecordPositionRoutine() { while (true) { if (!isRewinding) { positionHistory.Enqueue(transform.position); int maxHistorySize = Mathf.RoundToInt(historyDuration / recordInterval); while (positionHistory.Count > maxHistorySize) positionHistory.Dequeue(); } yield return new WaitForSeconds(recordInterval); } }
    private IEnumerator RewindRoutine() { isRewinding = true; UpdateUI(); playerRenderer.material = rewindMaterial; Stack<Vector3> rewindStack = new Stack<Vector3>(); foreach (Vector3 pos in positionHistory) rewindStack.Push(pos); positionHistory.Clear(); while (rewindStack.Count > 0) { Vector3 targetPosition = rewindStack.Pop(); while (Vector3.Distance(transform.position, targetPosition) > 0.01f) { transform.position = Vector3.MoveTowards(transform.position, targetPosition, rewindMoveSpeed * Time.deltaTime); yield return null; } } playerRenderer.material = normalMaterial; isReplaying = false; UpdateUI(); }
}
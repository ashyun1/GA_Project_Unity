using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    public float turnSpeed = 10f; // 회전 속도
    public float characterHeightOffset = 0.5f; // 캐릭터가 바닥에서 살짝 떠 있는 높이

    private List<Vector3> currentPath;
    private int currentPathIndex = -1;
    private bool isMoving = false;

    void Start()
    {
        // 시작 시 캐릭터의 높이를 설정
        Vector3 pos = transform.position;
        pos.y = characterHeightOffset;
        transform.position = pos;
    }

    // MazeGenerator에서 호출될 함수 (요구사항 7)
    public void MoveAlongPath(List<Vector3> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.Log("이동할 경로가 없습니다.");
            return;
        }

        StopAllCoroutines(); // 기존 이동 중단

        // 경로 시작 전 캐릭터의 Y 높이를 보정
        Vector3 startPos = transform.position;
        startPos.y = characterHeightOffset;
        transform.position = startPos;

        currentPath = path;
        currentPathIndex = 0;
        isMoving = true;

        Debug.Log("자동 이동 시작!");

        StartCoroutine(FollowPathCoroutine());
    }

    // 외부에서 이동을 강제 중단할 때 사용
    public void StopMovement()
    {
        StopAllCoroutines();
        isMoving = false;
        currentPath = null;
        Debug.Log("자동 이동 중단.");
    }

    private IEnumerator FollowPathCoroutine()
    {
        while (currentPathIndex < currentPath.Count)
        {
            Vector3 targetPosition = currentPath[currentPathIndex];
            // Y축 좌표를 캐릭터의 현재 Y 높이로 고정
            targetPosition.y = transform.position.y;

            // 목적지까지 이동
            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                // 회전: 목적지를 바라보도록 부드럽게 회전
                Vector3 direction = (targetPosition - transform.position);
                direction.y = 0; // 평면 회전만

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                }

                // 이동
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

                yield return null;
            }

            // 목적지에 도착했으므로 다음 지점으로 이동 준비
            transform.position = targetPosition; // 정확한 위치 보정
            currentPathIndex++;
        }

        // 경로 끝에 도착
        isMoving = false;
        Debug.Log("자동 이동 완료!");
        currentPath = null;
    }
}
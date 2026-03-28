using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKTargetFollower : MonoBehaviour
{
    [Header("Hand Targets")]
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Rig rig;

    [Header("Position Settings")]
    [SerializeField] private float handDistance = 1.5f;
    [SerializeField] private float horizontalSpread = 0.3f;
    [SerializeField] private float verticalOffset = -0.2f;
    [SerializeField] private float smoothSpeed = 12f;

    [Header("Angle Limit")]
    [SerializeField] private float maxAngle = 60f; // 60 градусов влево и вправо (всего 120)

    private Camera playerCamera;
    public void Initialize()
    {
        playerCamera = Camera.main;
        rig.weight = 1f;
    }
    private void Update()
    {
        if (playerCamera == null || playerBody == null) return;

        // Получаем направления камеры
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        Vector3 cameraUp = playerCamera.transform.up;

        // Базовая точка перед камерой
        Vector3 basePosition = playerCamera.transform.position + cameraForward * handDistance;

        Vector3 toCamera = (playerCamera.transform.position - playerBody.position).normalized;
        Vector3 playerForward = playerBody.forward;
        playerForward.y = 0;
        toCamera.y = 0;

        float forwardDot = Vector3.Dot(toCamera, playerForward);

        // Если камера спереди (forwardDot > 0), меняем знак
        float spreadMultiplier = forwardDot > 0 ? -1f : 1f;

        // Позиции рук с учетом направления
        Vector3 rightPos = basePosition + cameraRight * (horizontalSpread * spreadMultiplier) + cameraUp * verticalOffset;
        Vector3 leftPos = basePosition + cameraRight * (-horizontalSpread * spreadMultiplier) + cameraUp * verticalOffset;

        // Ограничиваем позиции
        rightPos = LimitPositionToAngle(rightPos);
        leftPos = LimitPositionToAngle(leftPos);

        // Применяем к рукам
        if (rightHandTarget != null)
        {
            rightHandTarget.position = Vector3.Lerp(rightHandTarget.position, rightPos, Time.deltaTime * smoothSpeed);
        }

        if (leftHandTarget != null)
        {
            leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, leftPos, Time.deltaTime * smoothSpeed);
        }

        float pitch = GetCameraPitch();

        // Правая рука
        if (rightHandTarget != null)
        {
            Vector3 rot = new Vector3(0f, -90f, -100f - pitch);
            rightHandTarget.localRotation = Quaternion.Lerp(
                rightHandTarget.localRotation,
                Quaternion.Euler(rot),
                Time.deltaTime * smoothSpeed
            );
        }

        // Левая рука
        if (leftHandTarget != null)
        {
            Vector3 rot = new Vector3(0f, 90f, 100f + pitch);
            leftHandTarget.localRotation = Quaternion.Lerp(
                leftHandTarget.localRotation,
                Quaternion.Euler(rot),
                Time.deltaTime * smoothSpeed
            );
        }
    }

    private float GetCameraPitch()
    {
        float angle = playerCamera.transform.eulerAngles.x;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    private Vector3 LimitPositionToAngle(Vector3 targetPos)
    {
        // Вектор от тела к целевой позиции
        Vector3 toTarget = targetPos - playerBody.position;

        // Проецируем на горизонтальную плоскость
        Vector3 toTargetFlat = toTarget;
        toTargetFlat.y = 0;

        // Направление тела
        Vector3 bodyForward = playerBody.forward;
        bodyForward.y = 0;

        // Вычисляем угол между направлением тела и направлением к цели
        float angle = Vector3.Angle(bodyForward, toTargetFlat);

        // Если угол больше допустимого, ограничиваем
        if (angle > maxAngle)
        {
            // Определяем, в какую сторону поворот (влево или вправо)
            float crossY = Vector3.Cross(bodyForward, toTargetFlat).y;
            float sign = crossY > 0 ? 1 : -1;

            // Поворачиваем направление тела на максимальный угол в нужную сторону
            Quaternion maxRotation = Quaternion.AngleAxis(sign * maxAngle, Vector3.up);
            Vector3 limitedDirection = maxRotation * bodyForward;

            // Сохраняем исходную дистанцию
            float distance = toTargetFlat.magnitude;

            // Новая позиция с ограниченным направлением
            Vector3 newPos = playerBody.position + limitedDirection * distance;

            // Сохраняем исходную высоту
            newPos.y = targetPos.y;

            return newPos;
        }

        return targetPos;
    }

    private void OnDrawGizmos()
    {
        if (playerBody == null) return;

        // Рисуем допустимую зону
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        float radius = handDistance * 1.5f;
        int segments = 30;

        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * playerBody.forward;
            Vector3 point = playerBody.position + dir * radius + Vector3.up * verticalOffset;

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }
            prevPoint = point;
        }

        // Рисуем границы
        Gizmos.color = Color.yellow;
        Vector3 rightBound = Quaternion.AngleAxis(maxAngle, Vector3.up) * playerBody.forward;
        Vector3 leftBound = Quaternion.AngleAxis(-maxAngle, Vector3.up) * playerBody.forward;

        Gizmos.DrawRay(playerBody.position, rightBound * radius);
        Gizmos.DrawRay(playerBody.position, leftBound * radius);

        // Текущие позиции рук
        if (rightHandTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rightHandTarget.position, 0.1f);
            Gizmos.DrawLine(playerBody.position, rightHandTarget.position);
        }

        if (leftHandTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(leftHandTarget.position, 0.1f);
            Gizmos.DrawLine(playerBody.position, leftHandTarget.position);
        }
    }
}
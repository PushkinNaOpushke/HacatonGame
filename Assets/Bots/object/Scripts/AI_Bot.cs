using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Bot : MonoBehaviour
{
    public float detectionHeight = 7f;       // ������ ����� ������ ��� ��������
    public float avoidanceSpeed = 10f;       // �������� ��������� ����� ��� ������� �����������
    public float driveSpeed = 20f;           // �������� �������� �������� ����������
    public LayerMask obstacleLayer;          // ���� ����������� ��� ��������

    private Vector3 offset;                  // �������� ��� ����� ������
    private List<Vector3> rayEndPoints = new List<Vector3>();  // �������� ����� ����� (��������)

    // ��������� �������
    public float forwardMultiplier = 35f;    // ����� ������� ������
    public float backMultiplier = 5f;        // ����� ������� �����
    public float sideMultiplier = 5f;        // ������ ������� �� �����
    public int rayCount = 20;                // ���������� ����� (��������)

    private BoxCollider carCollider;         // ��������� BoxCollider ����������
    private List<Collider> knownObstacles = new List<Collider>();  // ������ ��� ����������� ������������ �����������

    private Vector3 avoidanceTarget;         // �����, � ������� �������� ���������� ��� ������� �����������
    private bool isAvoiding = false;         // ����, ���������, ��� ���������� ������ ��������� �����������

    void Start()
    {
        // �������� ������ �� BoxCollider ����������
        carCollider = GetComponent<BoxCollider>();

        // ������������� �������� �� ������ (��� �����������)
        offset = new Vector3(0, carCollider.bounds.size.y / 2 + detectionHeight, -carCollider.bounds.size.z / 2);
    }

    void Update()
    {
        Vector3 startPosition = transform.position + transform.TransformDirection(offset);  // ����� ������ ��� ��������

        // ��������� �������� ����� ��� �����
        GenerateRayEndPoints();

        // ��������� � ��������� �������� (��������)
        DrawAndProcessRays(startPosition, obstacleLayer);

        // ������ �������� ����������: ������ ����������� ��� �������� �� ������� ��������
        if (isAvoiding)
        {
            MoveTowardsTarget();
        }
        else
        {
            DriveForward();
        }
    }

    // ��������� �������� ����� ��������, ����� ��� ����������� ������ ������ ����������
    void GenerateRayEndPoints()
    {
        rayEndPoints.Clear();  // ������� ������ ����� ���������� ����� �����

        // ����������� �������� ����������
        float carWidth = carCollider.bounds.size.x;   // ������ ����������
        float carLength = carCollider.bounds.size.z;  // ����� ����������

        // ��������� ����� �� �������
        for (int i = 0; i < rayCount; i++)
        {
            float angle = (float)i / rayCount * 2 * Mathf.PI; // ���� ��� ������ �����
            float xOffset = Mathf.Cos(angle) * sideMultiplier;   // �������� �� ��� X (������)
            float zOffset = Mathf.Sin(angle) * (angle < Mathf.PI ? forwardMultiplier : backMultiplier); // �������� �� ��� Z (����� ������� ������ � �����)

            // �������� ����� ���� ������������ ������ �������
            Vector3 endPoint = new Vector3(xOffset, 0, zOffset);
            rayEndPoints.Add(transform.TransformPoint(endPoint)); // �������������� � ������� ����������
        }
    }

    // ����� ��� ��������� ����� � ��������� ��������
    void DrawAndProcessRays(Vector3 startPosition, LayerMask layer)
    {
        foreach (var endPoint in rayEndPoints)
        {
            Vector3 direction = endPoint - startPosition; // ����������� ����
            Ray ray = new Ray(startPosition, direction);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, direction.magnitude, layer))
            {
                Collider obstacle = hit.collider;

                // ���������, ���������� �� ����������� �����
                if (!IsObstacleKnown(obstacle))
                {
                    // ���� ����������� �����, ��������� ��� � ������
                    knownObstacles.Add(obstacle);

                    // ������������ ����� ����������� ��� ������� ����������� � ������ ��� �������
                    PlanAvoidance(obstacle);
                    Debug.DrawLine(startPosition, hit.point, Color.red);
                }
            }
            else
            {
                // ���� ����������� ���, ������ ������ ������
                Debug.DrawLine(startPosition, endPoint, Color.green);
            }
        }
    }

    // ��������, ���� �� ����������� ��� ���������
    bool IsObstacleKnown(Collider obstacle)
    {
        return knownObstacles.Contains(obstacle);
    }

    // ������ �������� ���������� ������
    void DriveForward()
    {
        // ������������ ���������� �����, ���� ��� ��������� �������
        transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);
    }

    // ������������ ������� ����������� � ������ ��� �������
    void PlanAvoidance(Collider obstacle)
    {
        // �������� ������� �����������
        Vector3 obstacleSize = obstacle.bounds.size;

        // ������������ ����������� ������� � ����������� �� ��������� ����������� � ��� �������
        Vector3 obstaclePosition = obstacle.transform.position;

        if (obstaclePosition.x > transform.position.x)
        {
            // ��������� ����� � ������ ������ �����������
            avoidanceTarget = transform.position + (transform.right * (obstacleSize.x + 5f)) + (transform.forward * 20f);
        }
        else
        {
            // ��������� ������ � ������ ������ �����������
            avoidanceTarget = transform.position + (-transform.right * (obstacleSize.x + 5f)); 
        }

        isAvoiding = true;  // �������� ���� �������
    }

    // �������� � ����� �������
    void MoveTowardsTarget()
    {
        Vector3 direction = (avoidanceTarget - transform.position).normalized;
        transform.Translate(direction * avoidanceSpeed * Time.deltaTime);
       
        // ���� �� �������� ����, ���������� ������
        if (Vector3.Distance(transform.position, avoidanceTarget) < 1f)
        {
            direction = (transform.forward * 20f).normalized;
            transform.Translate(direction * avoidanceSpeed * Time.deltaTime);
            isAvoiding = false;
        }
    }
}

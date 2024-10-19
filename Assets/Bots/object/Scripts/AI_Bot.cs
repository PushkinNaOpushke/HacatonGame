using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Bot : MonoBehaviour
{
    public float detectionHeight = 7f;       // Высота точки исхода для векторов
    public float avoidanceSpeed = 10f;       // Скорость изменения курса при объезде препятствия
    public float driveSpeed = 20f;           // Основная скорость движения автомобиля
    public LayerMask obstacleLayer;          // Слой препятствий для коллизий

    private Vector3 offset;                  // Смещение для точки исхода
    private List<Vector3> rayEndPoints = new List<Vector3>();  // Конечные точки лучей (векторов)

    // Параметры эллипса
    public float forwardMultiplier = 35f;    // Длина эллипса вперед
    public float backMultiplier = 5f;        // Длина эллипса назад
    public float sideMultiplier = 5f;        // Ширина эллипса по бокам
    public int rayCount = 20;                // Количество лучей (векторов)

    private BoxCollider carCollider;         // Компонент BoxCollider автомобиля
    private List<Collider> knownObstacles = new List<Collider>();  // Список для запоминания обнаруженных препятствий

    private Vector3 avoidanceTarget;         // Точка, к которой движется автомобиль для объезда препятствия
    private bool isAvoiding = false;         // Флаг, указывает, что автомобиль сейчас объезжает препятствие

    void Start()
    {
        // Получаем ссылку на BoxCollider автомобиля
        carCollider = GetComponent<BoxCollider>();

        // Устанавливаем смещение по высоте (над автомобилем)
        offset = new Vector3(0, carCollider.bounds.size.y / 2 + detectionHeight, -carCollider.bounds.size.z / 2);
    }

    void Update()
    {
        Vector3 startPosition = transform.position + transform.TransformDirection(offset);  // Точка исхода для векторов

        // Генерация конечных точек для лучей
        GenerateRayEndPoints();

        // Отрисовка и обработка векторов (сенсоров)
        DrawAndProcessRays(startPosition, obstacleLayer);

        // Логика движения автомобиля: объезд препятствий или движение по прямому маршруту
        if (isAvoiding)
        {
            MoveTowardsTarget();
        }
        else
        {
            DriveForward();
        }
    }

    // Генерация конечных точек векторов, чтобы они формировали эллипс вокруг автомобиля
    void GenerateRayEndPoints()
    {
        rayEndPoints.Clear();  // Очищаем список перед генерацией новых точек

        // Определение размеров автомобиля
        float carWidth = carCollider.bounds.size.x;   // Ширина автомобиля
        float carLength = carCollider.bounds.size.z;  // Длина автомобиля

        // Генерация точек по эллипсу
        for (int i = 0; i < rayCount; i++)
        {
            float angle = (float)i / rayCount * 2 * Mathf.PI; // Угол для каждой точки
            float xOffset = Mathf.Cos(angle) * sideMultiplier;   // Смещение по оси X (ширина)
            float zOffset = Mathf.Sin(angle) * (angle < Mathf.PI ? forwardMultiplier : backMultiplier); // Смещение по оси Z (длина эллипса вперед и назад)

            // Конечная точка луча относительно пивота объекта
            Vector3 endPoint = new Vector3(xOffset, 0, zOffset);
            rayEndPoints.Add(transform.TransformPoint(endPoint)); // Преобразование в мировые координаты
        }
    }

    // Метод для отрисовки лучей и обработки коллизий
    void DrawAndProcessRays(Vector3 startPosition, LayerMask layer)
    {
        foreach (var endPoint in rayEndPoints)
        {
            Vector3 direction = endPoint - startPosition; // Направление луча
            Ray ray = new Ray(startPosition, direction);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, direction.magnitude, layer))
            {
                Collider obstacle = hit.collider;

                // Проверяем, обнаружено ли препятствие ранее
                if (!IsObstacleKnown(obstacle))
                {
                    // Если препятствие новое, добавляем его в список
                    knownObstacles.Add(obstacle);

                    // Рассчитываем новое направление для объезда препятствия с учётом его размера
                    PlanAvoidance(obstacle);
                    Debug.DrawLine(startPosition, hit.point, Color.red);
                }
            }
            else
            {
                // Если препятствий нет, рисуем полный вектор
                Debug.DrawLine(startPosition, endPoint, Color.green);
            }
        }
    }

    // Проверка, было ли препятствие уже запомнено
    bool IsObstacleKnown(Collider obstacle)
    {
        return knownObstacles.Contains(obstacle);
    }

    // Логика движения автомобиля вперед
    void DriveForward()
    {
        // Передвижение автомобиля прямо, если нет активного объезда
        transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);
    }

    // Планирование объезда препятствия с учётом его размера
    void PlanAvoidance(Collider obstacle)
    {
        // Получаем размеры препятствия
        Vector3 obstacleSize = obstacle.bounds.size;

        // Рассчитываем направление объезда в зависимости от положения препятствия и его размера
        Vector3 obstaclePosition = obstacle.transform.position;

        if (obstaclePosition.x > transform.position.x)
        {
            // Объезжаем слева с учётом ширины препятствия
            avoidanceTarget = transform.position + (transform.right * (obstacleSize.x + 5f)) + (transform.forward * 20f);
        }
        else
        {
            // Объезжаем справа с учётом ширины препятствия
            avoidanceTarget = transform.position + (-transform.right * (obstacleSize.x + 5f)); 
        }

        isAvoiding = true;  // Включаем флаг объезда
    }

    // Движение к точке объезда
    void MoveTowardsTarget()
    {
        Vector3 direction = (avoidanceTarget - transform.position).normalized;
        transform.Translate(direction * avoidanceSpeed * Time.deltaTime);
       
        // Если мы достигли цели, прекращаем объезд
        if (Vector3.Distance(transform.position, avoidanceTarget) < 1f)
        {
            direction = (transform.forward * 20f).normalized;
            transform.Translate(direction * avoidanceSpeed * Time.deltaTime);
            isAvoiding = false;
        }
    }
}

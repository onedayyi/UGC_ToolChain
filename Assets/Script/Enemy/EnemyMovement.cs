using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class EnemyMovement : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;  // 要追踪的目标（玩家）

    [Header("巡逻设置")]
    public Vector3[] waypointPositions;  // 巡逻点数组
    public float patrolWaitTime = 2f;  // 巡逻点等待时间

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isPatrolling = false;  // 是否在巡逻模式

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("敌人没有NavMeshAgent组件！");
            return;
        }
    }
    void DelayedStart()
    {
        if (waypointPositions != null && waypointPositions.Length > 0)
        {
            StartPatrol();
            Debug.Log($" 敌人自动开始巡逻，路径点数量: {waypointPositions.Length}");
        }
    }
    /// <summary>
    /// 设置路径点（由Spawn_Floor调用）- 直接使用Vector3数组
    /// </summary>
    public void SetPathPoints(Vector3[] positions, float waitTime)
    {
        waypointPositions = positions;
        patrolWaitTime = waitTime;

        Debug.Log($"设置路径点: {positions.Length} 个点");
        for (int i = 0; i < positions.Length; i++)
        {
            Debug.Log($"  路径点 {i}: {positions[i]}");
        }
        DelayedStart();
    }
    // 【公共接口】开始巡逻
    public void StartPatrol()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError(" 无法开始巡逻：没有NavMeshAgent组件！");
                return;
            }
        }
        if (waypointPositions == null || waypointPositions.Length == 0)
        {
            Debug.LogWarning("无法开始巡逻：没有路径点");
            return;
        }

        Debug.Log($"开始巡逻，共有 {waypointPositions.Length} 个路径点");
        isPatrolling = true;
        target = null;
        currentWaypointIndex = 0;
        isWaiting = false;

        // 走向第一个巡逻点
        MoveToCurrentWaypoint();
    
    }
    // 【公共接口】停止巡逻
    public void StopPatrol()
    {
        isPatrolling = false;
        if (agent != null && agent.hasPath)
        {
            agent.ResetPath();
        }
        Debug.Log("停止巡逻");
    }

    // 【公共接口】设置追踪目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            isPatrolling = false;  // 有目标时停止巡逻
        }
    }

    void Update()
    {
        if (agent == null) return;

        // 如果有目标，追踪目标
        if (target != null)
        {
            agent.SetDestination(target.position);
            return;
        }
        // 巡逻模式
        if (isPatrolling && waypointPositions != null && waypointPositions.Length > 0)
        {
            // 如果正在等待，计时
            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= patrolWaitTime)
                {
                    //Debug.Log($" 路径点 {currentWaypointIndex} 停留结束");
                    isWaiting = false;
                    MoveToNextWaypoint();
                }
                return;
            }

            // 检查是否到达当前目标点
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                //Debug.Log($"到达路径点 {currentWaypointIndex}: {waypointPositions[currentWaypointIndex]}");
                isWaiting = true;
                waitTimer = 0f;
            }
        }
    }

    // 移动到当前路径点
    void MoveToCurrentWaypoint()
    {
        if (waypointPositions == null || waypointPositions.Length == 0) return;
        if (currentWaypointIndex >= waypointPositions.Length)
        {
            currentWaypointIndex = 0;
        }

        Vector3 targetPosition = waypointPositions[currentWaypointIndex];
        agent.SetDestination(targetPosition);
        Debug.Log($"🎯 前往路径点 {currentWaypointIndex}: {targetPosition}");
    }

    // 移动到下一个路径点
    void MoveToNextWaypoint()
    {
        if (waypointPositions == null || waypointPositions.Length == 0) return;

        currentWaypointIndex = (currentWaypointIndex + 1) % waypointPositions.Length;
        MoveToCurrentWaypoint();
    }

    // 绘制巡逻路线（调试用）
    void OnDrawGizmosSelected()
    {
        if (waypointPositions == null || waypointPositions.Length == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypointPositions.Length; i++)
        {
            Gizmos.DrawSphere(waypointPositions[i], 0.3f);

            int next = (i + 1) % waypointPositions.Length;
            Gizmos.DrawLine(waypointPositions[i], waypointPositions[next]);
        }
    }
}
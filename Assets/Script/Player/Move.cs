using UnityEngine;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // 获取输入轴的值
        float horizontal = 0f;  // A/D 左右
        float vertical = 0f;     // W/S 前后
        float upDown = 0f;       // 空格/Shift 上下

        // 检测按键
        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.Space)) upDown += 1f;
        if (Input.GetKey(KeyCode.LeftShift)) upDown -= 1f;

        // 计算移动向量
        Vector3 moveDirection = new Vector3(horizontal, upDown, vertical);

        // 应用移动
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
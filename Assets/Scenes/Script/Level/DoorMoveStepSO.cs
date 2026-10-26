using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "关卡流程/动作/移动门", fileName = "DoorMoveStep")]
public class DoorMoveStepSO : FlowStepSO
{
    public float moveDistance = 5f;
    public float moveDuration = 2f;

    // 动作不需要监听条件，留空
    public override void StartStep(LevelFlowController controller) { }
    public override void StopStep(LevelFlowController controller) { }

    // 只执行动作
    public override void ExecuteAction(LevelFlowController controller)
    {
        if (controller.doorToMove != null)
        {
            SystemMessageManager.Instance.Achievement("大门已开启");
            controller.StartCoroutine(MoveDoorUp(controller.doorToMove));
        }
    }

    private IEnumerator MoveDoorUp(Transform door)
    {
        Vector3 startPos = door.position;
        Vector3 targetPos = startPos + Vector3.up * moveDistance;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
            door.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        door.position = targetPos;
    }
}
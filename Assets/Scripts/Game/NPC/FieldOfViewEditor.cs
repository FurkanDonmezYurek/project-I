using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCManager))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        NPCManager manager = (NPCManager)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(
            manager.transform.position,
            Vector3.up,
            Vector3.forward,
            360,
            manager.radius
        );

        Vector3 viewAngle01 = DirectionFromAngle(
            manager.transform.eulerAngles.y,
            -manager.angle / 2
        );
        Vector3 viewAngle02 = DirectionFromAngle(
            manager.transform.eulerAngles.y,
            manager.angle / 2
        );

        Handles.color = Color.yellow;
        Handles.DrawLine(
            manager.transform.position,
            manager.transform.position + viewAngle01 * manager.radius
        );
        Handles.DrawLine(
            manager.transform.position,
            manager.transform.position + viewAngle02 * manager.radius
        );

        // if (manager.canSeePlayer)
        // {
        //     Handles.color = Color.green;
        //     Handles.DrawLine(manager.transform.position, manager.playerRef.transform.position);
        // }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(
            Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
            0,
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)
        );
    }
}

using UnityEngine;
using System.Collections;

public class Liner : MonoBehaviour
{

    [SerializeField, Range(0, 10)]
    float time;

    [SerializeField]
    Vector2 endPosition;

    //[SerializeField]
    //AnimationCurve curve;

    private float startTime;
    private Vector2 startPosition;

    public bool iMove = false;

    public void OnMove(Vector2 endpos, int d)
    {

        // 1マスを0,5秒で移動
        time = d * 0.5f;

        if (time <= 0)
        {
            transform.position = endPosition;
            //enabled = false;
            return;
        }

        endPosition = endpos;

        startTime = Time.timeSinceLevelLoad;
        startPosition = transform.position;

        iMove = true;
    }


    void Update()
    {
        if (iMove)
        {
            var diff = Time.timeSinceLevelLoad - startTime;
            if (diff > time)
            {
                transform.position = endPosition;
                //enabled = false;
            }

            var rate = diff / time;
            //var pos = curve.Evaluate(rate);

            transform.position = Vector3.Lerp(startPosition, endPosition, rate);
            //transform.position = Vector3.Lerp (startPosition, endPosition, pos);

            if (rate >= 1)
            {
                Debug.Log("リセット");
                time = 1;
                iMove = false;
            }
        }


    }


}
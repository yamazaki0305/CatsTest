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

    public void OnStart(Vector3 toPos, int k)
    {
        iMove = true;

        // 距離によってアニメーションの時間を変化
        float duration = 0f;
        float def = 1.2f;
        for(int i=0;i<k;i++)
        {
            duration += def;
            def = def * 0.5f;
        }

        /*
        switch (k)
        {
            case 1:
                duration = 1f; break;
            case 2:
                duration = 1.8f; break;
            case 3:
                duration = 2.3f; break;
            case 4:
                duration = 3.6f; break;
            case 5:
                duration = 3.6f; break;
            case 6:
                duration = 4.0f; break;
            case 7:
                duration = 4.4f; break;
        }
        */

        startPosition = transform.position;
        // この関数を呼び出すとオブジェクトが移動する
        StartCoroutine(MoveTo(startPosition, toPos, duration));
    }

    // fromPosが移動元の座標、toPosが移動先の座標、durationが移動の秒数
    IEnumerator MoveTo(Vector3 fromPos, Vector3 toPos, float duration)
    {
        float time = 0;

        while (true)
        {
            time += (Time.deltaTime / duration);

            if (time > 1)
            {
                time = 1;
            }

            float easingValue = EasingLerps.EasingLerp(EasingLerps.EasingLerpsType.Bounce, EasingLerps.EasingInOutType.EaseOut, time, 0, 1);
            Vector3 lerpValue = Vector3.Lerp(fromPos, toPos, easingValue);
            this.transform.position = lerpValue;

            if (time == 1)
            {
                iMove = false;
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void OnMove(Vector2 endpos, int d)
    {

        // 1マスを0,5秒で移動
        time = d * 0.3f;

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

        /*
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

            float easingValue = EasingLerps.EasingLerp(EasingLerps.EasingLerpsType.Quint, EasingLerps.EasingInOutType.EaseIn, time, 0, 1);
            //transform.position = Vector3.Lerp (startPosition, endPosition, pos);

            if (rate >= 1)
            {
                Debug.Log("リセット");
                time = 1;
                iMove = false;
            }
        }
        */


    }


}
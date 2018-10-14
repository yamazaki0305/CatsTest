using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Example
{
    public class EasingExample : MonoBehaviour
    {
        private void Start()
        {
            // この関数を呼び出すとオブジェクトが移動する
            StartCoroutine(MoveTo(Vector3.zero, Vector3.down, 3));
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

                float easingValue = EasingLerps.EasingLerp(EasingLerps.EasingLerpsType.Quint, EasingLerps.EasingInOutType.EaseIn, time, 0, 1);
                Vector3 lerpValue = Vector3.Lerp(fromPos, toPos, easingValue);
                this.transform.position = lerpValue;

                if (time == 1)
                {
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
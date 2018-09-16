using UnityEngine;

// 設定された解像度の範囲のみを映すカメラコンポーネント
[RequireComponent( typeof( Camera ) )]
public class FixResolutionCamera : MonoBehaviour
{
	// 解像度
	[SerializeField]
	Vector2 targetResolution = new Vector2();

	// カメラの範囲を解像度の範囲に設定
	void Start()
	{
		if( Screen.height > targetResolution.y )
		{
			var width = (float) Screen.width / Screen.height * targetResolution.y;
			Screen.SetResolution( Mathf.CeilToInt( width ), Mathf.CeilToInt( targetResolution.y ), true );
		}

		var myCamera = GetComponent<Camera>();

		var rawResolution = new Vector2( Screen.width, Screen.height );

		var rawAspectX = rawResolution.x / rawResolution.y;
		var rawAspectY = rawResolution.y / rawResolution.x;
		var targetAspectX = targetResolution.x / targetResolution.y;
		var targetAspectY = targetResolution.y / targetResolution.x;

		var scaledResolution = targetResolution;
		if( rawAspectX > targetAspectX )
		{
			scaledResolution.x = targetResolution.y * rawAspectX;
		}
		if( rawAspectY > targetAspectY )
		{
			scaledResolution.y = targetResolution.x * rawAspectY;
		}

		var cameraRect = new Rect();
		cameraRect.width = targetResolution.x / scaledResolution.x;
		cameraRect.height = targetResolution.y / scaledResolution.y;
		cameraRect.x = ( 1.0f - cameraRect.width ) / 2;
		cameraRect.y = ( 1.0f - cameraRect.height ) / 2;
		myCamera.rect = cameraRect;

		Screen.SetResolution( (int) scaledResolution.x, (int) scaledResolution.y, false );
	}
}

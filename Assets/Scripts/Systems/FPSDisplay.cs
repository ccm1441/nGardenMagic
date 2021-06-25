using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
	public Text text;
	float msec;
	float fps;

	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
		FPSUpdate();
	}

   private void FPSUpdate()
    {
		 msec = deltaTime * 1000.0f;
		 fps = 1f / deltaTime;
		text.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
	}
}
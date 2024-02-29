using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardGenerator : MonoBehaviour
{

	public GameObject objectToRender;
	public int imageWidth = 128;
	public int imageHeight = 128;
	public bool grab = false;

	void Start()
	{
		CameraSetup();
	}

	void OnPostRender()
	{
		if (!grab) { ConvertToImage(); grab = true; }
	}

	void CameraSetup()
	{
		Camera cam = Camera.main;
		cam.orthographic = true;

		float rw = imageWidth;
		rw /= Screen.width;
		float rh = imageHeight;
		rh /= Screen.height;
		cam.rect = new Rect(0, 0, rw, rh);
		cam.backgroundColor = new Vector4(0, 0, 0, 0);

		Bounds bb = objectToRender.GetComponent<Renderer>().bounds;

		cam.transform.position = bb.center;
		cam.transform.position.Set(cam.transform.position.x, cam.transform.position.y, -1.0f + (bb.min.z * 2.0f));
		cam.nearClipPlane = 0.5f;
		cam.farClipPlane = -cam.transform.position.z + 10.0f + bb.max.z;
		cam.orthographicSize = 1.01f * Mathf.Max((bb.max.y - bb.min.y) / 2.0f, (bb.max.x - bb.min.x) / 2.0f);
		cam.transform.position.Set(cam.transform.position.x, cam.orthographicSize * 0.05f, cam.transform.position.y);
	}

	void ConvertToImage()
	{
		var tex = new Texture2D(imageWidth, imageHeight);
		tex.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
		tex.Apply();

		Camera cam = Camera.main;
		Color bCol = cam.backgroundColor;
		Color alpha = new Vector4(0, 0, 0, 0);
		alpha.a = 0.0f;
		for (int y = 0; y < imageHeight; y++)
		{
			for (int x = 0; x < imageWidth; x++)
			{
				Color c = tex.GetPixel(x, y);
				if (c.r != bCol.r)
					tex.SetPixel(x, y, new Vector4(c.r * 2, c.g * 2, c.b * 2, 1));
			}
		}
		tex.Apply();

		byte[] bytes = tex.EncodeToPNG();
		Destroy(tex);

		System.IO.File.WriteAllBytes(Application.dataPath + "/billboard.png", bytes);
	}
}

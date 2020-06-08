using UnityEngine;
using System.Collections;

public class ShowVersion : MonoBehaviour 
{
	public BuildPlusSO buildPlus;
	Vector2 scrollPos;
	
	void Start () 
	{
		Debug.Log(buildPlus.build.CurrentVersion.ToString());
		Debug.Log(buildPlus.build.releaseNotes);
	}
	
	void OnGUI()
	{
		GUILayout.Label("Version " + buildPlus.build.CurrentVersion.ToString());
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.Label(buildPlus.build.releaseNotes);
		GUILayout.EndScrollView();			
	}
}

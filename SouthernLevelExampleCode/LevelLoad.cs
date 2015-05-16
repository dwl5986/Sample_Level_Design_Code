using UnityEngine;
using System.Collections;
//using UnityEditor;

//[ExecuteInEditMode]

public class LevelLoad : MonoBehaviour {

	public TextAsset level;//Text File being loaded
	public int numLevels = 8;//number of level segments to load
	public GameObject[] staticObstacles;//level obstacles(rocks and roots)
	public GameObject gatorPrefab;//level obstacle(alligator)
	public GameObject hillBillyPrefab;//level obstacle(hillbilly boat)
	public GameObject bridgePrefab;//level obstacle(bridge)
	public GameObject collectiblePrefab;//level collectible
	public bool loadAll = false;
	public int levelToLoad = 0;

	void OnEnable()
	{
		if(loadAll)
		{
			for (int i = 1; i < numLevels + 1; i++)
			{
				level = Resources.Load ("Levels/Level" + i) as TextAsset;
				SplitLevel(level.text, i);
				Debug.Log("Loaded Level " + i);
			}
		}
		else
		{
			level = Resources.Load ("Levels/Level" + levelToLoad) as TextAsset;
            SplitLevel(level.text, levelToLoad);
			Debug.Log("Loaded Level " + levelToLoad);
		}
		this.enabled = false;
	}

	public void SplitLevel(string map, int number)
	{
		GameObject levelSegment = new GameObject ();
		levelSegment.transform.position = Vector3.zero;

		string[] lines = map.Split("\n"[0]);//Splits text file into rows

		for (int i = 0; i < lines.Length; i++)//goes through rows of characters
		{
			string[] line = lines [i].Split ("," [0]);//splits up each character to parse what object to load
			for(int j = 0; j < line.Length; j++)//goes through each character of the line in order to load specific objects
			{
				Vector3 position = new Vector3(j * 1.2f, 6.4f - (i * 1.2f), 0.0f);//Values decided based on game "grid" and character position
				switch(line[j])
				{
				case "R":
					GameObject obstacle = (GameObject)Instantiate(staticObstacles[Random.Range(0, 5)], position, Quaternion.identity);//loads a rock or root
					obstacle.transform.parent = levelSegment.transform;
					break;
				case "A":
					GameObject gator = (GameObject)Instantiate(gatorPrefab, position, Quaternion.identity);//loads an alligator
					gator.transform.parent = levelSegment.transform;
					break;
				case "H":
					GameObject hillBilly = (GameObject)Instantiate(hillBillyPrefab, position, Quaternion.identity);//loads a hillbilly boat
					hillBilly.transform.parent = levelSegment.transform;
					break;
				case "B":
					if(i == 4)
					{
						GameObject bridge = (GameObject)Instantiate(bridgePrefab, new Vector3(j * 1.2f + .3f, 2.8f, 0.0f), Quaternion.identity);//loads a bridge
						bridge.transform.parent = levelSegment.transform;
					}
					break;
				case "C":
					GameObject collectible = (GameObject)Instantiate(collectiblePrefab, position, Quaternion.identity);//loads a collectible
					collectible.transform.parent = levelSegment.transform;
					break;
				}
			}
		}
		DestroyImmediate(levelSegment);
	}
	
}

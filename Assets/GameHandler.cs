using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour {
	public Button btnMain;
	public Button btnProc;
	public Sprite spForward;
	public Sprite spLeft;
	public Sprite spRight;
	public Sprite spLight;
	public Sprite spP1;
	public Sprite spBlank;
	public Sprite spTarget;
	public Sprite spOn;
	public GameObject goRocket;
	public GameObject goPath;
	public List<GameObject> targets;

	private string type; //either main or proc
	private List<int> mainSteps; // keeps track of the steps the user entered for main
	private List<int> procSteps; // keeps track of the steps the user entered for proc
	private int maxMain = 12; // maximum steps allowed in main
	private int maxProc = 8; // maximum steps allowed in proc
	private Dictionary<int, Sprite> iconDict;

	private int direction; //0-right, 1-down, 2-left, 3-up; maybe circular array in the future
	private Vector3 startPos;
	private Quaternion startRot;

	// Use this for initialization
	void Start () {
		mainSteps = new List<int> ();
		procSteps = new List<int> ();
		direction = 0;

		// initializing the icon dictionary
		iconDict = new Dictionary<int, Sprite> () {
			{ 0, spForward },
			{ 1, spLeft },
			{ 2, spRight },
			{ 3, spLight },
			{ 4, spP1 }
		};

		// store start position and rotation
		startPos = goRocket.transform.position;
		startRot = goRocket.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < mainSteps.Count; i++) {
			Image img = (Image)GameObject.Find (string.Format("img_m{0:00}", i+1)).GetComponent<Image>();
			img.sprite = iconDict[mainSteps[i]];
		}
		for (int i = 0; i < procSteps.Count; i++) {
			Image img = (Image)GameObject.Find (string.Format("img_p{0:00}", i+1)).GetComponent<Image>();
			img.sprite = iconDict[procSteps[i]];
		}
	}

	public void SetTypeToMain()
	{
		type = "main";
		btnMain.image.color = Color.yellow;
		btnProc.image.color = Color.white;
	}

	public void SetTypeToProc()
	{
		type = "proc";
		btnMain.image.color = Color.white;
		btnProc.image.color = Color.yellow;
	}

	public bool TypeSelected()
	{
		if (type != null)
			return true;
		else
			return false;
	}

	public void AddToStepList(int index)
	{
		if (!TypeSelected ()) {
			Debug.Log ("Type Not Selected Yet");
			return;
		}

		if (type == "main") {
			if (mainSteps.Count < maxMain)
				mainSteps.Add (index);
		}
		else //type is proc
		{
			if (index == 4) {
				Debug.Log ("Recursion is blocked, out of scope!");
				return;
			}
			if(procSteps.Count < maxProc)
				procSteps.Add (index);
		}
	}

	public void Reset()
	{
		// clear lists
		mainSteps.Clear();
		procSteps.Clear();

		// reset steps UI
		for (int i = 0; i < maxMain; i++) {
			Image img = (Image)GameObject.Find (string.Format("img_m{0:00}", i+1)).GetComponent<Image>();
			img.sprite = spBlank;
		}
		for (int i = 0; i < maxProc; i++) {
			Image img = (Image)GameObject.Find (string.Format("img_p{0:00}", i+1)).GetComponent<Image>();
			img.sprite = spBlank;
		}

		// reset rocket position and orientation
		goRocket.transform.position = startPos;
		goRocket.transform.rotation = startRot;
		direction = 0;

		// reset targets
		foreach (GameObject go in targets) {
			SpriteRenderer sr = go.GetComponent<SpriteRenderer> ();
			sr.sprite = spTarget;
		}
	}

	public void Run()
	{
		StartCoroutine(ExecuteSteps());
	}

	IEnumerator ExecuteSteps()
	{
		foreach (int action in mainSteps) {
			if (action == 0) { // move forward
				MoveForward ();
				yield return new WaitForSeconds (1f);
			} else if (action == 1) { // turn left
				Turn(true);
				yield return new WaitForSeconds (1f);
			} else if (action == 2) { // turn right
				Turn(false);
				yield return new WaitForSeconds (1f);
			} else if (action == 3) { // toggle light
				toggleLight();
				yield return new WaitForSeconds (1f);
			} else if (action == 4) { // run P1
				foreach (int act in procSteps) {
					if (act == 0) { // move forward
						MoveForward ();
					} else if (act == 1) { // turn left
						Turn (true);
					} else if (act == 2) { // turn right
						Turn (false);
					} else if (act == 3) { // toggle light
						toggleLight ();
					}
					yield return new WaitForSeconds (1f); // pause for 1 second bewtween each step
				}
			}
		}
		// TODO: check success
		if (Success ()) {
			Debug.Log ("Level complete!");
		} else {
			Debug.Log ("Didn't light all the targets!");
		}
	}

	public void MoveForward()
	{
		Vector3 finalPosition = new Vector3(0,0,0);
		//int dir = direction.Peek();
		if (direction == 0) {
			finalPosition = new Vector3 (1.5f, 0, 0); //1.5f is the perfect distance
		} else if (direction == 1) {
			finalPosition = new Vector3 (0, -1.5f, 0);
		} else if (direction == 2) {
			finalPosition = new Vector3 (-1.5f, 0, 0);
		} else if (direction == 3) {
			finalPosition = new Vector3 (0, 1.5f, 0);
		}
		if (PathExist (goRocket.transform.position + finalPosition))
			goRocket.transform.position += finalPosition;
		else
			Debug.Log ("Path does not exist");
	}

	public void Turn(bool isLeft)
	{
		if (isLeft) {
			goRocket.transform.Rotate (0, 0, 90);
		} else {
			goRocket.transform.Rotate (0, 0, -90);
		}
		newDir (isLeft);
	}

	public void newDir(bool isLeft)
	{
		if (isLeft) {
			direction--;
			direction = direction < 0 ? 3 : direction;
		} else {
			direction++;
			direction = direction > 3 ? 0 : direction;
		}
	}

	public bool PathExist(Vector3 newPos)
	{
		// Check if new position has path
		RectTransform rt = (RectTransform)goPath.transform;
		float w = rt.rect.width;
		float h = rt.rect.height;

		// if overlap, path exist
		if (Physics2D.OverlapBox (newPos, new Vector2 (w, h), 0f) != null)
			return true;
		return false;
	}

	public void toggleLight()
	{
		// Check if current position is a target
		RectTransform rt = (RectTransform)goPath.transform;
		float w = rt.rect.width;
		float h = rt.rect.height;

		// if overlap, path exist
		Collider2D colliders = Physics2D.OverlapBox (goRocket.transform.position, new Vector2 (w, h), 0f); // assume all other checks are successful, there should always be 1 item in this array
		GameObject block = colliders.gameObject;
		SpriteRenderer sr = block.GetComponent<SpriteRenderer> ();

		// check if path is a target, then toggle blue/yellow
		if (sr.sprite == spTarget) {
			sr.sprite = spOn;
		} else if (sr.sprite == spOn) {
			sr.sprite = spTarget;
		}
	}

	public bool Success()
	{
		// TODO: check if all target are lit
		foreach (GameObject go in targets) {
			SpriteRenderer sr = go.GetComponent<SpriteRenderer> ();
			if (sr.sprite == spTarget)
				return false;
		}
		return true;
	}
}

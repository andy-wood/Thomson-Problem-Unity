using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Points : MonoBehaviour {

	const int nInitialPoints = 3;
	const float targetDistance = 5.0f;
	const float sphereScale = 0.5f;
	const float convergenceSpeed = 0.05f;
	const float stressExponent = 1.0f;

	List<GameObject> spheres;
	GameObject shape;

	void Start () 
	{
		spheres = new List<GameObject> ();
		shape = new GameObject ();

		for (int i = 0; i < nInitialPoints; ++i) 
			addPoint ();
	}

	void Update () 
	{
		// minimize ();
		// center ();

		search ();
		move ();


		// Debug lines

		for (int i = 0; i < spheres.Count; ++i)
			for (int j = i + 1; j < spheres.Count; ++j)
				Debug.DrawLine (spheres [i].transform.position, spheres [j].transform.position);

		// User interaction

		if (Input.GetKey(KeyCode.R))
			shape.transform.Rotate (0, 1.5f, 1.0f);

		if (Input.GetKeyDown (KeyCode.Period))
			addPoint ();

		if (Input.GetKeyDown (KeyCode.Comma))
			removePoint ();
	}

	void OnGUI()
	{
		var style = new GUIStyle ();
		style.fontSize = 50;
		style.normal.textColor = Color.white;
		GUI.Label (new Rect (30, 30, 1000, 200), this.bestMeasure.ToString (), style);
	}

	void addPoint()
	{
		spheres.Add (GameObject.CreatePrimitive (PrimitiveType.Sphere));
		int i = spheres.Count - 1;
		spheres [i].transform.localPosition = Random.insideUnitSphere * targetDistance * 2;
		spheres [i].transform.localScale *= sphereScale;
		spheres [i].transform.SetParent (shape.transform);

		resetSearch ();
	}

	void removePoint()
	{
		if (spheres.Count > 1)
		{
			GameObject.DestroyImmediate (spheres [spheres.Count - 1]);
			spheres.RemoveAt (spheres.Count - 1);

			resetSearch ();
		}
	}

	int numPairs(int nElements)
	{
		return nElements * (nElements - 1) / 2;
	}

	void minimize()
	{
		float averageDistance = 0;

		for (int i = 0; i < spheres.Count; ++i)
			for (int j = i + 1; j < spheres.Count; ++j)
				averageDistance += (spheres [i].transform.localPosition - spheres [j].transform.localPosition).magnitude;

		averageDistance /= numPairs (spheres.Count);;

		Vector3[,] forceVectors = new Vector3[spheres.Count, spheres.Count];

		for (int i = 0; i < spheres.Count; ++i)
		{
			for (int j = i + 1; j < spheres.Count; ++j)
			{
				Vector3 vector = spheres [i].transform.localPosition - spheres [j].transform.localPosition;
				// Vector3 velocity = vector.normalized * convergenceSpeed * Mathf.Pow(Mathf.Abs (vector.magnitude - averageDistance), stressExponent);

				if ((vector.magnitude < averageDistance && vector.magnitude < targetDistance) ||
					(vector.magnitude > averageDistance && vector.magnitude > targetDistance))
				{
					forceVectors [i, j] = 
						vector.normalized * convergenceSpeed *
						(vector.magnitude - averageDistance); /* 
						Mathf.Max(Mathf.Pow(vector.magnitude, stressExponent), 1.0f);*/
				}
			}
		}

		for (int i = 0; i < spheres.Count; ++i)
		{
			for (int j = i + 1; j < spheres.Count; ++j)
			{
				spheres [i].transform.localPosition -= forceVectors[i, j];
				spheres [j].transform.localPosition += forceVectors[i, j];
			}
		}
	}

	const float epsilon = 0.0001f;
	float searchDistance;
	float searchDistanceFactor = 1.0f;

	List<Vector3> velocities = new List<Vector3>();
	List<Vector3> positions = new List<Vector3>();

	bool moving = false;
	const float moveTime = 0.05f;
	int frame;

	float bestMeasure;
	int iCandidate;
	int iWinner;
	Vector3 newPosition;

	void resetSearch()
	{
		moving = false;
		searchDistance = 1.0f;

		positions.Clear ();

		foreach (var sphere in spheres)
			positions.Add (sphere.transform.localPosition);

		bestMeasure = measure ();
		iCandidate = 0;
		iWinner = -1;
	}

	void search()
	{
		Vector3 oldPosition = positions [iCandidate];
		float maxDistance = 0;
		int iFarthest = iCandidate;

		for (int i = 0; i < positions.Count; ++i)
		{
			if (i != iCandidate)
			{
				float distance = (positions [i] - positions [iCandidate]).magnitude;

				if (distance > maxDistance)
				{
					maxDistance = distance;
					iFarthest = i;
				}
			}
		}

		positions [iCandidate] += 
			(positions [iFarthest] - positions [iCandidate]).normalized * Random.value * searchDistance * searchDistanceFactor;

		float newMeasure = measure ();

		if (!isBetter (bestMeasure, newMeasure))
		{
			positions [iCandidate] = oldPosition + Random.insideUnitSphere * searchDistance * searchDistanceFactor;
			newMeasure = measure ();
		}

		if (isBetter(bestMeasure, newMeasure))
		{
			bestMeasure = newMeasure;
			iWinner = iCandidate;
			newPosition = positions [iCandidate];
		}

		positions [iCandidate] = oldPosition;

		++iCandidate;

		if (iCandidate == positions.Count)
		{
			iCandidate = 0;

			if (iWinner > -1)
			{
				positions [iWinner] = newPosition;
				searchDistance = 1.0f;
				centerPositions ();
				normalizeToTargetDistance ();

				iWinner = -1;
			}
		}
		
		searchDistance += 0.1f;

		if (searchDistance * searchDistanceFactor > targetDistance * 4.0f)
			searchDistance = 1.0f;

		if (!moving)
		{
			velocities.Clear ();

			for (int i = 0; i < spheres.Count; ++i)
				velocities.Add (positions [i] - spheres [i].transform.localPosition);

			frame = 0;
			moving = true;
		}
	}

	bool isBetter(float oldValue, float newValue)
	{
		return newValue < oldValue && Mathf.Abs (newValue - oldValue) > epsilon;
	}

	void move()
	{
		for (int i = 0; i < spheres.Count; ++i)
			spheres [i].transform.localPosition += velocities [i] * (1 / (60.0f * moveTime));

		++frame;

		if (frame == 60 * moveTime)
			moving = false;
	}

	float measure()
	{
		if (positions.Count < 3)
			return 0;

		int nEdges = numPairs (positions.Count);
		float[] edges = new float[nEdges];

		int e = 0;
		float totalLength = 0;

		for (int i = 0; i < positions.Count; ++i)
		{
			for (int j = i + 1; j < positions.Count; ++j)
			{
				float length = (positions [i] - positions [j]).magnitude;
				edges [e++] = length;
				totalLength += length;
			}
		}

		float measure = 0;

		for (int i = 0; i < nEdges; ++i)
			for (int j = i + 1; j < nEdges; ++j)
				measure += Mathf.Abs (edges [i] - edges [j]);

		return measure / totalLength;
	}

	void center()
	{
		Vector3 center = new Vector3();

		foreach (var sphere in spheres)
			center += sphere.transform.localPosition;

		center /= spheres.Count;

		foreach (var sphere in spheres)
			sphere.transform.localPosition -= center;
	}

	void centerPositions()
	{
		Vector3 center = new Vector3();

		foreach (var position in positions)
			center += position;

		center /= positions.Count;

		for (int i = 0; i < positions.Count; ++i)
			positions [i] -= center;
	}

	void normalizeToTargetDistance()
	{
		if (positions.Count <= 1)
			return;

		float average = 0;

		for (int i = 0; i < positions.Count; ++i)
			for (int j = i + 1; j < positions.Count; ++j)
				average += (positions [i] - positions [j]).magnitude;

		average /= numPairs (positions.Count);
		float scale = targetDistance / average;

		for (int i = 0; i < positions.Count; ++i)
			positions[i] *= scale;
	}
}

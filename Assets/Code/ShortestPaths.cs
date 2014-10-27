using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShortestPaths {


	public List<Vector2> positions;
	private List<Path> paths;

	public ShortestPaths()
	{
		positions = new List<Vector2> ();
		paths = new List<Path>();
	}

	public void define(Vector2 position)
	{
		if (!hasKey (position))
		{
			positions.Add (position);
			paths.Add (null);
		}
	}

	public void add(Vector2 position, Path path)
	{
		define (position);
		set (position, path);
	}

	public void set (Vector2 position, Path path)
	{
		int keyIndex = findKey (position);
		if (keyIndex != -1)
		{
			paths[keyIndex] = path;
		}
	}

	public Path get(Vector2 position)
	{
		int keyIndex = findKey (position);
		if (keyIndex != -1)
		{
			return paths[keyIndex];
		}
		throw new KeyNotFoundException ("ShortestPaths instance did not contain index: " + position.ToString ());
	}

	public void remove(Vector2 position)
	{
		int keyIndex = findKey (position);
		if (keyIndex != -1)
		{
			positions.RemoveAt(keyIndex);
			paths.RemoveAt (keyIndex);
		}
	}

	public bool hasKey(Vector2 position)
	{
		return (findKey (position) != -1);
	}

	private int findKey(Vector2 position)
	{
		for(int i = 0; i < positions.Count; i++)
		{
			if (positions[i] == position)
			{
				return i;
			}
		}
		return -1;
	}

}

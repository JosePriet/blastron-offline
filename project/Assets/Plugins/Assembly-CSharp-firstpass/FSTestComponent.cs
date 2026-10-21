using System.Collections.Generic;
using CatsintheSky.FarseerDebug;
using UnityEngine;

public class FSTestComponent : MonoBehaviour
{
	private Test currentTest;

	private List<Test> allTests = new List<Test>();

	private int curTestID;

	private void Start()
	{
		allTests.Add(new TheoJansenTest(base.transform));
		allTests.Add(new CCDTest(base.transform));
		allTests.Add(new BridgeTest(base.transform));
		allTests.Add(new RagdollTest(base.transform));
		allTests.Add(new BuoyancyTest(base.transform));
		currentTest = allTests[curTestID];
		currentTest.Start();
	}

	private void Update()
	{
		if (currentTest != null)
		{
			currentTest.Update();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			ChangeTest(-1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			ChangeTest(1);
		}
	}

	private void OnGUI()
	{
		if (currentTest != null)
		{
			currentTest.OnGUI();
		}
	}

	private void ChangeTest(int val)
	{
		currentTest.Stop();
		curTestID += val;
		if (curTestID < 0)
		{
			curTestID = allTests.Count - 1;
		}
		if (curTestID >= allTests.Count)
		{
			curTestID = 0;
		}
		currentTest = allTests[curTestID];
		currentTest.Start();
	}
}

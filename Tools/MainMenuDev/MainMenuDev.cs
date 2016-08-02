﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.VR.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VR.Tools;

[ExecuteInEditMode]
public class MainMenuDev : MonoBehaviour, IRay, IInstantiateUI, IMainMenu
{

	[SerializeField]
	private Canvas m_MainMenuPrefab;
	private Canvas m_MenuCanvas;

	private RectTransform m_Layout;
	private GameObject m_ButtonTemplate;

	public Transform rayOrigin { get; set; }

	public List<Type> menuTools { private get; set; }
	public Func<IMainMenu, Type, bool> selectTool { private get; set; }

	public Func<GameObject, GameObject> instantiateUI { private get; set; }

	void Start()
	{
		if (m_MenuCanvas == null)
		{
			var go = instantiateUI(m_MainMenuPrefab.gameObject);
			m_MenuCanvas = go.GetComponent<Canvas>();
			m_Layout = m_MenuCanvas.GetComponentInChildren<GridLayoutGroup>().GetComponent<RectTransform>();
			m_ButtonTemplate = m_Layout.GetChild(0).gameObject;
			m_ButtonTemplate.SetActive(false);
		}
		m_MenuCanvas.transform.SetParent(rayOrigin, false);
		CreateToolButtons();
		CreateWorkspaceButtons();
	}

	private void CreateWorkspaceButtons()
	{
		foreach (Type t in U.Object.GetExtensionsOfClass(typeof(Workspace))) {
			var newButton = U.Object.InstantiateAndSetActive(m_ButtonTemplate, m_Layout, false);
			//TODO: prettify name
			newButton.name = t.Name;
			var text = newButton.GetComponentInChildren<Text>();
			text.text = t.Name;
			var button = newButton.GetComponent<Button>();
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => {
				//HACK: Delay call for serialization bug
				EditorApplication.delayCall += () => {
					//HACK: GameObject.Find to get EVR reference. How should we do this?  
					Workspace.ShowWorkspace<DummyWorkspace>(GameObject.Find("EditorVR").transform);
					U.Object.Destroy(this);
				};
			});
			button.onClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
		}
	}

	void OnDestroy()
	{
		if (m_MenuCanvas != null)
			U.Object.Destroy(m_MenuCanvas.gameObject);
	}

	private void CreateToolButtons()
	{
		foreach (var menuTool in menuTools)
		{
			var newButton = U.Object.InstantiateAndSetActive(m_ButtonTemplate, m_Layout, false);
			newButton.name = menuTool.Name;
			var text = newButton.GetComponentInChildren<Text>();
			text.text = menuTool.Name;
			var button = newButton.GetComponent<Button>();
			AddButtonListener(button, menuTool);
		}
	}

	private void AddButtonListener(Button b, Type t)
	{
		b.onClick.RemoveAllListeners();
		b.onClick.AddListener(() =>
		{
			if (selectTool(this, t))
				U.Object.Destroy(this);
		});
		b.onClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
	}
}

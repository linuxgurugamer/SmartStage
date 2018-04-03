﻿using System;
using UnityEngine;

using ClickThroughFix;

namespace SmartStage
{
	public class ComboBox
	{
		// Easy to use combobox class
		// ***** For users *****
		// Call the Box method with the latest selected item, list of text entries
		// and an object identifying who is making the request.
		// The result is the newly selected item.
		// There is currently no way of knowing when a choice has been made

		// Position of the popup
		private static Rect rect;
		// Identifier of the caller of the popup, null if nobody is waiting for a value
		private static object popupOwner = null;
		private static string[] entries;
		private static bool popupActive;
		// Result to be returned to the owner
		private static int selectedItem;
		// Unity identifier of the window, just needs to be unique
		private static int id = GUIUtility.GetControlID(FocusType.Passive);
		// ComboBox GUI Style
		private static GUIStyle style;

		private static float scale = 1;

		static ComboBox()
		{
			style = new GUIStyle(GUI.skin.label);
			Texture2D t = new Texture2D(1, 1);
			t.SetPixel(0, 0, new Color(0, 0, 0));
			t.Apply();
			style.active.background = t;
			style.hover.background = t;
			style.hover.textColor = Color.yellow;
		}

		public static bool DrawGUI()
		{
			if (popupOwner == null || rect.height == 0 || ! popupActive)
				return false;

			// Make sure the rectangle is fully on screen
			rect.x = Math.Max(0, Math.Min(rect.x, Screen.width / scale - rect.width));
			rect.y = Math.Max(0, Math.Min(rect.y, Screen.height / scale - rect.height));

			rect = ClickThruBlocker.GUILayoutWindow(id, rect, identifier =>
				{
					selectedItem = GUILayout.SelectionGrid(-1, entries, 1, style);
					if (GUI.changed)
						popupActive = false;
				}, "", style);

			//Cancel the popup if we click outside
			if (Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))
				popupOwner = null;
			return rect.Contains(Event.current.mousePosition);
		}

		public static int Box(int selectedItem, string[] entries, object caller)
		{
			// Trivial cases (0-1 items)
			if (entries.Length == 0)
				return 0;
			if (entries.Length == 1)
			{
				GUILayout.Label(entries[0]);
				return 0;
			}

			// A choice has been made, update the return value
			if (popupOwner == caller && ! ComboBox.popupActive)
			{
				popupOwner = null;
				selectedItem = ComboBox.selectedItem;
				GUI.changed = true;
			}

			bool guiChanged = GUI.changed;
			if (GUILayout.Button(entries[selectedItem]))
			{
				// We will set the changed status when we return from the menu instead
				GUI.changed = guiChanged;
				// Update the global state with the new items
				popupOwner = caller;
				popupActive = true;
				ComboBox.entries = entries;
				// Magic value to force position update during repaint event
				rect = new Rect(0, 0, 0, 0);
			}
			// The GetLastRect method only works during repaint event, but the Button will return false during repaint
			if (Event.current.type == EventType.Repaint && popupOwner == caller && rect.height == 0)
			{
				rect = GUILayoutUtility.GetLastRect();
				// But even worse, I can't find a clean way to convert from relative to absolute coordinates
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				Vector2 clippedMousePos = Event.current.mousePosition;
				rect.x = (rect.x + mousePos.x) / scale - clippedMousePos.x;
				rect.y = (rect.y + mousePos.y) / scale - clippedMousePos.y;
			}

			return selectedItem;
		}
	}
}


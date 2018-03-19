
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MasterEditorWindow : EditorWindow {

    private static GUIStyle editorMasterStyle;
    private static EditorWindow windowCache;

    public static GUIStyle EditorMasterStyle {
        get {
            if(editorMasterStyle == null) {
                InitializeMasterStyle();
            }

            return editorMasterStyle;
        }
    }

    private static void InitializeMasterStyle() {
        editorMasterStyle = new GUIStyle();

        Texture2D tex = new Texture2D(2, 2);
        Color[] colors = new Color[4];
        for (int i = 0; i < 4; i++) {
            colors[i] = new Color(1.0f, 1.0f, 1.0f, 8.0f);
        }

        tex.SetPixels(colors);

        editorMasterStyle.normal.background = tex;
    }

    [MenuItem("Controls/Editor Settings")]
    public static void ShowWindow() {
        if(windowCache == null) {
            windowCache = EditorWindow.GetWindow(typeof(MasterEditorWindow));
            windowCache.Show();
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI() {

    }
}
#endif

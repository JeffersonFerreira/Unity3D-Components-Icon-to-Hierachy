using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public static class ComponentsIconOnHierachy
{
    static bool folderIconFound = true;
    static Texture m_folderIconCache;
    static Texture m_openEyeIconCache;
    static Texture m_closedEyeIconCache;

    const string EDITOR_ICONS_FOLDER = "ComponentsIconOnHierachy";

    static Texture folderIconTexture
    {
        get { return m_folderIconCache ?? (m_folderIconCache = LoadIcon("folder-icon.png")); } 
    }

    static Texture openEyeTexture
    {
        get { return m_openEyeIconCache ?? (m_openEyeIconCache = LoadIcon("open eye.png")); }
    }

    static Texture closedEyeTexture
    {
        get { return m_closedEyeIconCache ?? (m_closedEyeIconCache = LoadIcon("closed eye.png")); }
    }

    static double lastClick;

    static ComponentsIconOnHierachy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGui;
    }

    static void HierarchyWindowItemOnGui(int instanceId, Rect selectionRect)
    {
        GameObject instanceIdToObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

        if (!instanceIdToObject)
            return;

        Component[] components = instanceIdToObject.GetComponents<Component>();
        bool isFolder = instanceIdToObject.transform.childCount > 0 && components.Length == 1;
        if (isFolder)
        {
            if (folderIconTexture == null)
                return;

            var pos = new Rect(selectionRect.x - 14, selectionRect.y, 16, 16);
            GUI.DrawTexture(pos, folderIconTexture);
        }

        if (instanceIdToObject.transform.childCount > 0 && GUI.Button(new Rect(selectionRect.x - 30f, selectionRect.y, 16, 16), instanceIdToObject.activeSelf? openEyeTexture:closedEyeTexture , GUIStyle.none))
        {
            instanceIdToObject.SetActive(!instanceIdToObject.activeSelf);
        }

        float startX = selectionRect.x + GetStringSize(components[0].gameObject.name.Trim());
        for (int i = 0; i < components.Length; i++)
        {
            if(components[i] == null)
                continue;
            
            if (components[i] is Transform)
                continue;

            if (components[i] is ParticleSystemRenderer)
                continue;

            GUIContent objectContent = EditorGUIUtility.ObjectContent(components[i], components[i].GetType());

            string componentNamespace = components[i].GetType().Namespace;
            bool isUserScript = string.IsNullOrEmpty(componentNamespace) || !componentNamespace.StartsWith("UnityEngine");

            if (isUserScript)
            {
                string componentName = components[i].GetType().ToString();
                float width = GetStringSize(componentName);

                if (GUI.Button(new Rect(startX + 16, selectionRect.y, width, 16), objectContent.image, GUIStyle.none))
                    PingScript(componentName);

                if (GUI.Button(new Rect(startX + 16 * 2, selectionRect.y + 1.5f, width, 16), componentName, GUIStyle.none))
                    PingScript(componentName);

                startX += width + 16;
            }
            else
            {
                startX += 17f;
                GUI.DrawTexture(new Rect(startX, selectionRect.y, 16, 16), objectContent.image);
            }
        }
    }

    static float GetStringSize(string s)
    {
        return GUIStyle.none.CalcSize(new GUIContent(s)).x;
    }  
         
    static void PingScript(string componentName)
    {   
        string assetGuid = AssetDatabase.FindAssets("t:script " + componentName).First();
        string guidToAssetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

        var scriptObject = AssetDatabase.LoadAssetAtPath<TextAsset>(guidToAssetPath);


        double curClickTime = EditorApplication.timeSinceStartup;

        if (curClickTime - lastClick < 0.3f && !guidToAssetPath.EndsWith(".dll"))
            InternalEditorUtility.OpenFileAtLineExternal(Path.Combine(Application.dataPath, guidToAssetPath.Remove(0, 7)), -1);
        else
            EditorGUIUtility.PingObject(scriptObject);

        lastClick = curClickTime;
    }

    static Texture LoadIcon(string iconPath)
    {
        if (!folderIconFound)
            return null;

        var texture = EditorGUIUtility.Load(Path.Combine(EDITOR_ICONS_FOLDER, iconPath)) as Texture;
        folderIconFound = texture;
        return texture;
    }
}
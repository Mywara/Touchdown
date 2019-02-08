using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ns3dRudder;
using System.IO;
using System.Linq;

public class AxesParamAddWindow : EditorWindow
{
    Controller3dRudderEditor m_controller3dRudder;
    private static Texture logo;
    string customAxesParamName;

	public void Init(Controller3dRudderEditor controller3dRudder)
    {
        m_controller3dRudder = controller3dRudder;        
        if (logo == null)
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>(Controller3dRudderEditor.PATH_EDITOR+ "3dRudderIcon.png");
        titleContent = new GUIContent("Add config", logo);        
        position = new Rect(Screen.width / 2, Screen.height / 2, 200, 150);
        Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Custom Axes Param name : ", EditorStyles.wordWrappedLabel);
        customAxesParamName = EditorGUILayout.TextField("", customAxesParamName);
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("OK"))
        {
            if (customAxesParamName.Length != 0)
            {
                customAxesParamName = customAxesParamName.Trim();
                if (Controller3dRudderEditor.AxesDefaultParamList.Where(x => x.ToUpper() == customAxesParamName.ToUpper()).ToArray().Length > 0)
                {
                    EditorUtility.DisplayDialog("Warning", "The name " + customAxesParamName  + " is forbidden (Default, Angle and Normalized), change the name please!", "Ok");
                }
                else
                {
                    IAxesParam axesParamCustom = ScriptableObject.CreateInstance<AxesParamCustom>();
                    string path = string.Format(Controller3dRudderEditor.PATH_ASSET, customAxesParamName);
                    AssetDatabase.CreateAsset(axesParamCustom, path);
                    m_controller3dRudder.AddConfig(customAxesParamName, path);

                    Close();
                }
            }
            else
            {
                Close();
            }
            
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }
}

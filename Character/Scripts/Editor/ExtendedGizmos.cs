using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode { 
    public class ExtendedGizmos {
        
        /// <summary>
        /// Draw a string at a given worldPos. 
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="worldPos">The world position to display it at</param>
        /// <param name="textColour">The colour to use</param>
        static public void DrawString(string text, Vector3 worldPos, Color? textColour = null, Color? backgroundolour = null)
        {
            UnityEditor.Handles.BeginGUI();
            Color restoreTextColour = GUI.color;
            Color restoreBackgroundColour = GUI.backgroundColor;

            if (textColour.HasValue)
            {
                GUI.color = textColour.Value;
            }

            SceneView view = UnityEditor.SceneView.currentDrawingSceneView;
            if (view != null && view.camera != null)
            {

                Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

                if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
                {
                    GUI.color = restoreTextColour;
                    GUI.backgroundColor = restoreBackgroundColour;
                    Handles.EndGUI();
                    return;
                }

                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
                var r = new Rect(screenPos.x - 2, -screenPos.y + view.position.height + 4, size.x + 2, size.y);
                GUI.Box(r, text, EditorStyles.numberField);
                GUI.Label(r, text);

                GUI.color = restoreTextColour;
                GUI.backgroundColor = restoreBackgroundColour;
            }

            Handles.EndGUI();
        }

        public static void DrawString(object goalDescription, Vector3 pos)
        {
            throw new NotImplementedException();
        }
    }
}

#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
// ReSharper disable AssignmentInConditionalExpression // This works best for IMGUI stuff

namespace Terrain
{
    
    [CustomEditor(typeof(TilesetRule))]
    public class TilesetRuleEditor : Editor
    {
        private bool ruleConditionFoldout = true;

        private int[,] controlIDs;
        
        private int hotControlId = 0;
        
        (int x, int y) activeHotControlIndex = (0, 0);


        public void RegexArray(TilesetRule tr)
        {
            var dataLen = tr.RuleData.GetLength(0);
            if (controlIDs == null)
            {
                controlIDs = new int[dataLen, dataLen];
            }
            // Debug.Log("Hot control ID start is " + hotControlId );

            EditorGUILayout.BeginVertical();
            for (int i = 0; i < dataLen; i++)
            {
                int aeshteticWidth = 240;
                var fullWidth = Screen.width;
                var widthDelta = fullWidth - aeshteticWidth;
                EditorGUILayout.BeginHorizontal(new GUIStyle()
                {
                    margin = new RectOffset()
                    {
                        bottom = 0,
                        left = widthDelta/2,
                        right = widthDelta/2,
                        top = 0
                    }
                });
                // EditorGUILayout.Space(widthDelta/4);
                for (int j = 0; j < dataLen; j++)
                {
                    // GUIContent content = new GUIContent($"[{i}:{j}]");
                    var newId = EditorGUIUtility.GetControlID(FocusType.Keyboard) + 1; // HACK : Dirty Hack
                    tr.RuleData[i, j] = EditorGUILayout.TextField("", tr.RuleData[i, j], new GUILayoutOption[]
                    {
                        GUILayout.MaxWidth(aeshteticWidth / dataLen),
                    });
                    controlIDs[i, j] = newId > 0 ? newId : controlIDs[i, j];
                    // Debug.Log("Obtained Control ID is " + controlIDs[i,j]);
                    if (controlIDs[i, j] == hotControlId)
                    {
                        // Debug.Log($"Active control index set to {i} : {j}");
                        activeHotControlIndex = (i, j);
                    }
                }
                // EditorGUILayout.Space(widthDelta/2);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        public void RegexHelperWidget(TilesetRule tr)
        {
            GUIStyle matchStyle = new GUIStyle()
            #if UNITY_EDITOR
            {
                margin = new RectOffset(3, 3, 0, 0),
                padding = new RectOffset(3, 3, 0, 0),
                fontStyle = FontStyle.Bold,
            };
            matchStyle.normal.textColor = Color.gray;
            // matchStyle.font.material.color = Color.white;

            string regexString = TilesetRule.TerrainCharacters;
            int idx = 0;
            var pattern = tr.RuleData[activeHotControlIndex.x, activeHotControlIndex.y];
            if (string.IsNullOrEmpty(pattern))
            {
                pattern = ".";
            }

            try
            {
                Regex rgx = new Regex(pattern);
                Match match = rgx.Match(regexString);
                while (idx < regexString.Length)
                {
                    EditorGUILayout.BeginHorizontal();
                    int charWidth = 11;
                    int charsToFit = Screen.width / charWidth;
                    if (idx + charsToFit > regexString.Length)
                    {
                        charsToFit = regexString.Length - idx;
                    }
                    for (int i = 0; i < charsToFit; i++)
                    {
                        if (idx < regexString.Length)
                        {
                            if (match.Success && idx >= match.Index)
                            #endif
                            {
                                GUI.color = Color.white;
                            }
                            else
                            {
                                GUI.color = Color.gray;
                            }

                            EditorGUILayout.LabelField($"{regexString[idx]}", matchStyle, new GUILayoutOption[]
                            {
                                GUILayout.MinWidth(7),
                                GUILayout.MaxWidth(7),
                            });
                            idx++;
                            if (idx > match.Index + match.Value.Length-1)
                            {
                                match = match.NextMatch();
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox("Regex Invalid !", MessageType.Error);
                // ignored - invalid regex - dont print crap to console 
            }

            GUI.color = Color.white;
        }
        
        public override void OnInspectorGUI()
        {

            var tr = (serializedObject.targetObject as TilesetRule);
            if (ruleConditionFoldout = EditorGUILayout.Foldout(ruleConditionFoldout, "Surroundings Match Rules"))
            {
                RegexArray(tr);
                RegexHelperWidget(tr);
            }

            var newId = EditorGUIUtility.keyboardControl;
            if (newId != hotControlId)
            {
                tr.CommitData();
            }
            hotControlId = newId;
            // string str2 = "";
            // for (int i = 0; i < dataLen; i++)
            // {
            //     for (int j = 0; j < dataLen; j++)
            //     {
            //         str2 += " " + controlIDs[i, j];
            //     }
            //     str2 += "\n";
            // }
            // Debug.Log(str2);
            // Debug.Log("Hot control ID end is " + hotControlId );
            base.OnInspectorGUI();
        }
    }
}
#endif

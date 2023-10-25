using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BKK.Extension;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BKK.GameEventArchitecture
{
    public sealed class GameEventExplorerWindow : SearchableEditorWindow
    {
        private List<GameEventListElement> projectEventList = new List<GameEventListElement>();
        private List<EventListenerBehaviour> listenerList = new List<EventListenerBehaviour>();
        
        private Vector2 scrollPos;

        private MultiColumnHeader projectColumnHeader;
        private MultiColumnHeader sceneColumnHeader;
        private MultiColumnHeaderState.Column[] projectColumns;
        private MultiColumnHeaderState.Column[] sceneColumns;
        
        private readonly Color lighterColor = Color.white * 0.3f;
        private readonly Color darkerColor = Color.white * 0.1f;
        private readonly string debugValueName = "debugValue";

        private float columnsWidth;
        private const float column0MinWidth = 300;
        private const float column1MinWidth = 500;
        private const float column2MinWidth = 300;
        private const float column3MinWidth = 300;
        private const float column4MinWidth = 150;

        private int tabIndex = 0;

        private string[] tabStrings = {"Project", "Scene"};

        private float columnHeight;

        private SearchField searchField;
        
        private string searchString = string.Empty;
        
        private Rect boxRect;

        public override void OnEnable()
        {
            base.OnEnable();
            
            InitializeProjectColumnHeader();
            InitializeSceneColumnHeader();
            
            SetSearchField();
            
            GetAssetList();

            SetWindow();

            AddCallbacks();
        }

        private void OnDestroy()
        {
            PlayerPrefs.SetString("GameEventExplorer_SearchString", searchString);
            PlayerPrefs.SetInt("GameEventExplorer_TabIndex", tabIndex);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 콜백을 초기화해줍니다.
        /// </summary>
        private void AddCallbacks()
        {
            // ※ 에셋 리스트를 프로젝트 폴더와 하이러키가 바뀔 때만 받아오게 합니다.
            // onGUI에서 이를 처리할 경우 런타임시 퍼포먼스가 크게 하락하는 문제가 있습니다.
            EditorApplication.projectChanged += GetAssetList;
            EditorApplication.hierarchyChanged += GetAssetList;
        }

        /// <summary>
        /// 표 헤더를 초기화 합니다.
        /// </summary>
        private void InitializeProjectColumnHeader()
        {
            projectColumns = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Game Event Asset","Game Event Asset"),
                    width = column0MinWidth,
                    minWidth = column0MinWidth,
                    maxWidth = 400,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Asset Path","Project path when Game Event Asset locate to"),
                    width = column1MinWidth,
                    minWidth = column1MinWidth,
                    maxWidth = 700,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Description", "Description for Game Event Asset."),
                    width = column2MinWidth,
                    minWidth = column2MinWidth,
                    maxWidth = 500,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Debug", "Additional Debug Function for Game Event."),
                    width = column4MinWidth,
                    minWidth = column4MinWidth,
                    maxWidth = 300,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false,
                },
            };

            projectColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(projectColumns))
            {
                height = 50
            };
            projectColumnHeader.visibleColumnsChanged += (multiColumnHeader) => multiColumnHeader.ResizeToFit();
            projectColumnHeader.sortingChanged += (multiColumnHeader) => SortAssetListInProject();
            
            projectColumnHeader.ResizeToFit();
        }

        private void InitializeSceneColumnHeader()
        {
            sceneColumns = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Game Event Listener","Game Event Listeners in current scene."),
                    width = column0MinWidth,
                    minWidth = column0MinWidth,
                    maxWidth = 400,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Game Event Asset","Game Event Asset"),
                    width = column0MinWidth,
                    minWidth = column0MinWidth,
                    maxWidth = 400,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Asset Path","Project path when Game Event Asset locate to"),
                    width = column1MinWidth,
                    minWidth = column1MinWidth,
                    maxWidth = 700,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Description", "Description for Game Event Asset."),
                    width = column2MinWidth,
                    minWidth = column2MinWidth,
                    maxWidth = 500,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Debug", "Additional Debug Function for Game Event."),
                    width = column4MinWidth,
                    minWidth = column4MinWidth,
                    maxWidth = 300,
                    autoResize = false,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false,
                },
            };

            sceneColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(sceneColumns))
            {
                height = 50
            };
            sceneColumnHeader.visibleColumnsChanged += (multiColumnHeader) => multiColumnHeader.ResizeToFit();
            sceneColumnHeader.sortingChanged += (multiColumnHeader) => SortAssetListInScene();
            sceneColumnHeader.ResizeToFit();
        }

        /// <summary>
        /// 검색 필드를 세팅합니다.
        /// </summary>
        private void SetSearchField()
        {
            searchField = new SearchField();
            searchString = PlayerPrefs.GetString("GameEventExplorer_SearchString");
            //searchString = string.Empty;
        }

        /// <summary>
        /// 윈도우 요소들의 기본값을 세팅합니다.
        /// </summary>
        private void SetWindow()
        {
            columnsWidth = column0MinWidth + column1MinWidth + column2MinWidth + column3MinWidth;
            
            var rect = position;
            rect.size = new Vector2(columnsWidth, 500);

            columnHeight = EditorGUIUtility.singleLineHeight * 2;
            
            tabIndex = PlayerPrefs.GetInt("GameEventExplorer_TabIndex");

            autoRepaintOnSceneChange = false;// 이걸 True로 하면 매 프레임 작동해서 퍼포먼스 하락합니다.  
        }

        /// <summary>
        /// 프로젝트에 있는 모든 게임 이벤트 에셋을 가져옵니다.
        /// </summary>
        private void GetAssetList()
        {
            GetAssetListInProject();
            GetAssetListInScene();
            Repaint();
        }

        /// <summary>
        /// 에셋 폴더 내의 에셋 리스트를 가져옵니다.
        /// </summary>
        private void GetAssetListInProject()
        {
            var assetGUIDList = AssetDatabase.FindAssets($"t:{typeof(GameEventAsset)}", null);

            projectEventList.Clear();
            foreach (var guid in assetGUIDList)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as GameEventAsset;
                Add(ref projectEventList, asset);
            }

            SortAssetListInProject();
        }

        /// <summary>
        /// 에셋 폴더 내의 에셋 리스트를 정렬합니다.
        /// </summary>
        private void SortAssetListInProject()
        {
            switch (projectColumnHeader.sortedColumnIndex)
            {
                case 0:
                    projectEventList = projectColumnHeader.IsSortedAscending(0)
                        ? projectEventList.OrderBy(a => a.gameEvent.name).ToList()
                        : projectEventList.OrderByDescending(a => a.gameEvent.name).ToList();
                    break;
                case 1:
                    projectEventList = projectColumnHeader.IsSortedAscending(1)
                        ? projectEventList.OrderBy(a => AssetDatabase.GetAssetPath(a.gameEvent)).ToList()
                        : projectEventList.OrderByDescending(a => AssetDatabase.GetAssetPath(a.gameEvent)).ToList();
                    break;
                case 2:
                    projectEventList = projectColumnHeader.IsSortedAscending(2)
                        ? projectEventList.OrderBy(a => a.gameEvent.description).ToList()
                        : projectEventList.OrderByDescending(a => a.gameEvent.description).ToList();
                    break;
            }
        }

        /// <summary>
        /// 현재 Scene의 에셋 리스트를 가져옵니다.
        /// </summary>
        private void GetAssetListInScene()
        {
            listenerList = GetAllListenerInScene();

            SortAssetListInScene();
        }

        /// <summary>
        /// 현재 Scene의 에셋 리스트를 정렬합니다.
        /// </summary>
        private void SortAssetListInScene()
        {
            switch (sceneColumnHeader.sortedColumnIndex)
            {
                case 0:
                    listenerList = sceneColumnHeader.IsSortedAscending(0)
                        ? listenerList.OrderBy(a => a.name).ToList()
                        : listenerList.OrderByDescending(a => a.name).ToList();
                    break;
                case 1:
                    listenerList = sceneColumnHeader.IsSortedAscending(1)
                        ? listenerList.OrderBy(a => a.GetGameEventAsset().name).ToList()
                        : listenerList.OrderByDescending(a => a.GetGameEventAsset().name).ToList();
                    break;
                case 2:
                    listenerList = sceneColumnHeader.IsSortedAscending(2)
                        ? listenerList.OrderBy(a => AssetDatabase.GetAssetPath(a.GetGameEventAsset())).ToList()
                        : listenerList.OrderByDescending(a => AssetDatabase.GetAssetPath(a.GetGameEventAsset())).ToList();
                    break;
                case 3:
                    listenerList = sceneColumnHeader.IsSortedAscending(3)
                        ? listenerList.OrderBy(a => a.GetGameEventAsset().description).ToList()
                        : listenerList.OrderByDescending(a => a.GetGameEventAsset().description).ToList();
                    break;
            }
        }

        /// <summary>
        /// 현재 Scene에 있는 모든 게임 이벤트 리스너를 가져와서 하이러키 순서로 정렬한 뒤 리턴합니다.
        /// </summary>
        /// <returns>현재 Scene에 있는 모든 게임 이벤트 리스너</returns>
        private List<EventListenerBehaviour> GetAllListenerInScene()
        {
            var objList = FindObjectsOfType<EventListenerBehaviour>(true).OrderBy(gel=>gel.transform.GetSiblingIndex()).ToArray();

            return objList.Where(gel => gel.GetGameEventAsset()).ToList();
        }

        private List<EventListenerBehaviour> Search(List<EventListenerBehaviour> list)
        {
            var searchStringWithoutWhiteSpace = searchString.Trim();
            return list.FindAll(e => e.name.Contains(searchStringWithoutWhiteSpace, StringComparison.OrdinalIgnoreCase) ||
                                     e.GetGameEventAsset().name.Contains(searchStringWithoutWhiteSpace, StringComparison.OrdinalIgnoreCase) ||
                                     e.GetGameEventAsset().description.ContainsWords(searchStringWithoutWhiteSpace, StringComparison.OrdinalIgnoreCase));
        }
        
        private List<GameEventListElement> Search(List<GameEventListElement> list)
        {
            var searchStringWithoutWhiteSpace = searchString.Trim();
            return list.FindAll(e => e.gameEvent.name.Contains(searchStringWithoutWhiteSpace, StringComparison.OrdinalIgnoreCase) || 
                                     e.gameEvent.description.ContainsWords(searchStringWithoutWhiteSpace, StringComparison.OrdinalIgnoreCase));
        }

        private void OnGUI()
        {
            Draw();
        }

        /// <summary>
        /// 게임 이벤트 익스플로러 윈도우의 전체 비주얼적인 내용을 표시합니다.
        /// </summary>
        private void Draw()
        {
            minSize = new Vector2(700, 525);
            var tempGeListenerList = new List<EventListenerBehaviour>();
            var tempGeList = new List<GameEventListElement>();
            if (searchField.HasFocus())
            {
                tempGeListenerList = Search(listenerList);
                tempGeList = Search(projectEventList);
            }
            else 
            {
                if (searchString == "")
                {
                    tempGeListenerList = listenerList.ToList();
                    tempGeList = projectEventList.ToList();
                }
                else
                {
                    tempGeListenerList = Search(listenerList);
                    tempGeList = Search(projectEventList);
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            tabIndex = GUILayout.Toolbar(tabIndex, tabStrings, GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("  * If you want change debug value, Go to target Game Event Asset Inspector.  ");
            var searchRect = GUILayoutUtility.GetRect(29f, 200f, 18f, 18f, EditorStyles.toolbarSearchField,
                GUILayout.Width(300), GUILayout.MinWidth(100));
            searchRect.x -= 7;
            searchString = searchField.OnToolbarGUI(searchRect, searchString);
            GUILayout.EndHorizontal();
            var boxStyle = new GUIStyle("GroupBox");
            boxStyle.padding = new RectOffset(1, 1, 1, 1);
            boxRect = EditorGUILayout.BeginVertical(boxStyle);
            switch (tabIndex)
            {
                case 0:
                    DrawListInProject(ref tempGeList);
                    break;
                case 1:
                    DrawListInScene(ref tempGeListenerList);
                    break;
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 에셋 폴더 내의 게임 이벤트 에셋 리스트를 표로 표시합니다.
        /// </summary>
        private void DrawListInProject(ref List<GameEventListElement> list)
        {
            Event currentEvent = Event.current;
            
            if (projectColumnHeader == null)
            {
                InitializeProjectColumnHeader();
            }

            GUILayout.FlexibleSpace();
            var windowRect = GUILayoutUtility.GetLastRect();
            windowRect.x -= EditorGUIUtility.singleLineHeight + 3;
            windowRect.width = position.width + scrollPos.x;
            windowRect.height = position.height + scrollPos.y;

            Rect columnRectPrototype = new Rect(source: windowRect)
            {
                height = columnHeight,
            };

            Rect columnHeaderRect = new Rect(windowRect)
            {
                x = boxRect.x + 1,
                height = columnHeight,
                width = boxRect.width - 2
            };

            Rect positionalRectAreaOfScrollView = GUILayoutUtility.GetRect(0, float.MaxValue, 0, position.height - 100);

            Rect viewRect = new Rect(source: windowRect)
            {
                x = boxRect.x - EditorGUIUtility.singleLineHeight / 2 - 1,
                xMax = this.projectColumns.Sum((column) => column.width),
                y = this.projectColumns.Sum((column) => columnHeight) - EditorGUIUtility.singleLineHeight - columnHeaderRect.height,
                yMax = (projectEventList.Count + 2) * columnHeight + EditorGUIUtility.singleLineHeight
            };
            
            this.projectColumnHeader.OnGUI(rect: columnHeaderRect, xScroll: scrollPos.x);

            this.scrollPos = GUI.BeginScrollView(
                position: positionalRectAreaOfScrollView,
                scrollPosition: this.scrollPos,
                viewRect: viewRect,
                alwaysShowHorizontal: false,
                alwaysShowVertical: false
            );
            
            Rect rowRect = default;
            for (var a = 0; a < list.Count; a++)
            {
                rowRect = new Rect(source: columnRectPrototype);

                rowRect.y += columnHeight * (a + 1);// 리스트 컬럼 아래에 있어야해서 한칸 더 아래에 그린다.

                EditorGUI.DrawRect(rect: rowRect, color: a % 2 == 0 ? this.darkerColor : this.lighterColor);

                // 0번째 열 Draw
                int columnIndex = 0;

                if (this.projectColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.projectColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.projectColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;

                    GUI.enabled = false;

                    EditorGUI.ObjectField(
                        position: this.projectColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        list[a].gameEvent,
                        typeof(BaseGameEvent),
                        false
                    );
                    GUI.enabled = true;
                }
                
                // 1번째 열 Draw
                columnIndex = 1;

                if (this.projectColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.projectColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.projectColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;

                    GUIStyle nameFieldGUIStyle = new GUIStyle(GUI.skin.label)
                    {
                        padding = new RectOffset(left: 10, right: 10, top: 2, bottom: 2)
                    };
                    EditorGUI.LabelField(
                        position: this.projectColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        label: new GUIContent(list[a].assetPath),
                        style: nameFieldGUIStyle
                    );

                    if (columnRect.Contains(currentEvent.mousePosition))
                    {
                        var a1 = a;
                        if (currentEvent.type == EventType.ContextClick)
                        {
                            GenericMenu menu = new GenericMenu();
                            var elements = list;
                            menu.AddItem(new GUIContent("Copy Asset Path"), false,
                                () => CopyPath(elements[a1].assetPath));
                            menu.AddItem(new GUIContent("Copy Full Path"), false,
                                () => CopyPath(Application.dataPath.Substring(0, Application.dataPath.Length - 6) +
                                               elements[a1].assetPath));
                            menu.ShowAsContext();
                            currentEvent.Use();
                        }
                        else if(currentEvent.type == EventType.MouseUp)
                        {
                            var asset = AssetDatabase.LoadMainAssetAtPath(list[a1].assetPath);
                            EditorGUIUtility.PingObject(asset);
                        }
                    }
                }
                
                // 2번째 열 Draw
                columnIndex = 2;

                if (this.projectColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.projectColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.projectColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;

                    GUIStyle nameFieldGUIStyle = new GUIStyle(GUI.skin.label)
                    {
                        padding = new RectOffset(left: 10, right: 10, top: 2, bottom: 2)
                    };
                    EditorGUI.LabelField(
                        position: this.projectColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        label: new GUIContent(list[a].gameEvent.description.Split('\n')[0], list[a].gameEvent.description),
                        style: nameFieldGUIStyle
                    );
                }
                
                // 3번째 열 Draw
                columnIndex = 3;

                if (this.projectColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.projectColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.projectColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;

                    var button1Rect = new Rect(
                        x: columnRect.x,
                        y: columnRect.y,
                        width: columnRect.width / 2,
                        height: columnRect.height
                    );
                    
                    var button2Rect = new Rect(
                        x: columnRect.x + columnRect.width / 2,
                        y: columnRect.y,
                        width: columnRect.width / 2,
                        height: columnRect.height
                    );
                    if (!EditorApplication.isPlaying) GUI.enabled = false;

                    if (GUI.Button(button1Rect, "Invoke"))
                    {
                        if (list[a].gameEvent is GameEvent gameEvent)// 일반 게임 이벤트인 경우 바로 호출
                        {
                            gameEvent.Raise();
                        }
                        else// 제네릭 타입 게임 이벤트인 경우 메서드 정보를 찾아서 호출
                        {
                            ExecuteCustomTypeDebugMethod(list[a].gameEvent, "Raise");
                        }
                    }
                    
                    if (GUI.Button(button2Rect, "Cancel"))
                    {
                        if (list[a].gameEvent is GameEvent gameEvent)// 일반 게임 이벤트인 경우 바로 호출
                        {
                            gameEvent.Cancel();
                        }
                        else// 제네릭 타입 게임 이벤트인 경우 메서드 정보를 찾아서 호출
                        {
                            ExecuteCustomTypeDebugMethod(list[a].gameEvent, "Cancel");
                        }
                    }
                    
                    if (!EditorApplication.isPlaying) GUI.enabled = true;
                }
            }

            var remainHeight = position.height - rowRect.height;

            var remainCount = (int) (remainHeight / (columnHeight));
            
            for (var a = 0; a < remainCount; a++)
            {
                rowRect = new Rect(source: columnRectPrototype);

                rowRect.y += columnHeight * (a + 1);

                EditorGUI.DrawRect(rect: rowRect, color: a % 2 == 0 ? this.darkerColor : this.lighterColor);
            }

            GUI.EndScrollView();
        }

        private void ExecuteCustomTypeDebugMethod(GameEventAsset gameEventAsset, string methodName)
        {
            Type customTypeGameEvent = gameEventAsset.GetType();
            MethodInfo method = customTypeGameEvent.BaseType.GetMethod(methodName,
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

            object debugValue =GetDebugValue(gameEventAsset);

            MethodExtension.ExecuteMethod(gameEventAsset, method, debugValue);
        }

        private object GetDebugValue(GameEventAsset gameEventAsset)
        {
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(gameEventAsset);
            SerializedProperty target = editor.serializedObject.FindProperty(debugValueName);
            return EditorExtension.GetFieldValue(target, debugValueName);
        }

        /// <summary>
        /// 게임 이벤트 익스플로 윈도우의 전체 비주얼적인 내용을 표시해줍니다.
        /// </summary>
        private void DrawListInScene(ref List<EventListenerBehaviour> list)
        {
            var currentEvent = Event.current;

            if (sceneColumnHeader == null)
            {
                InitializeSceneColumnHeader();
            }

            GUILayout.FlexibleSpace();
            var windowRect = GUILayoutUtility.GetLastRect();
            windowRect.x -= EditorGUIUtility.singleLineHeight + 3;
            windowRect.width = position.width + scrollPos.x;
            windowRect.height = position.height + scrollPos.y;

            Rect columnRectPrototype = new Rect(source: windowRect)
            {
                height = columnHeight,
            };
            
            Rect columnHeaderRect = new Rect(windowRect)
            {
                x = boxRect.x + 1,
                height = columnHeight,
                width = boxRect.width - 2
            };

            Rect positionalRectAreaOfScrollView = GUILayoutUtility.GetRect(0, float.MaxValue, 0, position.height - 100);

            Rect viewRect = new Rect(source: windowRect)
            {
                x = boxRect.x - EditorGUIUtility.singleLineHeight / 2 - 1,
                xMax = this.sceneColumns.Sum(column => column.width),
                y = this.sceneColumns.Sum(column => columnHeight) - EditorGUIUtility.singleLineHeight - columnHeaderRect.height,
                yMax = (list.Count + 2) * columnHeight + EditorGUIUtility.singleLineHeight
            };
            
            this.sceneColumnHeader.OnGUI(rect: columnHeaderRect, xScroll: scrollPos.x);

            this.scrollPos = GUI.BeginScrollView(
                position: positionalRectAreaOfScrollView,
                scrollPosition: this.scrollPos,
                viewRect: viewRect,
                alwaysShowHorizontal: false,
                alwaysShowVertical: false
            );
            
            Rect rowRect = default;
            for (var a = 0; a < list.Count; a++)
            {
                rowRect = new Rect(source: columnRectPrototype);

                // BKK TODO: 무슨 이유에서인지 프로젝트 탭 그리는 로직과 동일한데 한칸 위에 그려져서 +2. 원인 찾을 것.
                rowRect.y += columnHeight * (a + 2);// 리스트 컬럼 아래에 있어야해서 한칸 더 아래에 그린다.

                EditorGUI.DrawRect(rect: rowRect, color: a % 2 == 0 ? this.darkerColor : this.lighterColor);
                
                // 0번째 열 Draw
                int columnIndex = 0;

                if (this.sceneColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.sceneColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.sceneColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;

                    GUI.enabled = false;
                    EditorGUI.ObjectField(
                        position: this.sceneColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        list[a],
                        typeof(EventListenerBehaviour),
                        true
                    );
                    GUI.enabled = true;
                }
                
                // 1번째 열 Draw
                columnIndex = 1;

                if (this.sceneColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.sceneColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.sceneColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;
                    
                    GUI.enabled = false;
                    
                    EditorGUI.ObjectField(
                        position: this.sceneColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        list[a].GetGameEventAsset(),
                        typeof(GameEventAsset),
                        false
                    );
                    GUI.enabled = true;
                }
                
                // 2번째 열 Draw
                columnIndex = 2;

                if (this.sceneColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.sceneColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.sceneColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;

                    GUIStyle nameFieldGUIStyle = new GUIStyle(GUI.skin.label)
                    {
                        padding = new RectOffset(left: 10, right: 10, top: 2, bottom: 2)
                    };
                    EditorGUI.LabelField(
                        position: this.sceneColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        label: new GUIContent(AssetDatabase.GetAssetPath(list[a].GetGameEventAsset())),
                        style: nameFieldGUIStyle
                    );

                    if (columnRect.Contains(currentEvent.mousePosition))
                    {
                        var a1 = a;
                        if (currentEvent.type == EventType.ContextClick)
                        {
                            GenericMenu menu = new GenericMenu();
                            var listeners = list;
                            menu.AddItem(new GUIContent("Copy Asset Path"), false,
                                () => CopyPath(AssetDatabase.GetAssetPath(listeners[a1])));
                            menu.AddItem(new GUIContent("Copy Full Path"), false,
                                () => CopyPath(Application.dataPath.Substring(0, Application.dataPath.Length - 6) +
                                               AssetDatabase.GetAssetPath(listeners[a1])));
                            menu.ShowAsContext();
                            currentEvent.Use();
                        }
                        else if(currentEvent.type == EventType.MouseUp)
                        {
                            var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(listenerList[a1].GetGameEventAsset()));
                            EditorGUIUtility.PingObject(asset);
                        }
                    }
                }
                
                // 3번째 열 Draw
                columnIndex = 3;

                if (this.sceneColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.sceneColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.sceneColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;

                    GUIStyle nameFieldGUIStyle = new GUIStyle(GUI.skin.label)
                    {
                        padding = new RectOffset(left: 10, right: 10, top: 2, bottom: 2)
                    };
                    
                    EditorGUI.LabelField(
                        position: this.sceneColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        label: new GUIContent(list[a].GetGameEventAsset().description.Split('\n')[0], list[a].GetGameEventAsset().description),
                        style: nameFieldGUIStyle
                    );
                }
                
                // 4번째 열 Draw
                columnIndex = 4;

                if (this.sceneColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                {
                    int visibleColumnIndex = this.sceneColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);

                    Rect columnRect = this.sceneColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                    
                    columnRect.y = rowRect.y;

                    var button1Rect = new Rect(
                        x: columnRect.x,
                        y: columnRect.y,
                        width: columnRect.width / 2,
                        height: columnRect.height
                    );
                    
                    var button2Rect = new Rect(
                        x: columnRect.x + columnRect.width / 2,
                        y: columnRect.y,
                        width: columnRect.width / 2,
                        height: columnRect.height
                    );
                    if (!EditorApplication.isPlaying) GUI.enabled = false;
                    
                    if (GUI.Button(button1Rect, "Invoke"))
                    {
                        if (list[a].GetGameEventAsset() is GameEvent gameEvent)// 일반 게임 이벤트인 경우 바로 호출
                        {
                            gameEvent.Raise();
                        }
                        else// 제네릭 타입 게임 이벤트인 경우 메서드 정보를 찾아서 호출
                        {
                            ExecuteCustomTypeDebugMethod(list[a].GetGameEventAsset(), "Raise");
                        }
                    }
                    if (GUI.Button(button2Rect, "Cancel"))
                    {
                        if (list[a].GetGameEventAsset() is GameEvent gameEvent)// 일반 게임 이벤트인 경우 바로 호출
                        {
                            gameEvent.Cancel();
                        }
                        else// 제네릭 타입 게임 이벤트인 경우 메서드 정보를 찾아서 호출
                        {
                            ExecuteCustomTypeDebugMethod(list[a].GetGameEventAsset(), "Cancel");
                        }
                    }
                    
                    if (!EditorApplication.isPlaying) GUI.enabled = true;
                }
            }
            
            var remainHeight = position.height - rowRect.height;

            var remainCount = (int) (remainHeight / columnHeight);
            
            for (var a = 0; a < remainCount; a++)
            {
                rowRect = new Rect(source: columnRectPrototype);

                rowRect.y += columnHeight * (a + 1);

                EditorGUI.DrawRect(rect: rowRect, color: a % 2 == 0 ? this.darkerColor : this.lighterColor);
            }

            GUI.EndScrollView();
        }

        private void CopyPath(string str)
        {
            GUIUtility.systemCopyBuffer = str;
        }

        /// <summary>
        /// 게임 이벤트 에셋을 윈도우에 표시할때 사용할 직렬화 클래스 리스트에 추가합니다. 
        /// </summary>
        /// <param name="asset"></param>
        private void Add(ref List<GameEventListElement> list, GameEventAsset asset)
        {
            var newData = new GameEventListElement
            {
                gameEvent = asset,
                name = asset.name,
                assetPath = AssetDatabase.GetAssetPath(asset),
                //scenePath = AssetDatabase.GetAssetOrScenePath(asset)
            };
            list.Add(newData);
        }

        /// <summary>
        /// 게임 이벤트 익스플로러 메뉴를 표시합니다.
        /// </summary>
        [MenuItem( "BKK/Game Event/Game Event Explorer",priority = 0)]
        public static void ShowWindow()
        {
            GameEventExplorerWindow window = EditorWindow.GetWindow<GameEventExplorerWindow>( false, "Game Event Explorer", true );
            window.titleContent = new GUIContent( "Game Event Explorer" );
        }
    }
    
    [System.Serializable]
    public class GameEventListElement
    {
        public GameEventAsset gameEvent;
        public string name;
        public string assetPath;
        //public string scenePath;
    }
}

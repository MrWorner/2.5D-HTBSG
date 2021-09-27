//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

////ИСТОЧНИК: https://github.com/EmpireWorld/unity-dijkstras-pathfinding/blob/master/Assets/Scripts/Editor/GraphEditor.cs

//[CustomEditor(typeof(MG_GlobalHexMap))]
//public class GraphEditor : Editor
//{
//    protected MG_GlobalHexMap _map;

//    void OnEnable()
//    {
//        _map = target as MG_GlobalHexMap;
//    }

//    void OnSceneGUI()
//    {

//        if (_map == null)
//        {
//            return;
//        }

//        //float mapX = 100.0f;
//        //float mapY = 100.0f;


//        //double minX;
//        //double maxX;
//        //double minY;
//        //double maxY;

//        //var vertExtent = Camera.main.GetComponent<Camera>().orthographicSize;
//        //var horzExtent = vertExtent * Screen.width / Screen.height;

//        //// Calculations assume _map is position at the origin
//        //minX = horzExtent - mapX / 2.0;
//        //maxX = mapX / 2.0 - horzExtent;
//        //minY = vertExtent - mapY / 2.0;
//        //maxY = mapY / 2.0 - vertExtent;

//        //Handles.DrawLine(_cell.WorldPos, cellN.WorldPos);



//        var cells = _map.GetCells();
//        for (int i = 0; i < cells.Count; i++)
//        {
//            MG_HexCell _cell = cells[i];
//            //var neighbours = _cell.GetNeighbours();
//            //------------Debug.Log("neighbours.Count = " + neighbours.Count);
//            //for (int j = 0; j < neighbours.Count; j++)
//            //{
//            //    MG_HexCell cellN = neighbours[j];


//            //    //------float distance = Vector3.Distance(_cell.WorldPos, cellN.WorldPos);
//            //    //-----------Vector3 diff = cellN.WorldPos - _cell.WorldPos;
//            //    //----------Handles.Label(_cell.WorldPos + (diff / 2), ("WooHOo" + distance.ToString()), EditorStyles.whiteBoldLabel);
//            //    if (true == false)
//            //    {
//            //        Color color = Handles.color;
//            //        Handles.color = Color.green;
//            //        Handles.DrawLine(_cell.WorldPos, cellN.WorldPos);
//            //        Handles.color = color;
//            //    }
//            //    else
//            //    {
//            //        Handles.DrawLine(_cell.WorldPos, cellN.WorldPos);
//            //    }
//            //}

//            GUIStyle style = new GUIStyle();
//            style.normal.textColor = Color.white;
//            style.fontSize = 16;
//            style.fontStyle = FontStyle.Bold;
//            Handles.Label(_cell.WorldPos, ("" + _cell.Pos), style);

//        }
//    }

//    public override void OnInspectorGUI()
//    {
//        //_map.nodes.Erase();
//        //foreach (Transform child in _map.transform)
//        //{
//        //    Node node = child.GetComponent<Node>();
//        //    if (node != null)
//        //    {
//        //        _map.nodes.AddItem(node);
//        //    }
//        //}
//        //base.OnInspectorGUI();
//        //EditorGUILayout.Separator();
//        //m_From = (Node)EditorGUILayout.ObjectField("From", m_From, typeof(Node), true);
//        //m_To = (Node)EditorGUILayout.ObjectField("To", m_To, typeof(Node), true);
//        //m_Follower = (Follower)EditorGUILayout.ObjectField("Follower", m_Follower, typeof(Follower), true);
//        //if (GUILayout.Button("Show Shortest Path"))
//        //{
//        //    m_Path = _map.GetShortestPath(m_From, m_To);
//        //    if (m_Follower != null)
//        //    {
//        //        m_Follower.Follow(m_Path);
//        //    }
//        //    Debug.Log(m_Path);
//        //    SceneView.RepaintAll();
//        //}
//    }
//}

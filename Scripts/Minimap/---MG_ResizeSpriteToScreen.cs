//using Sirenix.OdinInspector;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class MG_ResizeSpriteToScreen : MonoBehaviour
//{
//    public SpriteRenderer spriteRenderer;
//    public Camera miniMapCamera;
//    public Tilemap tilemap;

//    public float x;
//    public float y;

//    [Button]
//    public void ResizeSpriteToScreen()
//    {
//        SpriteRenderer sr = spriteRenderer;

//        spriteRenderer.transform.localScale = new Vector3(1, 1, 1);

//        float width = sr.sprite.bounds.size.x;
//        float height = sr.sprite.bounds.size.y;

//        float worldScreenHeight = miniMapCamera.orthographicSize * 2f;

//        Vector3 vector = spriteRenderer.transform.localScale;
//        vector.y = worldScreenHeight / height;
//        vector.x = worldScreenHeight / width;

//        if (vector.y > vector.x)
//        {
//            vector.y = vector.x;
//        }
//        else
//        {
//            vector.x = vector.y;
//        }

//        spriteRenderer.transform.localScale = vector;
//    }
//    [Button]
//    public void ResizeTileMapToScreen()
//    {
//        float width = tilemap.localBounds.size.x;
//        float height = tilemap.localBounds.size.y;

//        //tilemap.transform.localScale = new Vector3(-25, -20, 1);
//        //tilemap.transform.localPosition = new Vector3(-25, -20, 0);

//        float worldScreenHeight = miniMapCamera.orthographicSize * 2f;

//        Vector3 vector = spriteRenderer.transform.localScale;
//        vector.y = worldScreenHeight / height;
//        vector.x = worldScreenHeight / width;

//        if (vector.y > vector.x)
//        {
//            vector.y = vector.x;
//        }
//        else
//        {
//            vector.x = vector.y;
//        }
//        x = vector.x;
//        y = vector.y;

//        //------------------------tilemap.transform.localScale = vector;
//        miniMapCamera.transform.localScale = vector;
//        //var aaa = tilemap.cellBounds.center;
//        //transform.position = tilemap.GetCellCenterWorld(aaa);
//        //Debug.Log(aaa);
//        //miniMapCamera.transform.position = new Vector3(aaa.x, aaa.y, -14f);

//        //GameObject huan = GameObject.Find("PinkPixel");
//        //huan.transform.position = new Vector3(aaa.x, aaa.y, 0);


//        //miniMapCamera.transform.position = new Vector3(26, 21, -14);
//        //miniMapCamera.orthographicSize = 30;
//    }



//    public static Bounds OrthographicBounds(Camera camera)
//    {
//        float screenAspect = (float)Screen.width / (float)Screen.height;
//        float cameraHeight = camera.orthographicSize * 2;
//        Bounds bounds = new Bounds(
//            camera.transform.position,
//            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
//        return bounds;
//    }
//}
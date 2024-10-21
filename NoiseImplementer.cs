using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NoiseGeneratorUtil;
using Unity.Processors;

public class NoiseImplementer : MonoBehaviour
{
    [SerializeField] int numPoints = 10;
    [SerializeField] int width = 256;
    [SerializeField] int height = 256;
    [SerializeField] int scale = 10;
    [SerializeField] float perlinOffset = 10;
    [SerializeField] float lacunarity = 1;
    [SerializeField] float persistence = 1;
    [SerializeField] int octaves = 3;


}
#if UNITY_EDITOR
    [CustomEditor(typeof(NoiseImplementer))]
    public class NoiseGeneratorEditor : Editor{
        private SerializedProperty widthProperty;
        private SerializedProperty heightProperty;
        private SerializedProperty pointsProperty;
        private SerializedProperty scaleProperty;
        private SerializedProperty offsetProperty;
        private SerializedProperty lacunarityProperty;
        private SerializedProperty persistenceProperty;
        private SerializedProperty octavesProperty;

        Texture2D PerlinWorley;
        private void OnEnable() {
            widthProperty = serializedObject.FindProperty("width");
            heightProperty = serializedObject.FindProperty("height");
            pointsProperty = serializedObject.FindProperty("numPoints");
            scaleProperty = serializedObject.FindProperty("scale");
            offsetProperty = serializedObject.FindProperty("perlinOffset");
            lacunarityProperty = serializedObject.FindProperty("lacunarity");
            persistenceProperty = serializedObject.FindProperty("persistence");
            octavesProperty = serializedObject.FindProperty("octaves")
            
        }
        
        public override void OnInspectorGUI()
        {
            
            base.OnInspectorGUI();
            int width = widthProperty.intValue;
            int height = heightProperty.intValue;
            int numPoints = pointsProperty.intValue;
            int scale = scaleProperty.intValue;
            float offset = offsetProperty.floatValue;
            float lacunarity = lacunarityProperty.floatValue;
            float persistence = persistenceProperty.floatValue;
            int octaves = octavesProperty.intValue;
        
            if(GUILayout.Button("Generate Worley",GUILayout.Width(180f))){
                //Texture2D WorlyNoise = NoiseGenerator.worleyNoise(256,256,10);
                Texture2D WorlyNoise = NoiseGenerator.worleyNoiseTile(width,height,numPoints);
                byte[] bytes = WorlyNoise.EncodeToPNG();
                System.IO.File.WriteAllBytes("Assets/WorleyNoise.png",bytes);
                Debug.Log("Finished Creating Texture Asset");
            }
            if(GUILayout.Button("Generate Perlin",GUILayout.Width(180f))){
                //Texture2D WorlyNoise = NoiseGenerator.worleyNoise(256,256,10);
                Texture2D PerlinNoise = NoiseGenerator.perlinNoise(width,height,scale,1);
                byte[] bytes = PerlinNoise.EncodeToPNG();
                System.IO.File.WriteAllBytes("Assets/PerlinNoise.png",bytes);
                Debug.Log("Finished Creating Texture Asset");
            }
            if(GUILayout.Button("Generate Perlin-Worley",GUILayout.Width(180f))){
                //Texture2D WorlyNoise = NoiseGenerator.worleyNoise(256,256,10);
                PerlinWorley = NoiseGenerator.perlinWorley(width,height,numPoints,scale,octaves,lacunarity,persistence);
                //GUI.DrawTexture(new Rect(200, 200, 100, 100),PerlinWorley);
            }
            if(GUILayout.Button("Save Perlin-Worley",GUILayout.Width(180f))){
                byte[] bytes = PerlinWorley.EncodeToPNG();
                System.IO.File.WriteAllBytes("Assets/PerlinWorley.png",bytes);
                Debug.Log("Finished Creating Texture Asset");
            }
            
            if(PerlinWorley){
                float imageWidth = EditorGUIUtility.currentViewWidth - 40;
                float imageHeight = imageWidth * PerlinWorley.height / PerlinWorley.width;
                Rect rect = GUILayoutUtility.GetRect(256,256);
                EditorGUI.DrawPreviewTexture(rect, PerlinWorley,null,ScaleMode.ScaleToFit);
            }
            
        }
    }
#endif
namespace NoiseGeneratorUtil
{
    public static class NoiseGenerator
    {
        //Coding train worley noise
        public static Texture2D  worleyNoise(int width, int height, int numPoints){
            Texture2D texture =  new Texture2D(width,height);
            Vector2[] points = new Vector2[numPoints];
            for(int i = 0;i<points.Length;i++){
                points[i] = new Vector2(Random.Range(0,width),Random.Range(0,height));
            }
            for(int x = 0;x<width;x++){
                for(int y = 0;y<height;y++){
                    float[] distance = new float[points.Length];
                    for(int i = 0;i<points.Length;i++){
                        distance[i] = Vector2.Distance(new Vector2(x,y),points[i]);
                    }
                    System.Array.Sort(distance);
                    float noise = distance[0]/(width/2);
                    noise = 1-noise;
                    float rand = Random.Range(0f,1f);
                    texture.SetPixel(x,y,new Color(noise,noise,noise));
                    
                }
            }
            return texture;
        }
        public static Texture2D  worleyNoiseTile(int width, int height, int numPoints){
            Texture2D texture =  new Texture2D(width,height);
            Vector2[] points = new Vector2[numPoints];
            List<Vector2> tilePoints = new List<Vector2>();
            for(int i = 0;i<points.Length;i++){
                points[i] = new Vector2(Random.Range(0,width),Random.Range(0,width));
            }
            for(int xOffset=-1;xOffset<=1;xOffset++){
                for(int yOffset=-1;yOffset<=1;yOffset++){
                    for(int i=0;i<numPoints;i++){
                        tilePoints.Add(new Vector2(points[i].x+(xOffset*width),points[i].y+(yOffset*height)));
                    }
                }
            }
            

            for(int x = 0;x<width;x++){
                for(int y = 0;y<height;y++){
                    float[] distance = new float[tilePoints.Count];
                    for(int i = 0;i<tilePoints.Count;i++){
                        distance[i] = Vector2.Distance(new Vector2(x,y),tilePoints[i]);
                    }
                    System.Array.Sort(distance);
                    float noise = distance[0]/(width/2);
                    noise = 1-noise;
                    float rand = Random.Range(0f,1f);
                    texture.SetPixel(x,y,new Color(noise,noise,noise));
                    
                    
                }
            }
            texture.Apply();
            return texture;
        }
        public static Texture2D perlinNoise(int width, int height,int scale,float offset){
            Texture2D perlinTex = new Texture2D(width,height);
            for(int x = 0; x<width; x++){
                for(int y = 0;y<height; y++){
                    //float px = (float)x/width;
                    //px = px*scale;
                    //float py = (float)y/height;
                    //py = py*scale;
                    float px = (float)(x/(float)width) * scale+offset;
					float py = (float)(y/(float)height) * scale+offset;

                    //float noise = Mathf.PerlinNoise(((float)x/width)*scale*offset,((float)y/height)*scale*offset)/offset;
                    float noise = Mathf.PerlinNoise(px,py);
                   
                    //noise = Mathf.InverseLerp (-1, 1, noise);
                     
                    //noise*=100;
                    perlinTex.SetPixel(x,y,new Color(noise,noise,noise));
                    //perlinTex.Apply();
                }
            }
            perlinTex.Apply();
            return perlinTex;
        }
        public static Texture2D worleyFBM(int width,int height){
            Texture2D worley1 = worleyNoiseTile(width,height,15);
            Texture2D worley2 = worleyNoiseTile(width,height,50);
            Texture2D worley3 = worleyNoiseTile(width,height,100);

            Texture2D worleyFBM = new Texture2D(width,height);
            for(int x=0;x<width;x++){
                for(int y=0;y<height;y++){
                    Color noise = worley1.GetPixel(x,y)*0.625f+worley2.GetPixel(x,y)*0.25f+worley3.GetPixel(x,y)*0.125f;
                    worleyFBM.SetPixel(x,y,noise);
                }
            }
            worleyFBM.Apply();
            return worleyFBM;
        }
        public static Texture2D perlinFBM(int width,int height,int octaves,float lacunarity,float persistence){
            Texture2D perlinFBM = new Texture2D(width,height);
            for(int x = 0;x<width;x++){
                for(int y = 0;y<height;y++){
                    float frequency = 1.0f / width*height;
                    float amplitude = persistence;
                    float noise = 0;
                    for(int i = 0;i<octaves;i++){
                        noise += Mathf.PerlinNoise(x * frequency,y*frequency)*amplitude;
                        frequency *= lacunarity;
                        amplitude *= persistence;
                    }
                    perlinFBM.SetPixel(x,y,new Color(noise,noise,noise));
                }
            }
            return perlinFBM; 
        }
        public static Texture2D perlinWorley(int width,int height,int numPoints,int octaves, float lacunarity,float persistence){
            Texture2D perlinWorley = new Texture2D(width,height);
            //Texture2D tempWorley = worleyFBM(width,height);
            //Texture2D tempPerlin = perlinFBM(width,height);
            //Texture2D tempPerlin = perlinNoise(width,height,scale,perlinOffset);
            Texture2D tempPerlin = perlinFBM(width,height,octaves,lacunarity,persistence);

            for(int x=0;x<width;x++){
                for(int y=0;y<height;y++){
                    //perlinWorley.SetPixel(x,y,tempWorley.GetPixel(x,y)*.4f+tempPerlin.GetPixel(x,y)-new Color(.5f,.5f,.5f));
                    perlinWorley = tempPerlin;
                    
                }
            }
            perlinWorley.Apply();
            return perlinWorley;
            
        }
    }
    
}

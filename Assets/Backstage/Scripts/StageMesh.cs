using System.Collections.Generic;
using UnityEngine;

namespace Backstage
{
    // A small, allocation-conscious 2D drawing layer. All art is original geometry.
    public sealed class StageMesh : MonoBehaviour
    {
        readonly List<Vector3> vertices = new List<Vector3>(24000);
        readonly List<Color> colors = new List<Color>(24000);
        readonly List<int> triangles = new List<int>(40000);
        Mesh mesh;
        Material material;
        bool ownsMaterial;

        public void Initialize()
        {
            mesh = new Mesh { name = "Backstage procedural art" };
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            material = Resources.Load<Material>("StageMaterial");
            if(material!=null&&(material.shader==null||!material.shader.isSupported))
            {
                Debug.LogWarning("Backstage: stage material shader unavailable, switching to fallback.");
                material=null;
            }
            if(material == null)
            {
                Shader shader=Resources.Load<Shader>("StageShader");
                if(shader==null||!shader.isSupported)
                {
                    Debug.LogWarning("Backstage: custom stage shader missing or unsupported, using a vertex-color fallback.");
                    shader=Shader.Find("Sprites/Default");
                }
                if(shader==null)shader=Shader.Find("Unlit/Color");
                if(shader==null)
                {
                    Debug.LogError("Backstage: no compatible shader available. Check Built-in Render Pipeline and Graphics settings.");
                    enabled=false;return;
                }
                material=new Material(shader);
                ownsMaterial=true;
            }
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
        public void Begin() { vertices.Clear(); colors.Clear(); triangles.Clear(); }
        public void End()
        {
            mesh.Clear();
            mesh.SetVertices(vertices); mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
        }
        public void Triangle(Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            int n = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            colors.Add(color); colors.Add(color); colors.Add(color);
            triangles.Add(n); triangles.Add(n + 1); triangles.Add(n + 2);
        }
        public void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        { Triangle(a,b,c,color); Triangle(a,c,d,color); }
        public void Rect(float x, float y, float w, float h, Color color)
        { Quad(new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),color); }
        public void Ellipse(Vector2 p, float rx, float ry, Color color, int segments = 20)
        {
            for(int i=0;i<segments;i++)
            {
                float a=i*Mathf.PI*2/segments, b=(i+1)*Mathf.PI*2/segments;
                Triangle(p,p+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),p+new Vector2(Mathf.Cos(b)*rx,Mathf.Sin(b)*ry),color);
            }
        }
        public void Disc(Vector2 p, float r, Color c, int segments=20) { Ellipse(p,r,r,c,segments); }
        public void Line(Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;
            Quad(a+n,b+n,b-n,a-n,color);
        }
        public void Ring(Vector2 p,float r,float width,Color c,int segments=40)
        {
            for(int i=0;i<segments;i++)
            {
                float a=i*Mathf.PI*2/segments,b=(i+1)*Mathf.PI*2/segments;
                Line(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,width,c);
            }
        }
        public void Glow(Vector2 p,float r,Color c)
        {
            for(int i=5;i>0;i--) { Color t=c; t.a=c.a*(6-i)*.024f; Disc(p,r*i/3,t,28); }
        }
        void OnDestroy()
        {
            if(mesh!=null) Destroy(mesh);
            if(ownsMaterial && material!=null) Destroy(material);
        }
    }
}

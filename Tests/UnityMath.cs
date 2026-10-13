// Test-only math shim; excluded from Unity because it lives outside Assets.
using System;
namespace UnityEngine
{
 public struct Vector2
 {
  public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}
  public float sqrMagnitude=>x*x+y*y;public float magnitude=>(float)Math.Sqrt(sqrMagnitude);public Vector2 normalized=>magnitude>.000001f?this/magnitude:zero;
  public static Vector2 zero=>new Vector2();public static Vector2 up=>new Vector2(0,1);public static Vector2 down=>new Vector2(0,-1);public static Vector2 left=>new Vector2(-1,0);public static Vector2 right=>new Vector2(1,0);
  public static Vector2 operator+(Vector2 a,Vector2 b)=>new Vector2(a.x+b.x,a.y+b.y);public static Vector2 operator-(Vector2 a,Vector2 b)=>new Vector2(a.x-b.x,a.y-b.y);public static Vector2 operator-(Vector2 a)=>new Vector2(-a.x,-a.y);public static Vector2 operator*(Vector2 a,float b)=>new Vector2(a.x*b,a.y*b);public static Vector2 operator*(float b,Vector2 a)=>a*b;public static Vector2 operator/(Vector2 a,float b)=>new Vector2(a.x/b,a.y/b);
  public static float Dot(Vector2 a,Vector2 b)=>a.x*b.x+a.y*b.y;public static float Distance(Vector2 a,Vector2 b)=>(a-b).magnitude;public static Vector2 ClampMagnitude(Vector2 a,float max)=>a.magnitude>max?a.normalized*max:a;
 }
 public struct Color {public float r,g,b,a;public Color(float r,float g,float b,float a=1){this.r=r;this.g=g;this.b=b;this.a=a;}public static Color white=>new Color(1,1,1);public static Color Lerp(Color a,Color b,float t)=>new Color(a.r+(b.r-a.r)*t,a.g+(b.g-a.g)*t,a.b+(b.b-a.b)*t,a.a+(b.a-a.a)*t);}
 public static class Mathf
 {
  public static float Pow(float a,float b)=>(float)Math.Pow(a,b);
  public const float PI=(float)Math.PI;
  public static float Clamp(float v,float a,float b)=>Math.Max(a,Math.Min(b,v));public static int Clamp(int v,int a,int b)=>Math.Max(a,Math.Min(b,v));public static float Clamp01(float v)=>Clamp(v,0,1);
  public static float Min(float a,float b)=>Math.Min(a,b);public static float Max(float a,float b)=>Math.Max(a,b);public static int Max(int a,int b)=>Math.Max(a,b);public static int Min(int a,int b)=>Math.Min(a,b);
  public static float Abs(float v)=>Math.Abs(v);public static float Sin(float v)=>(float)Math.Sin(v);public static float Cos(float v)=>(float)Math.Cos(v);public static float Sqrt(float v)=>(float)Math.Sqrt(v);public static int CeilToInt(float v)=>(int)Math.Ceiling(v);
 }
}

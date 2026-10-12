using UnityEngine;
namespace Backstage
{
    // Swept point against a capsule: exact sides and circular endpoints, earliest contact.
    public static class StageCollision
    {
        public static bool Circle(Vector2 p,Vector2 delta,Vector2 center,float radius,out float t,out Vector2 normal)
        {
            t=2;normal=Vector2.zero;Vector2 m=p-center;float a=Vector2.Dot(delta,delta),c=Vector2.Dot(m,m)-radius*radius;
            if(c<=0){normal=m.sqrMagnitude>.00001f?m.normalized:-delta.normalized;if(Vector2.Dot(delta,normal)<0){t=0;return true;}return false;}
            if(a<.0000001f)return false;float b=Vector2.Dot(m,delta),disc=b*b-a*c;if(disc<0)return false;
            float hit=(-b-Mathf.Sqrt(disc))/a;if(hit<0||hit>1)return false;t=hit;normal=(p+delta*t-center).normalized;return true;
        }
        public static bool Capsule(Vector2 p,Vector2 delta,Vector2 a,Vector2 b,float radius,out float t,out Vector2 normal)
        {
            t=2;normal=Vector2.zero;Vector2 ab=b-a;float length=ab.magnitude;
            if(length<.00001f)return Circle(p,delta,a,radius,out t,out normal);
            Vector2 axis=ab/length,perp=new Vector2(-axis.y,axis.x);float side=Vector2.Dot(p-a,perp),speed=Vector2.Dot(delta,perp);
            for(int sign=-1;sign<=1;sign+=2)
            {
                Vector2 n=perp*sign;if(Vector2.Dot(delta,n)>=-.000001f)continue;
                float hit=(sign*radius-side)/speed;
                if(hit<0&&Mathf.Abs(side)<radius)hit=0;
                float along=Vector2.Dot(p+delta*hit-a,axis);
                if(hit>=0&&hit<=1&&along>=0&&along<=length&&hit<t){t=hit;normal=n;}
            }
            float end;Vector2 en;
            if(Circle(p,delta,a,radius,out end,out en)&&end<t){t=end;normal=en;}
            if(Circle(p,delta,b,radius,out end,out en)&&end<t){t=end;normal=en;}
            return t<=1;
        }
        public static bool Border(Vector2 p,Vector2 delta,float r,out float t,out Vector2 normal)
        {
            t=2;normal=Vector2.zero;
            Plane(p.x,delta.x,-12.6f+r,Vector2.right,ref t,ref normal);
            Plane(p.x,delta.x,12.6f-r,Vector2.left,ref t,ref normal);
            Plane(p.y,delta.y,-5.85f+r,Vector2.up,ref t,ref normal);
            Plane(p.y,delta.y,5.75f-r,Vector2.down,ref t,ref normal);
            return t<=1;
        }
        static void Plane(float p,float d,float edge,Vector2 n,ref float best,ref Vector2 normal)
        {
            float inward=n.x+n.y;if(d*inward>=-.000001f)return;
            float t=Mathf.Max(0,(edge-p)/d);
            if(t<=1&&t<best){best=t;normal=n;}
            else if(Mathf.Abs(t-best)<.00001f)normal=(normal+n).normalized;
        }
    }
}

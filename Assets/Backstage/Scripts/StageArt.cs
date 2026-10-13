using UnityEngine;

namespace Backstage
{
    public sealed class StageArt
    {
        readonly StageMesh g;
        static readonly Color ink=C("111525"), gold=C("D7AF68"), teal=C("92C9C7"), rose=C("DE797F");
        public StageArt(StageMesh mesh) { g=mesh; }
        public static Color C(string hex,float alpha=1)
        { Color c; ColorUtility.TryParseHtmlString("#"+hex,out c); c.a=alpha; return c; }

        public void Draw(StageWorld w,float time,Vector2 cursor,bool showCursor,float cutscene=-1)
        {
            g.Begin();
            Background(time,w.Level);
            if(cutscene>=0){Ending(w,time,cutscene);Foreground(time);g.End();return;}
            Decorations(w.Level,time);
            StageDressing(w.Level,time);
            Light(new Vector2(-10.9f,6.5f),new Vector2(w.Lamps[0].Mode==0?-3:4,-5.7f),C("F4DFA9",.085f));
            Light(new Vector2(10.9f,6.5f),new Vector2(w.Lamps[1].Mode==0?3:-4,-5.7f),C("6CE1DB",.085f));
            foreach(Barrier b in w.Barriers)
            {
                Vector2 n=new Vector2(-(b.B-b.A).y,(b.B-b.A).x).normalized;
                float halfWidth=b.Width*.5f;
                g.Quad(b.A-n*halfWidth,b.A+n*halfWidth,b.B+n*halfWidth,b.B-n*halfWidth,C("0B1221",.88f));
                g.Disc(b.B,halfWidth,C("0B1221",.88f));
                float pulse=Mathf.Max(w.Lamps[0].Pulse,w.Lamps[1].Pulse)/.65f;
                Color edge=Color.Lerp(C("8CE1DF",.65f),C("FFE2A1",.95f),pulse);
                g.Line(b.A-n*halfWidth,b.B-n*halfWidth,.03f+pulse*.025f,edge);
                g.Line(b.A+n*halfWidth,b.B+n*halfWidth,.03f+pulse*.025f,edge);
            }
            for(int i=0;i<8;i++)
            {
                float x=-11+i*3.1f;
                Vector2 a=new Vector2(x,5.6f),b=new Vector2(x+Mathf.Sin(time*.4f+i)*1.3f,-5.6f);
                g.Line(a,b,.012f,C("F8DAAE",.10f));
            }
            Puppet(w,time);
            for(int i=0;i<w.Props.Length;i++) Prop(w.Props[i],i,w.Level);
            for(int i=0;i<w.Props.Length;i++)
            {
                if(w.Lamps[(i==1||i==3)?1:0].Pending<=0) continue;
                Barrier future=w.ShadowFor(i,true);
                for(int k=0;k<12;k++)
                    g.Line(future.A+(future.B-future.A)*(k/12f),future.A+(future.B-future.A)*((k+.5f)/12f),.07f,gold);
            }
            for(int i=0;i<w.Lamps.Length;i++) Switch(w.Lamps[i],i,time);
            foreach(Mine m in w.Mines) ButtonMine(m,time);
            foreach(Candy c in w.Candies)Sweet(c,time);
            if(w.Tutorial) TutorialMarker(w,time);
            if(showCursor)Trajectory(w);
            foreach(Enemy e in w.Enemies)
            {
                if(e.Kind==EnemyKind.Rabbit) Rabbit(e,time);
                else if(e.Kind==EnemyKind.Mask) Mask(e,time);
                else if(e.Kind==EnemyKind.Guard) Guard(e,time);
                else Hand(e,time);
            }
            Shadow(w.Player,w.Aim,w.BodyDirection,time,w.Invulnerable>0 && Mathf.Sin(time*35)>0);
            foreach(Bullet b in w.Bullets)
            {
                Color c=b.Friendly?(b.Bounces>0?teal:C("B4A5EF")):C("FF9D77");
                g.Glow(b.Position,.26f,c); g.Line(b.Position-b.Velocity.normalized*.35f,b.Position,.075f,c);
                g.Disc(b.Position,b.Friendly? .1f:.125f,C("FFF2CC"),12);
            }
            if(w.State==PlayState.Playing)
            {
                foreach(ShotEcho echo in w.Echoes)
                {
                    float fade=Mathf.Pow(echo.Life/.65f,2);Color c=C("A3E4DB",fade*.55f);
                    g.Line(echo.Origin,echo.Turn1,.025f,c);
                    if(echo.Turns>1)g.Line(echo.Turn1,echo.Turn2,.025f,c);
                    g.Line(echo.Turns>1?echo.Turn2:echo.Turn1,echo.End,.025f,c);
                    g.Ring(echo.Turn1,.14f,.02f,c,16);
                    if(echo.Turns>1)g.Ring(echo.Turn2,.14f,.02f,c,16);
                }
                foreach(StageAccent accent in w.Accents)
                {
                    float progress=1-accent.Life/accent.Duration;Color c=C(accent.Defeat?"F5CE88":"A3E4DB",(1-progress)*.65f);
                    g.Ring(accent.Position,.12f+progress*(accent.Defeat? .9f:.38f),.035f*(1-progress)+.008f,c,24);
                    if(accent.Defeat)for(int i=0;i<4;i++){Vector2 d=StageWorld.Rotate(Vector2.up,i*Mathf.PI*.5f+.4f);g.Line(accent.Position+d*(.18f+progress*.45f),accent.Position+d*(.36f+progress*.72f),.025f,c);}
                }
            }
            foreach(Spark s in w.Sparks)
            {
                Color c=s.Color;c.a=s.Life/s.MaxLife;
                g.Line(s.Position-s.Velocity*.06f,s.Position,.04f,c);
            }
            Foreground(time);
            if(showCursor)
            {
                g.Ring(cursor,.19f,.018f,C("F6E8CD",.8f),20);
                g.Disc(cursor,.025f,teal,8);
                for(int i=0;i<4;i++) { Vector2 d=StageWorld.Rotate(Vector2.up,i*Mathf.PI/2);g.Line(cursor+d*.25f,cursor+d*.34f,.022f,gold); }
            }
            g.End();
        }
        public void DrawDuel(StageDuel d,float time,Vector2 cursor)
        {
            g.Begin();Background(time,2);Decorations(2,time);
            g.Ring(Vector2.zero,4.5f,.055f,C("D7AF68",.19f),80);
            foreach(Barrier b in StageDuel.Cover)
            {
                Vector2 normal=new Vector2(-(b.B-b.A).y,(b.B-b.A).x).normalized*(b.Width*.5f);
                g.Quad(b.A-normal,b.A+normal,b.B+normal,b.B-normal,C("0B1221",.88f));
                g.Line(b.A-normal,b.B-normal,.05f,C("8CE1DF",.85f));
                g.Line(b.A+normal,b.B+normal,.05f,C("8CE1DF",.85f));
            }
            for(int i=0;i<10;i++)
            {
                float x=-10+i*2.1f;
                g.Line(new Vector2(x,-5.3f),new Vector2(x+.8f,-5.3f),.045f,C("D7AF68",.18f));
            }
            DuelActor(d.HostPosition,d.HostAim,C("86E1DB"),d.HostFlash,time);
            if(d.Connected)DuelActor(d.GuestPosition,d.GuestAim,C("F094A9"),d.GuestFlash,time);
            foreach(DuelShot s in d.Shots)
            {
                Color c=s.HostShot?teal:rose;
                g.Glow(s.Position,.25f,c);g.Disc(s.Position,.13f,c);
            }
            Foreground(time);
            g.Ring(cursor,.2f,.025f,gold);
            g.End();
        }
        void DuelActor(Vector2 p,Vector2 aim,Color edge,float flash,float time)
        {
            g.Glow(p,.65f,edge);
            g.Ellipse(p+Vector2.down*.36f,.61f,.24f,C("090E1A",.8f));
            g.Triangle(p+new Vector2(-.51f,-.37f),p+new Vector2(.51f,-.37f),p+Vector2.up*.53f,edge);
            g.Triangle(p+new Vector2(-.43f,-.32f),p+new Vector2(.43f,-.32f),p+Vector2.up*.46f,flash>0?Color.white:ink);
            g.Disc(p+Vector2.up*.23f,.33f,edge);
            g.Disc(p+Vector2.up*.23f,.28f,ink);
            Vector2 a=aim.sqrMagnitude>.01f?aim.normalized:Vector2.up;
            g.Line(p+a*.12f,p+a*.8f,.17f,edge);
            g.Disc(p+a*.83f,.12f,C("FFF1D4"));
            g.Disc(p+Vector2.up*.27f+Vector2.left*.10f,.04f,C("FFF1D4"));
            g.Disc(p+Vector2.up*.27f+Vector2.right*.10f,.04f,C("FFF1D4"));
        }
        void Background(float time,int level)
        {
            g.Rect(-22,-13,44,26,C("0B101A"));
            g.Rect(-13.4f,-6.5f,26.8f,13.8f,C("2C2630"));
            // Painted wooden planks with reproducible wood grain, no textures required.
            for(int i=0;i<26;i++)
            {
                float x=-13+i;
                g.Rect(x,-6.2f,.98f,12.3f,Color.Lerp(C(level==1?"3E3437":level==2?"332B46":"202F3D"),C(level==1?"665048":level==2?"635164":"46546B"),(i%5)*.15f));
                for(int j=0;j<5;j++)
                {
                    float y=-5.9f+j*2.4f+(i%3)*.25f;
                    g.Line(new Vector2(x+.08f,y),new Vector2(x+.88f,y),.018f,C("1C202B",.6f));
                    g.Disc(new Vector2(x+.12f,y+.1f),.022f,C("B68C5B",.45f),6);
                    g.Line(new Vector2(x+.24f,y+.2f),new Vector2(x+.3f,y+1.9f),.014f,C("C19160",.12f));
                }
            }
            g.Rect(-13,-6.15f,26,.055f,gold); g.Rect(-13,5.9f,26,.035f,C("D7AF68",.6f));
            g.Ring(Vector2.zero,5,.025f,C("C7A779",.13f),80);
            g.Ring(Vector2.zero,5.2f,.012f,C("C7A779",.1f),80);
            // Cardboard town at the rear of the stage.
            for(int i=0;i<(level==1?11:0);i++)
            {
                float x=-12+i*2.35f, h=1.1f+(i%3)*.35f;
                g.Rect(x,4.75f,1.8f,h,C(i%2==0?"3D4851":"48505A"));
                g.Triangle(new Vector2(x-.15f,4.75f+h),new Vector2(x+.65f,5.7f+h),new Vector2(x+2,4.75f+h),C("222C3C"));
                g.Rect(x+.32f,5.1f,.27f,.42f,C("E2BC78",.45f));
                g.Rect(x+1.05f,5.25f,.27f,.42f,C("E2BC78",.3f));
            }
            // Moon, hanging on an actual visible thread.
            g.Line(new Vector2(7,8),new Vector2(7,6.6f),.02f,gold);
            g.Disc(new Vector2(7,6.3f),.65f,C("E2C796"),32);
            g.Disc(new Vector2(7.3f,6.5f),.59f,C("202333"),32);
            for(int i=0;i<22;i++)
            {
                float x=Mathf.Sin(i*15.7f)*11.7f,y=Mathf.Cos(i*6.2f+time*.12f)*4.9f;
                g.Disc(new Vector2(x,y),.018f,C("FFE5B3",.22f+.12f*Mathf.Sin(time+i)),6);
            }
        }
        void Sweet(Candy c,float time)
        {
            Vector2 p=c.Position+Vector2.up*Mathf.Sin(time*3)*.07f;Color candy=C("F2A7CB");
            g.Glow(p,.44f,candy);
            g.Triangle(p+new Vector2(-.23f,0),p+new Vector2(-.58f,.23f),p+new Vector2(-.58f,-.23f),teal);
            g.Triangle(p+new Vector2(.23f,0),p+new Vector2(.58f,.23f),p+new Vector2(.58f,-.23f),teal);
            g.Ellipse(p,.33f,.23f,candy,20);g.Line(p+new Vector2(-.1f,-.18f),p+new Vector2(.05f,.19f),.1f,C("FFF1D9"));
            g.Line(p+new Vector2(.07f,-.18f),p+new Vector2(.22f,.15f),.07f,C("FFF1D9"));
        }
        void Trajectory(StageWorld w)
        {
            Vector2 p=w.Player+w.Aim*.12f,d=w.Aim;float travel=18;
            for(int bounce=0;bounce<3;bounce++)
            {
                float fraction;Vector2 normal;bool hit=w.TraceWall(p,d*travel,.11f,out fraction,out normal);
                float length=hit?travel*fraction:travel;bool prop=false;
                foreach(Vector2 chair in w.Props){float f;Vector2 n;if(StageCollision.Circle(p,d*travel,chair,.69f,out f,out n)&&f*travel<length){length=f*travel;prop=true;}}
                for(float t=0;t<length;t+=.38f)g.Line(p+d*t,p+d*Mathf.Min(length,t+.16f),.025f,C("BCA9EB",.34f));
                if(!hit||prop)break;Vector2 end=p+d*length;g.Ring(end,.13f,.024f,teal,14);
                travel-=length;if(travel<=0)break;p=end+normal*.004f;d-=2*Vector2.Dot(d,normal)*normal;
            }
        }
        void Decorations(int level,float time)
        {
            // Bright rails match the collision limits exactly.
            g.Line(new Vector2(-12.6f,-5.85f),new Vector2(12.6f,-5.85f),.055f,teal);
            g.Line(new Vector2(-12.6f,5.75f),new Vector2(12.6f,5.75f),.055f,teal);
            g.Line(new Vector2(-12.6f,-5.85f),new Vector2(-12.6f,5.75f),.055f,teal);
            g.Line(new Vector2(12.6f,-5.85f),new Vector2(12.6f,5.75f),.055f,teal);
            for(int side=-1;side<=1;side+=2)
            {
                Vector2 p=new Vector2(side*12.95f,-3.6f);
                g.Rect(p.x-.25f,p.y-.3f,.5f,.65f,C("A46A60"));
                for(int j=0;j<4;j++){Vector2 tip=p+new Vector2(Mathf.Sin(j*2)*.23f,.5f+j*.25f);g.Line(p,tip,.035f,teal);Star(tip,.16f,gold);}
                g.Ring(new Vector2(side*12.95f,1.6f),.30f,.07f,gold,20);
                for(int j=0;j<6;j++)Star(new Vector2(side*(10.5f-j*.5f),6.6f+(j%2)*.28f),.09f,C("D7AF68",.6f));
            }
            if(level==1)
            {
                // Paper houses, torn flats, toy bridges and a painted route.
                for(int i=0;i<7;i++)
                {
                    Vector2 p=new Vector2(-9.5f+i*3,-5.6f+(i%2)*.25f);
                    g.Rect(p.x,p.y,1.6f,.18f,C("C0A17A",.28f));
                    g.Line(p,p+new Vector2(1.6f,.35f),.04f,C("E8C791",.22f));
                    Star(p+new Vector2(.7f,.15f),.11f,C("F3D5A6",.4f));
                }
                g.Ring(new Vector2(6,3),1.35f,.036f,C("E9BC82",.17f));
                g.Rect(-11.4f,1.9f,2.2f,.14f,C("745E4F",.55f));
                g.Line(new Vector2(-11.1f,1.9f),new Vector2(-10.7f,2.75f),.06f,C("C9B08A",.55f));
                g.Line(new Vector2(-9.5f,1.9f),new Vector2(-9.9f,2.75f),.06f,C("C9B08A",.55f));
                g.Rect(7.9f,-3.3f,1.5f,.12f,C("785C49",.5f));
                g.Ellipse(new Vector2(8.6f,-2.85f),.55f,.35f,C("C7A57B",.18f));
            }
            if(level==2)
            {
                for(int i=0;i<7;i++)
                {
                    Vector2 p=new Vector2(-10+i*3.3f,6.25f);g.Line(p,new Vector2(p.x,7.7f),.014f,gold);
                    g.Ellipse(p,.43f,.56f,C(i%2==0?"DFC8A5":"8F5169"));
                    g.Ellipse(p+new Vector2(-.15f,.09f),.07f,.11f,ink);g.Ellipse(p+new Vector2(.15f,.09f),.07f,.11f,ink);
                    g.Line(p+new Vector2(-.16f,-.17f),p+new Vector2(.16f,-.2f),.035f,ink);
                }
                for(int i=0;i<8;i++)g.Rect(-10+i*2.8f,-5.5f,.8f,.022f,C("D7AF68",.18f));
                g.Rect(-10.8f,-1.8f,2.1f,.16f,C("7A6775",.48f));
                g.Ellipse(new Vector2(-9.75f,-1.22f),.55f,.42f,C("E8D8C4",.16f));
                g.Rect(8.35f,2.6f,1.8f,.14f,C("776171",.46f));
                g.Line(new Vector2(8.55f,2.74f),new Vector2(9.85f,3.36f),.05f,C("D7AF68",.28f));
            }
            else if(level==3)
            {
                for(int i=0;i<5;i++)
                {
                    Vector2 p=new Vector2(-10+i*5,6.2f);g.Ring(p,.48f,.12f,gold,24);g.Ring(p,.22f,.055f,C("634256"),20);
                    for(int j=0;j<10;j++){Vector2 d=StageWorld.Rotate(Vector2.up,j*Mathf.PI/5+time*.08f);g.Line(p+d*.40f,p+d*.63f,.12f,gold);}
                }
                g.Ring(Vector2.zero,4.7f,.025f,C("8CE1DF",.12f),60);
                for(int i=0;i<12;i++){Vector2 d=StageWorld.Rotate(Vector2.up,i*Mathf.PI/6);g.Line(d*4.25f,d*4.6f,.04f,C("D7AF68",.18f));}
                g.Rect(-10.7f,3.6f,1.6f,.14f,C("74504F",.5f));
                g.Rect(9.3f,-2.9f,1.9f,.14f,C("74504F",.5f));
                g.Ellipse(new Vector2(10.1f,-2.38f),.48f,.34f,C("E6D6C2",.14f));
            }
        }
        void StageDressing(int level,float time)
        {
            // These remain on stage edges and are decorative, not invisible colliders.
            // A player can always distinguish solid props by their visible shadow barrier.
            for(int i=0;i<6;i++)
            {
                float x=-10.8f+i*4.1f;
                float swing=Mathf.Sin(time*.6f+i)*.08f;
                g.Line(new Vector2(x,7.4f),new Vector2(x+swing,5.28f),.018f,C("E7D8C6",.35f));
                if(level==1)Star(new Vector2(x+swing,5.18f),.16f,C("F5D9AA",.75f));
                else if(level==2)
                {
                    g.Ellipse(new Vector2(x+swing,5.18f),.25f,.31f,C(i%2==0?"B8A6A6":"D6C3A6"));
                    g.Disc(new Vector2(x+swing-.09f,5.22f),.04f,C("232638"));
                    g.Disc(new Vector2(x+swing+.09f,5.22f),.04f,C("232638"));
                }
                else g.Ring(new Vector2(x+swing,5.15f),.25f,.055f,C("D9B988",.72f),25);
            }
            // Stage-front side dressing: book stacks, old crates, broken folding flats.
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*11.62f;
                g.Rect(x-.51f,-4.75f,1.04f,.27f,C("6D594A",.83f));
                g.Rect(x-.45f,-4.46f,.91f,.21f,C("A48871",.7f));
                g.Rect(x-.4f,-4.21f,.82f,.18f,C("59687E",.7f));
                g.Line(new Vector2(x,-4.1f),new Vector2(x,-3.4f),.06f,C("C5AA8A",.7f));
                Star(new Vector2(x,-3.18f),.19f,C("E9D6B6",.66f));
            }
            if(level==1)
            {
                // Cardboard houses and paper windmill at the back of the rehearsal set.
                for(int i=0;i<4;i++)
                {
                    float x=-8.8f+i*5.7f;
                    g.Rect(x,4.66f,.85f,.72f,C("6C7485",.53f));
                    g.Triangle(new Vector2(x-.14f,5.34f),new Vector2(x+.4f,5.85f),new Vector2(x+1,5.34f),C("A59691",.7f));
                    g.Rect(x+.32f,4.89f,.21f,.26f,C("E5BD8E",.47f));
                }
            }
            else if(level==2)
            {
                // A row of masks waiting behind their curtain.
                for(int i=0;i<3;i++)
                {
                    Vector2 q=new Vector2(-7.8f+i*7.65f,4.43f);
                    g.Ellipse(q,.36f,.53f,C("BCAAA5",.45f));
                    g.Disc(q+new Vector2(-.13f,.1f),.06f,C("283041"));
                    g.Disc(q+new Vector2(.13f,.1f),.06f,C("283041"));
                    g.Line(q+new Vector2(-.15f,-.17f),q+new Vector2(.12f,-.19f),.04f,C("644C56"));
                }
            }
            else
            {
                // Broken clockwork implies the Puppeteer's authority is fading.
                for(int i=0;i<3;i++)
                {
                    Vector2 q=new Vector2(-8+i*7.9f,4.65f);
                    g.Ring(q,.45f,.07f,C("D1B18A",.56f),28);
                    g.Line(q,q+StageWorld.Rotate(Vector2.up,time*.25f+i)*.29f,.055f,C("D0B498",.65f));
                }
            }
        }
        void Ending(StageWorld w,float time,float elapsed)
        {
            // A three-act finale in the playable scene: the Puppeteer's hand fades,
            // the strings break one by one, and finally the shadow guides the doll to light.
            float door=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-4.8f)/3.6f));
            float farewell=Mathf.Clamp01((elapsed-2.7f)/2.0f);
            Vector2 hand=new Vector2(Mathf.Sin(time*.9f)*.28f,5.1f+farewell*4.5f);
            float handFade=1-Mathf.Clamp01((elapsed-2.8f)/2.2f);
            if(handFade>0)HandModel(hand,gold,false,handFade,false);
            for(int i=0;i<4;i++)
            {
                float snappedAt=.8f+i*.58f;
                float fall=Mathf.Clamp01((elapsed-snappedAt)/1.9f);
                Vector2 end=w.PuppetPosition+new Vector2((i-1.5f)*.28f,.42f);
                Vector2 top=hand+new Vector2((i-1.5f)*.38f,-.75f);
                if(elapsed<snappedAt)
                    g.Line(end,top,.028f,C("E4D4A9",Mathf.Min(1,handFade+.15f)));
                else if(fall<1)
                {
                    // Loose ends of each snapped wire flutter down and disappear.
                    Vector2 drift=new Vector2(Mathf.Sin(time*5+i)*.55f,-fall*2.6f);
                    g.Line(end+drift*.3f,end+drift+new Vector2(0,.6f*(1-fall)),.024f,C("E4D4A9",1-fall));
                    for(int j=0;j<3;j++)Star(end+new Vector2(Mathf.Sin(j*3+i)*.4f,(j+1)*.23f),.09f,C("FFE8BB",1-fall));
                }
            }
            float opening=Mathf.Clamp01((elapsed-3.8f)/2.1f);
            g.Rect(9.4f,-3.2f,3.1f,7.5f,C("211E2D"));
            g.Rect(9.65f+(.3f-opening*.3f),-2.9f,2.5f*opening,7,C("F9E7B2",.87f));
            g.Glow(new Vector2(10.7f,.3f),1.7f+opening*.9f,C("FFDF9A",opening));
            for(int i=0;i<12;i++)
            {
                float r=(i+1)*.24f;
                g.Line(new Vector2(9.6f,-3+r),new Vector2(11.9f,-3+r),.015f,C("FBE4A5",opening*.23f));
            }
            float movement=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-5)/3.2f));
            Vector2 offset=new Vector2(movement*8.4f,-movement*.5f);
            Puppet(w,time,offset,false);
            Shadow(w.PuppetPosition+offset+new Vector2(-1.45f,-.65f),Vector2.right,Vector2.right,time,false,false);
            for(int i=0;i<24;i++)
            {float x=Mathf.Sin(i*13.3f)*11,y=5.5f-Mathf.Repeat(time*.7f+i*.3f,10);Star(new Vector2(x,y),.055f,C("FFE8B9",.38f));}
        }
        void TutorialMarker(StageWorld w,float time)
        {
            if(w.TutorialStep==5&&w.TutorialHealthIntro>0)return;
            if(w.TutorialStep>=6)return;
            Vector2 p=w.Player+Vector2.up*1.3f;
            if(w.TutorialStep==1 && w.Enemies.Count>0)p=w.Enemies[0].Position;
            if(w.TutorialStep==2)p=w.Lamps[0].Position;
            if(w.TutorialStep==3)p=w.Props[0]+new Vector2(2,-.2f);
            if(w.TutorialStep==5 && w.Candies.Count>0)p=w.Candies[0].Position;
            if(w.TutorialStep==4)p=w.Player;
            float pulse=1+Mathf.Sin(time*5)*.13f;
            g.Ring(p,.67f*pulse,.045f,gold,35);
            g.Ring(p,1.0f*pulse,.023f,C("F6E5BF",.3f),40);
            g.Triangle(p+Vector2.up*1.35f,p+new Vector2(-.2f,1.7f),p+new Vector2(.2f,1.7f),gold);
        }
        void Light(Vector2 a,Vector2 b,Color c)
        {
            g.Triangle(a,b+Vector2.left*4,b+Vector2.right*4,c);
            g.Triangle(a,b+Vector2.left*2.6f,b+Vector2.right*2.6f,new Color(c.r,c.g,c.b,c.a*.7f));
            g.Glow(b,1.6f,c);
        }

        void Prop(Vector2 p,int index,int level)
        {
            // Four objects share one collision profile, so their shadows and cover stay fair.
            Color wood=C("927254"),dark=C("1D2633");
            g.Ellipse(p+new Vector2(.07f,-.29f),.75f,.24f,C("080D1A",.49f));
            int kind=(index+level)%4;
            if(kind==0) // a wooden chair
            {
                g.Line(p+new Vector2(-.36f,-.33f),p+new Vector2(-.36f,.95f),.12f,wood);
                g.Line(p+new Vector2(.36f,-.33f),p+new Vector2(.36f,.95f),.12f,wood);
                g.Rect(p.x-.4f,p.y+.46f,.8f,.34f,C("B89469"));
                g.Rect(p.x-.49f,p.y-.02f,.98f,.26f,C("C5A27A"));
                Star(p+Vector2.up*.64f,.14f,C("E8D4B4"));
            }
            else if(kind==1) // a vintage mirror on a wooden stand
            {
                g.Line(p+new Vector2(-.42f,-.4f),p+new Vector2(-.42f,.54f),.12f,wood);
                g.Line(p+new Vector2(.42f,-.4f),p+new Vector2(.42f,.54f),.12f,wood);
                g.Ellipse(p+Vector2.up*.37f,.47f,.62f,C("A88E78"),30);
                g.Ellipse(p+Vector2.up*.39f,.36f,.51f,C("506B79"),30);
                g.Line(p+new Vector2(-.25f,.23f),p+new Vector2(.12f,.68f),.035f,C("C4E0DB",.35f));
                Star(p+Vector2.up*1.02f,.1f,C("E2C9A0"));
            }
            else if(kind==2) // toy trunk or stage crate
            {
                g.Rect(p.x-.52f,p.y-.43f,1.04f,.99f,C("715B55"));
                g.Rect(p.x-.43f,p.y-.31f,.86f,.76f,C("AC8A6A"));
                g.Line(p+new Vector2(-.4f,-.23f),p+new Vector2(.4f,.36f),.055f,C("D6B48B",.75f));
                g.Rect(p.x-.08f,p.y+.17f,.16f,.2f,C("E6C791"));
                g.Line(p+new Vector2(-.42f,.48f),p+new Vector2(.42f,.48f),.04f,dark);
            }
            else // painted stage flat on a wooden support
            {
                g.Line(p+new Vector2(-.30f,-.44f),p+new Vector2(-.26f,.03f),.12f,wood);
                g.Line(p+new Vector2(.30f,-.44f),p+new Vector2(.26f,.03f),.12f,wood);
                g.Rect(p.x-.53f,p.y-.13f,1.06f,.94f,C("846449"));
                g.Rect(p.x-.42f,p.y-.03f,.84f,.74f,C("243449"));
                g.Line(p+new Vector2(-.50f,.78f),p+new Vector2(.50f,.78f),.07f,C("C4A079"));
                g.Line(p+new Vector2(-.50f,-.10f),p+new Vector2(.50f,-.10f),.07f,C("B88F68"));
                Star(p+Vector2.up*.34f,.25f,C("D7AF68"));
                g.Rect(p.x-.46f,p.y-.49f,.92f,.12f,C("71553E"));
            }
        }
        void Puppet(StageWorld w,float time,Vector2 offset=default(Vector2),bool showStatus=true)
        {
            Vector2 p=w.PuppetPosition+offset; float bow=w.State==PlayState.Won?Mathf.Sin(time*3)*.12f:0;
            if(w.State!=PlayState.Won)
            {
                for(int i=0;i<4;i++)
                {
                    Vector2 q=p+new Vector2((i-1.5f)*.25f,.42f);
                    g.Line(q,new Vector2((i-1.5f)*.75f,6.8f),.016f,C("DFC187",.58f));
                }
            }
            g.Ellipse(p+new Vector2(0,-.83f),.76f,.23f,C("0A1021",.48f));
            // The base remains a complete, readable little actor at every camera size.
            for(int side=-1;side<=1;side+=2)
            {
                g.Line(p+new Vector2(side*.16f,-.32f),p+new Vector2(side*.27f,-.86f),.13f,C("D4B48E"));
                g.Disc(p+new Vector2(side*.27f,-.59f),.075f,C("9D7867"));
                g.Ellipse(p+new Vector2(side*.29f,-.91f),.18f,.095f,C("692E3E"));
                g.Line(p+new Vector2(side*.37f,.25f),p+new Vector2(side*.72f,.01f+bow),.12f,C("D5B48D"));
                g.Disc(p+new Vector2(side*.72f,.01f+bow),.08f,C("E8C7A1"));
            }
            g.Triangle(p+new Vector2(-.68f,-.31f),p+new Vector2(.68f,-.31f),p+new Vector2(0,.53f),C("E1D9CB"));
            g.Triangle(p+new Vector2(-.59f,-.22f),p+new Vector2(.59f,-.22f),p+new Vector2(0,.55f),C("84A8AD"));
            g.Triangle(p+new Vector2(-.48f,-.19f),p+new Vector2(.48f,-.19f),p+new Vector2(0,.50f),C("BBD4CE"));
            for(int i=-2;i<=2;i++)g.Line(p+new Vector2(i*.21f,-.25f),p+new Vector2(i*.17f,.03f),.018f,C("FAE9D5",.7f));
            g.Ellipse(p+new Vector2(0,.35f),.27f,.27f,C("AFC8C5"));
            g.Triangle(p+new Vector2(-.21f,.35f),p+new Vector2(.21f,.35f),p+new Vector2(0,.61f),C("F8E8D2"));
            g.Disc(p+new Vector2(0,.43f),.055f,C("8D4254"));
            g.Ellipse(p+new Vector2(0,.89f),.44f,.43f,C("B88758"));
            g.Ellipse(p+new Vector2(0,.78f),.365f,.35f,C("F3D6B2"));
            for(int side=-1;side<=1;side+=2)
            {
                g.Ellipse(p+new Vector2(side*.38f,.65f),.13f,.24f,C("C8945E"));
                g.Disc(p+new Vector2(side*.43f,.5f),.12f,C("DAAB70"));
                g.Ellipse(p+new Vector2(side*.13f,.82f),.045f,.07f,C("222335"));
                g.Disc(p+new Vector2(side*.13f-.012f,.845f),.014f,C("FFF8E9"));
                g.Disc(p+new Vector2(side*.27f,.69f),.035f,C("DE9993",.65f));
            }
            g.Line(p+new Vector2(-.11f,.98f),p+new Vector2(.13f,1.11f),.12f,C("E7B877"));
            g.Line(p+new Vector2(-.07f,.64f),p+new Vector2(.07f,.64f),.025f,C("A16968"));
            g.Triangle(p+new Vector2(-.36f,1.13f),p+new Vector2(.29f,1.13f),p+new Vector2(-.03f,1.82f),C("712B3A"));
            g.Triangle(p+new Vector2(-.13f,1.18f),p+new Vector2(.16f,1.18f),p+new Vector2(-.01f,1.65f),C("A44E5A"));
            g.Ellipse(p+new Vector2(-.02f,1.14f),.41f,.09f,C("5B2736"));
            g.Disc(p+new Vector2(-.03f,1.81f),.08f,gold);
            if(showStatus)
            {
                g.Ring(p,.98f,.022f,w.PuppetInvulnerable>0?rose:C("D7AF68",.26f));
                g.Rect(p.x-.65f,p.y-1.12f,1.3f,.065f,C("392B38"));
                g.Rect(p.x-.65f,p.y-1.12f,1.3f*Mathf.Max(0,w.PuppetHealth/12),.065f,gold);
            }
        }

        void Switch(StageLamp lamp,int index,float time)
        {
            Vector2 p=lamp.Position;
            Color c=lamp.Pending>0?Color.Lerp(gold,rose,.5f+.5f*Mathf.Sin(time*24)):gold;
            g.Glow(p,.6f,c); g.Disc(p,.48f,ink);g.Ring(p,.42f,.06f,c);
            g.Disc(p,.21f,C("594437"));
            Vector2 direction=StageWorld.Rotate(Vector2.up,(lamp.Mode==0?-.8f:.8f)*(index==0?1:-1));
            g.Line(p-direction*.18f,p+direction*.29f,.1f,c);g.Disc(p+direction*.29f,.09f,c);
            g.Line(p+Vector2.up*.51f,new Vector2(index==0?-10.9f:10.9f,6.35f),.018f,C("D7AF68",.42f));
        }

        void Shadow(Vector2 p,Vector2 aim,Vector2 bodyDirection,float time,bool flash,bool armed=true)
        {
            Color edge=C("8AA6D9");
            Color body=C("0F1220");
            float bob=Mathf.Sin(time*5.4f)*.032f;
            Vector2 eyes=p+new Vector2(0,.2f+bob)+aim*.03f;
            g.Glow(p,.48f,C("8AA6D9",.45f));
            g.Ellipse(p+Vector2.down*.42f,.58f,.17f,C("090C17",.55f));
            g.Ellipse(p+new Vector2(0,.02f),.46f,.36f,flash?C("505A7A"):body,22);
            g.Ellipse(p+new Vector2(0,.34f+bob),.34f,.28f,flash?C("5B678A"):body,20);
            // A soft, shifting shadow contour rather than tentacles or a physical creature.
            for(int i=0;i<3;i++)
            {
                float sway=Mathf.Sin(time*2.1f+i*1.6f)*.05f;
                Vector2 lower=p+new Vector2((i-1)*.26f+sway,-.39f-(i%2)*.05f);
                g.Ellipse(lower,.21f,.18f,body,16);
            }
            if(armed)
            {
                g.Line(p+aim*.14f,p+aim*.62f,.14f,C("8AA6D9",.8f));
                g.Disc(p+aim*.66f,.1f,body,12);
            }
            g.Ellipse(eyes+Vector2.left*.08f,.04f,.065f,C("F6EFCE"),10);
            g.Ellipse(eyes+Vector2.right*.08f,.04f,.065f,C("F6EFCE"),10);
            g.Line(p+Vector2.down*.34f,p+Vector2.down*.34f+bodyDirection*.2f,.06f,C("7488B5",.8f));
        }

        void Rabbit(Enemy e,float time)
        {
            Vector2 p=e.Position+Vector2.up*Mathf.Sin(time*3.2f)*.028f;
            Color fur=e.Flash>0?Color.Lerp(C("191925"),Color.white,.65f):C("191925");
            Color trim=C("877B79",.78f);
            g.Ellipse(p+Vector2.down*.40f,.46f,.12f,C("090D16",.55f));
            g.Line(p+new Vector2(-.14f,.12f),p+new Vector2(-.25f,.78f),.16f,fur);
            g.Line(p+new Vector2(.14f,.12f),p+new Vector2(.25f,.78f),.16f,fur);
            g.Line(p+new Vector2(-.25f,.78f),p+new Vector2(-.29f,.85f),.075f,trim);
            g.Line(p+new Vector2(.25f,.78f),p+new Vector2(.29f,.85f),.075f,trim);
            g.Ellipse(p,.34f,.38f,trim);g.Ellipse(p,.30f,.34f,fur);
            g.Disc(p+new Vector2(-.12f,.07f),.055f,C("F05D68"));
            g.Disc(p+new Vector2(.12f,.07f),.055f,C("F05D68"));
            g.Disc(p+new Vector2(0,-.13f),.035f,C("B58278"));
            EnemyHealth(e,p+Vector2.up*1.04f,.8f);
        }
        void Mask(Enemy e,float time)
        {
            Vector2 p=e.Position;Color edge=e.Flash>0?Color.white:C("D27470");
            g.Ellipse(p+Vector2.down*.45f,.73f,.21f,C("11101C",.7f));
            g.Triangle(p+new Vector2(-.58f,-.4f),p+new Vector2(.58f,-.4f),p+new Vector2(0,.47f),C("733D4B"));
            for(int i=-1;i<=1;i+=2)
            {
                g.Line(p+new Vector2(i*.31f,.03f),p+new Vector2(i*.72f,-.16f),.15f,edge);
                g.Disc(p+new Vector2(i*.72f,-.16f),.16f,ink,16);
            }
            g.Ellipse(p+Vector2.up*.26f,.44f,.5f,edge,24);
            g.Ellipse(p+new Vector2(-.16f,.3f),.18f,.34f,C("EAD5AF"));
            g.Ellipse(p+new Vector2(.16f,.3f),.18f,.34f,C("49273F"));
            g.Line(p+new Vector2(-.27f,.35f),p+new Vector2(-.09f,.39f),.045f,ink);
            g.Line(p+new Vector2(.09f,.4f),p+new Vector2(.27f,.34f),.05f,rose);
            g.Line(p+new Vector2(-.23f,.15f),p+new Vector2(-.13f,.1f),.035f,ink);
            g.Line(p+new Vector2(.1f,.12f),p+new Vector2(.25f,.15f),.04f,rose);
            EnemyHealth(e,p+Vector2.up*.95f,1.1f);
        }
        void Guard(Enemy e,float time)
        {
            Vector2 p=e.Position;Color trim=e.Flash>0?Color.white:C("F1AD72");
            g.Glow(p,.88f,trim);
            g.Ellipse(p+Vector2.down*.45f,.82f,.25f,C("11101C",.8f));
            g.Triangle(p+new Vector2(-.76f,-.4f),p+new Vector2(.76f,-.4f),p+new Vector2(0,.6f),C("783F59"));
            g.Ellipse(p,.64f,.67f,C("5A435C"));g.Ring(p,.68f,.075f,trim,30);
            g.Ellipse(p+Vector2.up*.16f,.42f,.48f,C("E8C8AA"));
            for(int i=-1;i<=1;i+=2)
            {g.Disc(p+new Vector2(i*.21f,.21f),.085f,C("281D31"));g.Line(p+new Vector2(i*.51f,.1f),p+new Vector2(i*.75f,-.38f),.18f,trim);}
            g.Line(p+new Vector2(-.18f,-.1f),p+new Vector2(.18f,-.1f),.04f,C("33243B"));
            g.Triangle(p+new Vector2(-.5f,.5f),p+new Vector2(0,1.0f),p+new Vector2(.5f,.5f),trim);
            EnemyHealth(e,p+Vector2.up*1.18f,1.55f);
        }
        void Hand(Enemy e,float time)
        {
            bool charging=e.Cooldown<.65f;
            Color baseColor=e.Flash>0?Color.white:charging?Color.Lerp(gold,rose,.55f+.4f*Mathf.Sin(time*24)):gold;
            HandModel(e.Position,baseColor,charging,1,true);
        }
        static Color Faded(Color color,float opacity){color.a*=opacity;return color;}
        void HandModel(Vector2 p,Color baseColor,bool charging,float opacity,bool suspension)
        {
            baseColor=Faded(baseColor,opacity);
            g.Glow(p,1.3f,Faded(charging?rose:gold,opacity));
            g.Ellipse(p+new Vector2(.2f,-.65f),1.7f,.45f,C("101321",.8f*opacity));
            g.Rect(p.x-.63f,p.y+.42f,1.26f,.8f,C("46344B",opacity));
            g.Rect(p.x-.72f,p.y+.4f,1.44f,.23f,baseColor);
            g.Ellipse(p,.97f,.72f,baseColor,28);
            g.Ellipse(p+new Vector2(0,.12f),.76f,.45f,C("B68A4E",opacity),24);
            for(int i=0;i<4;i++)
            {
                float x=(i-1.5f)*.48f, length=.73f+(i==1 || i==2? .2f:0);
                g.Line(p+new Vector2(x,-.24f),p+new Vector2(x,-length),.35f,baseColor);
                g.Disc(p+new Vector2(x,-length),.18f,baseColor);
                g.Line(p+new Vector2(x-.1f,-.48f),p+new Vector2(x+.1f,-.48f),.024f,C("6B4D43",opacity));
                g.Disc(p+new Vector2(x,-length-.01f),.095f,Faded(ink,opacity),12);
                if(charging)g.Disc(p+new Vector2(x,-length),.065f,Faded(rose,opacity),12);
            }
            g.Line(p+new Vector2(-.75f,.03f),p+new Vector2(-1.3f,-.33f),.37f,baseColor);
            g.Disc(p+new Vector2(-1.3f,-.33f),.19f,baseColor);
            g.Ring(p+new Vector2(.26f,-.08f),.18f,.07f,C("FFE5A2",opacity),20);
            g.Disc(p+new Vector2(.26f,-.08f),.095f,C("8F3245",opacity),12);
            if(suspension)for(int i=0;i<2;i++)
                g.Line(p+new Vector2(i==0?-.5f:.5f,.7f),new Vector2(i==0?-2:2,7.1f),.022f,C("D7AF68",.45f*opacity));
        }
        void ButtonMine(Mine m,float time)
        {
            Color c=m.Age<1.2f?gold:Color.Lerp(gold,rose,.5f+.5f*Mathf.Sin(time*12));
            g.Ring(m.Position,1.15f,.022f,new Color(c.r,c.g,c.b,.27f));
            g.Glow(m.Position,.45f,c);g.Disc(m.Position,.31f,C("392D3A"));g.Ring(m.Position,.26f,.06f,c);
            for(int i=0;i<4;i++)g.Disc(m.Position+new Vector2(i%2==0?-.08f:.08f,i<2?-.08f:.08f),.04f,c,8);
        }
        void EnemyHealth(Enemy e,Vector2 p,float width)
        {
            if(e.Health>=e.MaxHealth) return;
            g.Rect(p.x-width*.5f,p.y,width,.045f,C("131B2B"));
            g.Rect(p.x-width*.5f,p.y,width*Mathf.Max(0,e.Health/e.MaxHealth),.045f,rose);
        }
        void Star(Vector2 p,float r,Color c)
        {
            for(int i=0;i<5;i++)
            {
                float a=Mathf.PI*.5f+i*Mathf.PI*.4f;
                g.Triangle(p,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,p+new Vector2(Mathf.Cos(a+.63f),Mathf.Sin(a+.63f))*r*.42f,c);
                g.Triangle(p,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,p+new Vector2(Mathf.Cos(a-.63f),Mathf.Sin(a-.63f))*r*.42f,c);
            }
        }
        void Foreground(float time)
        {
            // Scalloped velvet wings frame the playable area.
            for(int side=-1;side<=1;side+=2)
            {
                g.Rect(side<0?-16:13.1f,-7.2f,2.9f,15.2f,C("351E31"));
                for(int i=0;i<7;i++)
                {
                    float x=side*(13.35f+i*.37f);
                    g.Rect(x-.18f,-6.6f,.32f,13.8f,Color.Lerp(C("4F293D"),C("7B3A4E"),(i%3)*.32f));
                    g.Ellipse(new Vector2(x,-6.6f),.18f,.48f,C("4F293D"));
                }
                g.Line(new Vector2(side*13.2f,-5.9f),new Vector2(side*13.2f,7.1f),.06f,gold);
            }
            g.Rect(-16,6.9f,32,2.4f,C("391F31"));
            for(int i=0;i<8;i++)
            {
                g.Ellipse(new Vector2(-14+i*4,7.15f),2.15f,.71f,C("71374A"),28);
                g.Ellipse(new Vector2(-14+i*4,7.28f),2.04f,.56f,C("55283E"),28);
            }
            g.Rect(-16,7.75f,32,.055f,gold);
            g.Rect(-14,-7.5f,28,1.1f,C("161827"));g.Rect(-14,-6.47f,28,.09f,gold);
            for(int i=0;i<17;i++)
            {
                Vector2 p=new Vector2(-12+i*1.5f,-6.8f);g.Glow(p,.27f,gold);g.Ellipse(p,.1f,.05f,C("FFE8B9"),12);
            }
            for(int side=-1;side<=1;side+=2)
            {
                Vector2 p=new Vector2(side*10.9f,6.4f);
                g.Line(p+Vector2.up*.5f,p,.13f,C("A88763"));g.Disc(p,.32f,ink);g.Ring(p,.26f,.05f,gold);
                g.Disc(p,.17f,C("F0D7A6"));
            }
        }
    }
}

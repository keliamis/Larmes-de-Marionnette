using System;
using System.Collections.Generic;
using UnityEngine;

namespace Backstage
{
    public enum PlayState { Title, Playing, Lost, Won, LevelClear }
    public enum EnemyKind { Rabbit, Mask, Guard, Puppeteer }
    public struct ShotInput { public Vector2 Move, Aim; public bool Fire, Dash; }
    public sealed class Enemy
    {
        public EnemyKind Kind;
        public Vector2 Position;
        public float Health, MaxHealth, Cooldown, Age, MineTimer, Flash, LightTimer=8;
        public float Radius { get { return Kind==EnemyKind.Puppeteer?1.12f:Kind==EnemyKind.Guard?.79f:Kind==EnemyKind.Mask?.62f:.43f; } }
    }
    public sealed class Bullet
    {
        public Vector2 Position, Velocity;
        public bool Friendly;
        public float Life=4, Radius=.11f, BounceGrace;
        public int Bounces, LastShadow=-1;
    }
    public sealed class Candy { public Vector2 Position; public float Life=20; }
    public sealed class Mine { public Vector2 Position; public float Age; }
    public sealed class Spark { public Vector2 Position, Velocity; public float Life, MaxLife; public Color Color; }
    public struct Barrier { public Vector2 A, B; public float Width; }
    public sealed class StageLamp
    {
        public Vector2 Position;
        public int Mode;
        public float Pending, Cooldown;
    }

    public sealed class StageWorld
    {
        public PlayState State=PlayState.Title;
        public readonly List<Enemy> Enemies=new List<Enemy>();
        public readonly List<Bullet> Bullets=new List<Bullet>();
        public readonly List<Candy> Candies=new List<Candy>();
        public readonly List<Mine> Mines=new List<Mine>();
        public readonly List<Spark> Sparks=new List<Spark>();
        public readonly List<Barrier> Barriers=new List<Barrier>();
        readonly System.Random random=new System.Random(715);
        public readonly Vector2[] Props={new Vector2(-7,1),new Vector2(6,-1),new Vector2(-1,-3.2f),new Vector2(4.5f,3)};
        public readonly StageLamp[] Lamps={new StageLamp{Position=new Vector2(-10.8f,3.9f)},new StageLamp{Position=new Vector2(10.8f,3.9f)}};
        public readonly Vector2 PuppetPosition=new Vector2(0,.8f);
        public Vector2 Player=new Vector2(-4,-3.5f), Aim=Vector2.up, BodyDirection=Vector2.up;
        public float Time, Health=6, PuppetHealth=12, PuppetInvulnerable, Invulnerable, FireCooldown, DashCooldown, DashTime;
        public float WaveWait=1.5f, Shake;
        public int Wave, Score, RicochetHits, LightSwitches, Ricochets, Level=1, Kills, CandyTaken, CandySpawned, Dashes, TutorialStep;
        public int WaveCount {get { return 3; }}
        public bool Tutorial;
        public float Stamina=100, StaminaDelay, TutorialMoved, TutorialFinishDelay;
        Vector2 dashDirection;
        public const float DashCost=34, StaminaRegen=25;
        public static readonly string[] LevelNames={"Репетиция","Сломанная сцена","Зал масок","Последняя нить"};
        public bool InCover, BossDefeated;
        public string Banner="Первый выход";
        public float BannerTime=3;
        public Action<float,float,float> Sound;

        public void Start(int level=1,bool tutorial=false)
        {
            Enemies.Clear(); Bullets.Clear(); Mines.Clear(); Sparks.Clear(); Candies.Clear();
            Level=Mathf.Clamp(level,1,3);Tutorial=tutorial;TutorialStep=0;TutorialMoved=0;TutorialFinishDelay=0;
            Stamina=100;StaminaDelay=0;Dashes=0;CandyTaken=0;CandySpawned=0;Kills=0;Ricochets=0;
            if(Level==1){Props[0]=new Vector2(-7,1);Props[1]=new Vector2(6,-1);Props[2]=new Vector2(-1,-3.2f);Props[3]=new Vector2(4.5f,3);}
            else if(Level==2){Props[0]=new Vector2(-6,2);Props[1]=new Vector2(7,0);Props[2]=new Vector2(-3,-3.8f);Props[3]=new Vector2(2.7f,3);}
            else{Props[0]=new Vector2(-8,1.5f);Props[1]=new Vector2(8,-1);Props[2]=new Vector2(1,-3.5f);Props[3]=new Vector2(-3.5f,3.2f);}
            Player=new Vector2(-4,-3.5f); Aim=BodyDirection=Vector2.up;
            Time=0; Health=6; PuppetHealth=12; PuppetInvulnerable=0;
            Invulnerable=1.5f; FireCooldown=0; DashCooldown=0; DashTime=0;
            WaveWait=1.5f; Wave=0; Score=0; RicochetHits=0; LightSwitches=0;
            BossDefeated=false; Shake=0; State=PlayState.Playing;
            foreach(StageLamp lamp in Lamps) { lamp.Mode=0; lamp.Pending=0; lamp.Cooldown=0; }
            Banner=tutorial?"БЕЗОПАСНАЯ РЕПЕТИЦИЯ":LevelNames[Level].ToUpper(); BannerTime=3;
            if(!tutorial){Candies.Add(new Candy{Position=new Vector2(0,-4.8f),Life=26});CandySpawned=1;}
            UpdateBarriers();
        }
        public Enemy Spawn(EnemyKind kind,Vector2 p)
        {
            float hp=kind==EnemyKind.Rabbit?3:kind==EnemyKind.Mask?7:kind==EnemyKind.Guard?12:38;
            var e=new Enemy {Kind=kind,Position=p,Health=hp,MaxHealth=hp,Cooldown=1.7f+(float)random.NextDouble(),MineTimer=3};
            Enemies.Add(e); return e;
        }
        public void NextWave()
        {
            Wave++;Banner="АКТ "+Level+"  •  ВОЛНА "+Wave+" / "+WaveCount;BannerTime=2;
            // Each act teaches a different tactical problem, and each finale combines them.
            if(Level==1)
            {
                Spawn(EnemyKind.Rabbit,new Vector2(-10,3));Spawn(EnemyKind.Rabbit,new Vector2(10,2));
                if(Wave>=2)Spawn(EnemyKind.Mask,new Vector2(9,-4.2f));
                if(Wave==3){Spawn(EnemyKind.Rabbit,new Vector2(-10,-4));Spawn(EnemyKind.Guard,new Vector2(7.7f,4));}
            }
            else if(Level==2)
            {
                Spawn(EnemyKind.Mask,new Vector2(-10,3.8f));Spawn(EnemyKind.Rabbit,new Vector2(10,-4.1f));
                if(Wave>=2){Spawn(EnemyKind.Mask,new Vector2(9.4f,4));Spawn(EnemyKind.Rabbit,new Vector2(-9,-3.9f));}
                if(Wave==3){Spawn(EnemyKind.Guard,new Vector2(3,4.9f));Spawn(EnemyKind.Mask,new Vector2(-8.2f,-4.7f));}
            }
            else
            {
                Spawn(EnemyKind.Rabbit,new Vector2(-10,1));Spawn(EnemyKind.Mask,new Vector2(10,2));
                if(Wave==2){Spawn(EnemyKind.Guard,new Vector2(-8,4.7f));Spawn(EnemyKind.Mask,new Vector2(8,-4.5f));}
                if(Wave==3){Spawn(EnemyKind.Puppeteer,new Vector2(0,4.8f));Banner="КУКЛОВОД НА СЦЕНЕ";BannerTime=4;}
            }
            // Enemies change the light too: shadows are now part of the fight, not a cosmetic effect.
            if(Wave>1)RequestLamp((Level+Wave)%2,true);
        }
        public void Step(float dt,ShotInput input)
        {
            if(State!=PlayState.Playing) return;
            dt=Mathf.Clamp(dt,0,.04f); Time+=dt;
            Invulnerable=Mathf.Max(0,Invulnerable-dt); PuppetInvulnerable=Mathf.Max(0,PuppetInvulnerable-dt);
            FireCooldown-=dt; DashCooldown-=dt; DashTime-=dt; BannerTime-=dt; Shake=Mathf.Max(0,Shake-dt*1.5f);
            foreach(StageLamp lamp in Lamps)
            {
                lamp.Cooldown=Mathf.Max(0,lamp.Cooldown-dt);
                if(lamp.Pending>0)
                {
                    lamp.Pending=Mathf.Max(0,lamp.Pending-dt);
                    if(lamp.Pending==0) {lamp.Mode=1-lamp.Mode; LightSwitches++; Beep(390,.12f,.08f);}
                }
            }
            UpdateBarriers();
            if(input.Aim.sqrMagnitude>.001f) Aim=input.Aim.normalized;
            if(input.Move.sqrMagnitude>.01f) BodyDirection=input.Move.normalized;
            StaminaDelay=Mathf.Max(0,StaminaDelay-dt);
            if(StaminaDelay<=0)Stamina=Mathf.Min(100,Stamina+StaminaRegen*dt);
            if(input.Dash && DashCooldown<=0 && Stamina>=DashCost && input.Move.sqrMagnitude>.01f)
            { dashDirection=input.Move.normalized;DashTime=.17f;DashCooldown=.25f;Stamina-=DashCost;StaminaDelay=.85f;Dashes++;Invulnerable=Mathf.Max(Invulnerable,.28f);Beep(260,.13f,.1f); }

            float speed=DashTime>0?17:5.2f;
            Player=MoveActor(Player,(DashTime>0?dashDirection:Vector2.ClampMagnitude(input.Move,1))*speed*dt,.48f);
            InCover=IsCovered(Player);
            if(input.Fire && FireCooldown<=0)
            { Shoot(Player,Aim,true); FireCooldown=.22f; Beep(155,.055f,.045f); }
            if(!Tutorial)for(int i=Enemies.Count-1;i>=0;i--) UpdateEnemy(Enemies[i],dt);
            UpdateBullets(dt);
            if(!BossDefeated)UpdateMines(dt);
            UpdateCandies(dt);UpdateSparks(dt);
            if(Tutorial){UpdateTutorial(dt,input);return;}
            if(Health<=0 || PuppetHealth<=0) { State=PlayState.Lost; Beep(65,.5f,.18f); }
            else if(BossDefeated)
            {
                State=PlayState.Won; Banner="КУКЛА СВОБОДНА"; Bullets.Clear(); Mines.Clear();
                Burst(PuppetPosition,new Color(.95f,.75f,.4f),45); Beep(660,.5f,.15f);
            }
            else if(Enemies.Count==0)
            {
                if(Wave>=WaveCount){State=PlayState.LevelClear;Bullets.Clear();Mines.Clear();Beep(540,.3f,.12f);}
                else{WaveWait-=dt;if(WaveWait<=0){NextWave();WaveWait=2.3f;}}
            }
        }
        public bool RequestLamp(int index,bool byBoss=false)
        {
            StageLamp lamp=Lamps[index];
            if(lamp.Pending>0 || lamp.Cooldown>0) return false;
            lamp.Pending=.75f; lamp.Cooldown=1.4f;
            Beep(620,.09f,.08f); Burst(lamp.Position,new Color(.95f,.75f,.4f),9);
            Banner=byBoss?"КУКЛОВОД ПОВОРАЧИВАЕТ ПРОЖЕКТОР":"СВЕТ МЕНЯЕТ ПОЛЕ БОЯ"; BannerTime=1.7f;
            return true;
        }
        void UpdateEnemy(Enemy e,float dt)
        {
            e.Age+=dt; e.Flash=Mathf.Max(0,e.Flash-dt); e.Cooldown-=dt;
            bool attacksBase=e.Kind==EnemyKind.Mask || ((int)(e.Age/3.5f))%3==2;
            Vector2 target=attacksBase?PuppetPosition:Player;
            Vector2 to=target-e.Position; float distance=to.magnitude; Vector2 dir=to.normalized;
            if(e.Kind==EnemyKind.Puppeteer)
            {
                e.Position=new Vector2(Mathf.Sin(Time*.45f)*5,4.5f+Mathf.Sin(Time*.7f)*.45f);
                if(e.Cooldown<=0)
                {
                    dir=(target-e.Position).normalized;
                    for(int k=0;k<3;k++)
                        Shoot(e.Position+new Vector2((k-1f)*.52f,-.55f),Rotate(dir,(k-1f)*.22f),false,4.4f);
                    e.Cooldown=3.0f; Shake=.12f; Beep(95,.15f,.12f);
                }
                e.LightTimer-=dt;
                if(e.LightTimer<=0) {RequestLamp((int)(e.Age/11)%2,true); e.LightTimer=11;}
                e.MineTimer-=dt;
                if(e.MineTimer<=0)
                { Mines.Add(new Mine{Position=Clamp(Player+new Vector2(Rand(-3,3),Rand(-2,2)),.6f)}); e.MineTimer=5.5f; }
            }
            else
            {
                float desired=e.Kind==EnemyKind.Rabbit?4.2f:e.Kind==EnemyKind.Guard?5.3f:6;
                Vector2 side=new Vector2(-dir.y,dir.x)*Mathf.Sin(e.Age*1.7f)*.7f;
                Vector2 move=dir*(distance>desired?1:distance<desired-1?-1:0)+side;
                e.Position=MoveActor(e.Position,move*(e.Kind==EnemyKind.Rabbit?2.8f:e.Kind==EnemyKind.Guard?1.05f:1.65f)*dt,e.Radius);
                if(e.Cooldown<=0)
                {
                    dir=(target-e.Position).normalized;
                    if(e.Kind==EnemyKind.Rabbit) Shoot(e.Position,dir,false,6.5f);
                    else if(e.Kind==EnemyKind.Guard)
                    {
                        Shoot(e.Position,Rotate(dir,-.17f),false,5.4f);
                        Shoot(e.Position,dir,false,5.4f);
                        Shoot(e.Position,Rotate(dir,.17f),false,5.4f);
                        if(e.Age>8 && (int)e.Age%5==0) RequestLamp((int)e.Age%2,true);
                    }
                    else
                    {
                        Vector2 n=new Vector2(-dir.y,dir.x)*.34f;
                        Shoot(e.Position+n,dir,false,5.6f);
                        Shoot(e.Position-n,Rotate(dir,.08f*Mathf.Sin(e.Age)),false,5.6f);
                    }
                    e.Cooldown=e.Kind==EnemyKind.Rabbit?2.05f:e.Kind==EnemyKind.Guard?3.35f:2.5f;
                }
            }
            if(Vector2.Distance(Player,e.Position)<e.Radius+.42f) HurtPlayer();
        }
        void Shoot(Vector2 p,Vector2 d,bool friendly,float speed=14)
        {
            if(Bullets.Count>=180)return;
            if(d.sqrMagnitude<.01f) d=Vector2.up;
            // Start just outside the shooter's centre; swept collision prevents firing through a nearby wall.
            Bullets.Add(new Bullet{Position=p+d.normalized*.12f,Velocity=d.normalized*speed,Friendly=friendly});
        }
        public bool TraceWall(Vector2 p,Vector2 delta,float radius,out float fraction,out Vector2 normal)
        {
            StageCollision.Border(p,delta,radius,out fraction,out normal);
            foreach(Barrier wall in Barriers)
            {
                float f;Vector2 n;if(StageCollision.Capsule(p,delta,wall.A,wall.B,wall.Width*.5f+radius,out f,out n)&&f<fraction){fraction=f;normal=n;}
            }
            return fraction<=1;
        }
        static bool TargetCircle(Vector2 p,Vector2 delta,Vector2 target,float radius,out float t,out Vector2 normal)
        {
            if((p-target).sqrMagnitude<=radius*radius){t=0;normal=Vector2.zero;return true;}
            return StageCollision.Circle(p,delta,target,radius,out t,out normal);
        }
        void UpdateBullets(float dt)
        {
            for(int i=Bullets.Count-1;i>=0;i--)
            {
                Bullet b=Bullets[i];b.Life-=dt;bool remove=b.Life<=0;float left=dt;
                for(int contact=0;contact<6&&!remove&&left>.000001f;contact++)
                {
                    Vector2 delta=b.Velocity*left,normal;float hit;bool wall=TraceWall(b.Position,delta,b.Radius,out hit,out normal);
                    if(!wall)hit=1.001f;int kind=wall?1:0,index=-1;float f;Vector2 n;
                    for(int j=0;j<Props.Length;j++)if(TargetCircle(b.Position,delta,Props[j],.58f+b.Radius,out f,out n)&&f<hit){hit=f;kind=2;}
                    if(b.Friendly)
                    {
                        for(int j=0;j<Lamps.Length;j++)if(TargetCircle(b.Position,delta,Lamps[j].Position,.48f+b.Radius,out f,out n)&&f<hit){hit=f;kind=3;index=j;}
                        for(int j=0;j<Enemies.Count;j++)if(TargetCircle(b.Position,delta,Enemies[j].Position,Enemies[j].Radius+b.Radius,out f,out n)&&f<hit){hit=f;kind=4;index=j;}
                    }
                    else
                    {
                        if(TargetCircle(b.Position,delta,Player,.48f+b.Radius,out f,out n)&&f<hit){hit=f;kind=5;}
                        if(TargetCircle(b.Position,delta,PuppetPosition,.65f+b.Radius,out f,out n)&&f<hit){hit=f;kind=6;}
                    }
                    if(kind==0){b.Position+=delta;break;}
                    b.Position+=delta*Mathf.Clamp01(hit);
                    if(kind==1)
                    {
                        if(b.Bounces>=2){remove=true;break;}
                        b.Velocity-=2*Vector2.Dot(b.Velocity,normal)*normal;b.Position+=normal*.003f;b.Bounces++;
                        if(b.Friendly)Ricochets++;
                        Burst(b.Position,new Color(.55f,.87f,.84f),5);Beep(780,.055f,.055f);
                        left*=1-Mathf.Clamp01(hit);continue;
                    }
                    remove=true;
                    if(kind==3)RequestLamp(index);
                    if(kind==5)HurtPlayer();if(kind==6)HurtPuppet();
                    if(kind==4)
                    {
                        Enemy e=Enemies[index];e.Health-=b.Bounces>0?2:1;e.Flash=.08f;
                        if(b.Bounces>0){RicochetHits++;Score+=25;}
                        Burst(b.Position,new Color(.6f,.85f,.85f),5);Beep(220,.05f,.035f);
                        if(e.Health<=0)
                        {
                            Enemies.RemoveAt(index);Kills++;Score+=e.Kind==EnemyKind.Puppeteer?1000:e.Kind==EnemyKind.Guard?350:e.Kind==EnemyKind.Mask?200:100;
                            Burst(e.Position,new Color(.94f,.6f,.3f),20);Beep(90,.13f,.12f);
                            // Only two sweets per act: one at entry and one earned after the third defeat.
                            if(!Tutorial && Kills==3 && CandySpawned<2)
                            {Candies.Add(new Candy{Position=MoveActor(e.Position,Vector2.zero,.25f),Life=22});CandySpawned++;}
                            if(e.Kind==EnemyKind.Puppeteer){BossDefeated=true;Enemies.Clear();return;}
                        }
                    }
                }
                if(remove)Bullets.RemoveAt(i);
            }
        }
        void UpdateCandies(float dt)
        {
            for(int i=Candies.Count-1;i>=0;i--)
            {
                Candy c=Candies[i];c.Life-=dt;
                if((Player-c.Position).sqrMagnitude<.85f*.85f&&Health<6)
                {Health=Mathf.Min(6,Health+2);Stamina=Mathf.Min(100,Stamina+20);CandyTaken++;Candies.RemoveAt(i);Burst(Player,new Color(.7f,1,.7f),18);Banner="КОНФЕТА: +2 ЗДОРОВЬЯ";BannerTime=1.5f;Beep(880,.16f,.1f);}
                else if(c.Life<=0)Candies.RemoveAt(i);
            }
        }
        void TutorialAdvance(int next,string line)
        {
            TutorialStep=next;
            Banner=line;
            BannerTime=1.45f;
        }
        void UpdateTutorial(float dt,ShotInput input)
        {
            Health=Mathf.Max(1,Health);PuppetHealth=12;
            if(TutorialStep==0)
            {
                TutorialMoved+=input.Move.magnitude*dt;
                if(TutorialMoved>.75f)
                {
                    TutorialAdvance(1,"Отлично! Теперь попробуй выстрел.");
                    var e=Spawn(EnemyKind.Rabbit,new Vector2(-1.5f,-5));e.Health=1;
                }
            }
            else if(TutorialStep==1&&Kills>0)TutorialAdvance(2,"Есть попадание! Поверни свет.");
            else if(TutorialStep==2&&LightSwitches>0){Ricochets=0;TutorialAdvance(3,"Супер. Теперь — рикошет от тени.");}
            else if(TutorialStep==3&&Ricochets>0){Dashes=0;TutorialAdvance(4,"Красиво! Остался рывок.");}
            else if(TutorialStep==4&&Dashes>0)
            {
                TutorialAdvance(5,"Хорошо! Осмотрись, затем дойди до конфеты.");
                Health=4;
                Candies.Add(new Candy{Position=MoveActor(Player+Vector2.right*2.8f,Vector2.zero,.25f),Life=999});
            }
            else if(TutorialStep==5&&CandyTaken>0)
            {
                TutorialAdvance(6,"Репетиция завершена. Ты готова к первому акту.");
                TutorialFinishDelay=2.5f;
            }
            else if(TutorialStep==6)
            {
                TutorialFinishDelay-=dt;
                if(TutorialFinishDelay<=0){State=PlayState.LevelClear;Bullets.Clear();}
            }
        }
        void UpdateMines(float dt)
        {
            for(int i=Mines.Count-1;i>=0;i--)
            {
                Mine m=Mines[i];m.Age+=dt;
                if(m.Age>1.2f && ((Player-m.Position).sqrMagnitude<1 || m.Age>6))
                {
                    if((Player-m.Position).sqrMagnitude<2.25f) HurtPlayer();
                    if((PuppetPosition-m.Position).sqrMagnitude<2.25f) HurtPuppet();
                    for(int j=0;j<8;j++) Shoot(m.Position,Rotate(Vector2.up,j*Mathf.PI/4),false,4.2f);
                    Burst(m.Position,new Color(1,.3f,.25f),20);Mines.RemoveAt(i);Shake=.2f;Beep(60,.18f,.15f);
                }
            }
        }
        void HurtPlayer()
        {
            if(Tutorial||Invulnerable>0) return;
            Health--;Invulnerable=1;Shake=.3f;Burst(Player,new Color(.2f,.9f,.9f),15);Beep(75,.2f,.13f);
        }
        void HurtPuppet()
        {
            if(Tutorial||PuppetInvulnerable>0) return;
            PuppetHealth--;PuppetInvulnerable=.6f;Shake=.22f;
            Burst(PuppetPosition,new Color(1,.4f,.35f),14);Beep(110,.18f,.12f);
            Banner="ЗАЩИТИ КУКЛУ!";BannerTime=1.5f;
        }
        public Barrier ShadowFor(int prop,bool future=false)
        {
            int lamp=(prop==1||prop==3)?1:0,mode=Lamps[lamp].Mode;
            if(future && Lamps[lamp].Pending>0)mode=1-mode;
            Vector2 direction;
            if(prop==0)direction=mode==0?new Vector2(1,-1):new Vector2(1,.35f);
            else if(prop==1)direction=mode==0?new Vector2(-1,1):new Vector2(-.45f,-1);
            else if(prop==2)direction=mode==0?new Vector2(1,-.25f):new Vector2(.15f,1);
            else direction=mode==0?new Vector2(-.7f,-1):new Vector2(.55f,-1);
            // Very slow swaying shadows make aiming and use of cover spatial decisions.
            if(!Tutorial)direction=Rotate(direction.normalized,Mathf.Sin(Time*.52f+prop*1.8f)*.15f);
            return new Barrier{A=Props[prop],B=Props[prop]+direction.normalized*(prop==3?3.7f:4.7f),Width=.7f};
        }
        public void UpdateBarriers()
        { Barriers.Clear();for(int i=0;i<Props.Length;i++)Barriers.Add(ShadowFor(i)); }
        public bool IsCovered(Vector2 p)
        {
            foreach(Barrier b in Barriers)if(SegmentDistance(p,b.A,b.B)<b.Width*.5f+1.1f)return true;
            return false;
        }
        Vector2 MoveActor(Vector2 position,Vector2 movement,float radius)
        {
            Vector2 p=position;int steps=Mathf.Max(1,Mathf.CeilToInt(movement.magnitude/.15f));
            for(int step=0;step<steps;step++)
            {p+=movement/steps;
            for(int pass=0;pass<4;pass++)
            {
                for(int i=0;i<Props.Length;i++)p=PushOut(p,Props[i],radius+.55f);
                foreach(Barrier b in Barriers)
                {
                    Vector2 nearest=ClosestPoint(p,b.A,b.B),delta=p-nearest;
                    float clearance=b.Width*.5f+radius;
                    if(delta.sqrMagnitude<clearance*clearance)
                    {
                        if(delta.sqrMagnitude<.0001f)delta=new Vector2(-(b.B-b.A).y,(b.B-b.A).x);
                        p=nearest+delta.normalized*clearance;
                    }
                }
                p=Clamp(p,radius);
            }
            }
            return p;
        }
        public void Burst(Vector2 p,Color c,int count)
        {
            if(Sparks.Count>600)Sparks.RemoveRange(0,Sparks.Count-600);
            for(int i=0;i<count;i++)
            {
                float life=Rand(.2f,.6f);
                Sparks.Add(new Spark{Position=p,Velocity=Rotate(Vector2.up,Rand(0,Mathf.PI*2))*Rand(.5f,3),Life=life,MaxLife=life,Color=c});
            }
        }
        void UpdateSparks(float dt)
        {
            for(int i=Sparks.Count-1;i>=0;i--)
            {Spark s=Sparks[i];s.Life-=dt;s.Position+=s.Velocity*dt;if(s.Life<=0)Sparks.RemoveAt(i);}
        }
        void Beep(float f,float t,float v) {if(Sound!=null)Sound(f,t,v);}
        float Rand(float a,float b) {return a+(float)random.NextDouble()*(b-a);}
        static Vector2 PushOut(Vector2 p,Vector2 center,float r)
        {Vector2 d=p-center;return d.sqrMagnitude<r*r?center+(d.sqrMagnitude>.001f?d.normalized:Vector2.right)*r:p;}
        public static Vector2 Clamp(Vector2 p,float r)
        {return new Vector2(Mathf.Clamp(p.x,-12.6f+r,12.6f-r),Mathf.Clamp(p.y,-5.85f+r,5.75f-r));}
        public static Vector2 Rotate(Vector2 v,float a)
        {return new Vector2(v.x*Mathf.Cos(a)-v.y*Mathf.Sin(a),v.x*Mathf.Sin(a)+v.y*Mathf.Cos(a));}
        static Vector2 ClosestPoint(Vector2 p,Vector2 a,Vector2 b)
        {Vector2 ab=b-a;return a+ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(.0001f,ab.sqrMagnitude));}
        public static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b) {return Vector2.Distance(p,ClosestPoint(p,a,b));}
        static float Cross(Vector2 a,Vector2 b) {return a.x*b.y-a.y*b.x;}
        public static float SegmentsDistance(Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {
            Vector2 r=b-a,s=d-c;float cross=Cross(r,s);
            if(Mathf.Abs(cross)>.00001f)
            {float t=Cross(c-a,s)/cross,u=Cross(c-a,r)/cross;if(t>=0 && t<=1 && u>=0 && u<=1)return 0;}
            return Mathf.Min(Mathf.Min(SegmentDistance(a,c,d),SegmentDistance(b,c,d)),Mathf.Min(SegmentDistance(c,a,b),SegmentDistance(d,a,b)));
        }
    }
}

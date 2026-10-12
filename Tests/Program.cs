using System;using Backstage;using UnityEngine;
class Checks
{
 static int count,failed;const float dt=1f/60;
 static void Check(string name,Action f){try{f();count++;Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e.Message);}}
 static void Assert(bool v,string reason){if(!v)throw new Exception(reason);}
 static void Near(float a,float b,float e=.002f){Assert(Math.Abs(a-b)<e,a+" != "+b);}
 static ShotInput Idle()=>new ShotInput{Aim=Vector2.up};
 static StageWorld World(int level=1){var w=new StageWorld();w.Start(level);return w;}
 static void Tick(StageWorld w,int n){for(int i=0;i<n;i++)w.Step(dt,Idle());}
 static Bullet B(Vector2 p,Vector2 velocity,bool friendly=true,int bounces=0)=>new Bullet{Position=p,Velocity=velocity,Friendly=friendly,Bounces=bounces};
 static void Main()
 {
  Check("Border reflection on four sides",()=>{
   foreach(var d in new[]{Vector2.left,Vector2.right,Vector2.up,Vector2.down}){var w=World();w.Player=new Vector2(-10,-4);w.Bullets.Add(B(new Vector2(d.x*12.3f,d.y*(d.y>0?5.4f:5.5f)),d*30));Tick(w,1);Assert(w.Bullets.Count==1&&w.Bullets[0].Bounces==1,"No reflection");Assert(Vector2.Dot(w.Bullets[0].Velocity,d)<0,"Wrong normal");}
  });
  Check("Exact corner reflection stays in bounds",()=>{float t;Vector2 n;Assert(StageCollision.Border(new Vector2(12,5.15f),new Vector2(1,1),.1f,out t,out n),"Miss");Assert(n.x<0&&n.y<0,"Corner normal");});
  Check("Fast swept capsule side cannot tunnel",()=>{float t;Vector2 n;Assert(StageCollision.Capsule(new Vector2(-5,0),new Vector2(20,0),new Vector2(0,-2),new Vector2(0,2),.4f,out t,out n),"Miss");Near(t,.23f);Near(n.x,-1);});
  Check("Capsule endpoint reflection",()=>{float t;Vector2 n;Assert(StageCollision.Capsule(new Vector2(0,4),new Vector2(0,-4),new Vector2(0,-2),new Vector2(0,2),.4f,out t,out n),"Miss endpoint");Near(n.y,1);});
  Check("Moving away from wall does not bounce twice",()=>{float t;Vector2 n;Assert(!StageCollision.Capsule(new Vector2(.401f,0),Vector2.right,new Vector2(0,-2),new Vector2(0,2),.4f,out t,out n),"Stuck bounce");});
  Check("Shadow reflects both friendly and hostile shots",()=>{foreach(bool friendly in new[]{true,false}){var w=World();Barrier wall=w.Barriers[0];Vector2 middle=(wall.A+wall.B)*.5f,n=new Vector2(-(wall.B-wall.A).y,(wall.B-wall.A).x).normalized;w.Bullets.Add(B(middle+n*.65f,-n*20,friendly));Tick(w,1);Assert(w.Bullets.Count==1&&w.Bullets[0].Bounces==1,"Shadow miss");}});
  Check("Third wall contact removes bullet",()=>{var w=World();w.Bullets.Add(B(new Vector2(12.3f,-4),Vector2.right*30,true,2));Tick(w,1);Assert(w.Bullets.Count==0,"Unlimited bounce");});
  Check("Enemy before wall receives hit before reflection",()=>{var w=World();var e=w.Spawn(EnemyKind.Mask,new Vector2(11.8f,-4));w.Bullets.Add(B(new Vector2(10.7f,-4),Vector2.right*80));Tick(w,1);Assert(e.Health==6,"Ordered collision wrong");});
  Check("Ricochet deals double damage and awards points",()=>{var w=World();var e=w.Spawn(EnemyKind.Mask,new Vector2(10,-4));w.Bullets.Add(B(e.Position,Vector2.right,true,1));Tick(w,1);Near(e.Health,5);Assert(w.RicochetHits==1&&w.Score==25,"Bonus absent");});
  Check("Wooden chair absorbs shot",()=>{var w=World();w.Bullets.Add(B(w.Props[0]+Vector2.left*.8f,Vector2.right*30));Tick(w,1);Assert(w.Bullets.Count==0,"Chair transparent");});
  Check("Friendly fire cannot hurt puppet",()=>{var w=World();w.Bullets.Add(B(w.PuppetPosition,Vector2.right*20));Tick(w,1);Near(w.PuppetHealth,12);});
  Check("Hostile shot damages puppet once",()=>{var w=World();w.Bullets.Add(B(w.PuppetPosition,Vector2.right*20,false));Tick(w,1);Near(w.PuppetHealth,11);});
  Check("Lamp warns before switching and rejects spam",()=>{var w=World();Assert(w.RequestLamp(0),"No request");Assert(!w.RequestLamp(0),"Spam");Tick(w,30);Assert(w.Lamps[0].Mode==0,"Early switch");Tick(w,20);Assert(w.Lamps[0].Mode==1&&w.LightSwitches==1,"No switch");});
  Check("Candy heals two and respects health cap",()=>{var w=World();w.Candies.Clear();w.Health=5;w.Candies.Add(new Candy{Position=w.Player});Tick(w,1);Near(w.Health,6);Assert(w.CandyTaken==1&&w.Candies.Count==0,"No pickup");});
  Check("Full health leaves candy available",()=>{var w=World();w.Candies.Clear();w.Candies.Add(new Candy{Position=w.Player});Tick(w,1);Assert(w.Candies.Count==1,"Wasted pickup");});
  Check("Candy expires",()=>{var w=World();w.Candies.Clear();w.Candies.Add(new Candy{Position=new Vector2(10,-4),Life=.001f});Tick(w,1);Assert(w.Candies.Count==0,"Pickup leak");});
  Check("Two dashes consume stamina, third is blocked",()=>{var w=World();for(int j=0;j<3;j++){w.Step(dt,new ShotInput{Move=Vector2.left,Aim=Vector2.up,Dash=true});Tick(w,16);}Assert(w.Dashes==2&&w.Stamina<34,"Unlimited dash");});
  Check("Stamina recovers after delay",()=>{var w=World();w.Stamina=0;w.StaminaDelay=.85f;Tick(w,30);Near(w.Stamina,0);Tick(w,90);Assert(w.Stamina>20,"No regen");});
  Check("Idle dash does not consume stamina",()=>{var w=World();w.Step(dt,new ShotInput{Dash=true});Near(w.Stamina,100);});
  Check("Dash cannot tunnel through a shadow",()=>{var w=World();Barrier wall=w.Barriers[0];Vector2 n=new Vector2(-(wall.B-wall.A).y,(wall.B-wall.A).x).normalized,mid=(wall.A+wall.B)*.5f;w.Player=mid+n*.9f;for(int i=0;i<15;i++)w.Step(dt,new ShotInput{Move=-n,Aim=n,Dash=i==0});Assert(Vector2.Dot(w.Player-mid,n)>.7f,"Crossed wall");});
  Check("Complete interactive tutorial",Tutorial);
  Check("All three levels reach their intended ending",Campaign);
  Check("Restart clears all transient state",()=>{var w=World(3);w.Score=500;w.Stamina=2;w.Candies.Add(new Candy());w.Bullets.Add(B(Vector2.zero,Vector2.up));w.Start(1);Assert(w.Score==0&&w.Bullets.Count==0&&w.Level==1&&w.Stamina==100&&!w.BossDefeated,"Dirty restart");});
  Check("Defeat stops simulation",()=>{var w=World();w.Health=0;Tick(w,1);Assert(w.State==PlayState.Lost,"No defeat");float time=w.Time;Tick(w,30);Near(w.Time,time);});
  Check("Boss victory clears hostile threats",()=>{var w=World(3);var boss=w.Spawn(EnemyKind.Puppeteer,new Vector2(0,4));boss.Health=1;w.Bullets.Add(B(new Vector2(0,4.5f),Vector2.up));w.Mines.Add(new Mine{Position=w.Player,Age=7});Tick(w,1);Assert(w.State==PlayState.Won&&w.Bullets.Count==0&&w.Mines.Count==0,"Late mine spoiled victory");});
  Check("Distinct 3-wave campaign and stronger guard",()=>{for(int l=1;l<=3;l++){var w=World(l);Assert(w.WaveCount==3,"Wave budget");w.Wave=2;w.NextWave();Assert(w.Wave==3,"No third wave");Assert(w.Enemies.Exists(e=>e.Kind==EnemyKind.Guard)||w.Enemies.Exists(e=>e.Kind==EnemyKind.Puppeteer),"No late heavyweight");}});
  Check("Candy allocation is bounded",()=>{var w=World();w.Invulnerable=w.PuppetInvulnerable=9999;for(int j=0;j<16;j++){var e=w.Spawn(EnemyKind.Rabbit,new Vector2(9,-4));e.Health=1;w.Bullets.Add(B(e.Position,Vector2.right));Tick(w,1);}Assert(w.CandySpawned<=2,"Infinite candy drops");});
  Check("Long sessions remain bounded on every level",Stress);
  Console.WriteLine("RESULT: "+count+" passed; "+failed+" failed.");Environment.ExitCode=failed==0?0:1;
 }
 static void Tutorial(){var w=new StageWorld();w.Start(1,true);for(int i=0;i<50;i++)w.Step(dt,new ShotInput{Move=Vector2.left});Assert(w.TutorialStep==1,"Move");w.Bullets.Add(B(w.Enemies[0].Position,Vector2.right));Tick(w,1);Assert(w.TutorialStep==2,"Shoot");w.RequestLamp(0);Tick(w,50);Assert(w.TutorialStep==3,"Switch");w.Bullets.Add(B(new Vector2(12.3f,-4),Vector2.right*30));Tick(w,1);Assert(w.TutorialStep==4,"Bounce");w.Step(dt,new ShotInput{Move=Vector2.left,Dash=true});Assert(w.TutorialStep==5,"Dash");w.DashTime=0;w.Player=w.Candies[0].Position;Tick(w,1);Assert(w.TutorialStep==6&&w.State==PlayState.Playing,"Candy pause");Tick(w,160);Assert(w.State==PlayState.LevelClear,"Completion delay");}
 static void Campaign(){for(int level=1;level<=3;level++){var w=World(level);w.Invulnerable=w.PuppetInvulnerable=9999;for(int frame=0;frame<18000&&w.State==PlayState.Playing;frame++){foreach(var e in w.Enemies)w.Bullets.Add(B(e.Position,Vector2.right));Tick(w,1);}Assert(w.State==(level==3?PlayState.Won:PlayState.LevelClear),"Cannot finish level "+level);Assert(w.Wave==3,"Missing third wave");}}
 static void Stress(){for(int level=1;level<=3;level++){var w=World(level);w.Invulnerable=w.PuppetInvulnerable=9999;for(int i=0;i<18000&&w.State==PlayState.Playing;i++){w.Step(dt,new ShotInput{Move=new Vector2(Mathf.Sin(i*.01f),Mathf.Cos(i*.01f)),Aim=new Vector2(Mathf.Cos(i*.02f),Mathf.Sin(i*.02f)),Fire=true,Dash=i%60==0});Assert(w.Bullets.Count<=180&&w.Sparks.Count<700,"Pool leak");Assert(!float.IsNaN(w.Player.x)&&Mathf.Abs(w.Player.x)<12.61f,"Invalid actor");foreach(var b in w.Bullets)Assert(!float.IsNaN(b.Position.x)&&Mathf.Abs(b.Position.x)<12.7f&&Mathf.Abs(b.Position.y)<6,"Escaped bullet");}}}
}

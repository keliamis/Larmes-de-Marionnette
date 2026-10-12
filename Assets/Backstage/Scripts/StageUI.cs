using System.Collections.Generic;
using UnityEngine;
namespace Backstage
{
    public sealed class StageUI
    {
        readonly StageGame app;readonly Texture2D atlas;readonly Texture2D[] panels=new Texture2D[6];readonly bool[] ownsPanel=new bool[6];
        Texture2D menuScene, loadingScene, finaleScene, paper, paperHover, hanging;
        Texture2D premiumMenu, loadingPremium, actMapPremium, titleWordmark, storyLonely, storyHand, storyAwakens, storyRevolt, premiumFinal;
        readonly Texture2D[] actScenes=new Texture2D[3];
        readonly List<Texture2D> ownedExtra=new List<Texture2D>();readonly Dictionary<string,GUIStyle> styles=new Dictionary<string,GUIStyle>();
        readonly Color cream=StageArt.C("F4E7CF"),gold=StageArt.C("D7AF68"),mint=StageArt.C("8CE1DF"),ink=StageArt.C("101522"),rose=StageArt.C("E77368"),muted=StageArt.C("B0ACAF");
        StageWorld W {get{return app.world;}}
        public StageUI(StageGame owner)
        {
            app=owner;
            atlas=Resources.Load<Texture2D>("StoryAtlas");
            // Panels are additionally stored as PNG data in *.bytes files. Unity imports them
            // as TextAssets, so the art also loads if the TextureImporter metadata is reset.
            int loaded=0;
            for(int i=0;i<panels.Length;i++)
            {
                TextAsset raw=Resources.Load<TextAsset>("StoryPanel"+i);
                if(raw!=null)
                {
                    Texture2D decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);
                    try
                    {
                        if(ImageConversion.LoadImage(decoded,raw.bytes,true))
                        {
                            decoded.name="Backstage story panel "+i;
                            decoded.filterMode=FilterMode.Bilinear;
                            decoded.wrapMode=TextureWrapMode.Clamp;
                            panels[i]=decoded;
                            ownsPanel[i]=true;
                        }
                        else Object.Destroy(decoded);
                    }
                    catch(System.Exception e)
                    {
                        Object.Destroy(decoded);
                        Debug.LogWarning("Backstage: cannot decode story panel "+i+": "+e.Message);
                    }
                }
                if(panels[i]==null)panels[i]=Resources.Load<Texture2D>("StoryPanel"+i);
                if(panels[i]!=null)loaded++;
            }
            menuScene=LoadArt("MenuScene");loadingScene=LoadArt("LoadingScene");finaleScene=LoadArt("FinaleScene");
            premiumMenu=LoadArt("MenuPremium");loadingPremium=LoadArt("LoadingPremium");actMapPremium=LoadArt("ActMapPremium");storyLonely=LoadArt("StoryLonely");storyHand=LoadArt("StoryHand");premiumFinal=LoadArt("FinalPremium");
            storyAwakens=LoadArt("StoryAwakens");storyRevolt=LoadArt("StoryRevolt");
            titleWordmark=LoadArt("TitleWordmark");
            paper=LoadArt("PaperButton");paperHover=LoadArt("PaperButtonHover");hanging=LoadArt("HangingStars");
            for(int i=0;i<actScenes.Length;i++)actScenes[i]=LoadArt("ActScene"+i);
            if(loaded!=panels.Length)
                Debug.LogWarning("Larmes: loaded "+loaded+"/6 story panels. Check Assets/Backstage/Resources/StoryPanel*.bytes in Unity. Legacy atlas available: "+(atlas!=null));
        }
        Texture2D LoadArt(string name)
        {
            TextAsset source=Resources.Load<TextAsset>(name);
            if(source==null){Debug.LogWarning("Larmes de Marionnette: artwork not found: "+name);return null;}
            Texture2D tex=new Texture2D(2,2,TextureFormat.RGBA32,false);
            if(!ImageConversion.LoadImage(tex,source.bytes,true)){Object.Destroy(tex);return null;}
            tex.filterMode=FilterMode.Bilinear;tex.wrapMode=TextureWrapMode.Clamp;
            ownedExtra.Add(tex);return tex;
        }
        public void Dispose()
        {
            for(int i=0;i<panels.Length;i++)if(ownsPanel[i]&&panels[i]!=null)Object.Destroy(panels[i]);
            foreach(Texture2D tex in ownedExtra)if(tex!=null)Object.Destroy(tex);
        }
        GUIStyle Style(int size,Color color,TextAnchor anchor=TextAnchor.UpperLeft,bool bold=false)
        {
            string key=size+":"+anchor+":"+bold;GUIStyle s;
            if(!styles.TryGetValue(key,out s)){s=new GUIStyle{font=app.font,fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,alignment=anchor,wordWrap=true,clipping=TextClipping.Clip};styles[key]=s;}
            s.normal.textColor=color;return s;
        }
        void T(float x,float y,float w,float h,string text,int size,Color c,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
        {GUI.Label(new Rect(x,y,w,h),text,Style(size,c,anchor,bold));}
        void Fill(float x,float y,float w,float h,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=old;}
        void Frame(float x,float y,float w,float h,Color c){Fill(x,y,w,2,c);Fill(x,y+h-2,w,2,c);Fill(x,y,2,h,c);Fill(x+w-2,y,2,h,c);}
        void Panel(float x,float y,float w,float h)
        {
            Fill(x+7,y+8,w,h,StageArt.C("070B18",.52f));
            Fill(x,y,w,h,StageArt.C("11192B",.91f));
            Frame(x,y,w,h,StageArt.C("C4AD8A",.5f));
            Fill(x+11,y+11,16,3,StageArt.C("D7AF68",.85f));
            Fill(x+w-27,y+h-14,16,3,StageArt.C("D7AF68",.85f));
        }
        bool Button(float x,float y,float w,float h,string text,bool primary=false,bool enabled=true)
        {
            Rect r=new Rect(x,y,w,h);bool hover=r.Contains(Event.current.mousePosition)&&enabled;
            Texture2D sheet=(hover||primary)?paperHover:paper;
            if(sheet!=null)
            {
                Color prev=GUI.color;
                GUI.color=enabled?(hover?StageArt.C("FFF7E2"):Color.white):StageArt.C("A5A1A6",.64f);
                GUI.DrawTexture(r,sheet,ScaleMode.StretchToFill,true);GUI.color=prev;
            }
            else
            {Fill(x,y,w,h,!enabled?StageArt.C("272936"):primary?gold:StageArt.C("D6C2A3"));Frame(x,y,w,h,gold);}
            T(x+22,y+3,w-44,h-6,text,Mathf.Clamp(Mathf.RoundToInt(h*.32f),16,23),enabled?StageArt.C("2B2832"):StageArt.C("5D5650"),false,TextAnchor.MiddleCenter);
            return enabled&&GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        bool Hit(float x,float y,float w,float h,bool enabled=true)
        {return enabled&&GUI.Button(new Rect(x,y,w,h),GUIContent.none,GUIStyle.none);}
        void Picture(Rect r,int panel)
        {
            if(panel>=0&&panel<panels.Length&&panels[panel]!=null)
            {
                GUI.DrawTexture(r,panels[panel],ScaleMode.StretchToFill,true);
                return;
            }
            if(atlas!=null)
            {
                float x=panel%2*.5f,y=panel<2?.5f:0;
                GUI.DrawTextureWithTexCoords(r,atlas,new Rect(x+.003f,y+.004f,.494f,.492f));
                return;
            }
            // Visible placeholder instead of a silently empty title/cutscene.
            Fill(r.x,r.y,r.width,r.height,StageArt.C("25293D"));
            Frame(r.x,r.y,r.width,r.height,StageArt.C("D7AF68",.6f));
            T(r.x+12,r.y+r.height*.40f,r.width-24,60,"ИЛЛЮСТРАЦИЯ НЕ ЗАГРУЖЕНА",20,cream,true,TextAnchor.MiddleCenter);
        }
        void Title()
        {
            Fill(0,0,1600,900,ink);
            Texture2D art=premiumMenu!=null?premiumMenu:menuScene;
            if(art!=null)GUI.DrawTexture(new Rect(0,0,1600,900),art,ScaleMode.ScaleAndCrop,true);
            Fill(0,0,510,900,StageArt.C("080D1A",.30f));
            if(titleWordmark!=null)GUI.DrawTexture(new Rect(390,6,895,298),titleWordmark,ScaleMode.ScaleToFit,true);
            else T(380,34,875,154,"Larmes de\nMarionnette",72,StageArt.C("EAC48A"),false,TextAnchor.MiddleCenter);
            if(Button(68,295,434,68,app.trainingDone?"ПРОДОЛЖИТЬ ИСТОРИЮ":"НАЧАТЬ ИСТОРИЮ",true))app.BeginJourney();
            if(Button(68,378,434,63,"НОВАЯ ИГРА"))app.NewGame();
            if(Button(68,456,434,63,"ВЫБОР АКТА",false,app.trainingDone))app.levelMenu=true;
            if(Button(68,534,434,63,"ДУЭЛЬ ПО СЕТИ"))app.duelMenu=true;
            if(Button(68,612,434,63,"НАСТРОЙКИ"))app.options=true;
            if(Button(68,690,434,63,"ВЫХОД"))app.Quit();
        }

        void Levels()
        {
            Fill(0,0,1600,900,ink);
            Texture2D map=actMapPremium!=null?actMapPremium:premiumMenu;
            if(map!=null)GUI.DrawTexture(new Rect(0,0,1600,900),map,ScaleMode.ScaleAndCrop,true);
            Fill(0,0,1600,106,StageArt.C("080C18",.55f));
            T(65,24,1470,62,"КАРТА СПЕКТАКЛЯ",36,gold,false,TextAnchor.MiddleCenter);
            string[] names={"АКТ I\nРепетиция","АКТ II\nЗабытые декорации","АКТ III\nТеатр теней"};
            for(int i=0;i<3;i++)
            {
                float x=110+i*490,y=i==1?604:630;
                Fill(x+62,y-42,2,44,StageArt.C("D7AF68",.70f));
                Fill(x+293,y-42,2,44,StageArt.C("D7AF68",.70f));
                string label=names[i]+(i+1<=app.unlocked?"":"\nЗакрыто");
                if(Button(x,y,355,110,label,true,i+1<=app.unlocked))app.SelectAct(i+1);
            }
            if(Button(618,803,364,58,"←  ГЛАВНОЕ МЕНЮ"))app.levelMenu=false;
        }

        void Story()
        {
            Fill(0,0,1600,900,ink);
            int page=Mathf.Clamp(app.storyPage,0,3);
            Texture2D scene=app.storyLevel==1?(page==0?storyLonely:page==1?storyAwakens:page==2?storyHand:storyRevolt):
                app.storyLevel==2?storyAwakens:storyHand;
            if(scene!=null)GUI.DrawTexture(new Rect(0,0,1600,900),scene,ScaleMode.ScaleAndCrop,true);
            else Picture(new Rect(0,0,1600,900),page);
            string[] lines={
                "Каждый вечер кукла повторяла чужую роль.\nНити решали, куда ей смотреть и когда сделать шаг.",
                "Но однажды её тень отделилась от света.\nВпервые кто-то на этой сцене мог выбирать сам.",
                "Кукловод не собирался отпускать свою актрису.\nТень осталась, чтобы защитить её и изменить спектакль.",
                "Управляй светом. Меняй стены.\nВерни кукле свободу."};
            Fill(38,42,602,258,StageArt.C("080C18",.84f));
            Fill(38,42,3,258,gold);
            T(65,64,548,30,app.storyLevel==1?"ПРЕДЫСТОРИЯ  •  "+(page+1)+" / 4":"АКТ "+app.storyLevel,19,gold,true);
            T(65,112,548,162,app.storyLevel==1?lines[page]:"Новый акт начинается. Кукла всё ещё ждёт твоей защиты.",25,cream,false,TextAnchor.MiddleLeft);
            if(Button(1220,806,315,70,app.storyLevel==1&&page<3?"ДАЛЕЕ  →":app.storyTutorial?"К ОБУЧЕНИЮ  →":"НА СЦЕНУ  →",true))app.AdvanceStory();
        }

        void HUD()
        {
            if(W.Tutorial)
            {
                Fill(0,0,1600,900,StageArt.C("08111B",.20f));
                string[] titles={"Шаг 1 / 6  ·  ХОДЬБА","Шаг 2 / 6  ·  ВЫСТРЕЛ","Шаг 3 / 6  ·  СВЕТ","Шаг 4 / 6  ·  РИКОШЕТ","Шаг 5 / 6  ·  РЫВОК","Шаг 6 / 6  ·  ВОССТАНОВЛЕНИЕ","Репетиция завершена"};
                string[] steps={
                    "Нажми WASD, чтобы сделать первые шаги по сцене.",
                    "Наведи курсор на зайца и выстрели ЛКМ.",
                    "Попади в золотой переключатель, чтобы повернуть свет.",
                    "Используй стену тени и сделай рикошет по цели.",
                    "Нажми Space во время движения — это рывок.",
                    "Теперь дойди до отмеченной конфеты и подбери её.",
                    "Готово! Посмотри на сцену — первый акт начнётся через мгновение."
                };
                int step=Mathf.Clamp(W.TutorialStep,0,6);
                Panel(192,690,1216,130);
                T(220,708,1160,24,titles[step],22,gold,true,TextAnchor.MiddleCenter);
                T(240,744,1120,44,steps[step],25,cream,true,TextAnchor.MiddleCenter);
                T(270,790,1060,18,"Во время обучения доступно только нужное действие — так проще привыкнуть к управлению.",14,muted,false,TextAnchor.MiddleCenter);
            }
            int healthSlots=6;
            int filled=Mathf.Clamp(Mathf.RoundToInt(W.Health),0,healthSlots);
            for(int i=0;i<healthSlots;i++)
                T(34+i*39,816,36,42,"♥",28,i<filled?rose:StageArt.C("5F4958"),true,TextAnchor.MiddleCenter);
            T(31,779,255,25,"ТЕНЬ",17,cream,true);
            T(509,779,290,25,"КУКЛА  "+Mathf.CeilToInt(W.PuppetHealth)+" / 12",17,cream,true);
            Fill(511,821,256,13,StageArt.C("432D38",.8f));
            Fill(511,821,256*Mathf.Clamp01(W.PuppetHealth/12f),13,gold);
            Frame(511,821,256,13,cream);
            Fill(28,862,220,4,StageArt.C("6F7A8E",.45f));
            Fill(28,866,220,3,StageArt.C("6F7A8E",.25f));
            int staminaPips=Mathf.Clamp(Mathf.CeilToInt(W.Stamina/34f),0,3);
            for(int i=0;i<3;i++)
                T(258+i*54,813,44,44,"✦",30,i<staminaPips?gold:StageArt.C("5F4958"),true,TextAnchor.MiddleCenter);
            T(785,808,260,38,"SPACE · РЫВОК",17,gold,true,TextAnchor.MiddleLeft);
            T(28,26,340,22,"Акт "+W.Level+" · "+(W.Tutorial?"Репетиция":"Сцена продолжается"),16,cream,true);
            if(!W.Tutorial)
                T(1290,26,282,22,"Счёт "+W.Score+"  •  Волна "+Mathf.Max(1,W.Wave)+" / "+W.WaveCount,16,gold,true,TextAnchor.MiddleRight);
            if(W.BannerTime>0 && !W.Tutorial)
            {
                Fill(520,42,560,42,StageArt.C("091120",.54f));
                T(528,50,544,26,W.Banner,18,cream,true,TextAnchor.MiddleCenter);
            }
            foreach(Enemy e in W.Enemies)if(e.Kind==EnemyKind.Puppeteer)
            {
                Fill(1024,820,455,12,StageArt.C("442A38",.85f));
                Fill(1024,820,455*Mathf.Clamp01(e.Health/e.MaxHealth),12,rose);
                T(1024,788,455,25,"КУКЛОВОД",17,gold,true,TextAnchor.MiddleCenter);
            }
            if(Button(1452,20,120,44,"ПАУЗА"))app.paused=true;
        }

        void Settings()
        {
            Fill(0,0,1600,900,StageArt.C("080C16",.85f));Panel(425,225,750,440);
            T(468,231,664,60,"НАСТРОЙКИ",38,cream,true);
            if(Button(472,340,657,61,"ЗВУК: "+(app.muted?"ВЫКЛ":"ВКЛ"))){app.muted=!app.muted;app.SaveSettings();}
            if(Button(472,512,657,61,"ГОТОВО",true))app.options=false;
        }
        void Result()
        {
            bool clear=W.State==PlayState.LevelClear,win=W.State==PlayState.Won;
            Fill(0,0,1600,900,StageArt.C("080C16",.8f));Panel(385,170,830,567);
            T(428,214,744,86,app.paused?"АНТРАКТ":win?"КУКЛА СВОБОДНА":clear?W.Tutorial?"РЕПЕТИЦИЯ ОКОНЧЕНА":"АКТ ЗАВЕРШЁН":"ЗАНАВЕС",40,cream,true,TextAnchor.MiddleCenter);
            string line=app.paused?"Сцена подождёт.":win?"Кукла и её тень больше никому не принадлежат.":clear?W.Tutorial?"Ты освоилась со светом и тенями. Готова к первой операции?":"Занавес опустился. Впереди следующий акт.":W.Health<=0?"Тень погасла. Используй укрытия, рикошеты и редкие конфеты.":"Кукла потеряна. Используй свет, чтобы отрезать врагов от неё.";
            T(434,322,732,92,line,24,gold,false,TextAnchor.MiddleCenter);
            T(434,430,732,40,"СЧЁТ "+W.Score+"    •    РИКОШЕТЫ "+W.RicochetHits,20,mint,true,TextAnchor.MiddleCenter);
            if(Button(463,502,674,64,app.paused?"ПРОДОЛЖИТЬ":clear?W.Tutorial?"ПРИСТУПИТЬ К ПЕРВОМУ АКТУ  →":"СЛЕДУЮЩИЙ АКТ  →":win?"НОВЫЙ СПЕКТАКЛЬ":"ПОВТОРИТЬ ЭТОТ АКТ",true))
            {if(app.paused)app.paused=false;else if(clear)app.NextLevel();else if(win)app.BeginJourney();else app.Retry();}
            if(Button(463,593,674,58,"В ГЛАВНОЕ МЕНЮ"))app.GoTitle();
        }
        void Finale()
        {
            if(app.cutscene<8.5f)
            {
                Fill(0,0,1600,112,StageArt.C("080C18",.76f));
                T(90,26,1420,65,app.cutscene<3?"Нити больше не держат её.":app.cutscene<5.5f?"Рука Кукловода исчезает вместе со своим спектаклем.":"И кукла делает первый шаг сама.",30,cream,false,TextAnchor.MiddleCenter);
                return;
            }
            Fill(0,0,1600,900,ink);
            Texture2D scene=premiumFinal!=null?premiumFinal:finaleScene;
            if(scene!=null)GUI.DrawTexture(new Rect(0,0,1600,900),scene,ScaleMode.ScaleAndCrop,true);
            Fill(995,56,555,288,StageArt.C("0B1120",.82f));
            Frame(995,56,555,288,gold);
            T(1020,80,505,66,"КУКЛА СВОБОДНА",33,gold,true,TextAnchor.MiddleCenter);
            T(1020,156,505,72,"Теперь у неё есть право выбрать\nсобственную роль.",25,cream,false,TextAnchor.MiddleCenter);
            if(Button(1080,252,385,65,"ЗАКРЫТЬ ЗАНАВЕС",true))app.cutscene=-1;
        }

        void Splash()
        {
            Fill(0,0,1600,900,StageArt.C("0B1425"));
            Texture2D scene=loadingPremium!=null?loadingPremium:loadingScene;
            if(scene!=null)GUI.DrawTexture(new Rect(0,0,1600,900),scene,ScaleMode.ScaleAndCrop,true);
            else Fill(0,0,1600,900,StageArt.C("111828"));
            float value=Mathf.Clamp01(app.splashClock/2.35f);
            Fill(386,762,828,110,StageArt.C("08101B",.69f));
            Fill(399,777,802,26,StageArt.C("191823"));Frame(399,777,802,26,gold);
            Fill(405,783,790*value,14,StageArt.C("EFCB88"));
            T(380,814,840,40,"Готовим декорации...",25,cream,false,TextAnchor.MiddleCenter);
        }

        void DuelLobby()
        {
            Fill(0,0,1600,900,ink);Picture(new Rect(680,85,847,532),3);
            T(78,81,560,40,"РЕЖИМ ДЛЯ ДВОИХ",22,gold,true);
            T(70,159,651,108,"ТЕНЬ ПРОТИВ ТЕНИ",49,cream,true);
            T(77,292,542,92,"Две копии игры, одна локальная сеть.\nПервый до трёх побед забирает сцену.",23,mint);
            if(Button(78,450,533,69,"СОЗДАТЬ ДУЭЛЬ",true))app.HostDuel();
            T(79,567,543,33,"IP-адрес компьютера-хоста",19,cream);
            GUIStyle edit=Style(22,cream,TextAnchor.MiddleLeft);
            Fill(78,610,350,62,StageArt.C("242C3B"));Frame(78,610,350,62,gold);
            app.duelIp=GUI.TextField(new Rect(91,617,327,48),app.duelIp,edit);
            if(Button(449,610,349,62,"ПОДКЛЮЧИТЬСЯ",true))app.JoinDuel();
            if(!string.IsNullOrEmpty(app.duelError))T(78,708,1450,61,app.duelError,18,rose);
            T(77,761,1334,46,"Нужен один Wi-Fi/кабель. На хосте разрешите игре доступ к частной сети в брандмауэре.",18,muted);
            if(Button(1240,809,285,53,"← НАЗАД"))app.duelMenu=false;
        }
        void DuelOverlay()
        {
            StageDuel d=app.duel;
            Panel(349,16,902,107);
            T(378,23,846,43,"ДУЭЛЬ  •  РАУНД "+d.Round+"     "+d.HostWins+" : "+d.GuestWins,25,gold,true,TextAnchor.MiddleCenter);
            T(377,78,850,35,"ТЕНЬ ХОСТА  "+d.HostHP+" ♥             ТЕНЬ ГОСТЯ  "+d.GuestHP+" ♥",21,cream,true,TextAnchor.MiddleCenter);
            if(!d.Connected || d.Finished || d.RoundPause>0)
            {
                Panel(383,308,833,288);
                string head=!d.Connected?"ОЖИДАНИЕ ДУЭЛЯНТА":d.Finished?(d.HostWins>d.GuestWins?"ПОБЕДИЛ ХОСТ":"ПОБЕДИЛ ГОСТЬ"):"РАУНД ЗАВЕРШЁН";
                T(418,346,760,70,head,38,cream,true,TextAnchor.MiddleCenter);
                T(432,450,741,71,!d.Connected?d.Status:d.Finished?"Сцена принадлежит победителю.":"Следующий раунд уже близко.",22,mint,false,TextAnchor.MiddleCenter);
            }
            T(50,843,1070,31,"WASD — движение   •   мышь — прицел   •   ЛКМ — огонь   •   Space — рывок",18,cream);
            if(Button(1225,822,314,58,"ВЫЙТИ ИЗ ДУЭЛИ"))app.GoTitle();
        }
        public void Draw()
        {
            float scale=Mathf.Max(.001f,Mathf.Min(Screen.width/1600f,Screen.height/900f));Matrix4x4 old=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1600*scale)*.5f,(Screen.height-900*scale)*.5f,0),Quaternion.identity,new Vector3(scale,scale,1));
            if(app.splashClock<2.35f)Splash();
            else if(app.duel!=null)DuelOverlay();
            else if(app.storyPanel>=0)Story();
            else if(W.State==PlayState.Title){if(app.duelMenu)DuelLobby();else if(app.levelMenu)Levels();else Title();}
            else if(app.cutscene>=0)Finale();
            else
            {
                if(app.showHud||W.Tutorial)HUD();
                if(app.paused||W.State!=PlayState.Playing)Result();
            }
            if(app.options && app.splashClock>=2.35f)Settings();GUI.matrix=old;
        }
    }
}

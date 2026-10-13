using System.Collections.Generic;
using UnityEngine;
namespace Backstage
{
    public sealed class StageUI
    {
        readonly StageGame app;readonly Texture2D atlas;readonly Texture2D[] panels=new Texture2D[6];
        Texture2D menuScene, loadingScene, finaleScene, paper, paperHover, hanging;
        Texture2D premiumMenu, loadingPremium, actMapPremium, titleWordmark, storyLonely, storyHand, storyAwakens, storyRevolt, premiumFinal;
        Texture2D menuStage,menuCharacters,menuGlow,menuLink,paperSigns,duelIllustration;
        readonly Rect[] signUV={new Rect(0.14117647f,0.82956879f,0.71950464f,0.15913758f),new Rect(0.15108359f,0.67145791f,0.69164087f,0.15195072f),new Rect(0.14179567f,0.49897331f,0.71640867f,0.16529774f),new Rect(0.13560372f,0.33675565f,0.72507740f,0.16221766f),new Rect(0.14922601f,0.18685832f,0.71331269f,0.14271047f),new Rect(0.14117647f,0.02464066f,0.71702786f,0.15297741f)};
        Rect characterUV=new Rect(0,0,1,1),titleUV=new Rect(0,0,1,1),paperUV=new Rect(0,0,1,1);
        float characterAspect=1.35f,titleAspect=2;
        Vector2 menuDrift;
        readonly Dictionary<string,Texture2D> artworkCache=new Dictionary<string,Texture2D>();
        readonly List<Texture2D> ownedExtra=new List<Texture2D>();readonly Dictionary<string,GUIStyle> styles=new Dictionary<string,GUIStyle>();
        readonly Dictionary<string,float> buttonScales=new Dictionary<string,float>();
        Vector2 mouse;
        string lastHoveredButton="";
        bool hoveredThisDraw;
        readonly Color cream=StageArt.C("F4E7CF"),gold=StageArt.C("D7AF68"),mint=StageArt.C("8CE1DF"),ink=StageArt.C("101522"),rose=StageArt.C("E77368"),muted=StageArt.C("B0ACAF");
        StageWorld W {get{return app.world;}}
        public StageUI(StageGame owner)
        {
            app=owner;
            atlas=Resources.Load<Texture2D>("StoryAtlas");
            string[] panelNames={"StoryLonely","StoryAwakens","StoryHand","StoryRevolt","FinalPremium","MenuPremium"};
            int loaded=0;
            for(int i=0;i<panels.Length;i++){panels[i]=LoadArt(panelNames[i]);if(panels[i]!=null)loaded++;}
            menuScene=LoadArt("MenuPremium");loadingScene=LoadArt("LoadingPremium");finaleScene=LoadArt("FinalPremium");
            premiumMenu=LoadArt("MenuPremium");loadingPremium=LoadArt("LoadingPremium");actMapPremium=LoadArt("ActMapPremium");storyLonely=LoadArt("StoryLonely");storyHand=LoadArt("StoryHand");premiumFinal=LoadArt("FinalPremium");
            storyAwakens=LoadArt("StoryAwakens");storyRevolt=LoadArt("StoryRevolt");
            titleWordmark=LoadArt("TitleWordmark",true);
            paperSigns=LoadArt("PaperSigns");duelIllustration=LoadArt("DuelIllustration");
            menuStage=LoadArt("MenuStage");menuCharacters=LoadArt("MenuCharacters",true);
            if(titleWordmark!=null){titleUV=AlphaBounds(titleWordmark);titleAspect=titleUV.width*titleWordmark.width/(titleUV.height*titleWordmark.height);titleWordmark.Apply(false,true);}
            if(menuCharacters!=null){characterUV=AlphaBounds(menuCharacters);characterAspect=characterUV.width*menuCharacters.width/(characterUV.height*menuCharacters.height);menuCharacters.Apply(false,true);}
            menuGlow=MakeMenuMask(false);menuLink=MakeMenuMask(true);
            hanging=LoadArt("HangingStars");
            if(loaded!=panels.Length)
                Debug.LogWarning("Larmes: loaded "+loaded+"/6 story panels. Check canonical Story*.bytes and FinalPremium.bytes in Unity. Legacy atlas available: "+(atlas!=null));
        }
        Texture2D LoadArt(string name,bool keepReadable=false)
        {
            Texture2D cached;if(artworkCache.TryGetValue(name,out cached))return cached;
            TextAsset source=Resources.Load<TextAsset>(name);
            if(source==null){Debug.LogWarning("Larmes de Marionnette: artwork not found: "+name);return null;}
            Texture2D tex=new Texture2D(2,2,TextureFormat.RGBA32,false);
            if(!ImageConversion.LoadImage(tex,source.bytes,!keepReadable)){Object.Destroy(tex);return null;}
            tex.filterMode=FilterMode.Bilinear;tex.wrapMode=TextureWrapMode.Clamp;
            ownedExtra.Add(tex);artworkCache[name]=tex;return tex;
        }
        static Rect AlphaBounds(Texture2D tex)
        {
            Color32[] pixels=tex.GetPixels32();int left=tex.width,right=-1,bottom=tex.height,top=-1;
            for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++)
            {
                if(pixels[y*tex.width+x].a<=4)continue;
                left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);
            }
            if(right<0)return new Rect(0,0,1,1);
            left=Mathf.Max(0,left-2);right=Mathf.Min(tex.width-1,right+2);bottom=Mathf.Max(0,bottom-2);top=Mathf.Min(tex.height-1,top+2);
            return new Rect((float)left/tex.width,(float)bottom/tex.height,(float)(right-left+1)/tex.width,(float)(top-bottom+1)/tex.height);
        }
        Texture2D MakeMenuMask(bool ring)
        {
            const int size=64;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=(x+.5f-size*.5f)/(size*.5f),dy=(y+.5f-size*.5f)/(size*.5f),radius=Mathf.Sqrt(dx*dx+dy*dy);
                float alpha=ring?Mathf.Clamp01(1-Mathf.Abs(radius-.75f)*12):Mathf.Pow(Mathf.Clamp01(1-radius),3);
                colors[y*size+x]=new Color(1,1,1,alpha);
            }
            texture.SetPixels(colors);texture.Apply(false,true);texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;ownedExtra.Add(texture);return texture;
        }
        void MenuSprite(Rect rect,Texture2D texture,Color tint)
        {Color old=GUI.color;GUI.color=tint;GUI.DrawTexture(rect,texture,ScaleMode.StretchToFill,true);GUI.color=old;}
        void MenuDust(bool foreground)
        {
            int count=foreground?18:32;
            for(int i=0;i<count;i++)
            {
                float phase=Mathf.Repeat(i*.139f+app.visualTime*(foreground ? .029f : .019f),1);
                float x=720+Mathf.Repeat(i*149.87f+Mathf.Sin(app.visualTime*.13f+i)*18,740)+menuDrift.x*(foreground?1.4f:.45f);
                float y=300+phase*480+menuDrift.y*(foreground?1.4f:.45f);
                float size=(foreground?10:7)+(i%3)*1.5f;
                MenuSprite(new Rect(x,y,size,size),menuGlow,StageArt.C("FFE5B0",Mathf.Sin(phase*Mathf.PI)*(foreground ? .85f : .55f)));
                float core=foreground?2.6f:1.8f;
                Fill(x+(size-core)*.5f,y+(size-core)*.5f,core,core,StageArt.C("FFE3A5",Mathf.Sin(phase*Mathf.PI)*(foreground? .68f:.42f)));
            }
        }
        public void Dispose()
        {
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
        void Fill(float x,float y,float w,float h,Color c){Color old=GUI.color;GUI.color=old*c;GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=old;}
        void Frame(float x,float y,float w,float h,Color c){Fill(x,y,w,2,c);Fill(x,y+h-2,w,2,c);Fill(x,y,2,h,c);Fill(x+w-2,y,2,h,c);}
        void Panel(float x,float y,float w,float h)
        {
            Fill(x+7,y+8,w,h,StageArt.C("070B18",.52f));
            Fill(x,y,w,h,StageArt.C("11192B",.91f));
            Frame(x,y,w,h,StageArt.C("C4AD8A",.5f));
            Fill(x+11,y+11,16,3,StageArt.C("D7AF68",.85f));
            Fill(x+w-27,y+h-14,16,3,StageArt.C("D7AF68",.85f));
        }
        bool Button(float x,float y,float w,float h,string text,bool primary=false,bool enabled=true,int textSize=0,int sign=-1,float tilt=0)
        {
            Rect r=new Rect(x,y,w,h);
            Matrix4x4 before=GUI.matrix;Vector2 localMouse=mouse;
            if(tilt!=0)
            {
                Vector2 d=mouse-r.center;float angle=-tilt*Mathf.Deg2Rad;
                localMouse=r.center+new Vector2(d.x*Mathf.Cos(angle)-d.y*Mathf.Sin(angle),d.x*Mathf.Sin(angle)+d.y*Mathf.Cos(angle));
                GUIUtility.RotateAroundPivot(tilt,r.center);
            }
            bool hover=r.Contains(localMouse)&&enabled&&GUI.enabled;
            string key=x+":"+y+":"+w+":"+h+":"+text;float scale;
            if(hover&&Event.current.type==EventType.Repaint)
            {
                hoveredThisDraw=true;
                if(lastHoveredButton!=key){lastHoveredButton=key;app.PlayButtonSound(false);}
            }
            if(!buttonScales.TryGetValue(key,out scale))scale=1;
            if(Event.current.type==EventType.Repaint){scale=Mathf.Lerp(scale,hover?1.05f:1,Mathf.Clamp01(Time.unscaledDeltaTime*12));buttonScales[key]=scale;}
            Matrix4x4 interactionMatrix=GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(scale,scale),r.center);
            Rect drawn=r;
            Texture2D sheet=paperSigns!=null?paperSigns:(hover||primary)?paperHover:paper;
            int variant=sign>=0?sign:Mathf.Abs(Mathf.RoundToInt(x*.1f+y*.03f))%6;
            Rect uv=paperSigns!=null?signUV[variant%6]:paperUV;
            if(sheet!=null)
            {
                Color prev=GUI.color;
                GUI.color=enabled?(hover?StageArt.C("FFF7E2"):Color.white):StageArt.C("A5A1A6",.64f);
                if(w<170)
                {
                    Fill(drawn.x,drawn.y,drawn.width,drawn.height,StageArt.C("18202C"));Frame(drawn.x,drawn.y,drawn.width,drawn.height,gold);
                }
                else DrawSign(drawn,sheet,uv);
                GUI.color=prev;
            }
            else
            {Fill(x,y,w,h,!enabled?StageArt.C("272936"):primary?gold:StageArt.C("D6C2A3"));Frame(x,y,w,h,gold);}
            int fontSize=textSize>0?textSize:Mathf.Clamp(Mathf.RoundToInt(h*.32f),16,23);
            T(drawn.x+22,drawn.y+3,drawn.width-44,drawn.height-6,text,fontSize,enabled?(w<170?cream:StageArt.C("2B2832")):StageArt.C("5D5650"),false,TextAnchor.MiddleCenter);
            GUI.matrix=interactionMatrix;
            bool clicked=enabled&&GUI.Button(r,GUIContent.none,GUIStyle.none);
            if(clicked)app.PlayButtonSound(true);
            GUI.matrix=before;return clicked;
        }
        void DrawSign(Rect target,Texture2D sheet,Rect uv)
        {
            // One continuous UV mapping: separate strips caused sampling seams during hover.
            GUI.DrawTextureWithTexCoords(target,sheet,uv,true);
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
                float x=panel%2*.5f,y=panel<2? .5f:0;
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
            menuDrift=new Vector2(Mathf.Sin(app.visualTime*.23f)*4,Mathf.Sin(app.visualTime*.61f)*2);
            Texture2D art=menuStage!=null?menuStage:premiumMenu!=null?premiumMenu:menuScene;
            if(art!=null)GUI.DrawTexture(new Rect(-12+menuDrift.x*.25f,-8+menuDrift.y*.25f,1624,916),art,ScaleMode.ScaleAndCrop,true);
            MenuSprite(new Rect(810,390,720,470),menuGlow,StageArt.C("FFD18D",.09f+Mathf.Sin(app.visualTime*.65f)*.014f));
            MenuDust(false);
            if(menuCharacters!=null)
            {
                float height=550,width=height*characterAspect;
                float left=1550-width;
                MenuSprite(new Rect(left-12,801,width*.70f,76),menuGlow,StageArt.C("05080E",.65f));
                MenuSprite(new Rect(left+width*.53f,810,width*.49f,61),menuGlow,StageArt.C("05080E",.60f));
                GUI.DrawTextureWithTexCoords(new Rect(left,840-height,width,height),menuCharacters,characterUV,true);
            }
            MenuDust(true);
            Fill(0,0,520,900,StageArt.C("080D1A",.15f));
            if(titleWordmark!=null)
            {
                float height=Mathf.Min(242,710/titleAspect),width=height*titleAspect;
                GUI.DrawTextureWithTexCoords(new Rect(370+(710-width)*.5f,50,width,height),titleWordmark,titleUV,true);
            }
            else T(380,78,760,154,"Larmes de\nMarionnette",64,StageArt.C("EAC48A"),false,TextAnchor.MiddleCenter);
            bool actMenu=app.unlocked>1||app.EncoreUnlocked;
            float y=actMenu?302:342;
            for(float linkY=y+12;linkY<(actMenu?853:805);linkY+=14)MenuSprite(new Rect(75,linkY,8,12),menuLink,StageArt.C("C99C5E",.82f));
            if(Button(52,y,516,90,app.trainingDone&&!app.journeyComplete?"ПРОДОЛЖИТЬ ИСТОРИЮ":"НАЧАТЬ ИСТОРИЮ",true,true,25,0,-1))app.BeginJourney();
            y+=110;
            if(actMenu){if(Button(72,y,410,68,"ВЫБОР АКТА",textSize:25,sign:1,tilt:.8f))app.levelMenu=true;y+=88;}
            if(Button(60,y,452,77,"ДУЭЛЬ ПО СЕТИ",textSize:25,sign:2,tilt:-.5f))app.duelMenu=true;y+=97;
            if(Button(66,y,466,80,"ДОСТИЖЕНИЯ",textSize:25,sign:3,tilt:.5f))app.achievementMenu=true;y+=100;
            if(Button(78,y,408,70,"НАСТРОЙКИ",textSize:25,sign:4,tilt:-1.2f))app.options=true;y+=90;
            if(Button(72,y,370,66,"ВЫХОД",textSize:25,sign:5,tilt:.6f))app.Quit();
        }

        void Achievements()
        {
            Fill(0,0,1600,900,ink);
            if(premiumMenu!=null)GUI.DrawTexture(new Rect(0,0,1600,900),premiumMenu,ScaleMode.ScaleAndCrop,true);
            Fill(0,0,1600,900,StageArt.C("080C18",.86f));
            int unlockedCount=0;foreach(bool done in app.achievements)if(done)unlockedCount++;
            T(100,47,1400,66,"ДОСТИЖЕНИЯ · "+unlockedCount+" / "+app.achievements.Length,41,gold,false,TextAnchor.MiddleCenter);
            T(180,116,1240,31,"Пять достижений истории и два испытания «На бис». Сохраняются между играми.",19,cream,false,TextAnchor.MiddleCenter);
            for(int i=0;i<app.achievements.Length;i++)
            {
                float y=164+i*89;bool done=app.achievements[i];
                Panel(245,y,1110,83);
                T(262,y+12,70,58,done?"✦":"◇",37,done?gold:muted,true,TextAnchor.MiddleCenter);
                T(350,y+9,770,29,StageGame.AchievementNames[i],23,done?gold:cream,true);
                T(350,y+43,770,29,StageGame.AchievementDescriptions[i],17,cream);
                string progress=done?"ОТКРЫТО":i==2?Mathf.Min(app.lifetimeHits,67)+" / 67":i==3?Mathf.Min(app.lifetimeRicochetHits,25)+" / 25":"НЕ ОТКРЫТО";
                T(1143,y+27,184,35,progress,17,done?gold:muted,false,TextAnchor.MiddleCenter);
            }
            if(Button(608,803,384,60,"←  ГЛАВНОЕ МЕНЮ"))app.achievementMenu=false;
        }
        void AchievementToast()
        {
            Panel(1030,109,520,100);
            T(1050,122,480,27,"ДОСТИЖЕНИЕ ОТКРЫТО",17,gold,true,TextAnchor.MiddleCenter);
            T(1050,160,480,32,StageGame.AchievementNames[app.achievementToast],23,cream,true,TextAnchor.MiddleCenter);
        }

        void Levels()
        {
            Fill(0,0,1600,900,ink);
            Texture2D map=actMapPremium!=null?actMapPremium:premiumMenu;
            if(map!=null)GUI.DrawTexture(new Rect(0,0,1600,900),map,ScaleMode.ScaleAndCrop,true);
            Fill(0,0,1600,106,StageArt.C("080C18",.55f));
            T(65,24,1470,62,"КАРТА СПЕКТАКЛЯ",36,gold,false,TextAnchor.MiddleCenter);
            string[] names={"АКТ I\nСломанная сцена","АКТ II\nЗал масок","АКТ III\nПоследняя нить"};
            for(int i=0;i<3;i++)
            {
                float x=110+i*490,y=i==1?604:630;
                Fill(x+62,y-42,2,44,StageArt.C("D7AF68",.70f));
                Fill(x+293,y-42,2,44,StageArt.C("D7AF68",.70f));
                string label=i+1<=app.unlocked?names[i]:names[i].Substring(0,names[i].IndexOf('\n'))+"\nЗакрыто";
                if(Button(x,y,355,110,label,true,i+1<=app.unlocked))app.SelectAct(i+1);
            }
            if(app.EncoreUnlocked)
            {
                Panel(90,755,950,118);
                if(Button(110,776,365,76,"НА БИС · КУКЛОВОД",true,sign:4))app.BeginEncore();
                T(505,772,505,35,"Три волны · Кукловод · рекорд времени",19,cream);
                int seconds=Mathf.FloorToInt(app.encoreBestTime);
                T(505,815,505,35,app.encoreBestTime>0?"Лучшее время: "+(seconds/60).ToString("00")+":"+(seconds%60).ToString("00"):"Чуть сложнее. Ещё один выход на сцену.",20,gold);
                if(Button(1130,799,365,68,"←  ГЛАВНОЕ МЕНЮ",sign:5))app.levelMenu=false;
            }
            else if(Button(618,803,364,58,"←  ГЛАВНОЕ МЕНЮ"))app.levelMenu=false;
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

        void ShadowHealth()
        {
            int filled=Mathf.Clamp(Mathf.RoundToInt(W.Health),0,6);
            T(31,779,255,25,"ТЕНЬ",17,cream,true);
            for(int i=0;i<6;i++)
            {
                bool healed=W.HealPulse>0&&i>=4&&i<filled;
                int size=healed?Mathf.RoundToInt(30+3*Mathf.Sin(app.visualTime*8)):28;
                T(34+i*39,816,36,42,"♥",size,i<filled?(healed?Color.Lerp(rose,cream,.35f):rose):StageArt.C("5F4958"),true,TextAnchor.MiddleCenter);
            }
        }
        void TutorialHUD()
        {
            string[] titles={"Шаг 1 / 6 · ХОДЬБА","Шаг 2 / 6 · ВЫСТРЕЛ","Шаг 3 / 6 · СВЕТ","Шаг 4 / 6 · РИКОШЕТ","Шаг 5 / 6 · РЫВОК","Шаг 6 / 6 · ВОССТАНОВЛЕНИЕ","Репетиция завершена"};
            string[] steps={"Нажми WASD, чтобы сделать первые шаги по сцене.","Наведи курсор на зайца и выстрели ЛКМ.","Попади в золотой переключатель, чтобы повернуть свет.","Направь выстрел в стену тени, чтобы сделать рикошет.","Нажми Space во время движения — это рывок.","Подбери отмеченную конфету — она вернёт два сердца.","Сердца восстановлены! Теперь ты готова к первому акту."};
            int step=Mathf.Clamp(W.TutorialStep,0,6);
            string line=step==5&&W.TutorialHealthIntro>0?"Посмотри на сердца: сейчас не хватает двух.\nКонфеты восстанавливают здоровье.":steps[step];
            Panel(330,113,940,150);
            T(360,134,880,30,titles[step],23,gold,true,TextAnchor.MiddleCenter);
            T(365,175,870,65,line,24,cream,false,TextAnchor.MiddleCenter);
            if(step>=5)
            {
                Panel(24,760,452,121);ShadowHealth();
                if(step==5&&W.TutorialHealthIntro>0)
                {
                    T(63,649,365,34,"ТВОЁ ЗДОРОВЬЕ",20,gold,true,TextAnchor.MiddleCenter);
                    T(191,684,90,74,"↓",55,gold,true,TextAnchor.MiddleCenter);
                }
                else if(step==5&&W.Candies.Count>0)
                {
                    Vector2 target=new Vector2(800+W.Candies[0].Position.x*50,450-W.Candies[0].Position.y*50);
                    T(Mathf.Clamp(target.x-120,25,1335),Mathf.Clamp(target.y-121,285,710),240,35,"КОНФЕТА · +2 ♥",20,cream,true,TextAnchor.MiddleCenter);
                }
                if(W.HealPulse>0)T(66,686,345,53,"+2 ♥",34,mint,true,TextAnchor.MiddleCenter);
            }
        }

        void HUD()
        {
            if(W.Tutorial)
            {
                TutorialHUD();
                T(28,26,340,22,"РЕПЕТИЦИЯ",16,cream,true);
                if(Button(1452,20,120,44,"ПАУЗА"))app.paused=true;
                return;
            }
            ShadowHealth();
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
            T(28,26,340,22,W.Encore?"НА БИС · КУКЛОВОД":"Акт "+W.Level+" · "+(W.Tutorial?"Репетиция":"Сцена продолжается"),16,cream,true);
            if(!W.Tutorial)
            {
                Fill(1082,18,330,44,StageArt.C("0B101C",.9f));
                T(1098,26,305,22,W.Encore?"Волна "+Mathf.Max(1,W.Wave)+" / 3 · Пули "+W.PlayerShots:"Счёт "+W.Score+"  •  Волна "+Mathf.Max(1,W.Wave)+" / "+W.WaveCount,16,gold,true,TextAnchor.MiddleRight);
            }
            if(W.ChainToast>0&&W.RicochetChain>0&&W.State==PlayState.Playing)
            {
                T(526,688,548,34,W.RicochetChain>1?"СЕРИЯ РИКОШЕТОВ  ×"+W.RicochetChain:"РИКОШЕТ · ТОЧНО В ЦЕЛЬ",21,gold,true,TextAnchor.MiddleCenter);
            }
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
            T(428,214,744,86,app.paused?"АНТРАКТ":win?W.Encore?"БРАВО! НА БИС!":"КУКЛА СВОБОДНА":clear?W.Tutorial?"РЕПЕТИЦИЯ ОКОНЧЕНА":"АКТ ЗАВЕРШЁН":"ЗАНАВЕС",40,cream,true,TextAnchor.MiddleCenter);
            string line=app.paused?"Сцена подождёт.":win?W.Encore?"Ещё один выход завершён. Попробуешь улучшить своё время?":"Кукла и её тень больше никому не принадлежат.":clear?W.Tutorial?"Ты освоилась со светом и тенями. Готова выйти на сцену?":"Занавес опустился. Впереди следующий акт.":W.Health<=0?"Тень погасла. Используй укрытия, рикошеты и редкие конфеты.":"Кукла потеряна. Используй свет, чтобы отрезать врагов от неё.";
            T(434,322,732,92,line,24,gold,false,TextAnchor.MiddleCenter);
            if(!W.Tutorial)
            {
                int seconds=Mathf.FloorToInt(W.Time),accuracy=W.PlayerShots>0?Mathf.Clamp(Mathf.RoundToInt(W.EnemyHits*100f/W.PlayerShots),0,100):0;
                string[] labels={W.Encore?"ЛУЧШЕЕ ВРЕМЯ":"СЧЁТ","ВРЕМЯ","ТОЧНОСТЬ","РИКОШЕТ / СЕРИЯ"};
                int bestSeconds=Mathf.FloorToInt(app.encoreBestTime);
                string best=app.encoreBestTime>0?(bestSeconds/60).ToString("00")+":"+(bestSeconds%60).ToString("00"):"—";
                string[] values={W.Encore?best:W.Score.ToString(),(seconds/60).ToString("00")+":"+(seconds%60).ToString("00"),accuracy+"%",W.RicochetHits+" / ×"+W.BestRicochetChain};
                for(int i=0;i<4;i++)
                {
                    float x=438+i*181;
                    if(i>0)Fill(x,425,1,54,StageArt.C("D7AF68",.35f));
                    T(x+7,421,167,22,labels[i],15,gold,true,TextAnchor.MiddleCenter);
                    T(x+7,448,167,37,values[i],27,cream,false,TextAnchor.MiddleCenter);
                }
            }
            if(Button(500,502,600,86,app.paused?"ПРОДОЛЖИТЬ":clear?W.Tutorial?"ПРИСТУПИТЬ К ПЕРВОМУ АКТУ  →":"СЛЕДУЮЩИЙ АКТ  →":win?W.Encore?"ЕЩЁ РАЗ НА БИС":"НАЧАТЬ ИСТОРИЮ":W.Encore?"ПОВТОРИТЬ ИСПЫТАНИЕ":"ПОВТОРИТЬ ЭТОТ АКТ",true,sign:3))
            {if(app.paused)app.paused=false;else if(clear)app.NextLevel();else if(win){if(W.Encore)app.BeginEncore();else app.NewGame();}else app.Retry();}
            if(Button(575,612,450,76,"В ГЛАВНОЕ МЕНЮ",sign:4))app.GoTitle();
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
            if(Button(1080,252,385,65,"ЗАКРЫТЬ ЗАНАВЕС",true))app.FinishFinale();
        }

        void EncoreInvitation()
        {
            Fill(0,0,1600,900,ink);
            if(premiumFinal!=null)GUI.DrawTexture(new Rect(0,0,1600,900),premiumFinal,ScaleMode.ScaleAndCrop,true);
            Fill(0,0,1600,900,StageArt.C("080D18",.68f));Panel(350,195,900,510);
            T(400,224,800,65,app.encoreJustUnlocked?"ОТКРЫТО ИСПЫТАНИЕ «НА БИС»":"ЕЩЁ ОДИН ВЫХОД НА СЦЕНУ?",34,gold,false,TextAnchor.MiddleCenter);
            T(415,320,770,104,"Три волны на знакомой сцене и усиленный Кукловод.\nУлучши своё время и попробуй получить новые достижения.",24,cream,false,TextAnchor.MiddleCenter);
            if(Button(510,461,580,86,"ВЫЙТИ НА БИС",true,sign:4))app.BeginEncore();
            if(Button(575,578,450,76,"В ГЛАВНОЕ МЕНЮ",sign:5))app.GoTitle();
        }

        void LoadingScreen(float clock,float duration,string label)
        {
            float fade=Mathf.Min(Mathf.Clamp01(clock/.3f),Mathf.Clamp01((duration-clock)/.45f));
            Color old=GUI.color;GUI.color=new Color(1,1,1,fade);
            Fill(0,0,1600,900,StageArt.C("0B1425"));
            Texture2D scene=loadingPremium!=null?loadingPremium:loadingScene;
            if(scene!=null)GUI.DrawTexture(new Rect(0,0,1600,900),scene,ScaleMode.ScaleAndCrop,true);
            float value=Mathf.SmoothStep(0,1,Mathf.Clamp01(clock/(duration-.5f)));
            Fill(0,704,1600,196,StageArt.C("080C16",.55f));
            Fill(490,754,620,27,StageArt.C("171622"));Frame(490,754,620,27,gold);
            Fill(496,760,608*value,15,StageArt.C("EFCB88"));
            T(360,798,880,44,label,27,cream,false,TextAnchor.MiddleCenter);
            GUI.color=old;
        }

        void DuelLobby()
        {
            Fill(0,0,1600,900,ink);
            if(menuStage!=null)GUI.DrawTexture(new Rect(0,0,1600,900),menuStage,ScaleMode.ScaleAndCrop,true);
            Fill(0,0,1600,900,StageArt.C("080D18",.68f));
            for(int i=0;i<7;i++)
            {
                float x=160+i*205,y=75+(i%3)*22;
                Fill(x,0,1,y,StageArt.C("A98751",.7f));T(x-20,y-8,40,48,"✦",28,gold,false,TextAnchor.MiddleCenter);
            }
            T(100,122,1400,66,"ТЕНЬ ПРОТИВ ТЕНИ",44,cream,false,TextAnchor.MiddleCenter);
            T(180,195,1240,33,"Два дуэлянта. Одна сцена. Три победы.",23,gold,false,TextAnchor.MiddleCenter);
            Panel(90,262,660,204);
            T(126,283,580,35,"ПРИГЛАСИТЬ НА СЦЕНУ",23,gold,true);
            T(126,326,580,42,"Создай дуэль и передай другу свой IP-адрес.",21,cream);
            if(Button(166,375,490,74,"СОЗДАТЬ ДУЭЛЬ",true,sign:2))app.HostDuel();
            Panel(90,490,660,243);
            T(126,510,580,35,"ПРИСОЕДИНИТЬСЯ",23,gold,true);
            T(126,552,580,29,"IP-адрес компьютера друга",19,cream);
            Fill(126,590,270,54,StageArt.C("292435"));Frame(126,590,270,54,gold);
            app.duelIp=GUI.TextField(new Rect(139,596,244,42),app.duelIp,Style(22,cream,TextAnchor.MiddleLeft));
            if(Button(414,581,298,76,"ПОДКЛЮЧИТЬСЯ",true,sign:1))app.JoinDuel();
            T(126,679,580,32,"Оба компьютера должны быть в одной сети.",18,cream);
            Fill(812,272,706,454,StageArt.C("A98650",.5f));
            if(duelIllustration!=null)GUI.DrawTexture(new Rect(820,280,690,438),duelIllustration,ScaleMode.ScaleAndCrop,true);
            T(842,747,646,35,"Свет сменится. Роли — тоже.",22,gold,false,TextAnchor.MiddleCenter);
            if(!string.IsNullOrEmpty(app.duelError))T(96,752,650,46,app.duelError,18,rose);
            T(96,814,1030,46,"На хосте разрешите игре доступ к частной сети в брандмауэре.",17,cream);
            if(Button(1210,809,306,68,"← НАЗАД",sign:5))app.duelMenu=false;
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
            if(Event.current.type==EventType.Repaint)hoveredThisDraw=false;
            float scale=Mathf.Max(.001f,Mathf.Min(Screen.width/1600f,Screen.height/900f));Matrix4x4 old=GUI.matrix;
            mouse=new Vector2((Input.mousePosition.x-(Screen.width-1600*scale)*.5f)/scale,(Screen.height-Input.mousePosition.y-(Screen.height-900*scale)*.5f)/scale);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1600*scale)*.5f,(Screen.height-900*scale)*.5f,0),Quaternion.identity,new Vector3(scale,scale,1));
            bool wasEnabled=GUI.enabled;GUI.enabled=wasEnabled&&!app.Transitioning&&app.splashClock>=3&&!app.options;
            if(app.duel!=null)DuelOverlay();
            else if(app.storyPanel>=0)Story();
            else if(W.State==PlayState.Title){if(app.achievementMenu)Achievements();else if(app.duelMenu)DuelLobby();else if(app.levelMenu)Levels();else Title();}
            else if(app.cutscene>=0)Finale();
            else if(app.encoreOffer)EncoreInvitation();
            else
            {
                if(app.showHud||W.Tutorial)HUD();
                if(app.paused||W.State!=PlayState.Playing)Result();
            }
            if(app.achievementToast>=0&&app.achievementToastTime>0&&app.splashClock>=3&&!app.achievementMenu&&!app.options)AchievementToast();
            if(app.options && app.splashClock>=3)
            {GUI.enabled=wasEnabled&&!app.Transitioning;Settings();}
            GUI.enabled=wasEnabled;
            if(app.splashClock<.3f)Fill(0,0,1600,900,ink);
            if(app.splashClock<3)LoadingScreen(app.splashClock,3,"Готовим декорации…");
            if(app.Transitioning)
            {
                if(app.transitionFadeOnly)
                {
                    float opacity=app.transitionClock<.7f?Mathf.SmoothStep(0,1,app.transitionClock/.55f):1-Mathf.SmoothStep(0,1,(app.transitionClock-.7f)/.7f);
                    Fill(0,0,1600,900,new Color(0,0,0,opacity));
                }
                else LoadingScreen(app.transitionClock,app.TransitionDuration,app.transitionLabel);
            }
            if(Event.current.type==EventType.Repaint&&!hoveredThisDraw)lastHoveredButton="";
            GUI.matrix=old;
        }
    }
}

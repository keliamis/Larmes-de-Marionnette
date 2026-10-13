using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Backstage
{
    [DefaultExecutionOrder(-100)]
    public sealed class StageGame : MonoBehaviour
    {
        internal StageWorld world;
        StageMesh mesh;
        StageArt art;
        Camera stageCamera;
        internal Font font;
        StageUI ui;
        internal bool options,levelMenu,reducedMotion,duelMenu,achievementMenu;
        internal readonly bool[] achievements=new bool[7];
        internal bool encoreOffer,encoreJustUnlocked;
        internal static readonly string[] AchievementNames={"Ни царапины","Верный защитник","67 попаданий","Мастер рикошета","Последняя нить","Точный выход","Кукла под защитой"};
        internal static readonly string[] AchievementDescriptions={"Завершить акт без единого урона тени.","Завершить акт без единого урона кукле.","Попасть во врагов 67 раз за все игры.","Попасть во врагов рикошетом 25 раз за все игры.","Победить Кукловода и освободить куклу.","Пройти «На бис» не более чем за 75 выстрелов.","Пройти «На бис» без урона кукле."};
        internal int lifetimeHits,lifetimeRicochetHits,achievementToast=-1;
        internal float achievementToastTime;
        readonly Queue<int> achievementQueue=new Queue<int>();
        int reportedHits,reportedRicochetHits;
        internal StageDuel duel;
        internal string duelIp="192.168.1.10",duelError="";
        internal int storyPanel=-1,storyLevel=1,unlocked=1,bestScore,continueLevel=1;
        internal bool trainingDone,storyTutorial,journeyComplete;
        internal bool EncoreUnlocked {get{return achievements[4];}}
        internal float encoreBestTime;
        internal float transitionClock=-1;
        internal string transitionLabel="Готовим декорации…";
        internal bool transitionFadeOnly;
        internal float TransitionDuration {get{return transitionFadeOnly?1.4f:2.6f;}}
        Action transitionAction;
        internal bool Transitioning {get{return transitionClock>=0;}}
        internal float cutscene=-1;
        internal int storyPage;
        internal float storyClock;
        int carryScore,levelScore;
        float carryHealth=6,carryPuppet=12;
        bool resultSaved;
        AudioSource audioSource;
        AudioSource bossMusic;
        AudioSource menuMusic;
        AudioSource combatMusic;
        AudioClip healChime;
        readonly Dictionary<int,AudioClip> sounds=new Dictionary<int,AudioClip>();
        internal bool paused, muted, showHud=true;
        float accumulator, noticeUntil, uiSoundUntil;
        internal float visualTime, splashClock;
        string notice="";
        Vector2 cursor;
        bool pendingDash;
        public string ScreenshotDirectory { get { return Path.Combine(Application.persistentDataPath,"Screenshots"); } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if(FindAnyObjectByType<StageGame>()==null) new GameObject("Backstage Game").AddComponent<StageGame>();
        }
        void Awake()
        {
            Application.targetFrameRate=60;
            world=new StageWorld();world.UpdateBarriers();world.Sound=PlayTone;
            // Use the operating system font: the project does not redistribute typeface files.
            try {font=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Cambria","Palatino Linotype","DejaVu Serif","Liberation Serif","Arial"},22);}
            catch(Exception e) {Debug.LogWarning("Backstage: system font unavailable: "+e.Message);}
            if(font==null)
            {
                try {font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");}
                catch(Exception e) {Debug.LogWarning("Backstage: built-in font fallback unavailable: "+e.Message);}
            }
            ui=new StageUI(this);
            unlocked=Mathf.Clamp(PlayerPrefs.GetInt("backstage.unlocked",1),1,3);bestScore=PlayerPrefs.GetInt("backstage.best",0);
            continueLevel=Mathf.Clamp(PlayerPrefs.GetInt("backstage.continue",unlocked),1,unlocked);
            trainingDone=PlayerPrefs.GetInt("backstage.training",0)==1;muted=PlayerPrefs.GetInt("backstage.muted",0)==1;
            journeyComplete=PlayerPrefs.GetInt("backstage.completed",PlayerPrefs.GetInt("backstage.achievement.4",0)==1&&trainingDone?1:0)==1;
            reducedMotion=false;
            lifetimeHits=PlayerPrefs.GetInt("backstage.hits",0);lifetimeRicochetHits=PlayerPrefs.GetInt("backstage.ricochetHits",0);
            for(int i=0;i<achievements.Length;i++)achievements[i]=PlayerPrefs.GetInt("backstage.achievement."+i,0)==1;
            encoreBestTime=PlayerPrefs.GetFloat("backstage.encore.waves.bestTime",0);
            var cameraObject=new GameObject("Stage Camera");
            stageCamera=cameraObject.AddComponent<Camera>();
            stageCamera.orthographic=true;stageCamera.orthographicSize=9;
            stageCamera.transform.position=new Vector3(0,0,-10);
            stageCamera.clearFlags=CameraClearFlags.SolidColor;stageCamera.backgroundColor=StageArt.C("0B101A");
            stageCamera.nearClipPlane=.1f;stageCamera.farClipPlane=50;
            cameraObject.AddComponent<AudioListener>();
            var artObject=new GameObject("Procedural stage");
            mesh=artObject.AddComponent<StageMesh>();mesh.Initialize();art=new StageArt(mesh);
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;
            healChime=Resources.Load<AudioClip>("HealChime");world.HealSound=PlayHealChime;
            bossMusic=gameObject.AddComponent<AudioSource>();bossMusic.playOnAwake=false;bossMusic.loop=true;
            bossMusic.clip=Resources.Load<AudioClip>("BossTheme");bossMusic.volume=0;bossMusic.spatialBlend=0;
            menuMusic=gameObject.AddComponent<AudioSource>();menuMusic.playOnAwake=false;menuMusic.loop=true;
            menuMusic.clip=Resources.Load<AudioClip>("MenuAmbience");menuMusic.volume=0;menuMusic.spatialBlend=0;
            combatMusic=gameObject.AddComponent<AudioSource>();combatMusic.playOnAwake=false;combatMusic.loop=true;
            combatMusic.clip=Resources.Load<AudioClip>("CombatTheme");combatMusic.volume=0;combatMusic.spatialBlend=0;
            world.Enemies.Add(new Enemy {Kind=EnemyKind.Rabbit,Position=new Vector2(-7,2),Health=3,MaxHealth=3});
            world.Enemies.Add(new Enemy {Kind=EnemyKind.Mask,Position=new Vector2(7,1),Health=7,MaxHealth=7});
            world.Player=new Vector2(-4,-2.5f);
        }
        void Update()
        {
            visualTime+=Time.unscaledDeltaTime;splashClock+=Time.unscaledDeltaTime;
            if(Transitioning)
            {
                transitionClock+=Time.unscaledDeltaTime;
                if(transitionClock>=(transitionFadeOnly? .7f:1.5f)&&transitionAction!=null){Action action=transitionAction;transitionAction=null;action();}
                if(transitionClock>=TransitionDuration)transitionClock=-1;
            }
            float aspect=(float)Screen.width/Mathf.Max(1,Screen.height);
            if(aspect>16f/9) stageCamera.rect=new Rect((1-16f/9/aspect)*.5f,0,16f/9/aspect,1);
            else stageCamera.rect=new Rect(0,(1-aspect/(16f/9))*.5f,1,aspect/(16f/9));
            cursor=stageCamera.ScreenToWorldPoint(Input.mousePosition);
            if(Input.GetKeyDown(KeyCode.M)){muted=!muted;SaveSettings();}
            if(Input.GetKeyDown(KeyCode.F1)) showHud=!showHud;
            if(Input.GetKeyDown(KeyCode.F12)) StartCoroutine(CaptureOne());
            if(cutscene>=0)cutscene=Mathf.Min(10.5f,cutscene+Time.unscaledDeltaTime);
            if(!Transitioning&&splashClock>=3&&Input.GetKeyDown(KeyCode.Escape))
            {if(duel!=null){LeaveDuel();}else if(achievementMenu)achievementMenu=false;else if(duelMenu)duelMenu=false;else if(options)options=false;else if(storyPanel>=0)GoTitle();else if(cutscene>=8.5f)FinishFinale();else if(encoreOffer)GoTitle();else if(world.State==PlayState.Playing)paused=!paused;}
            if(!Transitioning&&splashClock>=3&&Input.GetKeyDown(KeyCode.Return))
            {if(storyPanel>=0)AdvanceStory();else if(cutscene>=8.5f)FinishFinale();}
            if(!Transitioning&&Input.GetKeyDown(KeyCode.R)&&storyPanel<0&&(paused||world.State==PlayState.Lost))Retry();
            if(!Transitioning&&Input.GetKeyDown(KeyCode.Space)) pendingDash=true;
            if(duel!=null&&!Transitioning)
            {
                Vector2 move=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),
                    (Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));
                Vector2 origin=duel.IsHost?duel.HostPosition:duel.GuestPosition;
                duel.Tick(Time.unscaledDeltaTime,new ShotInput{Move=move,Aim=cursor-origin,Fire=Input.GetMouseButton(0),Dash=pendingDash});
                pendingDash=false;accumulator=0;
            }
            else if(!Transitioning&&world.State==PlayState.Playing && !paused && !options && storyPanel<0)
            {
                Vector2 move=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),
                    (Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));
                Vector2 aim=cursor-world.Player;
                bool fire=Input.GetMouseButton(0);
                accumulator=Mathf.Min(accumulator+Time.unscaledDeltaTime,.15f);
                while(accumulator>=1f/60)
                {
                    ShotInput stepInput=new ShotInput {Move=move,Aim=aim,Fire=fire,Dash=pendingDash};
                    stepInput=LimitTutorialInput(stepInput);
                    world.Step(1f/60,stepInput);
                    pendingDash=false; accumulator-=1f/60;
                }
            }
            else { accumulator=0; pendingDash=false; }
            UpdateAchievementCounters();
            if(!resultSaved&&(world.State==PlayState.LevelClear||world.State==PlayState.Won))
            {
                resultSaved=true;
                if(world.Encore)
                {
                    if(world.State==PlayState.Won)
                    {
                        if(world.PlayerShots<=75)UnlockAchievement(5);
                        if(world.PuppetDamageTaken==0)UnlockAchievement(6);
                    }
                    if(world.State==PlayState.Won&&(encoreBestTime<=0||world.Time<encoreBestTime)){encoreBestTime=world.Time;PlayerPrefs.SetFloat("backstage.encore.waves.bestTime",encoreBestTime);}
                }
                else if(world.Tutorial){trainingDone=true;PlayerPrefs.SetInt("backstage.training",1);}
                else{encoreJustUnlocked=world.State==PlayState.Won&&!EncoreUnlocked;RecordActAchievements();unlocked=Mathf.Max(unlocked,Mathf.Min(3,world.Level+1));continueLevel=Mathf.Min(3,world.Level+1);PlayerPrefs.SetInt("backstage.unlocked",unlocked);PlayerPrefs.SetInt("backstage.continue",continueLevel);PlayerPrefs.SetInt("backstage.checkpointScore",world.Score);bestScore=Mathf.Max(bestScore,world.Score);PlayerPrefs.SetInt("backstage.best",bestScore);}
                if(world.State==PlayState.Won&&!world.Encore){cutscene=0;journeyComplete=true;PlayerPrefs.SetInt("backstage.completed",1);}
                PlayerPrefs.Save();
            }
            UpdateAchievementToast();UpdateBossMusic();UpdateMenuMusic();UpdateCombatMusic();
            Vector2 shake=!reducedMotion&&world.State==PlayState.Playing && !paused?new Vector2(Mathf.Sin(visualTime*79),Mathf.Cos(visualTime*63))*world.Shake*.12f:Vector2.zero;
            stageCamera.transform.position=new Vector3(shake.x,shake.y,-10);
            if(duel!=null)art.DrawDuel(duel,reducedMotion?0:visualTime,cursor);
            else art.Draw(world,reducedMotion?0:visualTime,cursor,world.State==PlayState.Playing && !paused&&storyPanel<0,cutscene);
        }
        ShotInput LimitTutorialInput(ShotInput input)
        {
            if(world==null || !world.Tutorial) return input;
            switch(world.TutorialStep)
            {
                case 0:
                    input.Fire=false;input.Dash=false;input.Aim=Vector2.right;break;
                case 1:
                    input.Move=Vector2.zero;input.Dash=false;break;
                case 2:
                    input.Move=Vector2.zero;input.Dash=false;break;
                case 3:
                    input.Dash=false;break;
                case 4:
                    input.Fire=false;break;
                case 5:
                    input.Fire=false;input.Dash=false;if(world.TutorialHealthIntro>0)input.Move=Vector2.zero;break;
            }
            return input;
        }

        internal void BeginStory(int level,bool tutorial=false,bool carry=false)
        {
            storyLevel=Mathf.Clamp(level,1,unlocked);storyTutorial=tutorial;storyPage=0;storyClock=0;
            carryScore=carry?world.Score:0;carryHealth=6;carryPuppet=12;
            storyPanel=storyLevel==1?0:storyLevel==2?1:2;paused=false;options=false;levelMenu=false;duelMenu=false;achievementMenu=false;
        }
        internal void BeginJourney()
        {
            if(Transitioning)return;
            if(journeyComplete){NewGame();return;}
            carryScore=trainingDone?PlayerPrefs.GetInt("backstage.checkpointScore",0):0;carryHealth=6;carryPuppet=12;
            if(trainingDone)StartLevel(continueLevel,false);
            else TransitionTo(()=>BeginStory(1,true),"",true);
        }
        internal void NewGame()
        {
            trainingDone=false;journeyComplete=false;unlocked=1;continueLevel=1;
            PlayerPrefs.SetInt("backstage.completed",0);
            PlayerPrefs.SetInt("backstage.training",0);
            PlayerPrefs.SetInt("backstage.unlocked",1);
            PlayerPrefs.SetInt("backstage.continue",1);
            PlayerPrefs.SetInt("backstage.checkpointScore",0);
            PlayerPrefs.Save();
            BeginJourney();
        }
        internal void AdvanceStory()
        {
            storyClock=0;
            if(storyLevel==1 && storyPage<3){storyPage++;return;}
            StartLevel(storyLevel,storyTutorial);
        }
        internal void TransitionTo(Action action,string label,bool fadeOnly=false)
        {
            if(Transitioning)return;
            transitionClock=0;transitionAction=action;transitionLabel=label;transitionFadeOnly=fadeOnly;accumulator=0;pendingDash=false;
        }
        internal void StartLevel(int level,bool tutorial=false,bool encore=false)
        {
            TransitionTo(()=>StartLevelNow(level,tutorial,encore),encore?"Готовим выход на бис…":tutorial?"Готовим репетицию…":"Готовим декорации · акт "+(level==1?"I":level==2?"II":"III")+"…");
        }
        void StartLevelNow(int level,bool tutorial,bool encore)
        {
            if(!tutorial&&!encore){continueLevel=Mathf.Clamp(level,1,3);PlayerPrefs.SetInt("backstage.continue",continueLevel);PlayerPrefs.SetInt("backstage.checkpointScore",carryScore);PlayerPrefs.Save();}
            world.Start(level,tutorial,encore);world.Score=tutorial||encore?0:carryScore;levelScore=world.Score;
            reportedHits=0;reportedRicochetHits=0;achievementMenu=false;levelMenu=false;duelMenu=false;
            if(!tutorial){world.Health=carryHealth;world.PuppetHealth=carryPuppet;}
            storyPanel=-1;storyClock=0;cutscene=-1;encoreOffer=false;paused=false;options=false;accumulator=0;pendingDash=false;resultSaved=false;
        }
        internal void NextLevel()
        {
            if(world.Tutorial){carryScore=0;carryHealth=6;carryPuppet=12;StartLevel(1,false);}
            else{carryScore=world.Score;carryHealth=6;carryPuppet=12;StartLevel(world.Level+1,false);}
        }
        internal void SelectAct(int level)
        {
            carryScore=0;carryHealth=6;carryPuppet=12;levelMenu=false;
            StartLevel(Mathf.Clamp(level,1,unlocked),false);
        }
        internal void BeginEncore()
        {
            if(!EncoreUnlocked||Transitioning)return;
            carryScore=0;carryHealth=6;carryPuppet=12;
            StartLevel(3,false,true);
        }
        internal void Retry(){carryScore=levelScore;carryHealth=6;carryPuppet=12;StartLevel(world.Level,world.Tutorial,world.Encore);}
        internal void FinishFinale(){cutscene=-1;encoreOffer=EncoreUnlocked;}
        internal void GoTitle(){encoreOffer=false;world.State=PlayState.Title;storyPanel=-1;storyClock=0;cutscene=-1;paused=false;options=false;levelMenu=false;duelMenu=false;achievementMenu=false;LeaveDuel();PlayerPrefs.Save();}
        internal void HostDuel()
        {
            try{LeaveDuel();duel=StageDuel.Host();duelError="";duelMenu=false;paused=false;world.State=PlayState.Title;}
            catch(Exception e){duelError="Не удалось создать матч: "+e.Message;}
        }
        internal void JoinDuel()
        {
            try{LeaveDuel();duel=StageDuel.Join(duelIp);duelError="";duelMenu=false;paused=false;world.State=PlayState.Title;}
            catch(Exception e){duelError="Подключение: "+e.Message;}
        }
        internal void LeaveDuel(){if(duel!=null){duel.Dispose();duel=null;}}
        internal void SaveSettings(){PlayerPrefs.SetInt("backstage.muted",muted?1:0);PlayerPrefs.SetInt("backstage.motion",reducedMotion?1:0);PlayerPrefs.Save();}
        internal void Quit(){
            PlayerPrefs.Save();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
            #else
            Application.Quit();
            #endif
        }

        void UpdateAchievementCounters()
        {
            if(world.Tutorial||world.Encore)return;
            int hits=Mathf.Max(0,world.EnemyHits-reportedHits),ricochets=Mathf.Max(0,world.RicochetHits-reportedRicochetHits);
            reportedHits=world.EnemyHits;reportedRicochetHits=world.RicochetHits;
            if(hits>0){lifetimeHits+=hits;PlayerPrefs.SetInt("backstage.hits",lifetimeHits);}
            if(ricochets>0){lifetimeRicochetHits+=ricochets;PlayerPrefs.SetInt("backstage.ricochetHits",lifetimeRicochetHits);}
            if(lifetimeHits>=67)UnlockAchievement(2);
            if(lifetimeRicochetHits>=25)UnlockAchievement(3);
        }
        void RecordActAchievements()
        {
            if(world.PlayerDamageTaken==0)UnlockAchievement(0);
            if(world.PuppetDamageTaken==0)UnlockAchievement(1);
            if(world.State==PlayState.Won)UnlockAchievement(4);
        }
        void UnlockAchievement(int index)
        {
            if(achievements[index])return;
            achievements[index]=true;PlayerPrefs.SetInt("backstage.achievement."+index,1);
            achievementQueue.Enqueue(index);PlayerPrefs.Save();
        }
        void UpdateAchievementToast()
        {
            achievementToastTime=Mathf.Max(0,achievementToastTime-Time.unscaledDeltaTime);
            if(achievementToastTime<=0&&achievementQueue.Count>0){achievementToast=achievementQueue.Dequeue();achievementToastTime=3.5f;}
        }
        internal void PlayButtonSound(bool click)
        {
            if(muted||audioSource==null)return;
            if(!click&&Time.unscaledTime<uiSoundUntil)return;
            uiSoundUntil=Time.unscaledTime+.09f;
            PlayTone(click?310:560,click? .075f:.045f,click? .023f:.012f);
        }
        void PlayHealChime()
        {
            if(muted)return;
            if(healChime!=null)audioSource.PlayOneShot(healChime,.045f);
            else PlayTone(1046,.35f,.13f);
        }
        void UpdateBossMusic()
        {
            if(bossMusic==null||bossMusic.clip==null)return;
            bool bossPresent=!Transitioning&&duel==null&&!world.Tutorial&&world.State==PlayState.Playing&&storyPanel<0&&world.Enemies.Exists(e=>e.Kind==EnemyKind.Puppeteer);
            if(bossPresent&&!bossMusic.isPlaying)bossMusic.Play();
            bossMusic.mute=muted;
            float target=(bossPresent && !paused && !options) ? .17f : 0f;
            bossMusic.volume=Mathf.MoveTowards(bossMusic.volume,target,Time.unscaledDeltaTime*.45f);
            if(!bossPresent&&bossMusic.volume<=.001f&&bossMusic.isPlaying)bossMusic.Stop();
        }
        void UpdateCombatMusic()
        {
            if(combatMusic==null||combatMusic.clip==null)return;
            bool active=duel==null&&!world.Tutorial&&world.State==PlayState.Playing&&storyPanel<0&&cutscene<0&&!Transitioning&&!world.Enemies.Exists(e=>e.Kind==EnemyKind.Puppeteer);
            if(active&&!combatMusic.isPlaying)combatMusic.Play();
            combatMusic.mute=muted;
            float target=active&&!paused&&!options? .10f:0;
            combatMusic.volume=Mathf.MoveTowards(combatMusic.volume,target,Time.unscaledDeltaTime*.25f);
            if(!active&&combatMusic.volume<=.001f&&combatMusic.isPlaying)combatMusic.Stop();
        }
        void UpdateMenuMusic()
        {
            if(menuMusic==null||menuMusic.clip==null)return;
            bool active=duel==null&&(Transitioning||encoreOffer||world.State==PlayState.Title||storyPanel>=0||cutscene>=0);
            if(active&&!menuMusic.isPlaying)menuMusic.Play();
            menuMusic.mute=muted;
            float target=active&&!paused ? .13f : 0f;
            menuMusic.volume=Mathf.MoveTowards(menuMusic.volume,target,Time.unscaledDeltaTime*.18f);
            if(!active&&menuMusic.volume<=.001f&&menuMusic.isPlaying)menuMusic.Stop();
        }

        void PlayTone(float frequency,float duration,float volume)
        {
            if(muted) return;
            int key=Mathf.RoundToInt(frequency)*1000+Mathf.RoundToInt(duration*1000);
            AudioClip clip;
            if(!sounds.TryGetValue(key,out clip))
            {
                int count=Mathf.CeilToInt(22050*duration);float[] data=new float[count];
                for(int i=0;i<count;i++)
                {
                    float t=(float)i/22050,env=Mathf.Sin(Mathf.PI*i/count)*Mathf.Exp(-4f*i/count);
                    data[i]=Mathf.Sin(2*Mathf.PI*frequency*t*(1-.3f*i/count))*env;
                }
                clip=AudioClip.Create("stage-tone-"+key,count,1,22050,false);clip.SetData(data,0);sounds.Add(key,clip);
            }
            audioSource.PlayOneShot(clip,volume);
        }
        void OnGUI() {if(world!=null&&ui!=null)ui.Draw();}
        void OnApplicationFocus(bool focus){if(!focus){if(world!=null&&world.State==PlayState.Playing)paused=true;PlayerPrefs.Save();}}
        void OnApplicationQuit(){PlayerPrefs.Save();}
        IEnumerator CaptureOne()
        {
            yield return new WaitForEndOfFrame();
            SaveFrame("Gameplay_"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+".png");
        }
        bool SaveFrame(string filename)
        {
            Texture2D image=null;
            try
            {
                Directory.CreateDirectory(ScreenshotDirectory);
                image=ScreenCapture.CaptureScreenshotAsTexture();
                string path=Path.Combine(ScreenshotDirectory,filename);
                File.WriteAllBytes(path,image.EncodeToPNG());
                notice="Скриншот сохранён: "+path;noticeUntil=Time.unscaledTime+8;Debug.Log(notice);
                return true;
            }
            catch(Exception e) { notice="Ошибка сохранения: "+e.Message;noticeUntil=Time.unscaledTime+10;Debug.LogException(e);return false; }
            finally {if(image!=null)Destroy(image);}
        }
        void OnDestroy() {PlayerPrefs.Save();LeaveDuel();if(ui!=null)ui.Dispose();foreach(AudioClip c in sounds.Values)Destroy(c);/* Built-in font may be shared; never destroy it here. */}
    }
}

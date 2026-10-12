using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Backstage.Editor
{
    [InitializeOnLoad]
    public static class BackstageTools
    {
        const string ScenePath="Assets/Backstage/Scenes/Backstage.unity";
        static BackstageTools() { EditorApplication.delayCall+=EnsureAssets; }
        static void EnsureAssets()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(AssetDatabase.LoadAssetAtPath<Material>("Assets/Backstage/Resources/StageMaterial.mat")==null)
            {
                var material=new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/Backstage/Resources/StageShader.shader"));
                AssetDatabase.CreateAsset(material,"Assets/Backstage/Resources/StageMaterial.mat");
            }
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            PlayerSettings.companyName="BackstageStudio";
            PlayerSettings.productName="За кулисами";PlayerSettings.bundleVersion="2.1.1";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            AssetDatabase.SaveAssets();
            if(!SessionState.GetBool("Backstage6000.SceneOpened",false))
            {
                SessionState.SetBool("Backstage6000.SceneOpened",true);
                var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if(string.IsNullOrEmpty(active.path) && !active.isDirty) EditorSceneManager.OpenScene(ScenePath);
            }
        }
        [MenuItem("Backstage/Play",priority=1)]
        public static void Play(){EnsureAssets();EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);EditorApplication.isPlaying=true;}
        [MenuItem("Backstage/Проверить иллюстрации", priority=2)]
        public static void CheckStoryArt()
        {
            int good=0;
            for(int i=0;i<4;i++)
            {
                string path="Assets/Backstage/Resources/StoryPanel"+i+".bytes";
                TextAsset panel=AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if(panel!=null&&panel.bytes!=null&&panel.bytes.Length>1000)
                {
                    good++;
                    Debug.Log("Backstage: панель "+i+" доступна ("+panel.bytes.Length+" байт)");
                }
                else Debug.LogError("Backstage: нет иллюстрации "+path+". Пожалуйста, повторно распакуйте весь архив.");
            }
            Debug.Log("Backstage: обнаружено "+good+" из 4 иллюстраций. Запустите Play для проверки отображения.");
        }
        [MenuItem("Backstage/Build Windows")]
        public static void BuildWindows(){BuildTargetProject(BuildTarget.StandaloneWindows64);}
        [MenuItem("Backstage/Build macOS")]
        public static void BuildMac(){BuildTargetProject(BuildTarget.StandaloneOSX);}
        [MenuItem("Backstage/Open Scene")]
        public static void OpenScene()
        {
            if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("Backstage/Open Screenshots Folder")]
        public static void Screenshots()
        {
            string path=Path.Combine(Application.persistentDataPath,"Screenshots");
            Directory.CreateDirectory(path);EditorUtility.RevealInFinder(path);
        }
        [MenuItem("Backstage/Build for Current Platform")]
        public static void Build()
        {
            BuildTargetProject(EditorUserBuildSettings.activeBuildTarget);
        }
        static void BuildTargetProject(BuildTarget target)
        {
            EnsureAssets();
            if(target!=BuildTarget.StandaloneOSX && target!=BuildTarget.StandaloneWindows64 && target!=BuildTarget.StandaloneLinux64)
                throw new Exception("Choose Windows, macOS or Linux in Build Settings first.");
            string name=target==BuildTarget.StandaloneOSX?"Backstage.app":target==BuildTarget.StandaloneWindows64?"Backstage.exe":"Backstage.x86_64";
            Directory.CreateDirectory("Builds");
            var report=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/"+name,target,BuildOptions.None);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Backstage build failed; see Console.");
        }
    }
}

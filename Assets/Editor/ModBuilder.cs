using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;

class SteamService
{
    bool _init = false;
    public void Init()
    {
        if (!_init)
        {
            _init = SteamAPI.Init();
        }
    }

    public void Logout()
    {
        if (_init)
        {
            SteamAPI.ReleaseCurrentThreadMemory();
            SteamAPI.Shutdown();
            _init = false;
        }
    }


    public void Update()
    {
        if (_init)
        {
            SteamAPI.RunCallbacks();
        }
    }

    public uint GetAppId()
    {
        if (_init)
        {
            return SteamUtils.GetAppID().m_AppId;
        }
        else
        {
            return AppId_t.Invalid.m_AppId;
        }
    }

    public bool IsSign()
    {
        return _init && SteamAPI.IsSteamRunning();
    }
}

public class ModBuilder : EditorWindow
{
    private CallResult<SteamUGCQueryCompleted_t> OnSteamUGCQueryCompletedCallResult;
    private CallResult<CreateItemResult_t> OnCreateItemResultCallResult;
    private CallResult<SubmitItemUpdateResult_t> OnSubmitItemUpdateResultCallResult;

    private List<SteamUGCDetails_t> _modList = new List<SteamUGCDetails_t>();
    private int _modIndex = -1;

    SteamService steam = new SteamService();
   // string _modName = typeof(ModEntryPoint).Assembly.GetName().Name;
    [MenuItem("Game/Build Mod")]
    static void BuildMod()
    {
        var window = EditorWindow.GetWindow<ModBuilder>();
        window.ShowWindow();
    }

    public void ShowWindow()
    {
        base.Show();
    }

    void RequestInfo()
    {
        steam.Init();
        if (steam.IsSign())
        {
            OnSteamUGCQueryCompletedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
            SteamAPICall_t handle = SteamUGC.SendQueryUGCRequest(SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), 1));
            OnSteamUGCQueryCompletedCallResult.Set(handle);
            OnSubmitItemUpdateResultCallResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdateResult);
            OnCreateItemResultCallResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
        }
    }

    void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t pCallback, bool bIOFailure)
    {
        string msg = "[" + SubmitItemUpdateResult_t.k_iCallback + " - SubmitItemUpdateResult] - " + pCallback.m_eResult + " -- " + pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement + " -- " + pCallback.m_nPublishedFileId;
        Debug.Log(msg);
        EditorUtility.DisplayDialog("Info", msg, "OK");
    }

    void OnCreateItemResult(CreateItemResult_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + CreateItemResult_t.k_iCallback + " - CreateItemResult] - " + pCallback.m_eResult + " -- " + pCallback.m_nPublishedFileId + " -- " + pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement);

        SteamUGCDetails_t details = new SteamUGCDetails_t
        {
            m_nPublishedFileId = pCallback.m_nPublishedFileId
        };
        _modList.Add(details);
        _modIndex = _modList.Count - 1;
    }


    void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + SteamUGCQueryCompleted_t.k_iCallback + " - SteamUGCQueryCompleted] - " + pCallback.m_handle + " -- " + pCallback.m_eResult + " -- " + pCallback.m_unNumResultsReturned + " -- " + pCallback.m_unTotalMatchingResults + " -- " + pCallback.m_bCachedData);

        for (uint i = 0; i < pCallback.m_unNumResultsReturned; i++)
        {
            bool ret = SteamUGC.GetQueryUGCResult(pCallback.m_handle, i, out SteamUGCDetails_t details);
            _modList.Add(details);
        }
    }

    private void Update()
    {
        steam.Update();
    }

    const string PATH_TO_ASSETS = "/assets";
    const string PATH_BUILD_BUNDLE = "Temp/ModBuild";

    private void OnGUI()
    {

        string modName = typeof(ModEntryPoint).Assembly.GetName().Name;
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Mod Name", modName);
        if(GUILayout.Button("Change"))
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/Scripts/MyMod.asmdef");
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("BUILD"))
        {
            if (modName.Length > 0)
            {
                if (Directory.Exists(PATH_BUILD_BUNDLE))
                {
                    Directory.Delete(PATH_BUILD_BUNDLE, true);
                }

                if (Directory.Exists(PATH_BUILD_BUNDLE))
                {
                    throw new System.Exception("Temp/ModBuild exist");
                }

                Directory.CreateDirectory(PATH_BUILD_BUNDLE);

                //HACK for unique id
                AssetImporter.GetAtPath("Assets/Resources").SetAssetBundleNameAndVariant(modName + "_resources", "");
                AssetDatabase.Refresh();

                BuildPipeline.BuildAssetBundles(PATH_BUILD_BUNDLE, BuildAssetBundleOptions.None/*BuildAssetBundleOptions.DisableWriteTypeTree*/, BuildTarget.StandaloneWindows64);

                //copy dll
                string modsFolder = Application.persistentDataPath + "/../../AtomTeam/Atom/Mods";

                if (!Directory.Exists(modsFolder))
                {
                    Directory.CreateDirectory(modsFolder);
                }

                Copy("Library/ScriptAssemblies/" + modName + ".dll", modsFolder + "/" + modName + ".dll");
                Copy("Library/ScriptAssemblies/" + modName + ".pdb", modsFolder + "/" + modName + ".pdb");

                //copy res
                string modResFolder = modsFolder;

                string dataAsset = Application.dataPath;
                int index = dataAsset.ToLower().IndexOf(PATH_TO_ASSETS);
                dataAsset = dataAsset.Remove(index, PATH_TO_ASSETS.Length);

                var resPath = dataAsset + "/Temp/ModBuild/" + modName + "_resources";
                if (File.Exists(resPath))
                {
                    Copy(resPath, modResFolder + "/" + modName + "_resources");
                }
                Copy("Library/ScriptAssemblies/" + modName + ".dll", "Temp/ModBuild/" + modName + ".dll");
                Copy("Library/ScriptAssemblies/" + modName + ".pdb", "Temp/ModBuild/" + modName + ".pdb");


                EditorUtility.RevealInFinder(modsFolder + "/" + modName + ".dll");
            }
        }

        GUILayout.Space(50);

        GUILayout.Label("Publish Settings", EditorStyles.boldLabel);

        if (steam.IsSign())
        {
            EditorGUILayout.LabelField("App Id", steam.GetAppId().ToString());

            if (_modIndex < 0)
            {
                for (int i = 0; i != _modList.Count; ++i)
                {
                    if (GUILayout.Button("Open Mod Item(" + _modList[i].m_rgchTitle + ")"))
                    {
                        _modIndex = i;
                    }
                }

                if (GUILayout.Button("Create New Mod Item"))
                {
                    SteamAPICall_t handle = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                    OnCreateItemResultCallResult.Set(handle);
                }
            }
            else
            {
                SteamUGCDetails_t details = _modList[_modIndex]; //copy temp
                EditorGUILayout.LabelField("Mod Id", details.m_nPublishedFileId.ToString());
                details.m_rgchTitle = EditorGUILayout.TextField("Title", details.m_rgchTitle);
                details.m_rgchDescription = EditorGUILayout.TextField("Description", details.m_rgchDescription);
                details.m_eVisibility = (ERemoteStoragePublishedFileVisibility)EditorGUILayout.EnumPopup(details.m_eVisibility);
                _modList[_modIndex] = details; //assign

                if (GUILayout.Button("Upload details"))
                {
                    var handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), details.m_nPublishedFileId);
                    SteamUGC.SetItemTitle(handle, details.m_rgchTitle);
                    SteamUGC.SetItemDescription(handle, details.m_rgchDescription);
                    SteamUGC.SetItemVisibility(handle, details.m_eVisibility);
                    SteamAPICall_t callHandle = SteamUGC.SubmitItemUpdate(handle, "");
                    OnSubmitItemUpdateResultCallResult.Set(callHandle);
                }

                GUILayout.Space(20);

                EditorGUILayout.HelpBox("Select and upload preview image to Steam", MessageType.Info);

                if (GUILayout.Button("Upload preview image"))
                {
                    var handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), details.m_nPublishedFileId);
                    SteamUGC.SetItemPreview(handle, EditorUtility.OpenFilePanel("Preview mod image", "", "png"));
                    SteamAPICall_t callHandle = SteamUGC.SubmitItemUpdate(handle, "");
                    OnSubmitItemUpdateResultCallResult.Set(callHandle);
                }

                GUILayout.Space(20);

                EditorGUILayout.HelpBox("Upload pre built mod content to Steam", MessageType.Info);

                if (GUILayout.Button("Upload content"))
                {
                    var handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), details.m_nPublishedFileId);

                    string dataAsset = Application.dataPath;
                    int index = dataAsset.ToLower().IndexOf(PATH_TO_ASSETS);
                    dataAsset = dataAsset.Remove(index, PATH_TO_ASSETS.Length);

                    string modsFolder = dataAsset + "/" + PATH_BUILD_BUNDLE;

                    SteamUGC.SetItemContent(handle, modsFolder);
                    SteamAPICall_t callHandle = SteamUGC.SubmitItemUpdate(handle, "");
                    OnSubmitItemUpdateResultCallResult.Set(callHandle);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Attach to Steam account", MessageType.Warning);

            if (GUILayout.Button("Attach"))
            {
                RequestInfo();
            }
        }
    }

    void Copy(string src, string dst)
    {
        Debug.Log("Copy " + src + " -> " + dst);
        File.Copy(src, dst, true);
    }

    void OnDestroy()
    {
        steam.Logout();
    }
}

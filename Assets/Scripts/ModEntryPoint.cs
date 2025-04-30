using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using JSon;
using System.Reflection;
using System.Runtime.CompilerServices;

//[assembly: AssemblyTitle("My Mod")] // ENTER MOD TITLE

namespace OnlineEvents
{
    public struct Login
    {
        public string sessionKey;
        public int uid;
        public int world_tile;
        public int cell;
        public JSon.JNode data;
    }

    public struct Travel
    {
        public int dir;
    }

    public struct EncounterBegin
    {
    }

    public struct EncounterEnd
    {
    }

    public struct ChatSend
    {
        public string msg;
    }

    public struct ChatGet
    {
        public string msg;
        public int initiator;
    }
}

public class ModEntryPoint : MonoBehaviour // ModEntryPoint - RESERVED LOOKUP NAME
{
    private bool _inBattle = false;
    private bool _signed = false;
    private bool _fetch = false;
    private int _uid = 0;
    private string _sessionKey = string.Empty;
    private int _world_tile = 0;
    private int _lastActionId = 0;
    public GameObject _loginForm;
    private GameObject _inGameForm;
    Dictionary<int, CharacterComponent> _characters = new Dictionary<int, CharacterComponent>();

    public static string server = "http://online.theatomgame.com/";

    void Start()
    {
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");
#if !UNITY_EDITOR
        ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));
#endif
        GlobalEvents.AddListener<GlobalEvents.GameStart>(GameLoaded);
        GlobalEvents.AddListener<GlobalEvents.LevelLoaded>(LevelLoaded);

        GlobalEvents.AddListener<OnlineEvents.Login>(OnLogin);
        GlobalEvents.AddListener<GlobalEvents.LevelLoaded>(OnLevelLoaded);
        GlobalEvents.AddListener<GlobalEvents.CharacterMove>(OnCharacterMove);
        GlobalEvents.AddListener<GlobalEvents.CharacterTurnEnd>(OnCharacterTurnEnd);
        GlobalEvents.AddListener<GlobalEvents.CharacterOnAttack>(OnCharacterOnAttack);
        GlobalEvents.AddListener<OnlineEvents.ChatSend>(OnChat);
        GlobalEvents.AddListener<OnlineEvents.Travel>(OnTravel);
        GlobalEvents.AddListener<OnlineEvents.EncounterBegin>(ToEncounterBegin);
        GlobalEvents.AddListener<OnlineEvents.EncounterEnd>(ToEncounterEnd);

        _loginForm = ResourceManager.Load<GameObject>("LoginForm", ResourceManager.EXT_PREFAB);

        StartCoroutine(ActionPollLoop());
    }

    void GameLoaded(GlobalEvents.GameStart evnt)
    {
        //Localization.LoadStrings("mymod_strings_");
        Game.World.console.DeveloperMode();
    }

    void OnTravel(OnlineEvents.Travel evnt)
    {
        StartCoroutine(TryTravel(evnt.dir));
    }

    IEnumerator TryTravel(int dir)
    {
        WebRequest request = new WebRequest();
        yield return request.Do(server + "worldmap_travel.php",
            new MultipartFormDataSection("uid", _uid.ToString()),
            new MultipartFormDataSection("dir", dir.ToString()),
            new MultipartFormDataSection("world_tile", _world_tile.ToString()));
    }

    void LevelLoaded(GlobalEvents.LevelLoaded evnt)
    {
        Debug.Log(evnt.levelName);
    }

    void OnCharacterOnAttack(GlobalEvents.CharacterOnAttack evnt)
    {
        Random.InitState(0);
    }

    void OnCharacterTurnEnd(GlobalEvents.CharacterTurnEnd evnt)
    {
        if (Game.World.Player.CharacterComponent == evnt.cc)
        {
            StartCoroutine(TryTurnEnd());
        }
    }

    IEnumerator TryTurnEnd()
    {
        WebRequest request = new WebRequest();
        yield return request.Do(server + "character_turnend.php",
            new MultipartFormDataSection("uid", _uid.ToString()),
            new MultipartFormDataSection("world_tile", _world_tile.ToString()));
    }


    string Payload(string key, string value)
    {
        string payload = "{\"" + key + "\":\"" + value + "\"}";
        return payload;
    }

    void OnChat(OnlineEvents.ChatSend chat)
    {
        StartCoroutine(SendAction(ActionType.Chat, Payload("message", chat.msg)));
    }

    IEnumerator SendAction(ActionType action, string payload)
    {
        WebRequest request = new WebRequest();
        yield return request.Do(server + "add_action.php",
            new MultipartFormDataSection("session_key", _sessionKey),
            new MultipartFormDataSection("type", ((int)action).ToString()),
            new MultipartFormDataSection("world_tile", _world_tile.ToString()),
            new MultipartFormDataSection("payload", payload)
            );
    }

    void OnLogin(OnlineEvents.Login evnt)
    {
        _signed = true;
        _uid = evnt.uid;
        _sessionKey = evnt.sessionKey;
        _world_tile = evnt.world_tile;
        _lastActionId = 0; // evnt.lastActionId;
        _fetch = true;

        Game.World.NextLevel("WorldMap_Online", "", false, false);
    }

    private void Whoop(CharacterComponent cc, string whoop, bool fromPlayer)
    {
        Game.World.HUD.Whoop(whoop, new Vector3(0, 5, 0), cc.transform, fromPlayer ? PlayerHUD.DefaultWhoopColor : PlayerHUD.FrendlyFireWhoopColor);
    }

    void ToEncounterBegin(OnlineEvents.EncounterBegin evnt)
    {
        Game.World.NextLevel("Z_1", "EnterPoint", false, false);
    }

    void ToEncounterEnd(OnlineEvents.EncounterEnd evnt)
    {
       Game.World.NextLevel("WorldMap_Online", "", false, false);
    }

    enum ActionType
    {
        CharacterMove,
        Chat,
        TurnEnd,
    }
    IEnumerator TryGetActions()
    {
        WebRequest request = new WebRequest();

        yield return request.Do(server + "get_actions.php",
            new MultipartFormDataSection("world_tile", _world_tile.ToString()),
            new MultipartFormDataSection("after_id", _lastActionId.ToString())
            );

        _actionTimer = 2.0f;

        if (request.Success)
        {
            foreach (JSon.JNode jAction in request.GetData()["actions"].AsArray)
            {
                _lastActionId = jAction["id"].AsInt;
                ActionType type = (ActionType) jAction["type"].AsInt;
                int initiator = jAction["initiator_id"].AsInt;
                int world_tile = jAction["world_tile"].AsInt;
                JSon.JNode data = jAction["payload"];

                if (type == ActionType.CharacterMove) // move
                {
                    if (initiator == _uid)
                    {
                        continue;
                    }
                    var n = Pathfinder.Instance.FindNodeByCell(data["x"].AsInt, data["y"].AsInt);
                    if (n != null)
                    {
                        GetCC(initiator).MoveTo(n.GetPosition());
                    }
                }

                if (type == ActionType.Chat) // chat
                {
                    GlobalEvents.PerformEvent(new OnlineEvents.ChatGet(){ msg = data["message"], initiator = initiator});
                    if(_inBattle)
                    {
                        Whoop(GetCC(initiator), data["msg"], initiator == _uid);
                    }
                }

                if (type == ActionType.TurnEnd) //turn end
                {
                    GetCC(initiator).Character.AP = 0;
                }
            }
        }
        else
        {
            //error handle
        }
    }

    CharacterComponent GetCC(int id)
    {
        if (id == _uid)
        {
            return Game.World.Player.CharacterComponent;
        }
        else
        {
            return _characters[id];
        }
    }

    IEnumerator TryCharacterMove(Vector2Int xy)
    {
        WebRequest request = new WebRequest();
        yield return request.Do(server + "character_move.php",
            new MultipartFormDataSection("uid", _uid.ToString()),
            new MultipartFormDataSection("x", xy.x.ToString()),
            new MultipartFormDataSection("y", xy.y.ToString())
            );

        if (request.Success)
        {

        }
        else
        {
            //error handle
        }
    }

    IEnumerator TryGetCharacters(int room)
    {
        WebRequest request = new WebRequest();
        yield return request.Do(server + "characters_get.php",
            new MultipartFormDataSection("room", room.ToString())
            );

        _fetch = true;
        _inBattle = true;
        if (request.Success)
        {
            foreach (JSon.JNode jData in request.GetData().AsArray)
            {
                int uid = jData["uid"].AsInt;
                int x = jData["x"].AsInt;
                int y = jData["y"].AsInt;

                CharacterComponent cc;
                if (uid != _uid) //skip player
                {
                    GameObject copy = GameObject.Instantiate(ResourceManager.Load<GameObject>("Entities/Creature/BaseMale11", ResourceManager.EXT_PREFAB), Vector3.zero, Quaternion.identity);
                    cc = copy.GetComponent<CharacterComponent>();
                    cc.Character.Caps = Character.CharacterCaps.Custom;
                    var controller = new NetworkControl(uid);
                    cc.Controller = controller;
                    controller.Start(cc);
                    _characters.Add(uid, cc);
                }
                else
                {
                    cc = Game.World.Player.CharacterComponent;
                }

                var n = Pathfinder.Instance.FindNodeByCell(x, y);
                if (n != null)
                {
                    cc.Teleport(n.GetPosition(), false);
                    Game.World.Player.CameraSnap = true;
                }
            }
        }
        else
        {
            //error handle
        }
    }


    void OnCharacterMove(GlobalEvents.CharacterMove evnt)
    {
        if (Game.World.Player.CharacterComponent == evnt.cc)
        {
            StartCoroutine(TryCharacterMove(evnt.cell));
        }
    }

    void OnLevelLoaded(GlobalEvents.LevelLoaded evnt)
    {
      //  StartCoroutine(TryGetCharacters(0));
    }

    float _actionTimer = 2;
    // Update is called once per frame
    void Update()
    {
        if (_loginForm && GameObject.Find("MainMenu_HUD(Clone)"))
        {
            var mainMenu = GameObject.Find("MainMenu_HUD(Clone)").transform.Find("Panel/MainMenu");

            mainMenu.Find("Continue").gameObject.SetActive(false);
            mainMenu.Find("NewGame").gameObject.SetActive(false);
            mainMenu.Find("LoadGame").gameObject.SetActive(false);

            Instantiate(_loginForm, mainMenu).transform.SetAsFirstSibling();
            _loginForm = null;

            Instantiate(ResourceManager.Load<GameObject>("ChatPanel", ResourceManager.EXT_PREFAB), Game.World.HUD.Log.transform);
        }

        if (_inGameForm == null && GameObject.Find("Game_HUD(Clone)/UI"))
        {
            var prefab = ResourceManager.Load<GameObject>("InGameForm", ResourceManager.EXT_PREFAB);
            var hud = GameObject.Find("Game_HUD(Clone)/UI");
            _inGameForm = Instantiate(prefab, hud.transform);
        }

        /*
        if (_signed && _fetch)
        {
            _actionTimer -= Time.deltaTime;
            if (_actionTimer <= 0)
            {
               StartCoroutine(TryGetActions());
            }
        }*/
    }

    IEnumerator ActionPollLoop()
    {
        while (true)
        {
            if (_signed && _fetch)
            {
                _actionTimer -= Time.deltaTime;
                if (_actionTimer <= 0)
                {
                    yield return TryGetActions();
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine.Networking;


public class NetworkControl : CharacterControl
{
    int _uid = 0;

    public NetworkControl(int uid)
    {
        _uid = uid;
    }

    protected override void OnStart()
    {
     
    }    

    public override void OnBattleEnd()
    {
   
    }

    public override void OnCollectBattleExp(int exp)
    {
    }

    public override void OnCollectExp(int exp)
    {
      
    }

    public override void OnTurnBegin()
    {
      
    }


    public override void OnTurnEnd()
    {
        base.OnTurnEnd();
    }


    public override void OnTurnUpdate()
    {
    }

    protected override void OnUpdate()
    {
    }

}

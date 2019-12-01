using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterHeroNode : FindObjectNode
{
    public string msg = "I'M SUPER HERO!";

    public override void Start()
    {
        var cc = GetObject().GetComponent<CharacterComponent>();
        CharacterProtoStats stats = cc.Character.CharProto.Stats;
        for (CharacterProtoStats.Primary it = 0; it <= CharacterStats.Primary.Luck; ++it)
        {
            stats.SetPrimaryValue(it, 10);
        }

        for (CharacterProtoStats.SkillValue it = 0; it != CharacterStats.SkillValue.Max; ++it)
        {
            stats.SetSkillBonus(it, 100);
        }

        Game.World.Msg(true, msg);
    }


    public override void Serialize(JSon.JNode node)
    {
        base.Serialize(node);

        node["msg"] = msg;
    }

    public override void Deserialize(JSon.JNode node)
    {
        base.Deserialize(node);

        msg = node["msg"].Value;
    }
}
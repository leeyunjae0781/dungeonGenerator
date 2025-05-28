using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[CreateAssetMenu]
public class NewCustomRuleTile : RuleTile<NewCustomRuleTile.Neighbor> {
    public bool alwaysConnect;
    public TileBase[] tilesToConnect;
    public bool checkSelf;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Any = 3;
        public const int Specified = 4;
        public const int Nothing = 5;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.This: return Check_This(tile);
            case Neighbor.NotThis: return Check_NotThis(tile);
            case Neighbor.Any: return Check_Any(tile);
            case Neighbor.Specified: return Check_Specified(tile);
            case Neighbor.Nothing: return Check_Nothing(tile);
        }
        return base.RuleMatch(neighbor, tile);
    }

    bool Check_This(TileBase tile)
    {
        // alwaysConnect가 활성화되어 있으면
        // 배열에 타일이 포함되어 있거나 타일이 같을 때 true 반환
        // 활성화되어 있지 않으면 타일이 같을 때 true 반환
        if (!alwaysConnect) return tile == this;
        else return tilesToConnect.Contains(tile) || tile == this;
    }

    bool Check_NotThis(TileBase tile)
    {
        // 타일이 다를 때 true 반환
        return tile != this;
    }

    bool Check_Any(TileBase tile) 
    {
        // checkSelf가 활성화되어 있으면 tile이 존재할 때 true 반환
        // 활성화되어 있지 않으면 tile이 존재하고, tile이 다를 때 true 반환
        if (checkSelf) return tile != null;
        else return tile != null && tile != this;
    }

    bool Check_Specified(TileBase tile)
    {
        // 배열에 타일이 포함되어 있으면 true 반환
        return tilesToConnect.Contains(tile);
    }

    bool Check_Nothing(TileBase tile)
    {
        // 타일이 비어 있으면 true 반환
        return tile == null;
    }
}
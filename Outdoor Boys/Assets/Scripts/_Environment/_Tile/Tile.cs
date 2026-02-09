using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _spriteRenderer;


    private TileData _data;
    public TileData data => _data;


    // Data
    public TileData Set_Data(TileScrObj setTile)
    {
        if (setTile == null)
        {
            Debug.Log("TileScrObj data empy");
            return null;
        }
        
        _data = new(setTile);
        return _data;
    }


    // Sprite
    public void Update_SetSprite(bool isBaseTile)
    {
        if (_data == null)
        {
            Debug.Log("_data is null!");
            return;
        }

        if (_data.tileScrObj == null)
        {
            Debug.Log("_data.tileScrObj is null!");
            return;
        }

        Sprite[] sprites = _data.tileScrObj.sprites;

        if (sprites.Length <= 1)
        {
            Debug.Log("_sprites missing for " + _data.tileScrObj + " !");
            return;
        }
        
        _spriteRenderer.sprite = isBaseTile ? sprites[1] : sprites[0];
    }
}

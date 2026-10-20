using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class sidebar : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase Tile;
    public float Height;
    public int rows;
    private bool selected = false;
    private TileBase currentTile;
    private List<string> types = new List<string> { "Normal", "RGB" };
    public List<int> tiletypes = new List<int>();
    private Vector3Int selection;
    private Vector3Int prevTile;
    public GameObject textPrefab;

    // Public fields for text positioning and scaling
    public float textOffsetX = 0.5f;
    public float textOffsetY = 0.5f;
    public float textScaleX = 1.0f;
    public float textScaleY = 1.0f;

    void Start()
    {
        LoadTileTypes();

        float scale = Height / rows;
        scale *= 0.7f;
        // Apply the calculated scale to the tilemap
        tilemap.transform.localScale = new Vector3(0.4f, scale, 1f);

        while (tiletypes.Count < rows)
        {
            tiletypes.Add(0);
        }
        ResizeTileMap();
        FillTilemap();
        EventManager.BroadcastMessage("rows:"+rows);
    }

    void ResizeTileMap()
    {
        tilemap.ClearAllTiles();

        // Resizing the bounds of the tilemap based on the new size
        BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(1, rows, 1));

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int scaledPos = new Vector3Int(x, y, 0);
                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile(scaledPos));
            }
        }
    }

    void Change(int pos)
    {
        tiletypes[pos] += 1;
        if (tiletypes[pos] > 1)
        {
            tiletypes[pos] = 0;
        }
        SaveTileTypes(); // Save tiletypes whenever they change
        UpdateTileText(pos); // Update the text on the tile
    }

    void FillTilemap()
    {
        for (int y = 0; y < rows; y++)
        {
            // Ensure tiles are placed starting from the bottom-left
            Vector3Int position = new Vector3Int(0, y, 0);
            tilemap.SetTile(position, Tile);

            // Instantiate text object on the tile
            Vector3 tileWorldPos = tilemap.CellToWorld(position) + new Vector3(textOffsetX, textOffsetY, 0); // Use public fields for positioning
            GameObject textObj = Instantiate(textPrefab, tileWorldPos, Quaternion.identity, transform);
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();
            textMesh.text = types[tiletypes[y]]; // Set the text based on tile type

            // Apply scaling
            textObj.transform.localScale = new Vector3(textScaleX, textScaleY, 1.0f); // Use public fields for scaling

            textObj.name = $"TileText_{y}"; // Name the text object for later reference
        }
    }

    void Select()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selection = tilemap.WorldToCell(mouseWorldPos);
            if (selection.x < 3)
            {
                selection.x = 0;
            }
            if (tilemap.HasTile(selection))
            {
                Vector3Int currentTile = selection;
                if (selected)
                {
                    if (currentTile == prevTile)
                    {
                        Change(selection.y);
                    }
                }
                prevTile = selection;
                selected = true;
            }
            else
            {
                selected = false;
            }
        }
    }

    void Update()
    {
        Select();
    }

    void SaveTileTypes()
    {
        for (int i = 0; i < tiletypes.Count; i++)
        {
            PlayerPrefs.SetInt($"tiletype_{i}", tiletypes[i]);
        }
        PlayerPrefs.Save();
    }

    void LoadTileTypes()
    {
        tiletypes.Clear();
        for (int i = 0; i < rows; i++)
        {
            if (PlayerPrefs.HasKey($"tiletype_{i}"))
            {
                tiletypes.Add(PlayerPrefs.GetInt($"tiletype_{i}"));
            }
            else
            {
                tiletypes.Add(0);
            }
        }
    }

    void UpdateTileText(int pos)
    {
        Transform textTransform = transform.Find($"TileText_{pos}");
        if (textTransform != null)
        {
            TextMeshPro textMesh = textTransform.GetComponent<TextMeshPro>();
            textMesh.text = types[tiletypes[pos]]; // Update the text based on tile type

            // Update position and scale based on public fields
            Vector3Int tilePosition = new Vector3Int(0, pos, 0);
            Vector3 tileWorldPos = tilemap.CellToWorld(tilePosition) + new Vector3(textOffsetX, textOffsetY, 0);
            textTransform.position = tileWorldPos;
            textTransform.localScale = new Vector3(textScaleX, textScaleY, 1.0f);
        }
    }
}

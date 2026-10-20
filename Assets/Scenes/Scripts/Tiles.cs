using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class EventManager
{
    public static event Action<string> OnMessageBroadcast;

    public static void BroadcastMessage(string message)
    {
        OnMessageBroadcast?.Invoke(message);
    }
}

public class Tiles : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase[] coloredTiles;  // Array to hold different thickness tiles
    public TileBase[] swappedTiles;  // Array to hold different thickness swapped tiles
    public TileBase[] selectedColoredTiles;  // Array for selected colored tiles with different thickness
    public TileBase[] selectedSwappedTiles;  // Array for selected swapped tiles with different thickness
    public TileBase[] rightSlideTiles; 
    public TileBase[] selectedRightSlideTiles; 
    public TileBase[] leftSlideTiles; 
    public TileBase[] selectedLeftSlideTiles; 
    public Vector3Int newSize;
    public float desiredTotalHeight = 5.2f;
    private int currentXPosition = 0;
    private List<List<int>> tileColor = new List<List<int>>();
    private Vector3Int selectionStart;
    private Vector3Int selectionEnd;
    private bool isSelecting = false;
    private string filePath;
    public string songPath;
    private float audioLength;
    private int visiblecells;
    private float scaleX = 0.5f;
    private int sidethickness = 0;
    private int rows;
    private float cellTime = 0.1f;
    
    private Vector3Int bottomLeft = new Vector3Int(0, 0, 10);
    private Vector3Int topRight = new Vector3Int(0, 0, 10);

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "Scenes", "text", "tiles.txt");
        float scaleY = desiredTotalHeight / newSize.y;
        tilemap.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        LoadSettings();
        visiblecells = (int)Math.Floor(14.7 / scaleX);
        
        LoadTilesAndResizeMap(newSize);
        EventManager.BroadcastMessage("CellWidthx:" + scaleX);
        EventManager.BroadcastMessage("CellWidthy:" + scaleY);
        
    }
   

    Vector3Int DetermineCellTime(Vector3Int size)
    {
        FillList(newSize);
        cellTime = (float)Math.Round((audioLength / tileColor[0].Count), 3);
        int totalCells = Mathf.CeilToInt(audioLength / (cellTime*10));
        size.x = totalCells;
        newSize.x = totalCells;
        EventManager.BroadcastMessage("cellTime:" + cellTime);
        
        return size;
    }
    void changecelltime(bool direction)
    {
        string filePath = Path.Combine(Application.dataPath, "Scenes", "text", "tiles.txt");
        if (direction)
        {
            cellTime = cellTime / 2;
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                List<List<int>> holdlist = new List<List<int>>();

                foreach (string line in lines)
                {
                    List<int> intList = new List<int>();
                    foreach (char ch in line)
                    {
                        if (int.TryParse(ch.ToString(), out int number))
                        {
                            intList.Add(number);
                            intList.Add(number); // Duplicate the number to halve the cellTime
                        }
                        else
                        {
                            Debug.LogWarning($"Warning: Unable to convert '{ch}' to an integer. Skipping.");
                        }
                    }
                    holdlist.Add(intList);
                }

                holdlist.Reverse();
                lines = new string[holdlist.Count];

                for (int i = 0; i < holdlist.Count; i++)
                {
                    char[] lineChars = new char[holdlist[i].Count];
                    for (int j = 0; j < holdlist[i].Count; j++)
                    {
                        lineChars[j] = holdlist[i][j].ToString()[0];
                    }
                    lines[holdlist.Count - 1 - i] = new string(lineChars);
                }

                File.WriteAllLines(filePath, lines);
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }
            EventManager.BroadcastMessage("cellTime:" + cellTime);
            EventManager.BroadcastMessage("CellWidthx:" + scaleX);
        }
        else
        {
            cellTime = cellTime * 2;
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                List<List<int>> holdlist = new List<List<int>>();

                foreach (string line in lines)
                {
                    List<int> intList = new List<int>();
                    foreach (char ch in line)
                    {
                        if (int.TryParse(ch.ToString(), out int number))
                        {
                            intList.Add(number);
                        }
                        else
                        {
                            Debug.LogWarning($"Warning: Unable to convert '{ch}' to an integer. Skipping.");
                        }
                    }
                    holdlist.Add(intList);
                }

                List<List<int>> holdlist2 = new List<List<int>>();
                for (int i = 0; i < holdlist.Count; i++)
                {
                    List<int> intList = new List<int>();
                    for (int j = 0; j < holdlist[i].Count; j += 2)
                    {
                        if (j + 1 < holdlist[i].Count && holdlist[i][j] == 1 && holdlist[i][j + 1] == 1)
                        {
                            intList.Add(holdlist[i][j]); // Add only one of the pair if both are 1
                        }
                        else
                        {
                            intList.Add(holdlist[i][j]); // Otherwise, add the current element
                        }
                    }
                    holdlist2.Add(intList);
                }

                holdlist2.Reverse();
                holdlist = holdlist2;
                lines = new string[holdlist.Count];

                for (int i = 0; i < holdlist.Count; i++)
                {
                    char[] lineChars = new char[holdlist[i].Count];
                    for (int j = 0; j < holdlist[i].Count; j++)
                    {
                        lineChars[j] = holdlist[i][j].ToString()[0];
                    }
                    lines[holdlist.Count - 1 - i] = new string(lineChars);
                }

                File.WriteAllLines(filePath, lines);
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }
            EventManager.BroadcastMessage("cellTime:" + cellTime);
            EventManager.BroadcastMessage("CellWidthx:" + scaleX);
            
            
        }
    }

    void LoadSettings()
    {
        // Load scaleX
        if (PlayerPrefs.HasKey("scaleX"))
        {
            scaleX = PlayerPrefs.GetFloat("scaleX");
        }
        else
        {
            scaleX = 0.5f; // Default value
        }
        if (PlayerPrefs.HasKey("sidethickness"))
        {
            sidethickness = PlayerPrefs.GetInt("sidethickness");
        }
        else
        {
            sidethickness = 0; // Default value
        }
        tilemap.transform.localScale = new Vector3(scaleX, tilemap.transform.localScale.y, tilemap.transform.localScale.z);
        EventManager.BroadcastMessage("CellWidthx:" + scaleX);
        

        // Load sidethickness
        
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("scaleX", scaleX);
        PlayerPrefs.SetInt("sidethickness", sidethickness);
        visiblecells = (int)Math.Floor(14.7 / scaleX);
        
        PlayerPrefs.Save();
    }

    void LoadTilesAndResizeMap(Vector3Int size)
    {
        //size = DetermineCellTime(size);
        FillList(size);
        ResizeTileMap(size);
        FillTilemap(size);
    }
    void LoadTiles(Vector3Int size)
    {
        //size = DetermineCellTime(size);
        FillList(size);
        FillTilemap(size);
    }


    void SaveList(Vector3Int size)
    {
        string filePath = Path.Combine(Application.dataPath, "Scenes", "text", "tiles.txt");
        string songFilePath = Path.Combine(Application.dataPath, "Scenes", "text", songPath);
        string[] lines = new string[tileColor.Count];

        for (int i = 0; i < tileColor.Count; i++)
        {
            char[] lineChars = new char[tileColor[i].Count];
            for (int j = 0; j < tileColor[i].Count; j++)
            {
                lineChars[j] = tileColor[i][j].ToString()[0];
            }
            lines[tileColor.Count - 1 - i] = new string(lineChars);
        }

        File.WriteAllLines(filePath, lines);
        File.WriteAllLines(songFilePath, lines);
    }

    void FillList(Vector3Int size)
    {
        tileColor.Clear();
        string[] lines = File.ReadAllLines(filePath);
        int x = size.x;

        if (lines != null && lines.Length > 0 && lines[0] != null)
        {
        x = lines[0].Length;
        }
        int y = size.y;
        if (x<((int)Mathf.Ceil(audioLength/.1f))){
            x = (int)Mathf.Ceil(audioLength/.1f);
            
        }

        if (File.Exists(filePath))
        {
            lines = File.ReadAllLines(filePath);
            List<List<int>> tempTileColor = new List<List<int>>();

            foreach (string line in lines)
            {
                List<int> intList = new List<int>();
                foreach (char ch in line)
                {
                    if (int.TryParse(ch.ToString(), out int number))
                    {
                        intList.Add(number);
                    }
                    else
                    {
                        Debug.LogWarning($"Warning: Unable to convert '{ch}' to an integer. Skipping.");
                    }
                }

                while (intList.Count < x)
                {
                    intList.Add(0);
                }

                if (intList.Count > x)
                {
                    intList.RemoveRange(x, intList.Count - x);
                }

                tempTileColor.Add(intList);
            }

            while (tempTileColor.Count < y)
            {
                tempTileColor.Add(new List<int>(new int[x]));
            }

            if (tempTileColor.Count > y)
            {
                tempTileColor.RemoveRange(y, tempTileColor.Count - y);
            }

            tempTileColor.Reverse();
            tileColor = tempTileColor;
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
        }
        SaveList(size);
    }

    void FillTilemap(Vector3Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int position = new Vector3Int(x + currentXPosition, y, 0);

                if (y < tileColor.Count && x < tileColor[y].Count)
                {
                    int tileIndex = Math.Min(sidethickness, coloredTiles.Length - 1);

                    if (tileColor[y][x] == 0)
                    {
                        tilemap.SetTile(position, coloredTiles[tileIndex]);
                    }
                    else if (tileColor[y][x] == 1)
                    {
                        tilemap.SetTile(position, swappedTiles[tileIndex]);
                    }
                    else if (tileColor[y][x] == 2)
                    {
                        tilemap.SetTile(position, selectedColoredTiles[tileIndex]);
                    }
                    else if (tileColor[y][x] == 3)
                    {
                        tilemap.SetTile(position, selectedSwappedTiles[tileIndex]);
                    }
                    else if (tileColor[y][x] == 4)
                    {
                        tilemap.SetTile(position, rightSlideTiles[tileIndex]);
                    }
                    else if (tileColor[y][x] == 5)
                    {
                        tilemap.SetTile(position, leftSlideTiles[tileIndex]);
                    }
                    else if (tileColor[y][x] == 6)
                    {
                        tilemap.SetTile(position, selectedRightSlideTiles[tileIndex]);
                    }
                    else if (tileColor[y][x] == 7)
                    {
                        tilemap.SetTile(position, selectedLeftSlideTiles[tileIndex]);
                    }
                }
            }
        }
    }

    void ResizeTileMap(Vector3Int size)
    {
        tilemap.ClearAllTiles();
        BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(size.x, size.y, size.z));

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int scaledPos = new Vector3Int(x, y, 0);
                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile(scaledPos));
            }
        }
    }

    void MouseSelect()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse down");
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tempSelectionStart = tilemap.WorldToCell(mouseWorldPos);
            tempSelectionStart.x -= currentXPosition;
            Debug.Log("Tile: " + tempSelectionStart);
            if (tilemap.HasTile(tempSelectionStart))
            {
                Debug.Log("Has Tile: " + tempSelectionStart);
                selectionStart = tilemap.WorldToCell(mouseWorldPos);
                selectionStart.x -= currentXPosition;
                isSelecting = true;
                EventManager.BroadcastMessage("SelectionStart:" + selectionStart.x);
            }
        }

        if (Input.GetMouseButton(0) && isSelecting)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tempSelectionEnd = tilemap.WorldToCell(mouseWorldPos);
            tempSelectionEnd.x -= currentXPosition;
            if (tilemap.HasTile(tempSelectionEnd))
            {
                selectionEnd = tilemap.WorldToCell(mouseWorldPos);
                selectionEnd.x -= currentXPosition;
                
                EventManager.BroadcastMessage("SelectionEnd:" + selectionEnd.x);
            }
            
        }

        if (isSelecting)
        {
            if ((bottomLeft.z == 10) || (bottomLeft != (new Vector3Int(Mathf.Min(selectionStart.x, selectionEnd.x), Mathf.Min(selectionStart.y, selectionEnd.y), 0)) || (topRight != (new Vector3Int(Mathf.Max(selectionStart.x, selectionEnd.x), Mathf.Max(selectionStart.y, selectionEnd.y), 0)))))
            {
            for (int y = 0; y < tileColor.Count; y++)
            {
                for (int x = 0; x < tileColor[y].Count; x++)
                {
                    if (tileColor[y][x]==2 || tileColor[y][x]==3 || tileColor[y][x]==6 || tileColor[y][x]==7){
                        tileColor[y][x]-=2;
                    }
                }
            }
            bottomLeft = new Vector3Int(Mathf.Min(selectionStart.x, selectionEnd.x), Mathf.Min(selectionStart.y, selectionEnd.y), 0);
            topRight = new Vector3Int(Mathf.Max(selectionStart.x, selectionEnd.x), Mathf.Max(selectionStart.y, selectionEnd.y), 0);
            for (int x = bottomLeft.x; x <= topRight.x; x++)
            {
                for (int y = bottomLeft.y; y <= topRight.y; y++)
                {
                    Vector3Int position = new Vector3Int(x + currentXPosition, y, 0);
                    if (tilemap.HasTile(position))
                    {
                        TileBase currentTile = tilemap.GetTile(position);
                        tileColor[y][x] += 2;
                    }
                }
            }
            }
            if (Input.GetMouseButton(0) && isSelecting){
            SaveList(newSize);
            LoadTiles(newSize);
            SaveList(newSize);
            
             }
        }
    }

    void OnEnable()
    {
        EventManager.OnMessageBroadcast += HandleMessage;
    }

    void OnDisable()
    {
        EventManager.OnMessageBroadcast -= HandleMessage;
    }

    void HandleMessage(string message)
    {
        int? messagenum = null;

        if (message == "OFF")
        {
            messagenum = 0;
        }
        else if (message == "ON")
        {
            messagenum = 1;
        }
        else if (message.Contains("RGBLEFTSLIDE"))
        {
            float scroll = 6;
            
            if (scroll > 0)
            {
                currentXPosition += Mathf.FloorToInt(scroll * 20);
                }
            else
            {
                currentXPosition -= Mathf.FloorToInt(-scroll * 20);
            }
            if (currentXPosition > 0)
            {
                currentXPosition = 0;
                }
                EventManager.BroadcastMessage("Scrolled:" + currentXPosition);
                LoadTiles(newSize);
                
            
        }
        else if (message.Contains("RGBRIGHTSLIDE"))
        {
            float scroll = -6;
            
            if (scroll > 0)
            {
                currentXPosition += Mathf.FloorToInt(scroll * 20);
                }
            else
            {
                currentXPosition -= Mathf.FloorToInt(-scroll * 20);
            }
            if (currentXPosition > 0)
            {
                currentXPosition = 0;
                }
                EventManager.BroadcastMessage("Scrolled:" + currentXPosition);
                LoadTiles(newSize);
        }
        else if (message.Contains("rows:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float leng))
            {
                rows = (int)leng;
            }
        }
        else if (message.Contains("AudioLengthis:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float leng))
            {
                audioLength = leng;
                DetermineCellTime(newSize);
                
                float cells = audioLength / cellTime;
                int totalCells = (int)Mathf.Ceil(cells);
                newSize.x = totalCells;
                LoadTilesAndResizeMap(newSize);
            }
        }
        else if (message.Contains("moveTiles"))
        {
            visiblecells = (int)Math.Floor((14.7 / scaleX));
            if (visiblecells > 0)
            {
                currentXPosition += Mathf.FloorToInt(-visiblecells);
            }
            else
            {
                currentXPosition -= Mathf.FloorToInt(visiblecells);
            }
            if (currentXPosition > 0)
            {
                currentXPosition = 0;
            }
            EventManager.BroadcastMessage("Scrolled:" + currentXPosition);
            LoadTilesAndResizeMap(newSize);
        }
        else if (message.Contains("shortencellwidth"))
        {
            scaleX -= 0.05f;
            tilemap.transform.localScale = new Vector3(scaleX, tilemap.transform.localScale.y, tilemap.transform.localScale.z);
            EventManager.BroadcastMessage("CellWidthx:" + scaleX);
            visiblecells = (int)Math.Floor(14.7 / scaleX);
            
            SaveSettings();
        }
        else if (message.Contains("extendcellwidth"))
        {
            scaleX += 0.05f;
            tilemap.transform.localScale = new Vector3(scaleX, tilemap.transform.localScale.y, tilemap.transform.localScale.z);
            EventManager.BroadcastMessage("CellWidthx:" + scaleX);
            visiblecells = (int)Math.Floor(14.7 / scaleX);
            SaveSettings();
        }
        else if (message.Contains("thickenside"))
        {
            sidethickness++;
            SetTileThickness();
            SaveSettings();
        }
        else if (message.Contains("shortenside"))
        {
            sidethickness--;
            SetTileThickness();
            SaveSettings();
        }
        else if (message.Contains("halvecells"))
        {
            changecelltime(true);
        }
        else if (message.Contains("doublecells"))
        {
            changecelltime(false);
        }
        if (message.Contains("all"))
        {
            currentXPosition = 0;
            EventManager.BroadcastMessage("Scrolled:" + currentXPosition);
            LoadTilesAndResizeMap(newSize);
            EventManager.BroadcastMessage("cellWidthx:" + scaleX);
        }
        
        if (messagenum.HasValue)
        {
            affect(messagenum.Value);
        }
    }

    void SetTileThickness()
    {
        if (sidethickness < 0) sidethickness = 0;
        if (sidethickness >= coloredTiles.Length) sidethickness = coloredTiles.Length - 1;
    }

    void affect(int num)
    {
        if (isSelecting)
        {
            Vector3Int bottomLeft = new Vector3Int(Mathf.Min(selectionStart.x, selectionEnd.x), Mathf.Min(selectionStart.y, selectionEnd.y), 0);
            Vector3Int topRight = new Vector3Int(Mathf.Max(selectionStart.x, selectionEnd.x), Mathf.Max(selectionStart.y, selectionEnd.y), 0);
            for (int x = bottomLeft.x; x <= topRight.x; x++)
            {
                for (int y = bottomLeft.y; y <= topRight.y; y++)
                {   
                    if (num>3){
                    List<int> tiletypes = new List<int>();
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
                    int number = tiletypes[y];
                    Debug.Log(number);
                    if (number==1){
                        Vector3Int pos = new Vector3Int(x + currentXPosition, y, 0);
                        if (tilemap.HasTile(pos))
                        {
                            tileColor[y][x] = num;
                        }
                        break;
                    }
                    else{
                        break;
                    }
                    }
                    Vector3Int position = new Vector3Int(x + currentXPosition, y, 0);
                    if (tilemap.HasTile(position))
                    {
                        tileColor[y][x] = num;
                        
                    }
                }
            }
            SaveList(newSize);
            LoadTiles(newSize);
        }
    }

    void Update()
    {
        
        MouseSelect();

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int select = tilemap.WorldToCell(mouseWorldPos);
            if (tilemap.HasTile(select))
            {
                if (scroll > 0)
                {
                    currentXPosition += Mathf.FloorToInt(scroll * 20);
                }
                else
                {
                    currentXPosition -= Mathf.FloorToInt(-scroll * 20);
                }
                if (currentXPosition > 0)
                {
                    currentXPosition = 0;
                }
                EventManager.BroadcastMessage("Scrolled:" + currentXPosition);
                LoadTiles(newSize);
            }
        }
    } 
}

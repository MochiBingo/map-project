using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;

public class testtilemap : MonoBehaviour
{
    //variables
    public Tilemap myTilemap;
    public Tile tree;
    public Tile empty;
    public Tile roadbarrier;
    public Tile tree2;
    public Tile hotdogcart;
    public Tile plant;
    public Tile plant2;
    public Tile WaterTR;
    public Tile WaterTL;
    public Tile WaterBR;
    public Tile WaterBL;
    public Tile WaterLeft;
    public Tile WaterRight;
    public Tile WaterTop;
    public Tile WaterBottom;
    public Tile WaterMiddle;
    public Tile Player;
    private System.Random rand = new System.Random();
    private const int columns = 45;
    private const int rows = 25;
    string filePath = @"E:\map-project-premade-map.txt";
    private int playerX = 1;
    private int playerY = 1;
    private string[] mapData;

    //Legend:
    //T=tree (random)
    //P=plant (random)
    //O=obstacle (random)
    //W=water

    //Rules:
    //map has a border (ONE)
    //Pond always has a proper border (as long as it was procedural, not premade (TWO)
    //never more than 10 hot dog carts (THREE)

    //calls the map to be made, and update's the player so you can see where they are
    void Start()
    {
        mapData = GenerateMapString(columns, rows);
        ConvertMapToTilemap(mapData);
        //LoadPremadeMap(filePath);
        UpdatePlayerTile();
    }

    //movement handler
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryMovePlayer(0, -1);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            TryMovePlayer(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryMovePlayer(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryMovePlayer(1, 0);
        }
    }

    //resets player off old tile, adds them to new tile
    void TryMovePlayer(int dx, int dy)
    {
        int newX = playerX + dx;
        int newY = playerY + dy;

        if (IsPositionValid(newX, newY))
        {
            Vector3Int oldPosition = new Vector3Int(playerX, -playerY, 0);
            myTilemap.SetTile(oldPosition, GetTileForCharacter(mapData[playerY][playerX]));

            playerX = newX;
            playerY = newY;
            UpdatePlayerTile();
        }
    }

    //actually makes the player visual go there
    void UpdatePlayerTile()
    {
        Vector3Int position = new Vector3Int(playerX, -playerY, 0);
        myTilemap.SetTile(position, Player);
    }

    //checks if you can move into that position (if it's ground)
    bool IsPositionValid(int x, int y)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows)
        {
            return false;
        }
        char tileAtPositon = mapData[y][x];
        return tileAtPositon != 'O' && tileAtPositon != 'W' && tileAtPositon != 'H' && tileAtPositon != 'P' && tileAtPositon != 'T' && tileAtPositon != '+' && tileAtPositon != '~' && tileAtPositon != '=' && tileAtPositon != '[' && tileAtPositon != ']' && tileAtPositon != '_' && tileAtPositon != '-' && tileAtPositon != '!';
    }

    //keeps count of how many hot dog carts
    private int hotdogcarts = 0;

    //makes the map, keeping a tree border and making the rest either obstacles, plants, or empty spots
    string[] GenerateMapString(int columns, int rows)
    {
        string[] mapinput=new string[rows];

        for (int y = 0; y < rows; y++)
        {
            char[] row = new char[columns];
            for (int x = 0; x < columns; x++)
            {
                if (x == 0 || x == columns - 1 || y == 0 || y == rows - 1)
                {
                    row[x] = 'T';
                }

                else
                {
                    row[x] = GenerateRandomTile(x, y);
                }
                
            }
            mapinput[y] = new string(row);
        }
        PlacePond(mapinput, columns, rows);

        return mapinput;
    }

    //makes a pond
    void PlacePond(string[] mapData, int columns, int rows)
    {
        int pondWidth = rand.Next(5, 10);
        int pondHeight = rand.Next(3, 8);

        int pondX = rand.Next(1, columns - pondWidth - 1);
        int pondY = rand.Next(1, rows - pondHeight - 1);

        for (int y = pondY; y < pondY + pondHeight; y++)
        {
            char[] row = mapData[y].ToCharArray();
            for (int x = pondX; x < pondX + pondWidth; x++)
            {
                row[x] = 'W';
            }
            mapData[y] = new string(row);
        }
        for (int x = pondX - 1; x < pondX + pondWidth + 1; x++)
        {
            if (pondY - 1 >= 0)
            {
                char[] topRow = mapData[pondY - 1].ToCharArray();
                if (x == pondX - 1)
                {
                    topRow[x] = '+';
                }
                else if (x == pondX + pondWidth)
                {
                    topRow[x] = '=';
                }
                else
                {
                    topRow[x] = '~';
                }
                mapData[pondY - 1] = new string(topRow);
            }
            if (pondY + pondHeight < rows)
            {
                char[] bottomRow = mapData[pondY + pondHeight].ToCharArray();
                if (x == pondX - 1)
                {
                    bottomRow[x] = '-';
                }
                else if (x == pondX + pondWidth)
                {
                    bottomRow[x] = '!';
                }
                else
                {
                    bottomRow[x] = '_';
                }
                mapData[pondY + pondHeight] = new string(bottomRow);
            }
            for (int y = pondY - 1; y < pondY + pondHeight + 1; y++)
            {
                // Left edge
                if (pondX - 1 >= 0)
                {
                    char[] leftRow = mapData[y].ToCharArray();
                    if (y == pondY - 1)
                        leftRow[pondX - 1] = '+'; // Top-left corner
                    else if (y == pondY + pondHeight)
                        leftRow[pondX - 1] = '-'; // Bottom-left corner
                    else
                        leftRow[pondX - 1] = '['; // Left edge
                    mapData[y] = new string(leftRow);
                }

                // Right edge
                if (pondX + pondWidth < columns)
                {
                    char[] rightRow = mapData[y].ToCharArray();
                    if (y == pondY - 1)
                        rightRow[pondX + pondWidth] = '='; // Top-right corner
                    else if (y == pondY + pondHeight)
                        rightRow[pondX + pondWidth] = '!'; // Bottom-right corner
                    else
                        rightRow[pondX + pondWidth] = ']'; // Right edge
                    mapData[y] = new string(rightRow);
                }
            }
        }
    }

    //chooses whether the space is a plant, obstacle, or empty
    private char GenerateRandomTile(int x, int y)
    {
        if (rand.Next(1, 1000) <= 25)
        {
            return 'P';
        }
        else if (hotdogcarts < 10 && rand.Next(1, 1000) <= 50)
        {
            hotdogcarts++;
            return 'H';
        }
        else if (rand.Next(1, 1000) >= 990)
        {
            return 'O';
        }
        else
        {
            return ' ';
        }
        
    }

    //makes the characters into their respective tiles
    void ConvertMapToTilemap(string[] mapData)
    {
        for (int y= 0; y < mapData.Length; y++)
        {
            for (int x=0; x < mapData[y].Length; x++)
            {
                char tileChar = mapData[y][x];

                if (x == playerX && y == playerY)
                {
                    continue;
                }
                Tile tileToPlace = GetTileForCharacter(tileChar);
                Vector3Int position = new Vector3Int(x,-y, 0);
                myTilemap.SetTile(position, tileToPlace);
            }
        }
    }
    
    //sorts what tiles are assigned to what characters
    private Tile GetTileForCharacter(char character)
    {
        return character switch
        {
            'T' => TreeSelection(),
            'p' => Player,
            'H' => hotdogcart,
            'O' => roadbarrier,
            'P' => PlantSelection(),
            'W' => WaterMiddle,
            '+' => WaterTL,
            '~' => WaterTop,
            '=' => WaterTR,
            '[' => WaterLeft,
            ']' => WaterRight,
            '-' => WaterBL,
            '_' => WaterBottom,
            '!' => WaterBR,
            ' ' => empty,
            _ => null,
        };
    }

    //chooses a random tree tile
    private Tile TreeSelection()
    {
        int randValue = rand.Next(1, 3);
        return randValue switch
        {
            1 => tree,
            2 => tree2,
            _ => null,
        };
    }

    //chooses a random plant tile
    private Tile PlantSelection()
    {
        int randValue = rand.Next(1, 3);
        return randValue switch
        {
            1 => plant,
            2 => plant2,
            _ => null,
        };
    }

    //loads a premade map from a text file
    void LoadPremadeMap(string mapFilePath)
    {
        string readText = File.ReadAllText(mapFilePath);
        string[] premadeMap = readText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        ConvertMapToTilemap(premadeMap);
    }
}

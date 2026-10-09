using System;
using UnityEngine;
using UnityEngine.UI;

public class Lightning : MonoBehaviour
{
    public GameObject cellPrefab;

    public const int HEIGHT = 180;
    public const int WIDTH = 320;
    public const float CELL_SIZE = 0.05f;

    public RawImage display;

    Cell[,] cells;
    Texture2D texture;
    Color[] pixels;

    Color negative = new Color(0.1f, 0.3f, 1.0f); // Blue
    Color neutral  = new Color(0.05f, 0.05f, 0.08f); // Near black
    //Color white = new Color(1f, 1f, 1f, 1f);
    Color positive = new Color(1.0f, 0.15f, 0.1f); // Red

    public enum Material
    {
        air,
        water,
        wood,
        copper
    }
    
    struct Cell
    {
        public float charge;
        public float electricPotential;
        public float ionLevel;
        public float conductivity;
        public float electricField;
        public bool isActive;
        public Material material;


        public Cell(int charge, float electricPotential)
        {
            this.charge = charge;
            this.electricPotential = electricPotential;
            this.ionLevel = 0f;
            this.conductivity = 0.2f;
            this.isActive = false;
            this.material = Material.air;
            this.electricField = 0f;
        }
    }
    
    void Start()
    {
        InitializeGrid();

        GenerateInitialConditions();
        Render();
    }

    
    void Update()
    {
        
    }

    Color GetPotentialColor(float potential)
    {
        potential = Mathf.Clamp(potential, -1f, 1f);

        if (potential < 0)
        {
            return Color.Lerp(neutral, negative, -potential);
        }
        else
        {
            return Color.Lerp(neutral, positive, potential);
        }
    }

    float GetPotential(int x, int y)
    {
        float groundNoise = Mathf.PerlinNoise(x * 0.05f, 0f);
        float lowerCloudNoise = Mathf.PerlinNoise(x * 0.04f, 10f);
        float upperCloudNoise = Mathf.PerlinNoise(x * 0.04f, 20f);

        int groundTop = 10 + Mathf.RoundToInt(groundNoise * 8f);
        int lowerCloudBottom = 126 + Mathf.RoundToInt(lowerCloudNoise * 8f);
        int lowerCloudTop = 156 + Mathf.RoundToInt(upperCloudNoise * 8f);
    

        if (y <= groundTop)
        {
            return UnityEngine.Random.Range(0.5f, 0.8f);
        }
        else if (y <= lowerCloudBottom)
        {
            return UnityEngine.Random.Range(-0.2f, 0.2f);
        }
        else if (y <= lowerCloudTop)
        {
            return UnityEngine.Random.Range(-1.0f, -0.7f);
        }
        else
        {
            return UnityEngine.Random.Range(0.7f, 1.0f);
        }
    }

    //Creates the necessary items for the grid
    void InitializeGrid()
    {
        cells = new Cell[HEIGHT, WIDTH];

        texture = new Texture2D(WIDTH, HEIGHT);
        pixels = new Color[WIDTH * HEIGHT];

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        display.texture = texture;        
    }

    // Instantiates each cell and gives it a potential
    void CreateCells()
    {
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                cells[y,x] = new Cell(0, GetPotential(x, y));
            }
        }
    }

    //Creates the cells and smooths their potentials
    void GenerateInitialConditions()
    {
        CreateCells();
        SmoothPotentials();
    }

    //Renders the new pixels and updates the texture
    void Render()
    {
        UpdatePixels();

        texture.SetPixels(pixels);
        texture.Apply();
    }

    void SmoothPotentials()
    {
        float[,] newPotentials = new float[HEIGHT, WIDTH];

        for (int y = 1; y < HEIGHT - 1; y++)
        {
            for (int x = 1; x < WIDTH - 1; x++)
            {
                float average = (
                    cells[y + 1, x].electricPotential +
                    cells[y, x + 1].electricPotential +
                    cells[y - 1, x].electricPotential +
                    cells[y, x - 1].electricPotential
                ) / 4f;

                newPotentials[y, x] = Mathf.Lerp(cells[y, x].electricPotential, average, 0.2f);
            }
            
        }
        for (int y = 1; y < HEIGHT - 1; y++)
        {
            for (int x = 1; x < WIDTH - 1; x++)
            {
                cells[y, x].electricPotential = newPotentials[y, x];
            }
        }
    }

    void UpdatePixels()
    {
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                int index = y * WIDTH + x;
                pixels[index] = GetPotentialColor(cells[y, x].electricPotential);
            }
        }
    }

    void CalculateElectricField()
    {
        for (int y = 1; y < HEIGHT - 1; y++)
        {
            for (int x = 1; x < WIDTH - 1; x++)
            {
                float left = cells[y, x - 1].electricPotential;
                float right = cells[y, x + 1].electricPotential;
                float top = cells[y + 1, x].electricPotential;
                float down = cells[y - 1, x].electricPotential;

                float fieldX = (right - left) / 2f;
                float fieldY = (top - down) / 2f;

                float fieldStrength = Mathf.Sqrt(fieldX * fieldX + fieldY * fieldY);

                cells[y, x].electricField = fieldStrength;
            }
        }
    }

    
}
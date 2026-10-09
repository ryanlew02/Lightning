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
        public Vector2 electricField;
        public bool isActive;
        public Material material;
        public bool fixedPotential;


        public Cell(int charge, float electricPotential, bool fixedPotential)
        {
            this.charge = charge;
            this.electricPotential = electricPotential;
            this.electricField = Vector2.zero;
            this.ionLevel = 0f;
            this.conductivity = 0.2f;
            this.isActive = false;
            this.material = Material.air;
            this.fixedPotential = fixedPotential;
        }
    }
    
    void Start()
    {
        InitializeGrid();

        GenerateInitialConditions();

        CalculateElectricField();

        SolvePotentials();

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

    (float potential, bool fixedPotential) GetPotential(int x, int y)
    {
        float groundNoise = Mathf.PerlinNoise(x * 0.05f, 0f);
        float lowerCloudNoise = Mathf.PerlinNoise(x * 0.04f, 10f);
        float upperCloudNoise = Mathf.PerlinNoise(x * 0.04f, 20f);

        int groundTop = 10 + Mathf.RoundToInt(groundNoise * 8f);
        int lowerCloudBottom = 126 + Mathf.RoundToInt(lowerCloudNoise * 8f);
        int lowerCloudTop = 156 + Mathf.RoundToInt(upperCloudNoise * 8f);
    

        if (y <= groundTop)
            return (UnityEngine.Random.Range(0.6f, 0.8f), true); //ground
        
        if (y <= lowerCloudBottom)
            return (UnityEngine.Random.Range(-0.2f, 0.2f), false); //air

        if (y <= lowerCloudTop)
            return (UnityEngine.Random.Range(-1.0f, -0.7f), true); //bottom cloud
        
        return (UnityEngine.Random.Range(0.7f, 1.0f), true); //top cloud
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
                var (potential, fixedPotential) = GetPotential(x, y);
                cells[y,x] = new Cell(0, potential, fixedPotential);
            }
        }
    }

    //Creates the cells and smooths their potentials
    void GenerateInitialConditions()
    {
        CreateCells();
    }

    //Renders the new pixels and updates the texture
    void Render()
    {
        UpdatePixels();

        texture.SetPixels(pixels);
        texture.Apply();
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

                float fieldX = -(right - left) / (2f * CELL_SIZE);
                float fieldY = -(top - down) / (2f * CELL_SIZE);

                cells[y, x].electricField = new Vector2(fieldX, fieldY);
            }
        }
    }

    void SolvePotentials()
    {
        for (int iteration = 0; iteration < 500; iteration++)
        {
            for (int y = 1; y < HEIGHT - 1; y++)
            {
                for (int x = 1; x < WIDTH - 1; x++)
                {
                    if (cells[y, x].fixedPotential)
                    {
                        continue;
                    }

                    float average = (
                        cells[y + 1, x].electricPotential +
                        cells[y - 1, x].electricPotential + 
                        cells[y, x + 1].electricPotential +
                        cells[y, x - 1].electricPotential
                    ) / 4f;

                    cells[y, x].electricPotential = average;
                }
            }
        }
    }
}
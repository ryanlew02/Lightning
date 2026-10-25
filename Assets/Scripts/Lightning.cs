using System;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UI;

public class Lightning : MonoBehaviour
{
    public GameObject cellPrefab;

    public const int HEIGHT = 180;
    public const int WIDTH = 320;
    public const float CELL_SIZE = 50f;

    private float potentialScale = 100000000f;
    private float fieldScale = 100000f;

    public RawImage display;

    Cell[,] cells;
    Texture2D texture;
    Color[] pixels;

    Color negative = new Color(0.1f, 0.3f, 1.0f); // Blue
    Color neutral  = new Color(0.05f, 0.05f, 0.08f); // Near black
    Color positive = new Color(1.0f, 0.15f, 0.1f); // Red
    Color borderColor = new Color(0.27f, 0.33f, 0.41f); // Slate Gray

    public enum ViewMode
    {
        Potential,
        ElectricField,
        ChargeDensity
    }

    public ViewMode viewMode = ViewMode.Potential;

    private enum Material
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


        public Cell(float charge, float electricPotential, bool fixedPotential)
        {
            this.charge = charge;
            this.electricPotential = electricPotential;
            this.fixedPotential = fixedPotential;

            this.electricField = Vector2.zero;
            this.ionLevel = 0f;
            this.conductivity = 0.2f;
            this.isActive = false;
            this.material = Material.air;
    
        }
    }
    
    //runs once at the start of the program
    void Start()
    {
        GenerateInitialConditions();

        SolvePotentialsAndCalculateElectricField();

        Render();
    }
    
    //runs every frame
    void Update()
    {
        Render();
    }

    //returns color based off potential
    Color GetPotentialColor(float potential)
    {
        float normalized = Mathf.Clamp(potential / potentialScale, -1f, 1f);



        if (normalized < 0f)
        {
            return Color.Lerp(neutral, negative, -normalized);
        }
        else
        {
            return Color.Lerp(neutral, positive, normalized);
        }
    }

    //get init potential for cell x, y
    (float potential, bool fixedPotential) GetPotential(int x, int y)
    {
        float groundNoise = Mathf.PerlinNoise(x * 0.08f, 0f);
        float lowerCloudNoise = Mathf.PerlinNoise(x * 0.08f, 10f);
        float upperCloudNoise = Mathf.PerlinNoise(x * 0.08f, 20f);

        int groundTop = 10 + Mathf.RoundToInt(groundNoise * 12f);
        int lowerCloudBottom = 126 + Mathf.RoundToInt(lowerCloudNoise * 12f);
        int lowerCloudTop = 156 + Mathf.RoundToInt(upperCloudNoise * 12f);
    

        if (y <= groundTop)
            return (0.8f, true); //ground
        
        if (y <= lowerCloudBottom)
            return (0.0f, false); //air

        if (y <= lowerCloudTop)
            return (-0.8f, true); //bottom cloud
        
        return (0.8f, true); //top cloud
    }

    //gets the charge for a given cell, using the Gaussian distribution
    float GetCharge(int x, int y)
    {
        float upperPositive = 0.9e-9f * 
            Gaussian(x, y, 160f, 150f, 45f, 12f); //experimental

        float middleNegative = -1.2e-9f * 
            Gaussian(x, y, 160f, 110f, 48f, 13f); //experimental

        float lowerPositive = 0.5e-9f * 
            Gaussian(x, y, 160f, 70f, 28f, 9f); //experimental
        
        return upperPositive + middleNegative + lowerPositive;
    }

    //creates the necessary items for the grid
    void InitializeGrid()
    {
        cells = new Cell[HEIGHT, WIDTH];

        texture = new Texture2D(WIDTH, HEIGHT);
        pixels = new Color[WIDTH * HEIGHT];

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        display.texture = texture;        
    }

    //instantiates each cell and gives it a potential
    void CreateCells()
    {
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                float groundNoise = Mathf.PerlinNoise(x * 0.08f, 0.0f);
                int groundTop = 10 + Mathf.RoundToInt(groundNoise * 12.0f);

                bool isGround = y <= groundTop;

                float charge = isGround ? 0.0f : GetCharge(x, y);
                float potential = 0.0f;

                cells[y,x] = new Cell(charge, potential, isGround);
            }
        }
    }

    //creates the cells and smooths their potentials
    void GenerateInitialConditions()
    {
        InitializeGrid();
        CreateCells();
    }

    //renders the new pixels and updates the texture
    void Render()
    {
        UpdatePixels();

        texture.SetPixels(pixels);
        texture.Apply();
    }

    //gets the potential for each cell and assigns it
    void UpdatePixels()
    {
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                int index = y * WIDTH + x;

                if (x < 2 || x > WIDTH - 3 || y < 2 || y > HEIGHT - 3) //border pixel
                {
                    pixels[index] = borderColor;
                    continue;
                }

                if (viewMode == ViewMode.Potential)
                {
                    pixels[index] = GetPotentialColor(cells[y, x].electricPotential);
                }
                else
                {
                    float strength = cells[y, x].electricField.magnitude;
                    pixels[index] = GetElectricFieldColor(strength);
                }
    
            }
        }
    }

    //create vector2 for each cell for electric field
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

    //calculate air cells potentials
    void SolvePotentials()
    {
        const int maxIterations = 5000;
        const float tolerance = 0.1f;
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            float maxChange = 0f;
        
            for (int y = 1; y < HEIGHT - 1; y++)
            {

                for (int x = 1; x < WIDTH - 1; x++)
                {
                    if (cells[y, x].fixedPotential)
                    {
                        continue;
                    }

                    float oldPotential = cells[y, x].electricPotential;
                    const float epsilon = 8.85418782e-12f * 1.0006f;

                    float average = (
                        cells[y + 1, x].electricPotential +
                        cells[y - 1, x].electricPotential + 
                        cells[y, x + 1].electricPotential +
                        cells[y, x - 1].electricPotential +
                        cells[y, x].charge * CELL_SIZE * CELL_SIZE / epsilon
                    ) / 4f;

                    cells[y, x].electricPotential = average;

                    float change = Mathf.Abs(average - oldPotential);

                    maxChange = Mathf.Max(maxChange, change);
                }
            }

            ApplyBoundryConditions();

            if (maxChange < tolerance)
            {
                //Debug.Log($"Converged after {iteration + 1} iterations");
                break;
            }
        }
    }

    //solve potentials and calculate electric field
    void SolvePotentialsAndCalculateElectricField()
    {
        SolvePotentials();
        CalculateElectricField();
    }

    //get the electric field color
    Color GetElectricFieldColor(float strength)
    {
        float normalized = Mathf.Clamp01(strength / fieldScale);

        return Color.Lerp(Color.black, Color.yellow, normalized);
    }

    //helper to get gaussian
    float Gaussian(float x, float y, float centerX, float centerY, float sigmaX, float sigmaY)
    {
        float dx = (x - centerX) / sigmaX;
        float dy = (y - centerY) / sigmaY;

        return Mathf.Exp(-0.5f * (dx * dx + dy * dy));
    }

    //makes boundry cells have the same conditions as the outline ones
    void ApplyBoundryConditions()
    {
        for (int y = 1; y < HEIGHT - 1; y++)
        {
            cells[y, 0].electricPotential = cells[y, 1].electricPotential;
            cells[y, WIDTH - 1].electricPotential = cells[y, WIDTH - 2].electricPotential;
        }

        for (int x = 0; x < WIDTH - 1; x++)
        {
            cells[0, x].electricPotential = cells[1, x].electricPotential;
            cells[HEIGHT - 1, x].electricPotential = cells[HEIGHT - 2, x].electricPotential;
        }
    }
}
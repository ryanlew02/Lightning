using UnityEngine;
using UnityEngine.UI;

public class Lightning : MonoBehaviour
{
    public GameObject cellPrefab;

    public const int HEIGHT = 180;
    public const int WIDTH = 320;
    public const float CELL_SIZE = 0.05f;

    public RawImage display;

    Cell[,] cells = new Cell[HEIGHT, WIDTH];
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
        }
    }
    
    void Start()
    {
        texture = new Texture2D(WIDTH, HEIGHT);

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        pixels = new Color[WIDTH * HEIGHT];

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {

                // Instantiate each cell with random potential
                float randomPotential = Random.Range(-1f, 1f);
                cells[y,x] = new Cell(0, randomPotential);

                //find 2d index and set the color of the pixel
                int index = y * WIDTH + x;
                pixels[index] = GetPotentialColor(cells[y, x].electricPotential);

            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        display.texture = texture;

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
}
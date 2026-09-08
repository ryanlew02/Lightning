using UnityEngine;

public class Lightning : MonoBehaviour
{
    public GameObject cellPrefab;

    public const int HEIGHT = 90;
    public const int WIDTH = 160;
    public const float CELL_SIZE = 0.1f;
    
    Cell[,] cells = new Cell[WIDTH, HEIGHT];
    SpriteRenderer[,] renderers = new SpriteRenderer[WIDTH, HEIGHT];

    Color strongNegative = new Color(0.08f, 0.15f, 0.45f); 
    Color mildNegative   = new Color(0.20f, 0.55f, 0.95f); 
    Color neutral        = new Color(0.03f, 0.04f, 0.08f); 
    Color mildPositive   = new Color(1.00f, 0.45f, 0.10f); 
    Color strongPositive = new Color(1.00f, 0.92f, 0.45f);

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
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                float randomPotential = Random.Range(-1f, 1f);
                cells[x,y] = new Cell(0, randomPotential);

                float xPos = (x - (WIDTH - 1) / 2f) * CELL_SIZE;
                float yPos = (y - (HEIGHT - 1) / 2f) * CELL_SIZE;

                Vector3 position = new Vector3(xPos, yPos, 0);

                GameObject square = Instantiate(cellPrefab, position, Quaternion.identity);
                square.transform.localScale = new Vector3(CELL_SIZE, CELL_SIZE, 1);
                renderers[x,y] = square.GetComponent<SpriteRenderer>();
                renderers[x,y].color = GetPotentialColor(cells[x,y].electricPotential);
            }
        }
    }

    
    void Update()
    {
        
    }

    Color GetPotentialColor(float potential)
    {
        if (potential <= -0.75f)
        {
            return strongNegative;
        }
        else if (potential < -0.25f)
        {
            return mildNegative;
        }
        else if (potential < 0.25f)
        {
            return neutral;
        }
        else if (potential < 0.75f)
        {
            return mildPositive;
        }
        else
        {
            return strongPositive;
        }
    }
}
using UnityEngine;

public class Lightning : MonoBehaviour
{
    public int HEIGHT = 90;
    public int WIDTH = 160;

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

        public Cell()
        {
            charge = 0f;
            electricPotential = 0f;
            ionLevel = 0f;
            conductivity = 0.2f;
            isActive = false;
            material = air;
        }
    }
    
    void Start()
    {
        Cell[,] cells = new Cell[WIDTH, HEIGHT];
    }

    
    void Update()
    {
        
    }
}

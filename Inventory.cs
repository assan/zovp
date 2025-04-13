using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Transform transformInv;
    public int ScalX;
    public int ScalY;
    public Cell CellPref;
    public Cell[,] cells;
    public Item DragedItem;
    // ����� ����������
    public EquipmentSlot headSlot;
    public EquipmentSlot ArmorSlot;
    public EquipmentSlot ClothesSlot;
    public EquipmentSlot weapon1Slot;
    public EquipmentSlot weapon2Slot;
    private bool isInitialized = false; // ����, ����� �����������, ���� �� ������ �������
    public static Inventory instance;




    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // ��������� ����� ����������
        if (headSlot == null) Debug.LogWarning("HeadSlot is not assigned in Inventory!");
        if (ArmorSlot == null) Debug.LogWarning("TorsoSlot is not assigned in Inventory!");
        if (ClothesSlot == null) Debug.LogWarning("ArmsSlot is not assigned in Inventory!");
        if (weapon2Slot == null) Debug.LogWarning("LegsSlot is not assigned in Inventory!");
        if (weapon1Slot == null) Debug.LogWarning("WeaponSlot is not assigned in Inventory!");


    }

    void Start()
    {

        // ���� ��������� ���������� �������, ������ ������
        if (gameObject.activeInHierarchy)
        {
            CreateNewInventory();
            ClearInventoryCells(); // ����������� ��� ������ ��� ������
        }


    }


    void Update()
    {

    }
    public void InitializeInventory()
    {
        if (!isInitialized)
        {
            CreateNewInventory();
            isInitialized = true;
        }
    }
    void CreateNewInventory()
    {
        cells = new Cell[ScalX, ScalY];
        for (int y = 0; y < ScalY; y++)
        {
            for (int x = 0; x < ScalX; x++)
            {
                var newCell = Instantiate(CellPref, transformInv);
                newCell.Initialized(this);
                newCell.x = x;
                newCell.y = y;
                newCell.isFree = true;
                newCell.cellIndex.text = x + " " + y;
                cells[x, y] = newCell;
            }
        }
    }
    public void Coloring(Cell cell, Vector2Int size, Color color)
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (cell.x + i < ScalX && cell.y + j < ScalY)
                {
                    cells[cell.x + i, cell.y + j].image.color = color;
                }
            }
        }
    }
    public void UpdateCellsColor(bool showOccupied = false)
    {
        for (int y = 0; y < ScalY; y++)
        {
            for (int x = 0; x < ScalX; x++)
            {
                if (showOccupied && !cells[x, y].isFree)
                {
                    cells[x, y].image.color = Color.red;
                }
                else
                {
                    cells[x, y].image.color = Color.black;
                }
            }
        }
    }
    public bool CheckCellFree(Cell cell, Vector2Int size)
    {
        int width = size.x;
        int height = size.y;

        for (int y = cell.y; y < cell.y + height; y++)
        {
            for (int x = cell.x; x < cell.x + width; x++)
            {
                if (x >= ScalX || y >= ScalY || x < 0 || y < 0)
                {
                    Debug.LogWarning($"CheckCellFree: Index out of bounds at ({x}, {y})");
                    return false;
                }
                if (!cells[x, y].isFree)
                {
                    Debug.Log($"CheckCellFree: Cell ({x}, {y}) is not free");
                    return false;
                }
            }
        }
        return true;
    }
    public void CellOkupation(Cell cell, Vector2Int size, bool free)
    {
        if (cell == null)
        {
            Debug.LogWarning("CellOkupation called with null cell!");
            return;
        }

        int width = size.x;
        int height = size.y;

        for (int y = cell.y; y < cell.y + height; y++)
        {
            for (int x = cell.x; x < cell.x + width; x++)
            {
                if (x >= 0 && x < ScalX && y >= 0 && y < ScalY)
                {
                    cells[x, y].isFree = free;
                    Debug.Log($"Cell ({x}, {y}) set to isFree = {free}");
                }
                else
                {
                    Debug.LogWarning($"CellOkupation: Index out of bounds at ({x}, {y})");
                }
            }
        }
    }
    public void ClearInventoryCells()
    {
        for (int y = 0; y < ScalY; y++)
        {
            for (int x = 0; x < ScalX; x++)
            {
                cells[x, y].isFree = true;
            }
        }
    }
    public void AddItemToInventory(Item item, Cell targetCell)
    {
        if (CheckCellFree(targetCell, item.GetSize()))
        {
            item.transform.SetParent(transformInv);
            item.SetPosition(item, targetCell);
            item.prefcell = targetCell;
            item.lastInventoryCell = targetCell;
            CellOkupation(targetCell, item.GetSize(), false);
            UpdateCellsColor(true);
        }
    }

}
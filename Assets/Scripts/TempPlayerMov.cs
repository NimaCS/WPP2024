using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 PlayerInput;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float slowSpeed;
    [SerializeField] private InputAction playerMovement;
    [SerializeField] private InputAction interact;
    [SerializeField] private List<PSeasonSO> pSeasons;
    [SerializeField] private SeasonTilesSO fallTiles;
    [SerializeField] private SeasonTilesSO winterTiles;
    [SerializeField] private SeasonTilesSO springTiles;
    [SerializeField] private SeasonTilesSO summerTiles;
    [SerializeField] private Tilemap displayTilemap;

    private void OnEnable()
    {
        playerMovement.Enable();
        interact.Enable();
    }

    private void OnDisable()
    {
        playerMovement.Disable();
        interact.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //PlayerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        PlayerInput = playerMovement.ReadValue<Vector2>().normalized;
    }

    void FixedUpdate()
    {
        Movem();
    }

    private void Movem()
    {
        float move = slowSpeed;
        Season season = SeasonIn();

        if (season != Season.None)
        {
            for (int i = 0; i < pSeasons.Count; i++)
            {
                string seasonName = pSeasons[i].seasonName;

                if (seasonName == "Fall" && season == Season.Fall)
                    move = sprintSpeed;
                else if (seasonName == "Winter" && season == Season.Winter)
                    move = sprintSpeed;
                else if (seasonName == "Spring" && season == Season.Spring)
                    move = sprintSpeed;
                else if (seasonName == "Summer" && season == Season.Summer)
                    move = sprintSpeed;
            }
        }
        else if (season == Season.None)
            move = moveSpeed;

        Vector2 moveForce = PlayerInput * move;
        rb.velocity = moveForce;
    }

    private Season SeasonIn()
    {
        Vector3Int tilePos = displayTilemap.WorldToCell(transform.position);
        TileBase tileType = displayTilemap.GetTile(tilePos);
        Season season = Season.None;

        for (int i = 0; i < fallTiles.tiles.Count; i++)
        {
            if (tileType == fallTiles.tiles[i])
                season = Season.Fall;
        }
        for (int i = 0; i < winterTiles.tiles.Count; i++)
        {
            if (tileType == winterTiles.tiles[i])
                season = Season.Winter;
        }
        for (int i = 0; i < springTiles.tiles.Count; i++)
        {
            if (tileType == springTiles.tiles[i])
                season = Season.Spring;
        }
        for (int i = 0; i < summerTiles.tiles.Count; i++)
        {
            if (tileType == summerTiles.tiles[i])
                season = Season.Summer;
        }

        return season;
    }

    private enum Season
    {
        None,
        Fall,
        Winter,
        Spring,
        Summer
    }
}
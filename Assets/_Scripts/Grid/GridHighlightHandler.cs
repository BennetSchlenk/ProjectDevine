using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class GridHighlightHandler : MonoBehaviour
{
    [SerializeField][ColorUsage(true, true)] private Color availableColor = Color.green;
    [SerializeField][ColorUsage(true, true)] private Color unavailableColor = Color.red;
    [SerializeField][ColorUsage(true, true)] private Color upgradeColor = Color.magenta;

    private Grid grid;

    private void Start()
    {
        grid = GetComponent<Grid>();
        GlobalData.OnCardDragged += OnCardDragged;
    }

    private void OnDestroy()
    {
        GlobalData.OnCardDragged -= OnCardDragged;
    }

    public void HideCells()
    {
        // Iterate through all the grid nodes and hide the highlight cell
        for (int x = 0; x < grid.GridSize.x; x++)
        {
            for (int y = 0; y < grid.GridSize.y; y++)
            {
                grid.GridNodes[x, y].HighlightCell.gameObject.SetActive(false);
            }
        }
    }

    public void ShowCellsForTowerPlacing(CardDataSO cardDataSO)
    {
        // Nodes that should be highlighted in green
        // - Buildable
        // - Not occupied by a tower
        // - Not occupied by an enemy spawn point
        // - Not occupied by an enemy target point
        // - Not walkable
        // - Occupied by a tower that can be upgraded

        for (int x = 0; x < grid.GridSize.x; x++)
        {
            for (int y = 0; y < grid.GridSize.y; y++)
            {
                GridNode node = grid.GridNodes[x, y];
                if (!node.Buildable)
                {
                    node.HighlightCell.SetColor(unavailableColor);
                    node.HighlightCell.gameObject.SetActive(!node.Walkable);
                    if (node.TowerObj != null && node.TowerObj.GetComponent<Tower>().CanUpgradeTower(cardDataSO))
                    {
                        node.HighlightCell.SetColor(upgradeColor);
                        
                    }
                }
                else if (node.TowerObj != null)
                {
                    if (node.TowerObj.GetComponent<Tower>().CanUpgradeTower(cardDataSO))
                    {
                        node.HighlightCell.SetColor(availableColor);
                        node.HighlightCell.gameObject.SetActive(true);
                    }
                    else
                    {
                        node.HighlightCell.SetColor(unavailableColor);
                        node.HighlightCell.gameObject.SetActive(true);
                    }
                }
                else
                {
                    node.HighlightCell.SetColor(availableColor);
                    node.HighlightCell.gameObject.SetActive(true);
                }
            }
        }
    }

    [ContextMenu("ShowCellsForModifierCards")]
    public void ShowCellsForModifierCards()
    {
        // Nodes that should be highlighted in green
        // - Tower has empty modifier slots

        for (int x = 0; x < grid.GridSize.x; x++)
        {
            for (int y = 0; y < grid.GridSize.y; y++)
            {
                GridNode node = grid.GridNodes[x, y];
                if (node.TowerObj != null)
                {
                    if (node.TowerObj.GetComponent<Tower>().CanApplyModifier())
                    {
                        node.HighlightCell.SetColor(upgradeColor);
                        node.HighlightCell.gameObject.SetActive(true);
                    }
                    else
                    {
                        node.HighlightCell.SetColor(unavailableColor);
                        node.HighlightCell.gameObject.SetActive(true);
                    }
                } else if (node.Walkable)
                {
                    node.HighlightCell.gameObject.SetActive(false);
                } else
                {
                    node.HighlightCell.SetColor(unavailableColor);
                    node.HighlightCell.gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnCardDragged(CardDataSO cardDataSO)
    {
        if (cardDataSO == null)
        {
            HideCells();
        }
        else
        {
            switch (cardDataSO.Type)
            {
                case CardType.Tower:
                    ShowCellsForTowerPlacing(cardDataSO);
                    break;
                case CardType.Modifier:
                    ShowCellsForModifierCards();
                    break;
            }
        }
    }
}

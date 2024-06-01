using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData
{
    public const float GridNodeSize = 2;
    public const string DefaultLevel = "DefaultLevel";
    public const string DefaultTheme = "DefaultTheme";
    public const string DefaultEnemyWaves = "DefaultEnemyWaves";

    public static Action<CardDataSO> OnCardDragged;
    public static Action<Tower> OnTowerSelected;
    public static HandController HandController;
}

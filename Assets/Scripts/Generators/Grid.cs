using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{   
    private Vector2Int grid;
    public int x => grid.x; // width
    public int y => grid.y; // height

    
    public Grid(int segmentsCount) {
        grid = CalculateGridSize(segmentsCount);
    }

    private Vector2Int CalculateGridSize(int segmentsCount) {
        int bestWidth = 1;
        int bestHeight = segmentsCount;
        int bestDiff = segmentsCount;

        for (int width = 1; width*width <= segmentsCount; width++) {
            if (segmentsCount % width != 0) {
                continue;
            }

            int height = segmentsCount / width;
            int diff = Mathf.Abs(width - height);

            if (diff < bestDiff) {
                bestDiff = diff;
                bestWidth = width;
                bestHeight = height;
            }
        }
        
        return new Vector2Int(bestWidth, bestHeight);
    }
}

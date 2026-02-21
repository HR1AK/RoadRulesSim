public class PlacedPiece
{
    public RoadPiece Prefab;
    public int RotationSteps; // 0,1,2,3 (0°,90°,180°,270°)

    public PlacedPiece(RoadPiece prefab, int rotationSteps)
    {
        Prefab = prefab;
        RotationSteps = rotationSteps;
    }
}
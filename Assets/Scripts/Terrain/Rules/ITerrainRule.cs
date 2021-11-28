namespace Terrain
{
    public interface ITerrainRule
    {
        bool IsValid(char[,] context);
    }
}
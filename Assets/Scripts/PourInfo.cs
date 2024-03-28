public class PourInfo
{
    public Bottle chosenBottle;
    public Bottle targetBottle;
    public LiquidType liquidType;
    public Direction direction;
    public int level;

    public PourInfo(Bottle chosenBottle, Bottle targetBottle, int level, LiquidType liquidType, Direction direction)
    {
        this.chosenBottle = chosenBottle;
        this.targetBottle = targetBottle;
        this.level = level;
        this.liquidType = liquidType;
        this.direction = direction;
    }

}
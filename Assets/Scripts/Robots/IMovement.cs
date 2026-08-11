using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum RobotCommand
{
    None = 0,
    TurnRight = 1,
    TurnLeft = 2,
}

//Movement system for robots
public abstract class IMovement : MonoBehaviour
{
    public Vector2Int position;
    public Vector2Int direction;
    public int health;
    public List<RobotCommand> on_flag;

    //Logic for when robot is placed
    public abstract void PlaceRobot(Vector2Int placement, Vector2Int direction);

    //Logic for moving robot, and what happens when it is moved to a new cell
    public abstract void MoveTo(Vector2Int new_pos);

    //Called for every step this robot takes
    public abstract void TakeStep();

    //Logic for when robot leaves the grid
    public abstract void LeaveGrid();

    //Logic for when robot steps on a mine
    public abstract void StepOnMine();

    public abstract void OnSeeFlag();
}

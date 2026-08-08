using System.Collections;
using UnityEngine;

public class DiggerBot : IMovement
{
    public void Start()
    {
        health = 1;
    }
    public override void LeaveGrid()
    {
        RobotController.Instance.ClaimRobot(gameObject);
    }

    public override void MoveTo(Vector2Int new_pos)
    {
        position = new_pos;
        if (MinesweeperLogic.Instance.IsValid(position.x, position.y) == false) { LeaveGrid(); return; }

        Cell new_cell = MinesweeperLogic.Instance.GetCell(new_pos.x, new_pos.y);
        transform.position = MinesweeperLogic.Instance.cellToWorldCoordinate(new_cell);
        if (new_cell.revealed == false)
        {
            MinesweeperLogic.Instance.Reveal(new_cell);
            if (new_cell.type == Cell.Type.Mine)
            {
                StepOnMine();
            }
        }
    }

    public override void PlaceRobot(Vector2Int placement, Vector2Int direction)
    {
        this.direction = direction;
        MoveTo(placement);
        StartCoroutine(ExecuteRobot());
    }

    IEnumerator ExecuteRobot()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            TakeStep();
        }
    }

    public override void TakeStep()
    {
        Vector2Int new_pos = position + direction;
        MoveTo(new_pos);
    }

    public override void StepOnMine()
    {
        health -= 1;
        if (health <= 0) Destroy(gameObject);
    }
}

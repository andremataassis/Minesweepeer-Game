using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//DiggerBot is the most basic bot. It can follow commands based on triggers, but other than that
//it just moves forward each step.
public class DiggerBot : IMovement
{
    private Coroutine myCoroutine;
    
    public void Awake()
    {
        SetHealth(1);
        on_flag = new List<RobotCommand>();
        gameObject.name = "DiggerBot" + Random.Range(0, 100);
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
        myCoroutine = StartCoroutine(ExecuteRobot());
    }
    IEnumerator ExecuteRobot()
    {
        while (true)
        {
            //Purely visual: make robot face direction
            transform.rotation = Quaternion.LookRotation(Vector3.forward, (Vector2)direction);

            yield return new WaitForSeconds(0.5f);
            TakeStep();
        }
    }

    public override void TakeStep()
    {
        Vector2Int new_pos = position + direction;
        Cell new_cell = MinesweeperLogic.Instance.GetCell(new_pos.x, new_pos.y);

        //Command triggers
        if (new_cell.flagged) OnSeeFlag();

        //Recalculate new position b/c robot commands may have changed it
        Vector2Int post_command_pos = position + direction;

        //Turning takes a step, we only call MoveTo() if the robot's direction didn't change
        if (post_command_pos != new_pos) return;
        MoveTo(new_pos);
    }

    public override void StepOnMine()
    {
        LoseHealth();
        if (GetHealth() <= 0) Destroy(gameObject);
    }

    public override void PauseRobot(bool pause)
    {
        if (pause && myCoroutine != null) { StopCoroutine(myCoroutine); myCoroutine = null;  }
        else myCoroutine = StartCoroutine(ExecuteRobot());
    }
}

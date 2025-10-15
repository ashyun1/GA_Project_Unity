using UnityEngine;
using System.Collections.Generic;

public class DungeonCombatManager : MonoBehaviour
{
    public class UnitTurnData
    {
        public string Name { get; private set; }
        public int Speed { get; private set; }

        public UnitTurnData(string name, int speed)
        {
            Name = name;
            Speed = speed;
        }
    }

    private Dictionary<string, (UnitTurnData data, float nextTime)> _unitState =
        new Dictionary<string, (UnitTurnData, float)>();

    private SimplePriorityQueue<string> _turnTimeQueue = new SimplePriorityQueue<string>();

    private const float TURN_BASE_TIME = 100f;
    private int turnCount = 1;

    void Start()
    {
        var units = new List<UnitTurnData>
        {
            new UnitTurnData("전사", 5),
            new UnitTurnData("마법사", 7),
            new UnitTurnData("궁수", 10),
            new UnitTurnData("도적", 12)
        };

        foreach (var unit in units)
        {
            _unitState[unit.Name] = (unit, 0f);

            float initialPriority = 0f - (unit.Speed / 1000f);

            _turnTimeQueue.Enqueue(unit.Name, initialPriority);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteNextTurn();
        }
    }

    private void ExecuteNextTurn()
    {
        if (_turnTimeQueue.Count == 0) return;

       
        string unitName = _turnTimeQueue.Dequeue(out float minTime);

        UnitTurnData currentUnitData = _unitState[unitName].data;

        Debug.Log($"{turnCount}턴 / {currentUnitData.Name}의 턴입니다.");

        float timeToNextTurn = TURN_BASE_TIME / currentUnitData.Speed;
        float newNextTime = minTime + timeToNextTurn;

        _unitState[unitName] = (currentUnitData, newNextTime);

        _turnTimeQueue.Enqueue(unitName, newNextTime);

        turnCount++;
    }
}
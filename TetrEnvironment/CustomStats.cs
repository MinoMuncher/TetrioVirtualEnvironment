using TetrEnvironment.Constants;

namespace TetrEnvironment;

public class CustomStats
{
    public int linesCleared { get;  set; }
    public int downstackCleared { get;  set; }
    public int keypresses { get;  set; }
    public List<int> attack { get;  set; } = new();
    public string type { get;  set; } = "NONE";
    public Tetromino.MinoType shape {get; set;} = Tetromino.MinoType.Empty;
    public int combo { get;  set; }
    public int BTBChain { get;  set; }
    public bool BTBClear { get;  set; }
    public double frameDelay { get;  set; }
    public double frameStamp;
    public List<int> attackRecieved { get;  set; } = new();
    public List<int> attackTanked { get;  set; } = new();

    public int lastAttackCol = -1;
    public Tetromino.MinoType[] ?board { get;  set; }

    public Tetromino.MinoType[] ?queue { get; set;}


    public CustomStats Clone()
    {
        return new CustomStats{
            linesCleared=linesCleared,
            downstackCleared=downstackCleared,
            keypresses=keypresses,
            attack=attack.ToList(),
            type=type,
            shape=shape,
            combo=combo,
            BTBChain=BTBChain,
            BTBClear=BTBClear,
            frameDelay=frameDelay,
            frameStamp=frameStamp,
            attackRecieved=attackRecieved.ToList(),
            attackTanked=attackTanked.ToList(),
            board= board==null ? null : (Tetromino.MinoType[])board.Clone(),
            queue = queue==null ? null : (Tetromino.MinoType[])queue.Clone(),
        };
    }
}
using UnityEngine;
using Puzzle;
using static ViewUtility;
using Cysharp.Threading.Tasks;
using LitMotion;

public class BlockViewer : MonoBehaviour, IViewer, IMovable
{
    [SerializeField] Map map;
    [SerializeField] PuzzleComponent comp;
    [SerializeField] float MoveDuration = 0.1f;

    void Awake()
    {
    }
    public async UniTask Process(IEvent component)
    {
        throw new System.NotImplementedException();
    }
    public async UniTask Return(IEvent component)
    {
        throw new System.NotImplementedException();
    }
    public async UniTask PropagateMove(Point from, Point to)
    {
        await LMotion.Create(PointToVector3(from), PointToVector3(to), MoveDuration)
                .WithEase(Ease.Linear)
                .Bind(x => transform.position = x.Pixelize(8));
    }
}
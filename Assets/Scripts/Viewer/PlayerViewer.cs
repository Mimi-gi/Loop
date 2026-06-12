using UnityEngine;
using Puzzle;
using static ViewUtility;
using Cysharp.Threading.Tasks;
using LitMotion;
using System.Linq;

public class PlayerViewer : MonoBehaviour, IViewer
{
    [SerializeField] float moveDuration = 0.2f;
    [SerializeField] Ease ease;
    [SerializeField] Animator _animator;
    [SerializeField] AnimationClip[] _animationClips;
    [SerializeField] AnimationClip[] _DisConnectClips;
    AsyncAnimator animator;
    AsyncAnimationClip[] clips;
    AsyncAnimationClip[] disConnectClips;

    void Awake()
    {
        animator = new AsyncAnimator(_animator);
        clips = _animationClips.Select(c => new AsyncAnimationClip(c)).ToArray();
        disConnectClips = _DisConnectClips.Select(c => new AsyncAnimationClip(c)).ToArray();
    }
    public async UniTask Connect()
    {
        await UniTask.CompletedTask;
    }
    public async UniTask Process(IEvent events)
    {
        switch (events.Type)
        {
            case Puzzle.EventType.PLMove:
                var moveEvent = events as PLMoveEvent;
                await LMotion.Create(0f, 1f, moveDuration)
                    .WithEase(ease)
                    .Bind(x => transform.position = Vector3.Lerp(PointToVector3(moveEvent.From), PointToVector3(moveEvent.To), x).Pixelize(8));
                break;
            case Puzzle.EventType.PLConnect:
                var connectEvent = events as PLConnectEvent;
                await ResolveAnimation(connectEvent.ConnectDirections);
                break;
            case Puzzle.EventType.PLDisConnect:
                var disConnectEvent = events as PLDisConnectEvent;
                await ResolveDisConnectAnimation(disConnectEvent.PreConnectDirections);
                break;
            default:
                break;
        }
        await UniTask.CompletedTask;
    }
    public async UniTask Return(IEvent events)
    {
        await UniTask.CompletedTask;
    }

    public async UniTask ResolveAnimation((bool up, bool down, bool left, bool right) isConnected)
    {
        string animationName = "Connect";
        if (isConnected.up) animationName += "D";
        if (isConnected.down) animationName += "U";
        if (isConnected.right) animationName += "R";
        if (isConnected.left) animationName += "L";
        var clip = clips.FirstOrDefault(c => c.Name == animationName);
        await animator.PlayAsync(clip);
    }

    public async UniTask ResolveDisConnectAnimation((bool up, bool down, bool left, bool right) isDisconnected)
    {
        string animationName = "DisConnect";
        if (isDisconnected.up) animationName += "D";
        if (isDisconnected.down) animationName += "U";
        if (isDisconnected.right) animationName += "R";
        if (isDisconnected.left) animationName += "L";
        var clip = disConnectClips.FirstOrDefault(c => c.Name == animationName);
        await animator.PlayAsync(clip);
    }

}
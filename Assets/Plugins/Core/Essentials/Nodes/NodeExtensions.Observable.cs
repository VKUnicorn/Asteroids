using Cysharp.Threading.Tasks;
using R3;

namespace Core {
    public static class NodeExtensions_Observable {
        public static UniTask WaitForActivation(this Node node) {
            return node.InternalState.Value == NodeStateEnum.Active 
                       ? UniTask.CompletedTask 
                       : node.InternalState.Where(x => x == NodeStateEnum.Active).FirstAsync(cancellationToken: node.GetCancellationToken()).AsUniTask();
        }

        public static Observable<Unit> GetActivationObservable(this Node node, bool skipLatestValueOnSubscribe = false) {
            var result = skipLatestValueOnSubscribe == false ? node.State.Skip(1) : node.State;
            return result.Where(x => x == NodeStateEnum.Active).AsUnitObservable();
        }
    }
}
using Leopotam.EcsLite;
using UnityEngine;

public sealed class ProgressSystem : IEcsInitSystem, IEcsRunSystem
{
    private EcsWorld _world;
    private EcsFilter _progressFilter;
    private EcsPool<ProgressComponent> _progressPool;
    private EcsPool<ProgressEventComponent> _progressEventPool;
    private EcsPool<LevelComponent> _levelPool;
    private EcsPool<ProgressProcessedFlag> _progressProcessedFlagPool;
    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _progressFilter = _world.Filter<ProgressComponent>().End();
        _progressPool = _world.GetPool<ProgressComponent>();
        _progressEventPool = _world.GetPool<ProgressEventComponent>();
        _levelPool = _world.GetPool<LevelComponent>();
        _progressProcessedFlagPool = _world.GetPool<ProgressProcessedFlag>();
        LoadProgress();
    }

    public void Run(IEcsSystems systems)
    {
        CheckFlags();
        CalculateProgress();
    }

    private void LoadProgress()
    {
        foreach (var entity in _progressFilter)
        {
            ref var progress = ref _progressPool.Get(entity);
            var key = $"Progress_{progress._businessId}";

            if (PlayerPrefs.HasKey(key))
            {
                progress._currentProgress = PlayerPrefs.GetFloat(key);
                float t = Mathf.Clamp01(progress._currentProgress / progress._maxProgress);
                progress._businessView.UpdateProgress(t);
            }
        }
    }
    private void CheckFlags()
    {
        foreach (var entity in _progressFilter)
        {
            if (_progressProcessedFlagPool.Has(entity))
            {
                _progressProcessedFlagPool.Del(entity);
                _progressEventPool.Del(entity);
            }
        }
    }
    private void CalculateProgress()
    {
        foreach (var entity in _progressFilter)
        {
            ref var progress = ref _progressPool.Get(entity);
            ref var level = ref _levelPool.Get(entity);

            if (level._level <= 0)
                return;

            progress._currentProgress += Time.deltaTime;
            float t = Mathf.Clamp01(progress._currentProgress / progress._maxProgress);
            progress._businessView.UpdateProgress(t);

            if (progress._currentProgress >= progress._maxProgress)
            {
                progress._currentProgress = 0;
                TriggerProgressCompleteEvent(entity);
            }
        }
    }
    //send event for IncomeCalculateSystem
    private void TriggerProgressCompleteEvent(int entity)
    {
        ref var progressEvent = ref _progressEventPool.Add(entity);
        progressEvent._ecsPackedEntity = _world.PackEntity(entity);
    }
}

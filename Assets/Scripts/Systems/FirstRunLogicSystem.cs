using Leopotam.EcsLite;

public sealed class FirstRunLogicSystem : IEcsInitSystem
{
    private EcsWorld _world;

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();

        foreach (var entity in _world.Filter<BusinessComponent>().End())
        {
            ref var business = ref _world.GetPool<BusinessComponent>().Get(entity);
            ref var lvl = ref _world.GetPool<LevelComponent>().Get(entity);

            if (business._id == 0 && lvl._level == 0)
            {
                LvlUpEvent(entity);
            }
        }
    }

    private void LvlUpEvent(int entity)
    {
        ref var lvlUpEvent = ref _world.GetPool<LvlUpEventComponent>().Add(_world.NewEntity());
        lvlUpEvent._businessId = 0;
        lvlUpEvent._ecsPackedEntity = _world.PackEntity(entity);
    }
}

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace BovineLabs.EntityLinks
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct EntityLinkResolverSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new ResolveLinksJob
            {
                StoreLookup = SystemAPI.GetBufferLookup<EntityLookupStoreBuffer>(true),
                ParentLookup = SystemAPI.GetComponentLookup<Parent>(true),
                TargetsLookup = SystemAPI.GetComponentLookup<EntityLinkTargets>(true)
            };

            // Schedule parallel for high performance entity resolution
            job.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ResolveLinksJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<EntityLookupStoreBuffer> StoreLookup;
        [ReadOnly] public ComponentLookup<Parent> ParentLookup;
        [ReadOnly] public ComponentLookup<EntityLinkTargets> TargetsLookup;

        private void Execute(
            Entity entity,
            in DynamicBuffer<EntityLookupRequestBuffer> requests,
            ref DynamicBuffer<EntityLookupResolveResult> results,
            EnabledRefRW<EntityLookupResolvedThisFrame> resolveTrigger)
        {
            results.Clear();

            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                var rule = request.ResolveRule;
                Entity found = Entity.Null;

                // 1. Self
                if (found == Entity.Null && (rule & ResolveRule.SelfTarget) != 0)
                {
                    found = TryFindInStore(entity, request.Key);
                }

                // 2. Parent Traversal (Walks up the transform hierarchy)
                if (found == Entity.Null && (rule & ResolveRule.Parent) != 0)
                {
                    Entity current = entity;
                    int depth = 0;
                    while (ParentLookup.TryGetComponent(current, out var parent) && depth < 64)
                    {
                        current = parent.Value;
                        found = TryFindInStore(current, request.Key);
                        if (found != Entity.Null) break;
                        depth++;
                    }
                }

                // 3. EntityLinkTargets Traversal (Owner, Source, Target)
                if (found == Entity.Null && TargetsLookup.TryGetComponent(entity, out var targets))
                {
                    if (found == Entity.Null && (rule & ResolveRule.Owner) != 0)
                        found = TryFindInStore(targets.Owner, request.Key);

                    if (found == Entity.Null && (rule & ResolveRule.Source) != 0)
                        found = TryFindInStore(targets.Source, request.Key);

                    if (found == Entity.Null && (rule & ResolveRule.Target) != 0)
                        found = TryFindInStore(targets.Target, request.Key);
                }

                // 4. ParentsTarget (Look at the Target of the Parent)
                if (found == Entity.Null && (rule & ResolveRule.ParentsTarget) != 0)
                {
                    if (ParentLookup.TryGetComponent(entity, out var parent))
                    {
                        if (TargetsLookup.TryGetComponent(parent.Value, out var parentTargets))
                        {
                            found = TryFindInStore(parentTargets.Target, request.Key);
                        }
                    }
                }

                // Store the result (even if Entity.Null, to maintain state)
                results.Add(new EntityLookupResolveResult 
                { 
                    Key = request.Key, 
                    Value = found 
                });
            }

            // Disable the trigger so we don't resolve again until requested by gameplay systems
            resolveTrigger.ValueRW = false;
        }

        // Helper to iterate the store buffer and find a matching key
        private Entity TryFindInStore(Entity targetEntity, byte key)
        {
            if (targetEntity != Entity.Null && StoreLookup.TryGetBuffer(targetEntity, out var store))
            {
                for (int i = 0; i < store.Length; i++)
                {
                    if (store[i].Key == key) 
                        return store[i].Value;
                }
            }
            return Entity.Null;
        }
    }
}
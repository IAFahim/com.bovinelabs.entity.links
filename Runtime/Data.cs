using System;
using Unity.Entities;

namespace BovineLabs.EntityLinks
{
    [InternalBufferCapacity(4)]
    public struct EntityLookupStoreBuffer : IBufferElementData
    {
        public byte Key;
        public Entity Value;
    }

    public struct EntityLookupRequestBuffer : IBufferElementData
    {
        public byte Key;
        public ResolveRule ResolveRule;
    }

    public struct EntityLookupResolvedThisFrame : IComponentData, IEnableableComponent { }

    public struct EntityLookupResolveResult : IBufferElementData
    {
        public byte Key;
        public Entity Value;
    }

    public struct EntityLinkTargets : IComponentData
    {
        public Entity Owner;
        public Entity Source;
        public Entity Target;
    }

    [Flags]
    public enum ResolveRule : byte
    {
        None = 0,
        Parent = 1 << 0,
        ParentsTarget = 1 << 1,
        SelfTarget = 1 << 2,
        Owner = 1 << 3,
        Source = 1 << 4,
        Target = 1 << 5
    }
}
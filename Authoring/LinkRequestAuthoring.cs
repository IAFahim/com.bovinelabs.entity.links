using System;
using BovineLabs.Core.Keys;
using BovineLabs.Reaction.Data.Core;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.EntityLinks.Authoring
{
    public class LinkRequestAuthoring : MonoBehaviour
    {
        public EntityLinkRequestBufferBakeData[] entityLinkLookupBufferBakeDatas = Array.Empty<EntityLinkRequestBufferBakeData>();
        public bool resolveAtStart;

        [Serializable]
        public class EntityLinkRequestBufferBakeData
        {
            [K(nameof(EntityLinkKeys))] public byte key;

            public ResolveRule resolveRule = ResolveRule.Parent | ResolveRule.Owner;
            public Target assignTo = Target.Target;

            public EntityLookupRequestBuffer ToEntityLookupStoreBuffer()
            {
                return new EntityLookupRequestBuffer
                {
                    Key = key,
                    ResolveRule = resolveRule,
                    AssignTo = assignTo
                };
            }
        }

        public class LinkComponentBaker : Baker<LinkRequestAuthoring>
        {
            public override void Bake(LinkRequestAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var requests = AddBuffer<EntityLookupRequestBuffer>(entity);

                foreach (var b in authoring.entityLinkLookupBufferBakeDatas) requests.Add(b.ToEntityLookupStoreBuffer());

                AddComponent<EntityLookupRequestedThisFrame>(entity);
                SetComponentEnabled<EntityLookupRequestedThisFrame>(entity, authoring.resolveAtStart);

                AddBuffer<EntityLookupResolveResult>(entity);
            }
        }
    }
}
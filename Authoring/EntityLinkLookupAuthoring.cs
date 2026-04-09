using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.EntityLinks.Authoring
{
    public class EntityLinkLookupAuthoring : MonoBehaviour
    {
        public EntityTagAuthoring[] links = Array.Empty<EntityTagAuthoring>();
        private void OnValidate() => links = GetComponentsInChildren<EntityTagAuthoring>(true);

        public class EntityLinkLookupResolverBaker : Baker<EntityLinkLookupAuthoring>
        {
            public override void Bake(EntityLinkLookupAuthoring authoring)
            {
                var entries = new List<EntityLookupStoreData>();

                foreach (var entityTagsMonoBehavior in authoring.links)
                {
                    entries.Add(new EntityLookupStoreData
                    {
                        Key = entityTagsMonoBehavior.key,
                        Value = GetEntity(entityTagsMonoBehavior, TransformUsageFlags.None),
                    });
                }

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<EntityLookupStoreBlob>();
                var array = builder.Allocate(ref root.Entries, entries.Count);

                for (var i = 0; i < entries.Count; i++)
                    array[i] = entries[i];

                var blobRef = builder.CreateBlobAssetReference<EntityLookupStoreBlob>(Allocator.Persistent);
                builder.Dispose();

                AddBlobAsset(ref blobRef, out _);

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EntityLookupStoreBlobComponent { Blob = blobRef });
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.EntityLinks.Authoring
{
    public class EntityLinkLookupResolverAuthoring : MonoBehaviour
    {
        public EntityTagsMonoBehavior[] links = Array.Empty<EntityTagsMonoBehavior>();
        private void OnValidate() => links = GetComponentsInChildren<EntityTagsMonoBehavior>(true);

        public class EntityLinkLookupResolverBaker : Baker<EntityLinkLookupResolverAuthoring>
        {
            public override void Bake(EntityLinkLookupResolverAuthoring authoring)
            {
                var entries = new List<EntityLookupStoreData>();

                foreach (var entityTagsMonoBehavior in authoring.links)
                {
                    foreach (var data in entityTagsMonoBehavior.tags)
                    {
                        if (!data.transform) continue;
                        entries.Add(new EntityLookupStoreData
                        {
                            Key = data.key,
                            Value = GetEntity(data.transform, TransformUsageFlags.None),
                        });
                    }
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
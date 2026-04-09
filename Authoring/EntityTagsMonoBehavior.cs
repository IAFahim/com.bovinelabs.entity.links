using System;
using BovineLabs.Core.Authoring;
using BovineLabs.Core.Keys;
using UnityEngine;

namespace BovineLabs.EntityLinks.Authoring
{
    public class EntityTagsMonoBehavior : MonoBehaviour
    {
        public EntityTagBakeData[] tags;

        [Serializable]
        public struct EntityTagBakeData
        {
            [K(nameof(EntityLinkKeys))] public byte key;
            public Transform transform;
        }

        private void OnValidate()
        {
            foreach (var entityTagBakeData in tags)
            {
                if(!entityTagBakeData.transform) return;
                if (!entityTagBakeData.transform.gameObject.TryGetComponent(out TransformAuthoring transformAuthoring))
                {
                    entityTagBakeData.transform.gameObject.AddComponent<TransformAuthoring>();
                }

            }
        }
    }
}
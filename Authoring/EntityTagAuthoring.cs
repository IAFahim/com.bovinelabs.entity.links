using BovineLabs.Core.Authoring;
using BovineLabs.Core.Keys;
using UnityEngine;

namespace BovineLabs.EntityLinks.Authoring
{
    public class EntityTagAuthoring : MonoBehaviour
    {
        [K(nameof(EntityLinkKeys))] public byte key;

        private void OnValidate()
        {
            if (!transform.gameObject.TryGetComponent(out TransformAuthoring transformAuthoring))
            {
                transform.gameObject.AddComponent<TransformAuthoring>();
            }
        }
    }
}
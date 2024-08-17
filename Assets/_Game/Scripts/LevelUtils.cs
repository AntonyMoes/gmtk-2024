using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts {
    public static class LevelUtils {
        public static void SetMaterial(Transform levelRoot, PhysicMaterial material) {
            var queue = new Queue<Transform>();
            queue.Enqueue(levelRoot);

            while (queue.Count > 0) {
                var obj = queue.Dequeue();
                var collider = obj.GetComponent<Collider>();
                if (collider != null) {
                    collider.material = material;
                }

                for (var i = 0; i < obj.childCount; i++) {
                    queue.Enqueue(obj.GetChild(i));
                }
            }
        }
    }
}
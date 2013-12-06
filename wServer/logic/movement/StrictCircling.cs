#region

using System;
using System.Collections.Generic;
using wServer.realm;

#endregion

namespace wServer.logic.movement
{
    internal class StrictCircling : Behavior
    {
        private static readonly Dictionary<Tuple<float, float, short?>, StrictCircling> instances =
            new Dictionary<Tuple<float, float, short?>, StrictCircling>();

        private readonly float angularSpeed;
        private readonly short? objType;
        private readonly float radius;
        private readonly Random rand = new Random();
        private readonly float speed;

        private StrictCircling(float radius, float speed, short? objType)
        {
            this.radius = radius;
            angularSpeed = speed/radius;
            this.speed = speed;
            this.objType = objType;
        }

        public static StrictCircling Instance(float radius, float speed, short? objType)
        {
            var key = new Tuple<float, float, short?>(radius, speed, objType);
            StrictCircling ret;
            if (!instances.TryGetValue(key, out ret))
                ret = instances[key] = new StrictCircling(radius, speed, objType);
            return ret;
        }

        protected override bool TickCore(RealmTime time)
        {
            if (Host.Self.HasConditionEffect(ConditionEffects.Paralyzed)) return true;
            var speed = this.speed*GetSpeedMultiplier(Host.Self);

            CirclingState state;
            object o;
            if (!Host.StateStorage.TryGetValue(Key, out o))
            {
                var dist = radius + 1;
                Host.StateStorage[Key] = state = new CirclingState
                {
                    target = WeakReference<Entity>.Create(GetNearestEntity(ref dist, objType)),
                    angle = (float) (2*Math.PI*rand.NextDouble())
                };
            }
            else
            {
                state = (CirclingState) o;

                state.angle += angularSpeed*(time.thisTickTimes/1000f);
                if (!state.target.IsAlive)
                {
                    Host.StateStorage.Remove(Key);
                    return false;
                }
                var target = state.target.Target;
                if (target == null || target.Owner == null)
                {
                    Host.StateStorage.Remove(Key);
                    return false;
                }
                var x = target.X + Math.Cos(state.angle)*radius;
                var y = target.Y + Math.Sin(state.angle)*radius;
                ValidateAndMove((float) x, (float) y);
                Host.Self.UpdateCount++;
            }

            if (state.angle >= Math.PI*2)
                state.angle -= (float) (Math.PI*2);
            return true;
        }

        private class CirclingState
        {
            public float angle;
            public WeakReference<Entity> target;
        }
    }
}
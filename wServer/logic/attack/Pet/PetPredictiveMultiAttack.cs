﻿#region

using System;
using System.Collections.Generic;
using wServer.realm;
using wServer.realm.entities;
using wServer.svrPackets;

#endregion

namespace wServer.logic.attack
{
    internal class PetPredictiveMultiAttack : Behavior
    {
        private static readonly Dictionary<Tuple<float, float, int, float, int>, PetPredictiveMultiAttack> instances =
            new Dictionary<Tuple<float, float, int, float, int>, PetPredictiveMultiAttack>();

        private readonly float angle;
        private readonly int numShot;
        private readonly int projectileIndex;
        private readonly float radius;
        private float predictFactor;
        private Random rand = new Random();

        private PetPredictiveMultiAttack(float radius, float angle, int numShot, float predictFactor,
            int projectileIndex)
        {
            this.radius = radius;
            this.angle = angle;
            this.numShot = numShot;
            this.predictFactor = predictFactor;
            this.projectileIndex = projectileIndex;
        }

        public static PetPredictiveMultiAttack Instance(float radius, float angle, int numShot, float predictFactor,
            int projectileIndex = 0)
        {
            var key = new Tuple<float, float, int, float, int>(radius, angle, numShot, predictFactor, projectileIndex);
            PetPredictiveMultiAttack ret;
            if (!instances.TryGetValue(key, out ret))
                ret =
                    instances[key] =
                        new PetPredictiveMultiAttack(radius, angle, numShot, predictFactor, projectileIndex);
            return ret;
        }

        private double Predict(Entity entity, ProjectileDesc desc)
        {
            var history = entity.TryGetHistory(100);
            if (history == null)
                return 0;

            var originalAngle = Math.Atan2(history.Value.Y - Host.Self.Y, history.Value.X - Host.Self.X);
            var newAngle = Math.Atan2(entity.Y - Host.Self.Y, entity.X - Host.Self.X);


            var bulletSpeed = desc.Speed/100f;
            var dist = Dist(entity, Host.Self);
            var angularVelo = (newAngle - originalAngle)/(100/1000f);
            return angularVelo*bulletSpeed;
        }

        protected override bool TickCore(RealmTime time)
        {
            if (Host.Self.HasConditionEffect(ConditionEffects.Stunned)) return false;
            var numShot = this.numShot;
            if (Host.Self.HasConditionEffect(ConditionEffects.Dazed))
                numShot = Math.Max(1, numShot/2);

            var dist = radius;
            var entity = GetNearestEntityPet(ref dist);
            if (entity != null)
            {
                var chr = Host as Character;
                var desc = chr.ObjectDesc.Projectiles[projectileIndex];
                var angleOffset = Predict(entity, desc);
                var startAngle = Math.Atan2(entity.Y - chr.Y, entity.X - chr.X) + angleOffset - angle*(numShot - 1)/2;

                byte prjId = 0;
                var prjPos = new Position {X = chr.X, Y = chr.Y};
                var dmg = chr.Random.Next(desc.MinDamage, desc.MaxDamage);
                for (var i = 0; i < numShot; i++)
                {
                    var prj = chr.CreateProjectile(
                        desc, chr.ObjectType, dmg, time.tickTimes,
                        prjPos, (float) (startAngle + angle*i));
                    chr.Owner.EnterWorld(prj);
                    if (i == 0)
                        prjId = prj.ProjectileId;
                }
                chr.Owner.BroadcastPacket(new MultiShootPacket
                {
                    BulletId = prjId,
                    OwnerId = Host.Self.Id,
                    BulletType = (byte) desc.BulletType,
                    Position = prjPos,
                    Angle = (float) startAngle,
                    Damage = (short) dmg,
                    NumShots = (byte) numShot,
                    AngleIncrement = angle,
                }, null);
                return true;
            }
            return false;
        }
    }
}
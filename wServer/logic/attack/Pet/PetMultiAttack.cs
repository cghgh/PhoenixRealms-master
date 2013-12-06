﻿#region

using System;
using System.Collections.Generic;
using wServer.realm;
using wServer.realm.entities;
using wServer.svrPackets;

#endregion

namespace wServer.logic.attack
{
    internal class PetMultiAttack : Behavior
    {
        private static readonly Dictionary<Tuple<float, float, int, float, int>, PetMultiAttack> instances =
            new Dictionary<Tuple<float, float, int, float, int>, PetMultiAttack>();

        private readonly float angle;
        private readonly int numShot;
        private readonly float offset;
        private readonly int projectileIndex;
        private readonly float radius;
        private Random rand = new Random();

        private PetMultiAttack(float radius, float angle, int numShot, float offset, int projectileIndex)
        {
            this.radius = radius;
            this.angle = angle;
            this.numShot = numShot;
            this.offset = offset;
            this.projectileIndex = projectileIndex;
        }

        public static PetMultiAttack Instance(float radius, float angle, int numShot, float offset = 0,
            int projectileIndex = 0)
        {
            var key = new Tuple<float, float, int, float, int>(radius, angle, numShot, offset, projectileIndex);
            PetMultiAttack ret;
            if (!instances.TryGetValue(key, out ret))
                ret = instances[key] = new PetMultiAttack(radius, angle, numShot, offset, projectileIndex);
            return ret;
        }

        protected override bool TickCore(RealmTime time)
        {
            if (Host.Self.HasConditionEffect(ConditionEffects.Stunned)) return false;
            var numShot = this.numShot;
            if (Host.Self.HasConditionEffect(ConditionEffects.Dazed))
                numShot = Math.Max(1, numShot/2);

            var dist = radius;
            var player = GetNearestEntityPet(ref dist);
            if (player != null)
            {
                var chr = Host as Character;
                var startAngle = Math.Atan2(player.Y - chr.Y, player.X - chr.X)
                                    - angle*(numShot - 1)/2
                                    + offset;
                var desc = chr.ObjectDesc.Projectiles[projectileIndex];

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
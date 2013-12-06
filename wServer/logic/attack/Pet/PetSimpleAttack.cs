﻿#region

using System;
using System.Collections.Generic;
using wServer.realm;
using wServer.realm.entities;
using wServer.svrPackets;

#endregion

namespace wServer.logic.attack
{
    internal class PetSimpleAttack : Behavior
    {
        private static readonly Dictionary<Tuple<float, int>, PetSimpleAttack> instances =
            new Dictionary<Tuple<float, int>, PetSimpleAttack>();

        private readonly int projectileIndex;
        private readonly float radius;
        private Random rand = new Random();

        private PetSimpleAttack(float radius, int projectileIndex)
        {
            this.radius = radius;
            this.projectileIndex = projectileIndex;
        }

        public static PetSimpleAttack Instance(float radius, int projectileIndex = 0)
        {
            var key = new Tuple<float, int>(radius, projectileIndex);
            PetSimpleAttack ret;
            if (!instances.TryGetValue(key, out ret))
                ret = instances[key] = new PetSimpleAttack(radius, projectileIndex);
            return ret;
        }

        protected override bool TickCore(RealmTime time)
        {
            if (Host.Self.HasConditionEffect(ConditionEffects.Stunned)) return false;
            var dist = radius;
            var player = GetNearestEntityPet(ref dist);
            if (player != null)
            {
                var chr = Host as Character;
                var angle = Math.Atan2(player.Y - chr.Y, player.X - chr.X);
                var desc = chr.ObjectDesc.Projectiles[projectileIndex];

                var prj = chr.CreateProjectile(
                    desc, chr.ObjectType, chr.Random.Next(desc.MinDamage, desc.MaxDamage),
                    time.tickTimes, new Position {X = chr.X, Y = chr.Y}, (float) angle);
                chr.Owner.EnterWorld(prj);
                if (projectileIndex == 0) //(false)
                    chr.Owner.BroadcastPacket(new ShootPacket
                    {
                        BulletId = prj.ProjectileId,
                        OwnerId = Host.Self.Id,
                        ContainerType = Host.Self.ObjectType,
                        Position = prj.BeginPos,
                        Angle = prj.Angle,
                        Damage = (short) prj.Damage
                    }, null);
                else
                    chr.Owner.BroadcastPacket(new MultiShootPacket
                    {
                        BulletId = prj.ProjectileId,
                        OwnerId = Host.Self.Id,
                        Position = prj.BeginPos,
                        Angle = prj.Angle,
                        Damage = (short) prj.Damage,
                        BulletType = (byte) (desc.BulletType),
                        AngleIncrement = 0,
                        NumShots = 1,
                    }, null);
                return true;
            }
            return false;
        }
    }
}
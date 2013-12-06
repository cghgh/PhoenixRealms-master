﻿#region

using System.Diagnostics;
using db.data;
using wServer.cliPackets;
using wServer.svrPackets;

#endregion

namespace wServer.realm.entities.player
{
    public partial class Player
    {
        public void PlayerShoot(RealmTime time, PlayerShootPacket pkt)
        {
            Debug.WriteLine(pkt.Position); // Was Commented!
            var item = XmlDatas.ItemDescs[pkt.ContainerType];
            if (item.ObjectType != Inventory[0].ObjectType && item.ObjectType != Inventory[1].ObjectType) return;
            var prjDesc = item.Projectiles[0]; //Assume only one
            projectileId = pkt.BulletId;
            var prj = CreateProjectile(prjDesc, item.ObjectType,
                0,
                pkt.Time + tickMapping, pkt.Position, pkt.Angle);
            Owner.EnterWorld(prj);
            Owner.BroadcastPacket(new AllyShootPacket
            {
                OwnerId = Id,
                Angle = pkt.Angle,
                ContainerType = pkt.ContainerType,
                BulletId = pkt.BulletId
            }, this);
            fames.Shoot(prj);
        }

        public void EnemyHit(RealmTime time, EnemyHitPacket pkt)
        {
            try
            {
                var entity = Owner.GetEntity(pkt.TargetId);
                var prj = (this as IProjectileOwner).Projectiles[pkt.BulletId];
                prj.Damage = (int) statsMgr.GetAttackDamage(prj.Descriptor.MinDamage, prj.Descriptor.MaxDamage);
                prj.ForceHit(entity, time);
                if (pkt.Killed && !(entity is Wall))
                {
                    psr.SendPacket(new UpdatePacket
                    {
                        Tiles = new UpdatePacket.TileData[0],
                        NewObjects = new[] {entity.ToDefinition()},
                        RemovedObjectIds = new[] {pkt.TargetId}
                    });
                    _clientEntities.Remove(entity);
                }
            }
            catch
            {
                /*Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("CAN'T REGISTER HIT by player " + Name);
                Console.ForegroundColor = ConsoleColor.White;
                 * 
                 * Disabled, this message spams when an enemy dies*/
            }
            /*
            if (entity != null && pkt.Killed)   //Tolerance
            {
                Projectile prj = (this as IProjectileOwner).Projectiles[pkt.BulletId];
                Position? entPos = entity.TryGetHistory((time.tickTimes - tickMapping) - pkt.Time);
                Position? prjPos = prj == null ? null : (Position?)prj.GetPosition(pkt.Time + tickMapping - prj.BeginTime);
                var tol1 = (entPos == null || prjPos == null) ? 10 : (prjPos.Value.X - entPos.Value.X) * (prjPos.Value.X - entPos.Value.X) + (prjPos.Value.Y - entPos.Value.Y) * (prjPos.Value.Y - entPos.Value.Y);
                var tol2 = prj == null ? 10 : (prj.X - entity.X) * (prj.X - entity.X) + (prj.Y - entity.Y) * (prj.Y - entity.Y);
                if (prj != null && (tol1 < 1 || tol2 < 1))
                {
                    prj.ForceHit(entity, time);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("CAN'T REGISTER HIT!" + " X: " + tol1 + " Y: " + tol2);
                    Console.ForegroundColor = ConsoleColor.White;
                    psr.SendPacket(new UpdatePacket()
                    {
                        Tiles = new UpdatePacket.TileData[0],
                        NewObjects = new ObjectDef[] { entity.ToDefinition() },
                        RemovedObjectIds = new int[] { pkt.TargetId }
                    });
                    clientEntities.Remove(entity);
                }
            }
            else if (pkt.Killed)
            {
                psr.SendPacket(new UpdatePacket()
                {
                    Tiles = new UpdatePacket.TileData[0],
                    NewObjects = Empty<ObjectDef>.Array,
                    RemovedObjectIds = new int[] { pkt.TargetId }
                });
            }*/
        }

        public void OtherHit(RealmTime time, OtherHitPacket pkt)
        {
            //System.Diagnostics.Debugger.Break();
            //Console.WriteLine("Other HIT! " + Objects.type2desc[Owner.GetEntity(pkt.ObjectId).ObjectType].Attributes["id"].Value
            //    + Objects.type2desc[Owner.GetEntity(pkt.TargetId).ObjectType].Attributes["id"].Value);
        }

        public void SquareHit(RealmTime time, SquareHitPacket pkt)
        {
            //Console.WriteLine("Square HIT! " + Objects.type2desc[Owner.GetEntity(pkt.ObjectId).ObjectType].Attributes["id"].Value);
        }

        public void PlayerHit(RealmTime time, PlayerHitPacket pkt)
        {
            //Console.WriteLine("Player HIT! " + Owner.GetEntity(pkt.ObjectId).Name);
        }

        public void ShootAck(RealmTime time, ShootAckPacket pkt)
        {
            //Console.WriteLine("ACK! " + Objects.type2desc[Owner.GetEntity(pkt.ObjectId).ObjectType].Attributes["id"].Value);
        }
    }
}
﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using wServer.svrPackets;

#endregion

namespace wServer.realm.entities.player
{
    public partial class Player
    {
        public const int SightRadius = 15;
        private const int AppoxAreaOfSight = (int) (Math.PI*SightRadius*SightRadius + 1);

        private readonly HashSet<Entity> _clientEntities = new HashSet<Entity>();
        private readonly HashSet<IntPoint> _clientStatic = new HashSet<IntPoint>(new IntPointComparer());
        private readonly Dictionary<Entity, int> _lastUpdate = new Dictionary<Entity, int>();
        private int _mapHeight;
        private int _mapWidth;
        private int _tickId;
/*
        private long tickIdTime = 0;
*/

        private IEnumerable<Entity> GetNewEntities()
        {
            foreach (var i in Owner.Players.Where(i => _clientEntities.Add(i.Value)))
            {
                yield return i.Value;
            }
            foreach (var i in Owner.PlayersCollision.HitTest(X, Y, SightRadius).OfType<Decoy>().Where(i => _clientEntities.Add(i)))
            {
                yield return i;
            }
            foreach (var i in Owner.EnemiesCollision.HitTest(X, Y, SightRadius))
            {
                if (i is Container)
                {
                    var owner = (i as Container).BagOwner;
                    if (owner != null && owner != AccountId) continue;
                }
                if (MathsUtils.DistSqr(i.X, i.Y, X, Y) <= SightRadius*SightRadius)
                {
                    if (_clientEntities.Add(i))
                        yield return i;
                }
            }
            if (questEntity != null && _clientEntities.Add(questEntity))
                yield return questEntity;
        }

        private IEnumerable<int> GetRemovedEntities()
        {
            foreach (var i in _clientEntities.Where(i => !(i is Player) || i.Owner == null))
            {
                if (MathsUtils.DistSqr(i.X, i.Y, X, Y) > SightRadius*SightRadius &&
                    !(i is StaticObject && (i as StaticObject).Static) &&
                    i != questEntity)
                    yield return i.Id;
                else if (i.Owner == null)
                    yield return i.Id;
            }
        }

        private IEnumerable<ObjectDef> GetNewStatics(int _x, int _y)
        {
            return (from i in Sight.GetSightCircle(SightRadius) let x = i.X + _x let y = i.Y + _y where x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight let tile = Owner.Map[x, y] where tile.ObjId != 0 && tile.ObjType != 0 && _clientStatic.Add(new IntPoint(x, y)) select tile.ToDef(x, y)).ToList();
        }

        private IEnumerable<IntPoint> GetRemovedStatics(int _x, int _y)
        {
            return from i in _clientStatic let dx = i.X - _x let dy = i.Y - _y let tile = Owner.Map[i.X, i.Y] where dx*dx + dy*dy > SightRadius*SightRadius ||
                                                                                                                    tile.ObjType == 0 let objId = Owner.Map[i.X, i.Y].ObjId where objId != 0 select i;
        }

        private void SendUpdate(RealmTime time)
        {
            _mapWidth = Owner.Map.Width;
            _mapHeight = Owner.Map.Height;
            var map = Owner.Map;
            var _x = (int) X;
            var _y = (int) Y;

            var sendEntities = new HashSet<Entity>(GetNewEntities());

            var list = new List<UpdatePacket.TileData>(AppoxAreaOfSight);
            var sent = 0;
            foreach (var i in Sight.GetSightCircle(SightRadius))
            {
                var x = i.X + _x;
                var y = i.Y + _y;
                WmapTile tile;
                if (x < 0 || x >= _mapWidth ||
                    y < 0 || y >= _mapHeight ||
                    map[x, y].TileId == 0xff ||
                    tiles[x, y] >= (tile = map[x, y]).UpdateCount) continue;
                list.Add(new UpdatePacket.TileData
                {
                    X = (short) x,
                    Y = (short) y,
                    Tile = (Tile) tile.TileId
                });
                tiles[x, y] = tile.UpdateCount;
                sent++;
            }
            fames.TileSent(sent);

            var dropEntities = GetRemovedEntities().Distinct().ToArray();
            _clientEntities.RemoveWhere(_ => Array.IndexOf(dropEntities, _.Id) != -1);

            foreach (var i in sendEntities)
                _lastUpdate[i] = i.UpdateCount;

            var newStatics = GetNewStatics(_x, _y).ToArray();
            var removeStatics = GetRemovedStatics(_x, _y).ToArray();
            var removedIds = new List<int>();
            foreach (var i in removeStatics)
            {
                removedIds.Add(Owner.Map[i.X, i.Y].ObjId);
                _clientStatic.Remove(i);
            }

            if (sendEntities.Count > 0 || list.Count > 0 || dropEntities.Length > 0 ||
                newStatics.Length > 0 || removedIds.Count > 0)
            {
                var packet = new UpdatePacket
                {
                    Tiles = list.ToArray(),
                    NewObjects = sendEntities.Select(_ => _.ToDefinition()).Concat(newStatics).ToArray(),
                    RemovedObjectIds = dropEntities.Concat(removedIds).ToArray()
                };
                psr.SendPacket(packet);
            }
            SendNewTick(time);
        }

        private void SendNewTick(RealmTime time)
        {
            var sendEntities = new List<Entity>();
            try
            {
                foreach (var i in _clientEntities.Where(i => i.UpdateCount > _lastUpdate[i]))
                {
                    sendEntities.Add(i);
                    _lastUpdate[i] = i.UpdateCount;
                }
            }

            catch
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Out.WriteLine("Crash halted - Nobody likes death...");
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (questEntity != null &&
                (!_lastUpdate.ContainsKey(questEntity) || questEntity.UpdateCount > _lastUpdate[questEntity]))
            {
                sendEntities.Add(questEntity);
                _lastUpdate[questEntity] = questEntity.UpdateCount;
            }
            var p = new NewTickPacket();
            _tickId++;
            p.TickId = _tickId;
            p.TickTime = time.thisTickTimes;
            p.UpdateStatuses = sendEntities.Select(_ => _.ExportStats()).ToArray();
            psr.SendPacket(p);

            SaveToCharacter();
        }
    }
}
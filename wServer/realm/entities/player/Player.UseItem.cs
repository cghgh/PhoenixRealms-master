﻿#region

using System;
using System.Collections.Generic;
using db;
using db.data;
using wServer.cliPackets;
using wServer.logic;
using wServer.svrPackets;

#endregion

namespace wServer.realm.entities.player
{
    partial class Player
    {
        private static readonly ConditionEffect[] NegativeEffs =
        {
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Slowed,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Paralyzed,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Weak,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Stunned,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Confused,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Blind,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Quiet,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.ArmorBroken,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Bleeding,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Dazed,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Sick,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Drunk,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Hallucinating,
                DurationMS = 0
            },
            new ConditionEffect
            {
                Effect = ConditionEffectIndex.Hexed,
                DurationMS = 0
            }
        };

        private bool buttonDown;

        public static int oldstat { get; set; }
        public static Position targetlink { get; set; }

        public void UseItem(RealmTime time, UseItemPacket pkt)
        {
            var container = Owner.GetEntity(pkt.Slot.ObjectId) as IContainer;
            var item = container.Inventory[pkt.Slot.SlotId];
            Activate(time, item, pkt.Position);
            if (item.Consumable)
            {
                if (item.SuccessorId != null)
                {
                    if (item.SuccessorId != item.ObjectId)
                    {
                        container.Inventory[pkt.Slot.SlotId] = XmlDatas.ItemDescs[XmlDatas.IdToType[item.SuccessorId]];
                        Owner.GetEntity(pkt.Slot.ObjectId).UpdateCount++;
                        UpdateCount++;
                    }
                }
                else
                {
                    container.Inventory[pkt.Slot.SlotId] = null;
                    Owner.GetEntity(pkt.Slot.ObjectId).UpdateCount++;
                    UpdateCount++;
                }
                SaveToCharacter();
                psr.Save();
            }
            if (container.SlotTypes[pkt.Slot.SlotId] != -1)
                fames.UseAbility();
        }

        public static void ActivateHealHp(Player player, int amount, List<Packet> pkts)
        {
            var maxHp = player.Stats[0] + player.Boost[0];
            var newHp = Math.Min(maxHp, player.HP + amount);
            if (newHp != player.HP)
            {
                pkts.Add(new ShowEffectPacket
                {
                    EffectType = EffectType.Potion,
                    TargetId = player.Id,
                    Color = new ARGB(0xffffffff)
                });
                pkts.Add(new NotificationPacket
                {
                    Color = new ARGB(0xff00ff00),
                    ObjectId = player.Id,
                    Text = "+" + (newHp - player.HP)
                });
                player.HP = newHp;
                player.UpdateCount++;
            }
        }

        private static void ActivateHealMp(Player player, int amount, List<Packet> pkts)
        {
            var maxMp = player.Stats[1] + player.Boost[1];
            var newMp = Math.Min(maxMp, player.MP + amount);
            if (newMp != player.MP)
            {
                pkts.Add(new ShowEffectPacket
                {
                    EffectType = EffectType.Potion,
                    TargetId = player.Id,
                    Color = new ARGB(0xffffffff)
                });
                pkts.Add(new NotificationPacket
                {
                    Color = new ARGB(0x6084e0), // changed from ff9000ff to 0xDE825F (prod color)
                    ObjectId = player.Id,
                    Text = "+" + (newMp - player.MP)
                });
                player.MP = newMp;
                player.UpdateCount++;
            }
        }

        private static void ActivateBoostStat(Player player, int idxnew, List<Packet> pkts)
        {
            var OriginalStat = 0;
            OriginalStat = player.Stats[idxnew] + OriginalStat;
            oldstat = OriginalStat;
        }

        private void ActivateShoot(RealmTime time, Item item, Position target)
        {
            var arcGap = item.ArcGap*Math.PI/180;
            var startAngle = Math.Atan2(target.Y - Y, target.X - X) - (item.NumProjectiles - 1)/2*arcGap;
            var prjDesc = item.Projectiles[0]; //Assume only one

            for (var i = 0; i < item.NumProjectiles; i++)
            {
                var proj = CreateProjectile(prjDesc, item.ObjectType,
                    (int) statsMgr.GetAttackDamage(prjDesc.MinDamage, prjDesc.MaxDamage),
                    time.tickTimes, new Position {X = X, Y = Y}, (float) (startAngle + arcGap*i));
                Owner.EnterWorld(proj);
                fames.Shoot(proj);
            }
        }

        private void ActivatePearl(RealmTime time, Item item, Position target)
        {
            var arcGap = item.ArcGap*Math.PI/180;
            var startAngle = Math.Atan2(target.Y - Y, target.X - X) - (item.NumProjectiles - 1)/2*arcGap;
            var prjDesc = item.Projectiles[0]; //Assume only one

            for (var i = 0; i < item.NumProjectiles; i++)
            {
                var proj = CreateProjectile(prjDesc, item.ObjectType,
                    (int) statsMgr.GetAttackDamage(prjDesc.MinDamage, prjDesc.MaxDamage),
                    time.tickTimes, new Position {X = X, Y = Y}, (float) (startAngle + arcGap*i));
                Owner.EnterWorld(proj);
                fames.Shoot(proj);
            }
        }

        private void PoisonEnemy(Enemy enemy, ActivateEffect eff)
        {
            try
            {
                if (eff.ConditionEffect != null)
                    enemy.ApplyConditionEffect(new[]
                    {
                        new ConditionEffect
                        {
                            Effect = (ConditionEffectIndex) eff.ConditionEffect,
                            DurationMS = (int) eff.EffectDuration
                        }
                    });
                var remainingDmg = (int) StatsManager.GetDefenseDamage(enemy, eff.TotalDamage, enemy.ObjectDesc.Defense);
                var perDmg = remainingDmg*1000/eff.DurationMS;
                WorldTimer tmr = null;
                var x = 0;
                tmr = new WorldTimer(100, (w, t) =>
                {
                    if (enemy.Owner == null) return;
                    w.BroadcastPacket(new ShowEffectPacket
                    {
                        EffectType = EffectType.Dead,
                        TargetId = enemy.Id,
                        Color = new ARGB(0xffddff00)
                    }, null);

                    if (x%10 == 0)
                    {
                        int thisDmg;
                        if (remainingDmg < perDmg) thisDmg = remainingDmg;
                        else thisDmg = perDmg;

                        enemy.Damage(this, t, thisDmg, true);
                        remainingDmg -= thisDmg;
                        if (remainingDmg <= 0) return;
                    }
                    x++;

                    tmr.Reset();

                    RealmManager.Logic.AddPendingAction(_ => w.Timers.Add(tmr), PendingPriority.Creation);
                });
                Owner.Timers.Add(tmr);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Out.WriteLine("Crash halted - Poisons!");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void Activate(RealmTime time, Item item, Position target)
        {
            MP -= item.MpCost;
            foreach (var eff in item.ActivateEffects)
            {
                switch (eff.Effect)
                {
                    case ActivateEffects.BulletNova:
                    {
                        var prjDesc = item.Projectiles[0]; //Assume only one
                        var batch = new Packet[21];
                        var s = Random.CurrentSeed;
                        Random.CurrentSeed = (uint) (s*time.tickTimes);
                        for (var i = 0; i < 20; i++)
                        {
                            var proj = CreateProjectile(prjDesc, item.ObjectType,
                                (int) statsMgr.GetAttackDamage(prjDesc.MinDamage, prjDesc.MaxDamage),
                                time.tickTimes, target, (float) (i*(Math.PI*2)/20));
                            Owner.EnterWorld(proj);
                            fames.Shoot(proj);
                            batch[i] = new ShootPacket
                            {
                                BulletId = proj.ProjectileId,
                                OwnerId = Id,
                                ContainerType = item.ObjectType,
                                Position = target,
                                Angle = proj.Angle,
                                Damage = (short) proj.Damage
                            };
                        }
                        Random.CurrentSeed = s;
                        batch[20] = new ShowEffectPacket
                        {
                            EffectType = EffectType.Trail,
                            PosA = target,
                            TargetId = Id,
                            Color = new ARGB(0xFFFF00AA)
                        };
                        Owner.BroadcastPackets(batch, null);
                    }
                        break;
                    case ActivateEffects.Shoot:
                    {
                        ActivateShoot(time, item, target);
                    }
                        break;
                    case ActivateEffects.StatBoostSelf:
                    {
                        var idx = -1;

                        switch ((StatsType) eff.Stats)
                        {
                            case StatsType.MaximumHP:
                                idx = 0;
                                break;
                            case StatsType.MaximumMP:
                                idx = 1;
                                break;
                            case StatsType.Attack:
                                idx = 2;
                                break;
                            case StatsType.Defense:
                                idx = 3;
                                break;
                            case StatsType.Speed:
                                idx = 4;
                                break;
                            case StatsType.Vitality:
                                idx = 5;
                                break;
                            case StatsType.Wisdom:
                                idx = 6;
                                break;
                            case StatsType.Dexterity:
                                idx = 7;
                                break;
                        }
                        var pkts = new List<Packet>();

                        ActivateBoostStat(this, idx, pkts);
                        var OGstat = oldstat;


                        var s = eff.Amount;
                        Boost[idx] += s;
                        UpdateCount++;
                        Owner.Timers.Add(new WorldTimer(eff.DurationMS, (world, t) =>
                        {
                            Boost[idx] = OGstat;
                            UpdateCount++;
                        }));
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.Potion,
                            TargetId = Id,
                            Color = new ARGB(0xffffffff)
                        }, null);
                    }
                        break;
                    case ActivateEffects.StatBoostAura:
                    {
                        var idx = -1;
                        switch ((StatsType) eff.Stats)
                        {
                            case StatsType.MaximumHP:
                                idx = 0;
                                break;
                            case StatsType.MaximumMP:
                                idx = 1;
                                break;
                            case StatsType.Attack:
                                idx = 2;
                                break;
                            case StatsType.Defense:
                                idx = 3;
                                break;
                            case StatsType.Speed:
                                idx = 4;
                                break;
                            case StatsType.Vitality:
                                idx = 5;
                                break;
                            case StatsType.Wisdom:
                                idx = 6;
                                break;
                            case StatsType.Dexterity:
                                idx = 7;
                                break;
                        }

                        var s = eff.Amount;
                        BehaviorBase.AOE(Owner, this, eff.Range, true, player =>
                        {
                            (player as Player).Boost[idx] += s;
                            player.UpdateCount++;
                            Owner.Timers.Add(new WorldTimer(eff.DurationMS, (world, t) =>
                            {
                                (player as Player).Boost[idx] -= s;
                                player.UpdateCount++;
                            }));
                        });
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(0xffffffff),
                            PosA = new Position {X = eff.Range}
                        }, null);
                    }
                        break;
                    case ActivateEffects.ConditionEffectSelf:
                    {
                        ApplyConditionEffect(new ConditionEffect
                        {
                            Effect = eff.ConditionEffect.Value,
                            DurationMS = eff.DurationMS
                        });
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(0xffffffff),
                            PosA = new Position {X = 1}
                        }, null);
                    }
                        break;
                    case ActivateEffects.ConditionEffectAura:
                    {
                        BehaviorBase.AOE(Owner, this, eff.Range, true, player =>
                        {
                            player.ApplyConditionEffect(new ConditionEffect
                            {
                                Effect = eff.ConditionEffect.Value,
                                DurationMS = eff.DurationMS
                            });
                        });
                        var color = 0xffffffff;
                        if (eff.ConditionEffect.Value == ConditionEffectIndex.Damaging)
                            color = 0xffff0000;
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(color),
                            PosA = new Position {X = eff.Range}
                        }, null);
                    }
                        break;
                    case ActivateEffects.Heal:
                    {
                        var pkts = new List<Packet>();
                        ActivateHealHp(this, eff.Amount, pkts);
                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;
                    case ActivateEffects.HealNova:
                    {
                        var pkts = new List<Packet>();
                        BehaviorBase.AOE(Owner, this, eff.Range, true,
                            player => { ActivateHealHp(player as Player, eff.Amount, pkts); });
                        pkts.Add(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(0xffffffff),
                            PosA = new Position {X = eff.Range}
                        });
                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;
                    case ActivateEffects.Magic:
                    {
                        var pkts = new List<Packet>();
                        ActivateHealMp(this, eff.Amount, pkts);
                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;
                    case ActivateEffects.MagicNova:
                    {
                        var pkts = new List<Packet>();
                        BehaviorBase.AOE(Owner, this, eff.Range, true,
                            player => { ActivateHealMp(player as Player, eff.Amount, pkts); });
                        pkts.Add(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(0xffffffff),
                            PosA = new Position {X = eff.Range}
                        });
                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;
                    case ActivateEffects.Teleport:
                    {
                        Move(target.X, target.Y);
                        UpdateCount++;
                        Owner.BroadcastPackets(new Packet[]
                        {
                            new GotoPacket
                            {
                                ObjectId = Id,
                                Position = new Position
                                {
                                    X = X,
                                    Y = Y
                                }
                            },
                            new ShowEffectPacket
                            {
                                EffectType = EffectType.Teleport,
                                TargetId = Id,
                                PosA = new Position
                                {
                                    X = X,
                                    Y = Y
                                },
                                Color = new ARGB(0xFFFFFFFF)
                            }
                        }, null);
                    }
                        break;
                    case ActivateEffects.VampireBlast:
                    {
                        var pkts = new List<Packet>();
                        pkts.Add(new ShowEffectPacket
                        {
                            EffectType = EffectType.Trail,
                            TargetId = Id,
                            PosA = target,
                            Color = new ARGB(0xFFFF0000)
                        });
                        pkts.Add(new ShowEffectPacket
                        {
                            EffectType = EffectType.Diffuse,
                            Color = new ARGB(0xFFFF0000),
                            TargetId = Id,
                            PosA = target,
                            PosB = new Position {X = target.X + eff.Radius, Y = target.Y}
                        });

                        var totalDmg = 0;
                        var enemies = new List<Enemy>();
                        BehaviorBase.AOE(Owner, target, eff.Radius, false, enemy =>
                        {
                            enemies.Add(enemy as Enemy);
                            totalDmg += (enemy as Enemy).Damage(this, time, eff.TotalDamage, false);
                        });
                        var players = new List<Player>();
                        BehaviorBase.AOE(Owner, this, eff.Radius, true, player =>
                        {
                            players.Add(player as Player);
                            ActivateHealHp(player as Player, totalDmg, pkts);
                        });

                        var rand = new Random();
                        for (var i = 0; i < 5; i++)
                        {
                            var a = enemies[rand.Next(0, enemies.Count)];
                            var b = players[rand.Next(0, players.Count)];
                            pkts.Add(new ShowEffectPacket
                            {
                                EffectType = EffectType.Flow,
                                TargetId = b.Id,
                                PosA = new Position {X = a.X, Y = a.Y},
                                Color = new ARGB(0xffffffff)
                            });
                        }

                        if (enemies.Count > 0)
                        {
                            var a = enemies[rand.Next(0, enemies.Count)];
                            var b = players[rand.Next(0, players.Count)];
                            pkts.Add(new ShowEffectPacket
                            {
                                EffectType = EffectType.Flow,
                                TargetId = b.Id,
                                PosA = new Position {X = a.X, Y = a.Y},
                                Color = new ARGB(0Xffffffff)
                            });
                        }

                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;
                    case ActivateEffects.Trap:
                    {
                        var effColor = new ARGB(0xff9000ff);
                        if (eff.Color != null)
                            effColor = new ARGB((uint) eff.Color);
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.Throw,
                            Color = effColor,
                            TargetId = Id,
                            PosA = target
                        }, null);
                        Owner.Timers.Add(new WorldTimer(1500, (world, t) =>
                        {
                            var trap = new Trap(
                                this,
                                eff.Radius,
                                eff.TotalDamage,
                                eff.ConditionEffect ?? ConditionEffectIndex.Slowed,
                                eff.EffectDuration);
                            trap.Move(target.X, target.Y);
                            world.EnterWorld(trap);
                        }));
                    }
                        break;
                    case ActivateEffects.StasisBlast:
                    {
                        var pkts = new List<Packet>();

                        var effColor = new ARGB(0xffffffff);
                        if (eff.Color != null)
                            effColor = new ARGB((uint) eff.Color);
                        pkts.Add(new ShowEffectPacket
                        {
                            EffectType = EffectType.Concentrate,
                            TargetId = Id,
                            PosA = target,
                            PosB = new Position {X = target.X + 3, Y = target.Y},
                            Color = effColor
                        });
                        BehaviorBase.AOE(Owner, target, 3, false, enemy =>
                        {
                            if (enemy.HasConditionEffect(ConditionEffects.StasisImmune))
                            {
                                pkts.Add(new NotificationPacket
                                {
                                    ObjectId = enemy.Id,
                                    Color = new ARGB(0xff00ff00),
                                    Text = "Immune"
                                });
                            }
                            else if (enemy.isPet)
                            {
                            }
                            else if (!enemy.HasConditionEffect(ConditionEffects.Stasis))
                            {
                                enemy.ApplyConditionEffect(
                                    new ConditionEffect
                                    {
                                        Effect = ConditionEffectIndex.Stasis,
                                        DurationMS = eff.DurationMS
                                    },
                                    new ConditionEffect
                                    {
                                        Effect = ConditionEffectIndex.Confused,
                                        DurationMS = eff.DurationMS
                                    }
                                    );
                                Owner.Timers.Add(new WorldTimer(eff.DurationMS, (world, t) =>
                                {
                                    enemy.ApplyConditionEffect(new ConditionEffect
                                    {
                                        Effect = ConditionEffectIndex.StasisImmune,
                                        DurationMS = 3000
                                    }
                                        );
                                }
                                    ));
                                pkts.Add(new NotificationPacket
                                {
                                    ObjectId = enemy.Id,
                                    Color = new ARGB(0xffff0000),
                                    Text = "Stasis"
                                });
                            }
                        });
                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;
                    case ActivateEffects.Decoy:
                    {
                        var decoy = new Decoy(this, eff.DurationMS, statsMgr.GetSpeed());
                        decoy.Move(X, Y);
                        Owner.EnterWorld(decoy);
                    }
                        break;
                    case ActivateEffects.MultiDecoy:
                    {
                        for (var i = 0; i < eff.Amount; i++)
                        {
                            var decoy = Decoy.DecoyRandom(this, eff.DurationMS, statsMgr.GetSpeed());
                            decoy.Move(X, Y);
                            Owner.EnterWorld(decoy);
                        }
                    }
                        break;
                    case ActivateEffects.Lightning:
                    {
                        Enemy start = null;
                        var angle = Math.Atan2(target.Y - Y, target.X - X);
                        var diff = Math.PI/3;
                        BehaviorBase.AOE(Owner, target, 6, false, enemy =>
                        {
                            if (!(enemy is Enemy)) return;
                            var x = Math.Atan2(enemy.Y - Y, enemy.X - X);
                            if (Math.Abs(angle - x) < diff)
                            {
                                start = enemy as Enemy;
                                diff = Math.Abs(angle - x);
                            }
                        });
                        if (start == null)
                            break;

                        var current = start;
                        var targets = new Enemy[eff.MaxTargets];
                        for (var i = 0; i < targets.Length; i++)
                        {
                            targets[i] = current;
                            float dist = 8;
                            var next = BehaviorBase.GetNearestEntity(current, ref dist, false,
                                enemy =>
                                    enemy is Enemy &&
                                    Array.IndexOf(targets, enemy) == -1 &&
                                    BehaviorBase.Dist(this, enemy) <= 6) as Enemy;

                            if (next == null) break;
                            current = next;
                        }

                        var pkts = new List<Packet>();
                        for (var i = 0; i < targets.Length; i++)
                        {
                            if (targets[i] == null) break;
                            var prev = i == 0 ? (Entity) this : targets[i - 1];
                            targets[i].Damage(this, time, eff.TotalDamage, false);
                            if (eff.ConditionEffect != null)
                                targets[i].ApplyConditionEffect(new ConditionEffect
                                {
                                    Effect = eff.ConditionEffect.Value,
                                    DurationMS = (int) (eff.EffectDuration*1000)
                                });
                            var shotColor = new ARGB(0xffff0088);
                            if (eff.Color != null)
                                shotColor = new ARGB((uint) eff.Color);
                            pkts.Add(new ShowEffectPacket
                            {
                                EffectType = EffectType.Lightning,
                                TargetId = prev.Id,
                                Color = shotColor,
                                PosA = new Position
                                {
                                    X = targets[i].X,
                                    Y = targets[i].Y
                                },
                                PosB = new Position {X = 350}
                            });
                        }
                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;
                    case ActivateEffects.PoisonGrenade:
                    {
                        try
                        {
                            Owner.BroadcastPacket(new ShowEffectPacket
                            {
                                EffectType = EffectType.Throw,
                                Color = new ARGB(0xffddff00),
                                TargetId = Id,
                                PosA = target
                            }, null);
                            var x = new Placeholder(1500);
                            x.Move(target.X, target.Y);
                            Owner.EnterWorld(x);
                            Owner.Timers.Add(new WorldTimer(1500, (world, t) =>
                            {
                                try
                                {
                                    Owner.BroadcastPacket(new ShowEffectPacket
                                    {
                                        EffectType = EffectType.AreaBlast,
                                        Color = new ARGB(0xffddff00),
                                        TargetId = x.Id,
                                        PosA = new Position {X = eff.Radius}
                                    }, null);
                                }
                                catch
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.Out.WriteLine("Crash halted - Nobody likes death...");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                var enemies = new List<Enemy>();
                                BehaviorBase.AOE(world, target, eff.Radius, false,
                                    enemy => PoisonEnemy(enemy as Enemy, eff));
                            }));
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.Out.WriteLine("Crash halted - Poison grenade??");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                        break;
                    case ActivateEffects.RemoveNegativeConditions:
                    {
                        BehaviorBase.AOE(Owner, this, eff.Range, true, player => { ApplyConditionEffect(NegativeEffs); });
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(0xffffffff),
                            PosA = new Position {X = eff.Range}
                        }, null);
                    }
                        break;
                    case ActivateEffects.RemoveNegativeConditionsSelf:
                    {
                        ApplyConditionEffect(NegativeEffs);
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(0xffffffff),
                            PosA = new Position {X = 1}
                        }, null);
                    }
                        break;
                    case ActivateEffects.IncrementStat:
                    {
                        var idx = -1;
                        switch ((StatsType) eff.Stats)
                        {
                            case StatsType.MaximumHP:
                                idx = 0;
                                break;
                            case StatsType.MaximumMP:
                                idx = 1;
                                break;
                            case StatsType.Attack:
                                idx = 2;
                                break;
                            case StatsType.Defense:
                                idx = 3;
                                break;
                            case StatsType.Speed:
                                idx = 4;
                                break;
                            case StatsType.Vitality:
                                idx = 5;
                                break;
                            case StatsType.Wisdom:
                                idx = 6;
                                break;
                            case StatsType.Dexterity:
                                idx = 7;
                                break;
                        }
                        Stats[idx] += eff.Amount;
                        var limit =
                            int.Parse(
                                XmlDatas.TypeToElement[ObjectType].Element(StatsManager.StatsIndexToName(idx))
                                    .Attribute("max")
                                    .Value);
                        if (Stats[idx] > limit)
                            Stats[idx] = limit;
                        UpdateCount++;
                    }
                        break;
                    case ActivateEffects.Create: //this is a portal
                    {
                        short objType;
                        if (!XmlDatas.IdToType.TryGetValue(eff.Id, out objType) ||
                            !XmlDatas.PortalDescs.ContainsKey(objType))
                            break; // object not found, ignore
                        var entity = Resolve(objType);
                        var w = RealmManager.GetWorld(Owner.Id); //can't use Owner here, as it goes out of scope
                        var TimeoutTime = XmlDatas.PortalDescs[objType].TimeoutTime;
                        var DungName = XmlDatas.PortalDescs[objType].DungeonName;

                        ARGB c;
                        c.A = 0;
                        c.B = 91;
                        c.R = 233;
                        c.G = 176;

                        if (eff.Id == "Wine Cellar Portal") //wine cellar incantation
                        {
                            var opened = false;
                            foreach (var i in w.StaticObjects)
                            {
                                if (i.Value.ObjectType == 0x0721) //locked wine cellar portal
                                {
                                    opened = true;
                                    entity.Move(i.Value.X, i.Value.Y);
                                    w.EnterWorld(entity);
                                    w.LeaveWorld(i.Value);
                                    UpdateCount++;
                                }
                            }
                            if (opened)
                            {
                                psr.SendPacket(new NotificationPacket
                                {
                                    Color = c,
                                    Text = DungName + " opened by " + psr.Account.Name,
                                    ObjectId = psr.Player.Id
                                });

                                w.BroadcastPacket(new TextPacket
                                {
                                    BubbleTime = 0,
                                    Stars = -1,
                                    Name = "",
                                    Text = DungName + " opened by " + psr.Account.Name
                                }, null);
                                w.Timers.Add(new WorldTimer(TimeoutTime*1000,
                                    (world, t) => //default portal close time * 1000
                                    {
                                        try
                                        {
                                            w.LeaveWorld(entity);
                                        }
                                        catch
                                            //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
                                        {
                                            Console.WriteLine(@"Couldn't despawn portal.");
                                        }
                                    }));
                            }
                        }
                        else
                        {
                            entity.Move(X, Y);
                            w.EnterWorld(entity);

                            psr.SendPacket(new NotificationPacket
                            {
                                Color = c,
                                Text = DungName + " opened by " + psr.Account.Name,
                                ObjectId = psr.Player.Id
                            });

                            w.BroadcastPacket(new TextPacket
                            {
                                BubbleTime = 0,
                                Stars = -1,
                                Name = "",
                                Text = DungName + " opened by " + psr.Account.Name
                            }, null);
                            w.Timers.Add(new WorldTimer(TimeoutTime*1000,
                                (world, t) => //default portal close time * 1000
                                {
                                    try
                                    {
                                        w.LeaveWorld(entity);
                                    }
                                    catch
                                        //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
                                    {
                                        Console.WriteLine(@"Couldn't despawn portal.");
                                    }
                                }));
                        }
                    }
                        break;
                    case ActivateEffects.Dye:
                    {
                        if (item.Texture1 != 0)
                        {
                            Texture1 = item.Texture1;
                        }
                        if (item.Texture2 != 0)
                        {
                            Texture2 = item.Texture2;
                        }
                        SaveToCharacter();
                    }
                        break;
                    case ActivateEffects.PartyAOE:
                    {
                        var randomnumber = Random.Next(1, 5);
                        ConditionEffectIndex partyeffect = 0;
                        var color = 0xffffffff;
                        if (randomnumber == 1)
                        {
                            partyeffect = ConditionEffectIndex.Damaging;
                            color = 0xffff0000;
                        }
                        if (randomnumber == 2)
                        {
                            color = 0xff00ff00;
                            partyeffect = ConditionEffectIndex.Speedy;
                        }
                        if (randomnumber == 3)
                        {
                            color = 0xffffd800;
                            partyeffect = ConditionEffectIndex.Berserk;
                        }

                        if (randomnumber == 4)
                        {
                            color = 0xff00ffff;
                            partyeffect = ConditionEffectIndex.Healing;
                        }
                        BehaviorBase.AOE(Owner, this, eff.Range, true, player =>
                        {
                            player.ApplyConditionEffect(new ConditionEffect
                            {
                                Effect = partyeffect,
                                DurationMS = eff.DurationMS
                            });
                        });


                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(color),
                            PosA = new Position {X = eff.Range}
                        }, null);
                    }
                        break;
                    case ActivateEffects.ShurikenAbility:
                    {
                        var w = RealmManager.GetWorld(Owner.Id);
                        ApplyConditionEffect(new ConditionEffect
                        {
                            Effect = ConditionEffectIndex.Speedy,
                            DurationMS = eff.DurationMS
                        });


                        var pt = eff.ObjectId;
                        short obj;
                        XmlDatas.IdToType.TryGetValue(pt, out obj);
                        var substitute = Resolve(obj);
                        substitute.PlayerOwner = this;
                        substitute.isPet = true;
                        w.EnterWorld(substitute);
                        substitute.Move(X, Y);
                        targetlink = target;
                    }
                        break;
                    case ActivateEffects.TomeDamage:
                    {
                        var pkts = new List<Packet>();
                        BehaviorBase.AOE(Owner, this, eff.Range, false,
                            enemy =>
                            {
                                (enemy as Enemy).Damage(this, time,
                                    (int) statsMgr.GetAttackDamage(eff.TotalDamage, eff.TotalDamage), false,
                                    new ConditionEffect[0]);
                            });
                        pkts.Add(new ShowEffectPacket
                        {
                            EffectType = EffectType.AreaBlast,
                            TargetId = Id,
                            Color = new ARGB(0xFF00FF00),
                            PosA = new Position {X = eff.Range}
                        });
                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;
                    case ActivateEffects.Mushroom:
                    {
                        var w = RealmManager.GetWorld(Owner.Id);
                        Size = eff.Amount;
                        UpdateCount++;
                        w.Timers.Add(new WorldTimer(eff.DurationMS, (world, t) =>
                        {
                            try
                            {
                                Size = 100;
                                UpdateCount++;
                            }
                            catch
                            {
                            }
                        }));
                    }
                        break;

                    case ActivateEffects.PearlAbility:
                    {
                        var w = RealmManager.GetWorld(Owner.Id);
                        var pt = eff.ObjectId;
                        short obj;
                        XmlDatas.IdToType.TryGetValue(pt, out obj);
                        var substitute = Resolve(obj);
                        substitute.PlayerOwner = this;
                        substitute.isPet = true;
                        w.EnterWorld(substitute);
                        substitute.Move(X, Y);
                        targetlink = target;
                        var pkts = new List<Packet>();
                        ActivateHealHp(this, eff.Amount, pkts);
                        Owner.BroadcastPackets(pkts, null);
                    }
                        break;

                    case ActivateEffects.PermaPet:
                    {
                        psr.Character.Pet = XmlDatas.IdToType[eff.ObjectId];
                        GivePet(XmlDatas.IdToType[eff.ObjectId]);
                        UpdateCount++;
                    }
                        break;
                    case ActivateEffects.MiniPot:
                    {
                        Client.Player.Stats[1] = 1;
                        Client.Player.Stats[0] = 1;
                        Client.Player.Stats[6] = 1;
                        Client.Player.Stats[7] = 1;
                        Client.Player.Stats[2] = 1;
                        Client.Player.Stats[3] = 1;
                        Client.Player.Stats[5] = 1;
                        Client.Player.Stats[4] = 1;
                    }
                        break;
                    case ActivateEffects.Backpack:
                    {
                        var bps = 1;
                        foreach (var i in psr.Character.Backpacks)
                        {
                            if (bps < i.Key)
                                bps = i.Key;
                        }
                        psr.Character.Backpacks.Add(bps + 1, new short[] {-1, -1, -1, -1, -1, -1, -1, -1});
                        new Database().SaveBackpacks(psr.Character, psr.Account);
                        SendInfo("Added backpack #" + (bps + 1));
                    }
                        break;
                    case ActivateEffects.Drake:
                    {
                        var w = RealmManager.GetWorld(Owner.Id);
                        var pt = eff.ObjectId;
                        short obj;
                        XmlDatas.IdToType.TryGetValue(pt, out obj);
                        var drake = Resolve(obj);
                        drake.PlayerOwner = this;
                        w.EnterWorld(drake);
                        drake.Move(X, Y);
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            TargetId = Id,
                            Color = new ARGB(0x9195A9),
                            EffectType = EffectType.AreaBlast,
                            PosA = new Position
                            {
                                X = 1
                            }
                        }, null);
                        Owner.Timers.Add(new WorldTimer(eff.DurationMS, (world, t) => { w.LeaveWorld(drake); }));
                    }
                        break;
                    case ActivateEffects.BuildTower:
                    {
                        var w = RealmManager.GetWorld(Owner.Id);
                        var pt = eff.ObjectId;

                        short obj;
                        XmlDatas.IdToType.TryGetValue(pt, out obj);
                        var tower = Resolve(obj);
                        tower.PlayerOwner = this;
                        tower.isPet = true;
                        w.EnterWorld(tower);
                        tower.Move(X, Y);
                        Owner.Timers.Add(new WorldTimer(eff.DurationMS, (world, t) => { w.LeaveWorld(tower); }));
                    }
                        break;
                    case ActivateEffects.MonsterToss:
                    {
                        var w = RealmManager.GetWorld(Owner.Id);
                        var pt = eff.ObjectId;
                        short obj;
                        XmlDatas.IdToType.TryGetValue(pt, out obj);
                        var monster = Resolve(obj);
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.Throw,
                            Color = new ARGB(0x000000),
                            TargetId = Id,
                            PosA = target
                        }, null);
                        Owner.Timers.Add(new WorldTimer(1500, (world, t) =>
                        {
                            w.EnterWorld(monster);
                            monster.Move(target.X, target.Y);
                        }));
                    }
                        break;
                    case ActivateEffects.Halo:
                    {
                        var halo = new Halo(this, eff.Range, eff.Amount, eff.DurationMS);
                        halo.Move(X, Y);
                        Owner.EnterWorld(halo);
                    }
                        break;
                    case ActivateEffects.Fame:
                    {
                        CurrentFame = psr.Account.Stats.Fame = new Database().UpdateFame(psr.Account, eff.Amount);
                       
                    }
                        break;
                    case ActivateEffects.SamuraiAbility:
                    {
                        var ydist = target.Y - Y;
                        var xdist = target.X - X;
                        var xwalkable = target.X + xdist/2;
                        var ywalkable = target.Y + ydist/2;
                        var tile = Owner.Map[(int) xwalkable, (int) ywalkable];
                        ObjectDesc desc;
                        if (XmlDatas.TileDescs[tile.TileId].NoWalk)
                            return;
                        if (XmlDatas.ObjectDescs.TryGetValue(tile.ObjType, out desc))
                        {
                            if (!desc.Static)
                                return;
                        }
                        Owner.BroadcastPacket(new ShowEffectPacket
                        {
                            EffectType = EffectType.Diffuse,
                            Color = new ARGB(0xFFFF0000),
                            TargetId = Id,
                            PosA = target,
                            PosB = new Position {X = target.X + eff.Radius, Y = target.Y}
                        }, null);
                        BehaviorBase.AOE(Owner, target, eff.Radius, false, enemy =>
                        {
                            (enemy as Enemy).Damage(this, time, eff.TotalDamage, false, new ConditionEffect
                            {
                                Effect = ConditionEffectIndex.Bleeding,
                                DurationMS = eff.DurationMS
                            });
                        });
                        Move(target.X + xdist/2, target.Y + ydist/2);
                        UpdateCount++;

                        Owner.BroadcastPackets(new Packet[]
                        {
                            new GotoPacket
                            {
                                ObjectId = Id,
                                Position = new Position
                                {
                                    X = X,
                                    Y = Y
                                }
                            },
                            new ShowEffectPacket
                            {
                                EffectType = EffectType.Teleport,
                                TargetId = Id,
                                PosA = new Position
                                {
                                    X = X,
                                    Y = Y
                                },
                                Color = new ARGB(0xFFFFFFFF)
                            }
                        }, null);
                        ApplyConditionEffect(new ConditionEffect
                        {
                            Effect = ConditionEffectIndex.Paralyzed,
                            DurationMS = eff.DurationMS2
                        });
                    }
                        break;
                    case ActivateEffects.Summon:
                    {
                        var w = RealmManager.GetWorld(Owner.Id);
                        var pt = eff.ObjectId + " Summon";

                        short obj;
                        XmlDatas.IdToType.TryGetValue(pt, out obj);
                        var summon = Resolve(obj);
                        summon.PlayerOwner = this;
                        summon.isPet = true;
                        w.EnterWorld(summon);
                        summon.Move(X, Y);
                        Owner.Timers.Add(new WorldTimer(eff.DurationMS, (world, t) => { w.LeaveWorld(summon); }));
                    }
                        break;
                   
                }
            }
            UpdateCount++;
        }
    }
}
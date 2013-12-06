#region

using System;
using wServer.logic.attack;
using wServer.logic.loot;
using wServer.logic.movement;

#endregion

namespace wServer.logic
{
    partial class BehaviorDb
    {
        private static _ Castle = Behav()
            .Init(0x0d82, Behaves("Oryx Brute",
                new RunBehaviors(
                    SimpleWandering.Instance(1, 1f),
                    Chasing.Instance(6, 6, 0, null),
                    Cooldown.Instance(400, SimpleAttack.Instance(2, projectileIndex: 0)),
                    Cooldown.Instance(300, SimpleAttack.Instance(2, projectileIndex: 0)),
                    Cooldown.Instance(400, SimpleAttack.Instance(2, projectileIndex: 0)),
                    Once.Instance(SpawnMinionImmediate.Instance(0x0d81, 1, 4, 4))
                    ),
                loot: new LootBehavior(LootDef.Empty,
                    Tuple.Create(100, new LootDef(0, 5, 0, 8,
                        Tuple.Create(0.1, (ILoot) new StatPotionLoot(StatPotion.Spd)),
                        Tuple.Create(0.03, (ILoot) MpPotionLoot.Instance),
                        Tuple.Create(0.03, (ILoot) HpPotionLoot.Instance)
                        )))
                ))
            .Init(0x0d71, Behaves("Suit of Armor",
                loot: new LootBehavior(LootDef.Empty,
                    Tuple.Create(100, new LootDef(0, 5, 0, 8,
                        Tuple.Create(0.03, (ILoot) MpPotionLoot.Instance),
                        Tuple.Create(0.03, (ILoot) HpPotionLoot.Instance)
                        )))
                ))
            .Init(0x0d72, Behaves("Suit of Armor Sm",
                loot: new LootBehavior(LootDef.Empty,
                    Tuple.Create(100, new LootDef(0, 5, 0, 8,
                        Tuple.Create(0.03, (ILoot) MpPotionLoot.Instance),
                        Tuple.Create(0.03, (ILoot) HpPotionLoot.Instance)
                        )))
                ))
            .Init(0x0d7e, Behaves("Oryx Suit of Armor",
                new RunBehaviors(
                    Chasing.Instance(4, 6, 0, null),
                    Cooldown.Instance(500, SimpleAttack.Instance(2, projectileIndex: 0))
                    ),
                loot: new LootBehavior(LootDef.Empty,
                    Tuple.Create(100, new LootDef(0, 5, 0, 8,
                        Tuple.Create(0.03, (ILoot) MpPotionLoot.Instance),
                        Tuple.Create(0.03, (ILoot) HpPotionLoot.Instance)
                        )))
                ))
            .Init(0x0d7f, Behaves("Oryx Insect Commander",
                new RunBehaviors(
                    SimpleWandering.Instance(1, 1f),
                    Cooldown.Instance(500, SimpleAttack.Instance(2, projectileIndex: 0)),
                    Once.Instance(SpawnMinionImmediate.Instance(0x0d80, 1, 10, 10))
                    ),
                loot: new LootBehavior(LootDef.Empty,
                    Tuple.Create(100, new LootDef(0, 5, 0, 8,
                        Tuple.Create(0.03, (ILoot) MpPotionLoot.Instance),
                        Tuple.Create(0.03, (ILoot) HpPotionLoot.Instance)
                        )))
                ))
            .Init(0x0d80, Behaves("Oryx Insect Minion",
                new RunBehaviors(
                    Cooldown.Instance(500, SimpleAttack.Instance(2, projectileIndex: 0)),
                    Swirling.Instance(2, 15)
                    ),
                loot: new LootBehavior(LootDef.Empty,
                    Tuple.Create(100, new LootDef(0, 5, 0, 8,
                        Tuple.Create(0.03, (ILoot) MpPotionLoot.Instance),
                        Tuple.Create(0.03, (ILoot) HpPotionLoot.Instance)
                        )))
                ))
            .Init(0x0d81, Behaves("Oryx Eye Warrior",
                new RunBehaviors(
                    SimpleWandering.Instance(1, 1f),
                    Chasing.Instance(7, 6, 0, null),
                    Cooldown.Instance(400, SimpleAttack.Instance(5, projectileIndex: 0))
                    ),
                loot: new LootBehavior(LootDef.Empty,
                    Tuple.Create(100, new LootDef(0, 5, 0, 8,
                        Tuple.Create(0.03, (ILoot) MpPotionLoot.Instance),
                        Tuple.Create(0.03, (ILoot) HpPotionLoot.Instance)
                        )))
                ))
            .Init(0x0d86, Behaves("Quiet Bomb",
                new RunBehaviors(
                    Cooldown.Instance(1000, RingAttack.Instance(40, 100, projectileIndex: 0)),
                    Cooldown.Instance(1020, Despawn.Instance)
                    )
                ))
            .Init(0x0d87, Behaves("Oryx's Living Floor",
                new RunBehaviors(
                    Cooldown.Instance(4000, TossEnemyAtPlayer.Instance(10, 0x0d86))
                    )
                ));
    }
}
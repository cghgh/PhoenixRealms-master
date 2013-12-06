#region

using System;
using wServer.logic.attack;
using wServer.logic.movement;
using wServer.logic.taunt;

#endregion

namespace wServer.logic
{
    partial class BehaviorDb
    {
        private static _ Shatters = Behav()
            .Init(0x5005, Behaves("Stone Knight",
                new RunBehaviors(
                    Chasing.Instance(4, 10, 2, null),
                    Cooldown.Instance(450,
                        PredictiveMultiAttack.Instance(10, 5*(float) Math.PI/60, 2, 0, projectileIndex: 0)),
                    Cooldown.Instance(4200, PredictiveMultiAttack.Instance(10, 10*(float) Math.PI/180, 3, 1, 1)
                        ))
                ))
            .Init(0x5007, Behaves("Stone Mage",
                new RunBehaviors(
                    Chasing.Instance(6, 10, 6, null),
                    Cooldown.Instance(100, MultiAttack.Instance(8, 5*(float) Math.PI/100, 2, 0, projectileIndex: 0))
                    )
                ))
            .Init(0x5006, Behaves("Fire Mage",
                new RunBehaviors(
                    Chasing.Instance(6, 10, 6, null),
                    Cooldown.Instance(450, MultiAttack.Instance(8, 5*(float) Math.PI/100, 2, 0, projectileIndex: 0)),
                    Cooldown.Instance(1000, MultiAttack.Instance(8, 5*(float) Math.PI/100, 4, 0, 1)),
                    Cooldown.Instance(1100, MultiAttack.Instance(8, 5*(float) Math.PI/100, 2, 0, 2))
                    )
                ))
            .Init(0x5009, Behaves("Ice Mage",
                new RunBehaviors(
                    Chasing.Instance(6, 10, 6, null),
                    Cooldown.Instance(2000, MultiAttack.Instance(8, 5*(float) Math.PI/100, 5, 0, projectileIndex: 0))
                    )
                ))
            .Init(0x5011, Behaves("Fire Adept",
                new RunBehaviors(
                    Chasing.Instance(6, 10, 6, null),
                    new QueuedBehavior(
                        Cooldown.Instance(300, MultiAttack.Instance(8, 5*(float) Math.PI/100, 1, 0, projectileIndex: 0)),
                        Cooldown.Instance(600, MultiAttack.Instance(8, 5*(float) Math.PI/100, 4, 0, 1)),
                        Cooldown.Instance(900, MultiAttack.Instance(8, 5*(float) Math.PI/100, 4, 0, 2))
                        ))
                ))
            .Init(0x5012, Behaves("Ice Adept",
                new RunBehaviors(
                    Chasing.Instance(8, 9, 5, null),
                    Cooldown.Instance(2500, MultiAttack.Instance(10, 5*(float) Math.PI/100, 8, 0, projectileIndex: 0)),
                    Cooldown.Instance(3500, MultiAttack.Instance(10, 5*(float) Math.PI/100, 1, 0, 1))
                    )
                ))
            .Init(0x5013, Behaves("Titanum",
                new RunBehaviors(
                    Cooldown.Instance(200, MultiAttack.Instance(8, 5*(float) Math.PI/100, 1, 0, projectileIndex: 0)),
                    If.Instance(
                        EntityGroupLesserThan.Instance(10, 10, "Titanumminions"),
                        Rand.Instance(
                            SpawnMinion.Instance(0x5005, 3, 3, 2000, 2000),
                            SpawnMinion.Instance(0x5007, 3, 3, 2000, 2000)
                            )))
                ))
            .Init(0x5015, Behaves("Bridge Sentinel",
                new RunBehaviors(
                    HpLesserPercent.Instance(0.1f, new SetKey(-1, 5)),
                    Once.Instance(new SetKey(-1, 1)),

                    #region Awake
                    IfEqual.Instance(-1, 1,
                        new RunBehaviors(
                            Cooldown.Instance(15000, (new SimpleTaunt("No one can cross this bridge!"))),
                            new QueuedBehavior(
                                Cooldown.Instance(100,
                                    MultiAttack.Instance(10, 5*(float) Math.PI/100, 3, 0, projectileIndex: 0)),
                                Cooldown.Instance(110, MultiAttack.Instance(10, 5*(float) Math.PI/100, 3, 0, 1)),
                                Cooldown.Instance(120, MultiAttack.Instance(10, 5*(float) Math.PI/100, 3, 0, 1)),
                                Cooldown.Instance(130, MultiAttack.Instance(10, 5*(float) Math.PI/100, 3, 0, 1)),
                                Cooldown.Instance(140, MultiAttack.Instance(10, 5*(float) Math.PI/100, 3, 0, 1)),
                                Cooldown.Instance(600)
                                ),
                            new QueuedBehavior(
                                Cooldown.Instance(3000, (SetConditionEffect.Instance(ConditionEffectIndex.Invulnerable))),
                                Cooldown.Instance(5000,
                                    (UnsetConditionEffect.Instance(ConditionEffectIndex.Invulnerable)))
                                ),
                            new QueuedBehavior(
                                Cooldown.Instance(15000),
                                new SetKey(-1, 2))
                            )
                        ),

                    #endregion
                
                    #region Sleepy
                    IfEqual.Instance(-1, 2,
                        new RunBehaviors(
                            new QueuedBehavior(
                                SetConditionEffect.Instance(ConditionEffectIndex.Invulnerable),
                                SetAltTexture.Instance(1),
                                TossEnemy.Instance(180, 3, 0x5022),
                                TossEnemy.Instance(270, 3, 0x5022),
                                TossEnemy.Instance(90, 3, 0x5022),
                                If.Instance(IsEntityPresent.Instance(20, 0x5022),
                                    new SetKey(-1, 3)),
                                Cooldown.Instance(10000)
                                ))),
                    IfEqual.Instance(-1, 3,
                        new RunBehaviors(
                            If.Instance(
                                EntityLesserThan.Instance(20, 1, 0x5022),
                                new SetKey(-1, 4))
                            )),

                    #endregion

                    #region Awake2
                    IfEqual.Instance(-1, 4,
                        new RunBehaviors(
                            Cooldown.Instance(15000, new SimpleTaunt("You chose the wrong way, and you will die!")),
                            new QueuedBehavior(
                                SetAltTexture.Instance(0),
                                Cooldown.Instance(100, MultiAttack.Instance(10, 5*(float) Math.PI/100, 5, 0, 2)),
                                Cooldown.Instance(120, MultiAttack.Instance(10, 5*(float) Math.PI/100, 5, 0, 3)),
                                Cooldown.Instance(600)
                                ),
                            new QueuedBehavior(
                                Cooldown.Instance(3000, (SetConditionEffect.Instance(ConditionEffectIndex.Invulnerable))),
                                Cooldown.Instance(5000,
                                    (UnsetConditionEffect.Instance(ConditionEffectIndex.Invulnerable)))
                                ),
                            new QueuedBehavior(
                                Cooldown.Instance(15000),
                                new SetKey(-1, 1)
                                )
                            )
                        ),

                    #endregion

                    #region NearDeath
                    IfEqual.Instance(-1, 5,
                        new QueuedBehavior(
                            Cooldown.Instance(100, MultiAttack.Instance(10, 5*(float) Math.PI/100, 5, 0, 2)),
                            Cooldown.Instance(110, MultiAttack.Instance(10, 5*(float) Math.PI/100, 5, 0, 3)),
                            Cooldown.Instance(120,
                                MultiAttack.Instance(10, 5*(float) Math.PI/100, 4, 0, projectileIndex: 0)),
                            Cooldown.Instance(130, MultiAttack.Instance(10, 5*(float) Math.PI/100, 3, 0, 1)),
                            Cooldown.Instance(140, MultiAttack.Instance(10, 5*(float) Math.PI/100, 3, 0, 1)),
                            Cooldown.Instance(150, MultiAttack.Instance(10, 5*(float) Math.PI/100, 3, 0, 1)),
                            Cooldown.Instance(800)
                            )
                        )
                    #endregion

                    )))
            .Init(0x5022, Behaves("Paladin Obelisk",
                new RunBehaviors(
                    Cooldown.Instance(600, MultiAttack.Instance(8, 5*(float) Math.PI/100, 8, 0, projectileIndex: 0)),
                    Cooldown.Instance(600, MultiAttack.Instance(8, 5*(float) Math.PI/100, 8, 0, 1)),
                    If.Instance(
                        EntityLesserThan.Instance(20, 10, 0x5020),
                        Rand.Instance(
                            SpawnMinionImmediate.Instance(0x5020, 5, 4, 4)))
                    )
                ))
            .Init(0x5020, Behaves("Stone Paladin",
                new RunBehaviors(
                    Chasing.Instance(4, 9, 1, null),
                    Cooldown.Instance(1000, MultiAttack.Instance(4, 5*(float) Math.PI/100, 3, 0, projectileIndex: 0))
                    )
                ));
    }
}
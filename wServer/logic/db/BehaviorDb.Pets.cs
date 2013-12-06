﻿#region

using System;
using wServer.logic.attack;
using wServer.logic.cond;
using wServer.logic.movement;

#endregion

namespace wServer.logic
{
    partial class BehaviorDb
    {
        private static _ Pets = Behav()
            .InitMany(0x1600, 0x1641, i => Behaves("Pet",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(8, 10, 3)),
                    Cooldown.Instance(3000, new PetsHealingHP(10, 30)),
                    Cooldown.Instance(3000, new PetsHealingMP(10, 20)),
                    Cooldown.Instance(3000, new PetAttack(50, 70, 10)
                        ))
                ))
            .Init(0x1659, Behaves("Josh's Pet",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(1500, new PetsHealingHP(80, 80)),
                    Cooldown.Instance(1500, new PetsHealingMP(50, 50)),
                    Cooldown.Instance(2000, new PetAttack(400, 500, 7)
                        ))
                ))
            .Init(0x1660, Behaves("Skeletal Guardian Pet",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(1500, new PetsHealingMP(75, 75))
                    ))
            )
            .Init(0x1661, Behaves("Knight Pet",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(1500, new PetsHealingHP(76, 76)),
                    Cooldown.Instance(1500, new PetsHealingMP(38, 38)),
                    Cooldown.Instance(2000, new PetAttack(200, 300, 7)
                        ))
                ))
                .Init(0x1662, Behaves("Lunar's Phoenix",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(1500, new PetsHealingHP(60, 70)),
                    Cooldown.Instance(1500, new PetsHealingMP(40, 50))
                        ))
                )
            .Init(0x1656, Behaves("Dallas's Pet",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(1500, new PetsHealingHP(69, 69)),
                    Cooldown.Instance(1500, new PetsHealingMP(40, 40))
                    ))
            )
            .Init(0x1657, Behaves("Astro's Griffin",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(1500, new PetsHealingHP(76, 76)),
                    Cooldown.Instance(1500, new PetsHealingMP(38, 38))
                    ))
            )
            .Init(0x1658, Behaves("Scootaloo",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(1500, new PetsHealingHP(50, 50)),
                    Cooldown.Instance(1500, new PetsHealingMP(60, 60))
                    ))
            )
            .Init(0x1663, Behaves("Twilight Sparkle",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(1500, new PetsHealingMP(70, 70)),
                    Cooldown.Instance(1500, PetMultiAttack.Instance(7, 10*(float) Math.PI/180, 4))
                    ))
            )
            .Init(0x1664, Behaves("Kythal the Necromancer",
                new RunBehaviors(
                    If.Instance(new PetBehaves(), PetChasing.Instance(13, 8, 3)),
                    Cooldown.Instance(800, new PetsHealingHP(45, 75)),
                    Cooldown.Instance(800, new PetsHealingMP(35, 65)),
                    Cooldown.Instance(750, PetMultiAttack.Instance(10, 10*(float) Math.PI/180, 5, 0, projectileIndex: 0)),
                    Cooldown.Instance(800, PetMultiAttack.Instance(10, 10*(float) Math.PI/180, 3, 25, 1))
                    )))
            .Init(0x1642, Behaves("Cube Pet",
                new RunBehaviors(
                    Cooldown.Instance(750,
                        PetPredictiveMultiAttack.Instance(25, 10*(float) Math.PI/180, 9, 1, projectileIndex: 0)),
                    Cooldown.Instance(1500, PetPredictiveMultiAttack.Instance(25, 10*(float) Math.PI/180, 4, 1, 1)),
                    If.Instance(new PetBehaves(), PetChasing.Instance(8, 10, 3))
                    )))
            .InitMany(0x1643, 0x1655, (i) => Behaves("Elite Pet",
            new RunBehaviors(
                new Switch(new IfValue("Speed", "Value", If.Instance(new PetBehaves(), PetChasing.Instance(new GetTag(i).GetInt("Speed", "Value", 10), new GetTag(i).GetInt("Speed", "Value", 10), 3))),
                    If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3))),
                Cooldown.Instance(new GetTag(i).GetInt("HP", "Cooldown", 1000), new IfTag("HP", new PetsHealingHP(new GetTag(i).GetInt("HP", "Min", 50), new GetTag(i).GetInt("HP", "Max", 100)))),
                Cooldown.Instance(new GetTag(i).GetInt("MP", "Cooldown", 1000), new IfTag("MP", new PetsHealingMP(new GetTag(i).GetInt("MP", "Min", 30), new GetTag(i).GetInt("MP", "Max", 60)))),
                Cooldown.Instance(new GetTag(i).GetInt("Dmg", "Cooldown", 1000), new IfTag("Dmg", new PetAttack(new GetTag(i).GetInt("Dmg", "Min", 50), new GetTag(i).GetInt("Dmg", "Max", 70), new GetTag(i).GetInt("Dmg", "Range", 10))))
                //Cooldown.Instance(1000, ThrowAttackPet.Instance(4, 10, 40, 70))
            )))
            .InitMany(0x6101, 0x61fe, i => Behaves("Donator Pet",
                new RunBehaviors(
                        If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(new GetTag(i).GetInt("HP", "Cooldown", 1000),
                        new IfTag("HP",
                            new PetsHealingHP(new GetTag(i).GetInt("HP", "Min", 0),
                                new GetTag(i).GetInt("HP", "Max", 0)))),
                    Cooldown.Instance(new GetTag(i).GetInt("MP", "Cooldown", 0),
                        new IfTag("MP",
                            new PetsHealingMP(new GetTag(i).GetInt("MP", "Min", 30),
                                new GetTag(i).GetInt("MP", "Max", 60)))),
                                Cooldown.Instance(1200, PetSimpleAttack.Instance(10))
                    )))
            .InitMany(0x61ff, 0x61ff, i => Behaves("Aceaku's Pet",
                new RunBehaviors(
                        If.Instance(new PetBehaves(), PetChasing.Instance(10, 10, 3)),
                    Cooldown.Instance(new GetTag(i).GetInt("HP", "Cooldown", 1000),
                        new IfTag("HP",
                            new PetsHealingHP(new GetTag(i).GetInt("HP", "Min", 0),
                                new GetTag(i).GetInt("HP", "Max", 0)))),
                    Cooldown.Instance(new GetTag(i).GetInt("MP", "Cooldown", 0),
                        new IfTag("MP",
                            new PetsHealingMP(new GetTag(i).GetInt("MP", "Min", 30),
                                new GetTag(i).GetInt("MP", "Max", 60))))
                    )))
            ;
    }
}
﻿#region

using System.Linq;
using db;
using wServer.cliPackets;
using wServer.logic.loot;
using wServer.realm.worlds;
using wServer.svrPackets;

#endregion

namespace wServer.realm.entities.player
{
    partial class Player
    {
        public string[] TierItems1;

        public void TextBoxButton(TextBoxButtonPacket pkt)
        {
            var type = pkt.Type;

            if (type == "test")
            {
                psr.SendPacket(new TextBoxPacket
                {
                    Button1 = "Yes",
                    Button2 = "No",
                    Message = "Do you want to enter the testing arena?",
                    Title = "Testing Arena Confirmation",
                    Type = "EnterTestArena"
                });
            }
            if (type == "NewClient")
            {
                psr.Disconnect();
            }
            if (type == "DecideArena")
            {
                if (pkt.Button == 1)
                {
                    psr.SendPacket(new TextBoxPacket
                    {
                        Button1 = "Enter",
                        Button2 = "Cancel",
                        Message = "Host an arena at the price of x fame?",
                        Title = "Arena Host Confirmation",
                        Type = "EnterArena2"
                    });
                }
                else
                {
                    psr.SendPacket(new TextBoxPacket
                    {
                        Button1 = "Enter",
                        Button2 = "Cancel",
                        Message = "Enter the arena solo at the price of 150 fame?",
                        Title = "Solo Arena Confirmation",
                        Type = "EnterArena1"
                    });
                }
            }
            if (type == "EnterTestArena")
            {
                if (pkt.Button == 1)
                {
                    if (Client.Account.Stats.Fame >= 150)
                    {
                        /*RealmManager.PlayerWorldMapping.TryAdd(this.AccountId, Owner);
                        psr.Reconnect(new ReconnectPacket()
                        {
                            Host = "",
                            Port = 2050,
                            GameId = world.Id,
                            Name = world.Name,
                            Key = Empty<byte>.Array,
                        });
                        */
                    }
                    else
                    {
                        SendHelp("Not Enough Fame");
                    }
                }
                else
                {
                    SendInfo("Cancelled entering arena.");
                }
            }
            if (type == "EnterArena1")
            {
                if (pkt.Button == 1)
                {
                    if (Client.Account.Stats.Fame >= 150)
                    {
                        using (var db = new Database())
                        {
                            db.UpdateFame(psr.Account, -150);
                            db.Dispose();
                        }

                        var world = RealmManager.GetWorld(World.NEXUS_ID);
                        var fworld = false;
                        foreach (var i in RealmManager.Worlds)
                            if (i.Value is BattleArenaMap)
                                if ((i.Value as BattleArenaMap).Joinable)
                                {
                                    world = i.Value;
                                    fworld = true;
                                    break;
                                }
                        if (!fworld)
                            world = RealmManager.AddWorld(new BattleArenaMap());

                        psr.Reconnect(new ReconnectPacket
                        {
                            Host = "",
                            Port = 2050,
                            GameId = world.Id,
                            Name = world.Name,
                            Key = Empty<byte>.Array,
                        });
                    }
                    else
                    {
                        SendHelp("Not Enough Fame");
                    }
                }
                else
                {
                    SendInfo("Cancelled entering arena.");
                }
            }
            if (type == "EnterArena2")
            {
                if (pkt.Button == 1)
                {
                    var world = RealmManager.GetWorld(World.NEXUS_ID);
                    var fworld = false;
                    foreach (var i in RealmManager.Worlds)
                        if (i.Value is BattleArenaMap2)
                            if ((i.Value as BattleArenaMap2).Joinable)
                            {
                                world = i.Value;
                                fworld = true;
                                break;
                            }
                    if (!fworld)
                        world = RealmManager.AddWorld(new BattleArenaMap2());

                    psr.Reconnect(new ReconnectPacket
                    {
                        Host = "",
                        Port = 2050,
                        GameId = world.Id,
                        Name = world.Name,
                        Key = Empty<byte>.Array,
                    });
                }
                else
                {
                    SendInfo("Cancelled entering arena.");
                }
            }
            if (type == "SheepHerding")
            {
                if (pkt.Button == 1)
                {
                    if (Client.Account.Stats.Fame >= 500)
                    {
                        using (var db = new Database())
                        {
                            db.UpdateFame(psr.Account, -500);
                            db.Dispose();
                        }

                        var world = RealmManager.GetWorld(World.NEXUS_ID);
                        var fworld = false;
                        foreach (var i in RealmManager.Worlds)
                            if (i.Value is Herding)
                                if ((i.Value as Herding).Joinable)
                                {
                                    world = i.Value;
                                    fworld = true;
                                    break;
                                }
                        if (!fworld)
                            world = RealmManager.AddWorld(new Herding());

                        psr.Reconnect(new ReconnectPacket
                        {
                            Host = "",
                            Port = 2050,
                            GameId = world.Id,
                            Name = world.Name,
                            Key = Empty<byte>.Array,
                        });
                    }
                    else
                    {
                        SendHelp("Not Enough Fame");
                    }
                }
                else
                {
                    SendInfo("Cancelled entering sheep herding.");
                }
            }
            if (type == "Zombies")
            {
                if (pkt.Button == 1)
                {
                    if (Client.Account.Stats.Fame >= 100)
                    {
                        using (var db = new Database())
                        {
                            db.UpdateFame(psr.Account, -100);
                            db.Dispose();
                        }

                        var world = RealmManager.GetWorld(World.NEXUS_ID);
                        var fworld = false;
                        foreach (var i in RealmManager.Worlds)
                            if (i.Value is ZombieMG)
                                if ((i.Value as ZombieMG).Joinable)
                                {
                                    world = i.Value;
                                    fworld = true;
                                    break;
                                }
                        if (!fworld)
                            world = RealmManager.AddWorld(new ZombieMG());

                        psr.Reconnect(new ReconnectPacket
                        {
                            Host = "",
                            Port = 2050,
                            GameId = world.Id,
                            Name = world.Name,
                            Key = Empty<byte>.Array,
                        });
                    }
                    else
                    {
                        SendHelp("Not Enough Fame");
                    }
                }
                else
                {
                    SendInfo("Cancelled entering zombies.");
                }
            }
            if (type == "SlotMachine1")
            {
                if (pkt.Button == 1)
                {
                    var weaponsT5 = TierLoot.WeaponItems[5].ToList();
                    var weaponsT6 = TierLoot.WeaponItems[6].ToList();
                    var weaponsT7 = TierLoot.WeaponItems[7].ToList();
                    var abilitiesT3 = TierLoot.AbilityItems[2].ToList();
                    var ringsT3 = TierLoot.RingItems[3].ToList();
                    var armorT6 = TierLoot.ArmorItems[6].ToList();
                    var armorT7 = TierLoot.ArmorItems[7].ToList();
                    var armorT8 = TierLoot.ArmorItems[8].ToList();

                    var calculator = Random.Next(1, 1000);
                    if (calculator <= 600)
                    {
                        SendHelp("Better luck next time!");
                    }
                    else if (calculator <= 700 && calculator > 600)
                    {
                        SendHelp("Congratulations! You won a T5 Weapon!");

                        weaponsT5.Shuffle();

                        var container = new Container(0x0507, 1000*60, true) {BagOwner = psr.Account.AccountId};
                        container.Inventory[0] = weaponsT5[0];
                        container.Move(X + (float) ((invRand.NextDouble()*2 - 1)*0.5),
                            Y + (float) ((invRand.NextDouble()*2 - 1)*0.5));
                        container.Size = 75;
                        Owner.EnterWorld(container);
                    }
                    else if (calculator <= 750 && calculator > 700)
                    {
                        SendHelp("Congratulations! You won a T6 Weapon!");

                        weaponsT6.Shuffle();

                        var container = new Container(0x0507, 1000*60, true) {BagOwner = psr.Account.AccountId};
                        container.Inventory[0] = weaponsT6[0];
                        container.Move(X + (float) ((invRand.NextDouble()*2 - 1)*0.5),
                            Y + (float) ((invRand.NextDouble()*2 - 1)*0.5));
                        container.Size = 75;
                        Owner.EnterWorld(container);
                    }
                    else if (calculator <= 787.5 && calculator > 775)
                    {
                        SendHelp("Congratulations! You won a T7 Weapon!");

                        weaponsT7.Shuffle();

                        var container = new Container(0x0507, 1000*60, true) {BagOwner = psr.Account.AccountId};
                        container.Inventory[0] = weaponsT7[0];
                        container.Move(X + (float) ((invRand.NextDouble()*2 - 1)*0.5),
                            Y + (float) ((invRand.NextDouble()*2 - 1)*0.5));
                        container.Size = 75;
                        Owner.EnterWorld(container);
                    }
                    else if (calculator <= 800 && calculator > 787.5)
                    {
                        SendHelp("Congratulations! You won a T3 Ability!");

                        abilitiesT3.Shuffle();

                        var container = new Container(0x0507, 1000*60, true) {BagOwner = psr.Account.AccountId};
                        container.Inventory[0] = abilitiesT3[0];
                        container.Move(X + (float) ((invRand.NextDouble()*2 - 1)*0.5),
                            Y + (float) ((invRand.NextDouble()*2 - 1)*0.5));
                        container.Size = 75;
                        Owner.EnterWorld(container);
                    }
                    else if (calculator <= 850 && calculator > 800)
                    {
                        SendHelp("Congratulations! You won a T6 Armor!");

                        armorT6.Shuffle();

                        var container = new Container(0x0507, 1000*60, true) {BagOwner = psr.Account.AccountId};
                        container.Inventory[0] = armorT6[0];
                        container.Move(X + (float) ((invRand.NextDouble()*2 - 1)*0.5),
                            Y + (float) ((invRand.NextDouble()*2 - 1)*0.5));
                        container.Size = 75;
                        Owner.EnterWorld(container);
                    }
                    else if (calculator <= 875 && calculator > 850)
                    {
                        SendHelp("Congratulations! You won a T7 Armor!");

                        armorT7.Shuffle();

                        var container = new Container(0x0507, 1000*60, true) {BagOwner = psr.Account.AccountId};
                        container.Inventory[0] = armorT7[0];
                        container.Move(X + (float) ((invRand.NextDouble()*2 - 1)*0.5),
                            Y + (float) ((invRand.NextDouble()*2 - 1)*0.5));
                        container.Size = 75;
                        Owner.EnterWorld(container);
                    }
                    else if (calculator <= 887.5 && calculator > 875)
                    {
                        SendHelp("Congratulations! You won a T8 Armor!");

                        armorT8.Shuffle();

                        var container = new Container(0x0507, 1000*60, true) {BagOwner = psr.Account.AccountId};
                        container.Inventory[0] = armorT8[0];
                        container.Move(X + (float) ((invRand.NextDouble()*2 - 1)*0.5),
                            Y + (float) ((invRand.NextDouble()*2 - 1)*0.5));
                        container.Size = 75;
                        Owner.EnterWorld(container);
                    }
                    else if (calculator <= 900 && calculator > 887.5)
                    {
                        SendHelp("Congratulations! You won a T3 Ring!");

                        ringsT3.Shuffle();

                        var container = new Container(0x0507, 1000*60, true) {BagOwner = psr.Account.AccountId};
                        container.Inventory[0] = ringsT3[0];
                        container.Move(X + (float) ((invRand.NextDouble()*2 - 1)*0.5),
                            Y + (float) ((invRand.NextDouble()*2 - 1)*0.5));
                        container.Size = 75;
                        Owner.EnterWorld(container);
                    }
                    else if (calculator <= 905 && calculator > 900)
                    {
                        SendHelp("Too bad! You only got 1 fame!");
                        Client.Database.UpdateFame(Client.Account, 1);
                        Fame += 1;
                        UpdateCount++;
                    }
                    else if (calculator <= 910 && calculator > 905)
                    {
                        SendHelp("Too bad! You only got 5 fame!");
                        Client.Database.UpdateFame(Client.Account, 5);
                        Fame += 5;
                        UpdateCount++;
                    }
                    else if (calculator <= 940 && calculator > 910)
                    {
                        SendHelp("You won back the fame you paid!");
                        Client.Database.UpdateFame(Client.Account, 10);
                        Fame += 10;
                        UpdateCount++;
                    }
                    else if (calculator <= 970 && calculator > 940)
                    {
                        SendHelp("Nice! You won 25 fame!");
                        Client.Database.UpdateFame(Client.Account, 25);
                        Fame += 25;
                        UpdateCount++;
                    }
                    else if (calculator <= 985 && calculator > 970)
                    {
                        SendHelp("Nice! You won 50 fame!");
                        Client.Database.UpdateFame(Client.Account, 50);
                        Fame += 50;
                        UpdateCount++;
                    }
                    else if (calculator <= 990 && calculator > 985)
                    {
                        SendHelp("Very Nice! You won 100 fame!");
                        Client.Database.UpdateFame(Client.Account, 100);
                        Fame += 100;
                        UpdateCount++;
                    }
                    else if (calculator <= 994 && calculator > 990)
                    {
                        SendHelp("Awesome! You won 500 fame!");
                        Client.Database.UpdateFame(Client.Account, 500);
                        Fame += 500;
                        UpdateCount++;
                    }
                    else if (calculator <= 997 && calculator > 994)
                    {
                        SendHelp("Amazing! You won 1000 fame!");
                        Client.Database.UpdateFame(Client.Account, 1000);
                        Fame += 1000;
                        UpdateCount++;
                    }
                    else if (calculator <= 999 && calculator > 997)
                    {
                        SendHelp("Amazing! You won 5000 fame!");
                        Client.Database.UpdateFame(Client.Account, 5000);
                        Fame += 5000;
                        UpdateCount++;
                    }
                    else if (calculator <= 1000 && calculator > 999)
                    {
                        SendHelp("Incredible! You won the 10000 fame jackpot!");
                        foreach (var i in RealmManager.Clients.Values)
                            i.SendPacket(new TextPacket
                            {
                                BubbleTime = 0,
                                Stars = -1,
                                Name = "#Announcement",
                                Text = Name + " has won the 10000 Fame jackpot on the bronze slot machines!"
                            });
                        Client.Database.UpdateFame(Client.Account, 10000);
                        Fame += 10000;
                        UpdateCount++;
                    }
                    psr.SendPacket(new BuyResultPacket
                    {
                        Result = 0
                    });
                }
                else
                {
                    SendInfo("Canceled");
                    psr.SendPacket(new BuyResultPacket
                    {
                        Result = 0
                    });
                }
            }
        }
    }
}
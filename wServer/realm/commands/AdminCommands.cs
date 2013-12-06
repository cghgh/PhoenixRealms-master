﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using db;
using db.data;
using MySql.Data.MySqlClient;
using wServer.realm.entities;
using wServer.realm.entities.player;
using wServer.realm.setpieces;
using wServer.realm.worlds;
using wServer.svrPackets;

#endregion

namespace wServer.realm.commands
{
    internal class SpawnCommand : ICommand
    {
        public string Command
        {
            get { return "spawn"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            int num;
            if (args.Length > 0 && int.TryParse(args[0], out num)) //multi
            {
                var name = string.Join(" ", args.Skip(1).ToArray());
                short objType;
                //creates a new case insensitive dictionary based on the XmlDatas
                var icdatas = new Dictionary<string, short>(XmlDatas.IdToType, StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType) ||
                    !XmlDatas.ObjectDescs.ContainsKey(objType))
                {
                    player.SendInfo("Unknown entity!");
                }
                else
                {
                    var c = int.Parse(args[0]);
                    if (player.Client.Account.Rank < 5 && c > 50)
                    {
                        player.SendError("Maximum spawn count is set to 50!");
                        return;
                    }


                    if (player.Client.Account.Rank > 4 && c > 50)
                    {
                        player.SendInfo("Bypass made!");
                    }

                    for (var i = 0; i < num; i++)
                    {
                        var entity = Entity.Resolve(objType);
                        entity.Move(player.X, player.Y);
                        player.Owner.EnterWorld(entity);
                    }
                    var dir = @"logs";
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    using (var writer = new StreamWriter(@"logs\SpawnLog.log", true))
                    {
                        writer.WriteLine(player.Name + " spawned " + c + " " + name + "s");
                    }
                    player.SendInfo("Success!");
                }
            }
            else
            {
                var name = string.Join(" ", args);
                short objType;
                //creates a new case insensitive dictionary based on the XmlDatas
                var icdatas = new Dictionary<string, short>(XmlDatas.IdToType, StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType) ||
                    !XmlDatas.ObjectDescs.ContainsKey(objType))
                {
                    player.SendHelp("Usage: /spawn <entity name>");
                }
                else
                {
                    var entity = Entity.Resolve(objType);
                    entity.Move(player.X, player.Y);
                    player.Owner.EnterWorld(entity);
                }
                var dir = @"logs";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                using (var writer = new StreamWriter(@"logs\SpawnLog.log", true))
                {
                    writer.WriteLine(player.Name + " spawned a " + name);
                }
            }
        }
    }

    internal class ArenaCommand : ICommand
    {
        public string Command
        {
            get { return "arena"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            var prtal = Entity.Resolve(0x1900);
            prtal.Move(player.X, player.Y);
            player.Owner.EnterWorld(prtal);
            var w = RealmManager.GetWorld(player.Owner.Id);
            w.Timers.Add(new WorldTimer(30*1000, (world, t) => //default portal close time * 1000

            {
                try
                {
                    w.LeaveWorld(prtal);
                }
                catch //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
                {
                    Console.Out.WriteLine("Couldn't despawn portal.");
                }
            }));
            foreach (var i in RealmManager.Clients.Values)
                i.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "",
                    Text = "Arena Opened by:" + " " + player.nName
                });
            foreach (var i in RealmManager.Clients.Values)
                i.SendPacket(new NotificationPacket
                {
                    Color = new ARGB(0xff00ff00),
                    ObjectId = player.Id,
                    Text = "Arena Opened by " + player.nName
                });
        }
    }

    internal class GraveCommand : ICommand
    {
        public string Command
        {
            get { return "grave"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            var name = "";
            var maxed = 0;
            var rand = new Random();
            try
            {
                if (args.Length == 0)
                {
                    var list = new List<string>();
                    var num = 0;
                    foreach (var i in RealmManager.Clients.Values)
                    {
                        list.Add(i.Account.Name);
                        num++;
                    }
                    var array = list.ToArray();
                    var random = new Random();
                    var length = array.Length;
                    name = array[random.Next(0, length - 1)];
                    maxed = rand.Next(1, 11);
                }
                else if (args.Length == 1)
                {
                    name = args[0];
                    maxed = rand.Next(1, 11);
                }
                else if (args.Length == 2)
                {
                    name = args[0];
                    maxed = int.Parse(args[1]);
                }
                else
                {
                    var list = new List<string>();
                    var num = 0;
                    foreach (var i in RealmManager.Clients.Values)
                    {
                        list.Add(i.Account.Name);
                        num++;
                    }
                    var array = list.ToArray();
                    var random = new Random();
                    var length = array.Length;
                    name = array[random.Next(0, length - 1)];
                    maxed = rand.Next(1, 11);
                }
            }
            catch
            {
            }
            short objType;
            int? time;
            switch (maxed)
            {
                case 11:
                    objType = 0x0725;
                    time = 5*60*1000;
                    break;
                case 10:
                    objType = 0x0724;
                    time = 60*1000;
                    break;
                case 9:
                    objType = 0x0723;
                    time = 30*1000;
                    break;
                case 8:
                    objType = 0x0735;
                    time = null;
                    break;
                case 7:
                    objType = 0x0734;
                    time = null;
                    break;
                case 6:
                    objType = 0x072b;
                    time = null;
                    break;
                case 5:
                    objType = 0x072a;
                    time = null;
                    break;
                case 4:
                    objType = 0x0729;
                    time = null;
                    break;
                case 3:
                    objType = 0x0728;
                    time = null;
                    break;
                case 2:
                    objType = 0x0727;
                    time = null;
                    break;
                case 1:
                    objType = 0x0726;
                    time = null;
                    break;
                default:
                    objType = 0x0725;
                    time = 5*60*1000;
                    break;
            }
            var obj = new StaticObject(objType, time, true, time == null ? false : true, false);
            obj.Move(player.X, player.Y);
            obj.Name = name;
            player.Owner.EnterWorld(obj);
        }
    }

    internal class AddEffCommand : ICommand
    {
        public string Command
        {
            get { return "addeff"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /addeff <effect name or effect number>");
            }
            else
            {
                try
                {
                    player.ApplyConditionEffect(new ConditionEffect
                    {
                        Effect = (ConditionEffectIndex) Enum.Parse(typeof (ConditionEffectIndex), args[0].Trim(), true),
                        DurationMS = -1
                    });
                    {
                        player.SendInfo("Success!");
                    }
                }
                catch
                {
                    player.SendError("Invalid effect!");
                }
            }
        }
    }

    internal class RemoveEffCommand : ICommand
    {
        public string Command
        {
            get { return "remeff"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /remeff <effect name or effect number>");
            }
            else
            {
                try
                {
                    player.ApplyConditionEffect(new ConditionEffect
                    {
                        Effect = (ConditionEffectIndex) Enum.Parse(typeof (ConditionEffectIndex), args[0].Trim(), true),
                        DurationMS = 0
                    });
                    player.SendInfo("Success!");
                }
                catch
                {
                    player.SendError("Invalid effect!");
                }
            }
        }
    }

    internal class GiveCommand : ICommand
    {
        public string Command
        {
            get { return "give"; }
        }

        public int RequiredRank
        {
            get { return 2; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /give <item name>");
            }
            else
            {
                var name = string.Join(" ", args.ToArray()).Trim();
                short objType;
                //creates a new case insensitive dictionary based on the XmlDatas
                var icdatas = new Dictionary<string, short>(XmlDatas.IdToType, StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType))
                {
                    player.SendError("Unknown type!");
                    return;
                }
                if (!XmlDatas.ItemDescs[objType].Secret || player.Client.Account.Rank >= 4)
                {
                    for (var i = 0; i < player.Inventory.Length; i++)
                        if (player.Inventory[i] == null)
                        {
                            player.Inventory[i] = XmlDatas.ItemDescs[objType];
                            player.UpdateCount++;

                            player.SendInfo("Success!");

                            var dir = @"logs";
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            using (var writer = new StreamWriter(@"logs\GiveLog.log", true))
                            {
                                writer.WriteLine(player.Name + " gave themselves a " + name);
                            }
                            return;
                        }
                }
                else
                {
                    player.SendError("Item cannot be given!");
                }
            }
        }
    }

    internal class TpCommand : ICommand
    {
        public string Command
        {
            get { return "tp"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                player.SendHelp("Usage: /tp <X coordinate> <Y coordinate>");
            }
            else
            {
                int x, y;
                try
                {
                    x = int.Parse(args[0]);
                    y = int.Parse(args[1]);
                }
                catch
                {
                    player.SendError("Invalid coordinates!");
                    return;
                }
                player.Move(x + 0.5f, y + 0.5f);
                player.SetNewbiePeriod();
                player.UpdateCount++;
                player.Owner.BroadcastPacket(new GotoPacket
                {
                    ObjectId = player.Id,
                    Position = new Position
                    {
                        X = player.X,
                        Y = player.Y
                    }
                }, null);
            }
        }
    }

    internal class SetpieceCommand : ICommand
    {
        public string Command
        {
            get { return "setpiece"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /setpiece <setpiece ID>");
            }
            else
            {
                try
                {
                    var piece = (ISetPiece) Activator.CreateInstance(Type.GetType(
                        "wServer.realm.setpieces." + args[0]));
                    piece.RenderSetPiece(player.Owner, new IntPoint((int) player.X + 1, (int) player.Y + 1));

                    player.SendInfo("Success!");
                }
                catch
                {
                    player.SendError("Cannot apply setpiece!");
                }
            }
        }
    }

    internal class DebugCommand : ICommand
    {
        public string Command
        {
            get { return "debug"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            player.Client.SendPacket(new TextBoxPacket
            {
                Button1 = "I Understand",
                Button2 = "Cancel",
                Message = "This is a test to see if you can force a disconect using TextBoxPackets",
                Title = "Testing!",
                Type = "Test"
            });
        }
    }

    internal class KillAll : ICommand
    {
        public string Command
        {
            get { return "killall"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /killall <entity name>");
            }
            else
            {
                foreach (var i in player.Owner.Enemies)
                {
                    if ((i.Value.ObjectDesc != null) &&
                        (i.Value.ObjectDesc.ObjectId != null) &&
                        (i.Value.ObjectDesc.ObjectId.Contains(args[0])))
                    {
                        i.Value.Damage(player, new RealmTime(), 1000*10000, true); //may not work for ents/liches
                        //i.Value.Owner.LeaveWorld(i.Value);
                    }
                }
                player.SendInfo("Success!");
            }
        }
    }

    internal class KillAllX : ICommand
        //this version gives XP points, but does not work for enemies with evaluation/inv periods
    {
        public string Command
        {
            get { return "killallx"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /killallx <entity name>");
            }
            else
            {
                foreach (var i in player.Owner.Enemies)
                {
                    if ((i.Value.ObjectDesc != null) &&
                        (i.Value.ObjectDesc.ObjectId != null) &&
                        (i.Value.ObjectDesc.ObjectId.Contains(args[0])))
                    {
                        i.Value.Damage(player, new RealmTime(), 1000*10000, true); //may not work for ents/liches, 
                        //i.Value.Owner.LeaveWorld(i.Value);
                    }
                }
                player.SendInfo("Success!");
            }
        }
    }

    internal class Kick : ICommand
    {
        public string Command
        {
            get { return "kick"; }
        }

        public int RequiredRank
        {
            get { return 2; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /kick <player name>");
            }
            else
            {
                try
                {
                    foreach (var i in player.Owner.Players)
                    {
                        if (i.Value.nName.ToLower() == args[0].ToLower().Trim())
                        {
                            player.SendInfo("Player Disconnected");
                            i.Value.Client.Disconnect();
                        }
                    }
                }
                catch
                {
                    player.SendError("Cannot kick!");
                }
            }
        }
    }

    internal class GetQuest : ICommand
    {
        public string Command
        {
            get { return "getquest"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            player.SendInfo("Loc: " + player.Quest.X + " " + player.Quest.Y);
        }
    }

    internal class OryxSay : ICommand
    {
        public string Command
        {
            get { return "osay"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /oryxsay <text>");
            }
            else
            {
                var saytext = string.Join(" ", args);
                player.SendEnemy("Oryx the Mad God", saytext);
            }
        }
    }

    internal class SWhoCommand : ICommand //get all players from all worlds (this may become too large!)
    {
        public string Command
        {
            get { return "swho"; }
        }

        public int RequiredRank
        {
            get { return 0; }
        }

        public void Execute(Player player, string[] args)
        {
            var sb = new StringBuilder("All conplayers: ");
            var players = 0;
            foreach (var w in RealmManager.Worlds)
            {
                var world = w.Value;
                if (w.Key != 0)
                {
                    var copy = world.Players.Values.ToArray();
                    if (copy.Length != 0)
                    {
                        for (var i = 0; i < copy.Length; i++)
                        {
                            sb.Append(copy[i].Name);
                            sb.Append(", ");
                            players++;
                        }
                    }
                }
            }
            sb.Append("players online: ");
            sb.Append(players.ToString());
            var fixedString = sb.ToString().TrimEnd(',', ' '); //clean up trailing ", "s

            player.SendInfo(fixedString);
        }
    }

    internal class Announcement : ICommand
    {
        public string Command
        {
            get { return "announce"; }
        } //msg all players in all worlds

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /announce <text>");
            }
            else
            {
                var saytext = string.Join(" ", args);

                foreach (var i in RealmManager.Clients.Values)
                    i.SendPacket(new TextPacket
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "#Announcement",
                        Text = " " + saytext
                    });
            }
        }
    }

    internal class Summon : ICommand
    {
        public string Command
        {
            get { return "summon"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /summon <player name>");
            }
            else
            {
                var plr = RealmManager.FindPlayer(args[0]);
                if (plr != null)
                {
                    plr.Client.Reconnect(new ReconnectPacket
                    {
                        Host = "",
                        Port = 2050,
                        GameId = player.Owner.Id,
                        Name = player.Owner.Name,
                        Key = Empty<byte>.Array,
                    });
                    return;
                }
                player.SendInfo(string.Format("Cannot summon, {0} not found!", args[0].Trim()));
            }
        }
    }

    internal class KillCommand : ICommand
    {
        public string Command
        {
            get { return "kill"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /kill <player name>");
            }
            else
            {
                foreach (var w in RealmManager.Worlds)
                {
                    //string death = string.Join(" ", args);
                    var world = w.Value;
                    if (w.Key != 0) // 0 is limbo??
                    {
                        foreach (var i in world.Players)
                        {
                            //Unnamed becomes a problem: skip them
                            if (i.Value.nName.ToLower() == args[0].ToLower().Trim() && i.Value.NameChosen)
                            {
                                if (args.Length > 1)
                                {
                                    i.Value.Death(string.Join(" ", args, 1, args.Length - 1));
                                }
                                else
                                {
                                    i.Value.Death("Server Admin");
                                }

                                return;
                            }
                        }
                    }
                }
                player.SendInfo(string.Format("Cannot kill, {0} not found!", args[0].Trim()));
            }
        }
    }

    internal class RestartCommand : ICommand
    {
        public string Command
        {
            get { return "restart"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                foreach (var w in RealmManager.Worlds)
                {
                    var world = w.Value;
                    if (w.Key != 0)
                    {
                        world.BroadcastPacket(new TextPacket
                        {
                            Name = "#Announcement",
                            Stars = -1,
                            BubbleTime = 0,
                            Text =
                                "Server restarting soon. Please be ready to disconnect. Estimated server down time: 30 Seconds - 1 Minute"
                        }, null);
                    }
                }
            }
            catch
            {
                player.SendError("Cannot say that in announcement!");
            }
        }
    }

    internal class VitalityCommand : ICommand
    {
        public string Command
        {
            get { return "vit"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /vit <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stats[5] = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class DefenseCommand : ICommand
    {
        public string Command
        {
            get { return "def"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /def <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stats[3] = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class AttackCommand : ICommand
    {
        public string Command
        {
            get { return "att"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /att <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stats[2] = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class DexterityCommand : ICommand
    {
        public string Command
        {
            get { return "dex"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /dex <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stats[7] = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class LifeCommand : ICommand
    {
        public string Command
        {
            get { return "hp"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /hp <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stats[0] = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class ManaCommand : ICommand
    {
        public string Command
        {
            get { return "mp"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /mp <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stats[1] = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class SpeedCommand : ICommand
    {
        public string Command
        {
            get { return "spd"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /spd <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stats[4] = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class WisdomCommand : ICommand
    {
        public string Command
        {
            get { return "wis"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /spd <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stats[6] = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class Whitelist : ICommand
    {
        public string Command
        {
            get { return "whitelist"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /whitelist <username>");
            }
            try
            {
                using (var dbx = new Database())
                {
                    var cmd = dbx.CreateQuery();
                    cmd.CommandText = "UPDATE accounts SET rank=1 WHERE name=@name";
                    cmd.Parameters.AddWithValue("@name", args[0]);
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        player.SendInfo("Could not whitelist!");
                    }
                    else
                    {
                        player.SendInfo("Account successfully whitelisted!");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Out.WriteLine(player.nName + " Has Whitelisted " + args[0]);
                        Console.ForegroundColor = ConsoleColor.White;

                        var dir = @"logs";
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        using (var writer = new StreamWriter(@"logs\WhitelistLog.log", true))
                        {
                            writer.WriteLine("[" + DateTime.Now + "]" + player.nName + " Has Whitelisted " + args[0]);
                        }
                    }
                    dbx.Dispose();
                }
            }
            catch
            {
                player.SendInfo("Could not whitelist!");
            }
        }
    }

    internal class Ban : ICommand
    {
        public string Command
        {
            get { return "ban"; }
        }

        public int RequiredRank
        {
            get { return 2; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /ban <username>");
            }
            try
            {
                using (var dbx = new Database())
                {
                    var cmd = dbx.CreateQuery();
                    cmd.CommandText = "UPDATE accounts SET banned=1, rank=0 WHERE name=@name";
                    cmd.Parameters.AddWithValue("@name", args[0]);
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        player.SendInfo("Could not ban");
                    }
                    else
                    {
                        foreach (var i in player.Owner.Players)
                        {
                            if (i.Value.nName.ToLower() == args[0].ToLower().Trim())
                            {
                                i.Value.Client.Disconnect();
                                player.SendInfo("Account successfully Banned");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Out.WriteLine(args[0] + " was Banned.");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                    }
                    dbx.Dispose();
                }
            }
            catch
            {
                player.SendInfo("Could not ban");
            }
        }
    }

    internal class UnBan : ICommand
    {
        public string Command
        {
            get { return "unban"; }
        }

        public int RequiredRank
        {
            get { return 2; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /unban <username>");
            }
            try
            {
                using (var dbx = new Database())
                {
                    var cmd = dbx.CreateQuery();
                    cmd.CommandText = "UPDATE accounts SET banned=0 WHERE name=@name";
                    cmd.Parameters.AddWithValue("@name", args[0]);
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        player.SendInfo("Could not unban");
                    }
                    else
                    {
                        player.SendInfo("Account successfully Unbanned");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Out.WriteLine(args[1] + " was Unbanned.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    dbx.Dispose();
                }
            }
            catch
            {
                player.SendInfo("Could not unban, please unban in database");
            }
        }
    }

    internal class Rank : ICommand
    {
        public string Command
        {
            get { return "rank"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length < 2)
            {
                player.SendHelp(
                    "Usage: /rank <username> <number>\n0: Player\n1: Donator\n2: Game Master\n3: Developer\n4: Head Developer\n5: Admin");
            }
            else
            {
                try
                {
                    using (var dbx = new Database())
                    {
                        var cmd = dbx.CreateQuery();
                        cmd.CommandText = "UPDATE accounts SET rank=@rank WHERE name=@name";
                        cmd.Parameters.AddWithValue("@rank", args[1]);
                        cmd.Parameters.AddWithValue("@name", args[0]);
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            player.SendInfo("Could not change rank");
                        }
                        else
                        {
                            player.SendInfo("Account rank successfully changed");
                        }
                        dbx.Dispose();
                    }
                }
                catch
                {
                    player.SendInfo("Could not change rank, please change rank in database");
                }
            }
        }
    }

    internal class GuildRank : ICommand
    {
        public string Command
        {
            get { return "grank"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length < 2)
            {
                player.SendHelp("Usage: /grank <username> <number>");
            }
            else
            {
                try
                {
                    using (var dbx = new Database())
                    {
                        var cmd = dbx.CreateQuery();
                        cmd.CommandText = "UPDATE accounts SET guildRank=@guildRank WHERE name=@name";
                        cmd.Parameters.AddWithValue("@guildRank", args[1]);
                        cmd.Parameters.AddWithValue("@name", args[0]);
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            player.SendInfo("Could not change guild rank. Use 10, 20, 30, 40, or 50 (invisible)");
                        }
                        else
                        {
                            player.SendInfo("Guild rank successfully changed");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Out.WriteLine(args[1] + "'s guild rank has been changed");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        dbx.Dispose();
                    }
                }
                catch
                {
                    player.SendInfo("Could not change rank, please change rank in database");
                }
            }
        }
    }

    internal class ChangeGuild : ICommand
    {
        public string Command
        {
            get { return "setguild"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length < 2)
            {
                player.SendHelp("Usage: /setguild <username> <guild id>");
            }
            else
            {
                try
                {
                    using (var dbx = new Database())
                    {
                        var cmd = dbx.CreateQuery();
                        cmd.CommandText = "UPDATE accounts SET guild=@guild WHERE name=@name";
                        cmd.Parameters.AddWithValue("@guild", args[1]);
                        cmd.Parameters.AddWithValue("@name", args[0]);
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            player.SendInfo("Could not change guild.");
                        }
                        else
                        {
                            player.SendInfo("Guild successfully changed");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Out.WriteLine(args[1] + "'s guild has been changed");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        dbx.Dispose();
                    }
                }
                catch
                {
                    player.SendInfo(
                        "Could not change guild, please change in database.                                Use /setguild <username> <guild id>");
                }
            }
        }
    }

    internal class TqCommand : ICommand
    {
        public string Command
        {
            get { return "tq"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (player.Quest == null)
            {
                player.SendError("Player does not have a quest!");
            }
            else
                player.Move(player.X + 0.5f, player.Y + 0.5f);
            //player.SetNewbiePeriod();
            player.UpdateCount++;
            player.Owner.BroadcastPacket(new GotoPacket
            {
                ObjectId = player.Id,
                Position = new Position
                {
                    X = player.Quest.X,
                    Y = player.Quest.Y
                }
            }, null);
            player.SendInfo("Success!");
        }
    }

    internal class GodMode : ICommand
    {
        public string Command
        {
            get { return "god"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (player.HasConditionEffect(ConditionEffects.Invincible))
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = 0
                });
                player.SendInfo("Godmode Off");
            }
            else
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = -1
                });
                player.SendInfo("Godmode On");
                foreach (var i in RealmManager.Clients.Values)
                    i.SendPacket(new NotificationPacket
                    {
                        Color = new ARGB(0xff00ff00),
                        ObjectId = player.Id,
                        Text = "Godmode Activated"
                    });
            }
        }
    }

    internal class GetIPCommand : ICommand
    {
        public string Command
        {
            get { return "ip"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            var plr = player.Owner.GetUniqueNamedPlayerRough(string.Join(" ", args));
            if (plr != null)
            {
                player.SendInfo(plr.Name + "'s IP: " + plr.Client.IP.Address);
                return;
            }
            foreach (var i in RealmManager.Worlds)
            {
                if (i.Key != 0 && i.Value.Id != player.Owner.Id)
                {
                    var p = i.Value.GetUniqueNamedPlayerRough(string.Join(" ", args));
                    if (p != null)
                    {
                        player.SendInfo(plr.Name + "'s IP: " + plr.Client.IP.Address);
                        return;
                    }
                }
            }
            player.SendError("Could not find player.");
        }
    }

    internal class BanIPCommand : ICommand
    {
        public string Command
        {
            get { return "ipban"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            var plr = player.Owner.GetUniqueNamedPlayerRough(string.Join(" ", args));
            if (plr != null)
            {
                player.SendInfo(plr.Name + "'s IP: " + plr.Client.IP.Address);
            }
        }
    }

    internal class VaultVisit : ICommand
    {
        public string Command
        {
            get { return "vault"; }
        }

        public int RequiredRank
        {
            get { return 2; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                foreach (var x in RealmManager.Worlds)
                {
                    foreach (var y in x.Value.Players)
                    {
                        if (y.Value.nName.ToLower() == args[0].ToLower().Trim())
                        {
                            var player2 = y.Value.Client;
                            var v = RealmManager.PlayerVault(player2);
                            var id = player2.Account.AccountId;
                            player.Client.Reconnect(new ReconnectPacket
                            {
                                Host = "",
                                Port = 2050,
                                GameId = v.Id,
                                Name = v.Name,
                                Key = Empty<byte>.Array,
                            });
                            return;
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }

    internal class VanishCommand : ICommand
    {
        public string Command
        {
            get { return "vanish"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length > 0)
            {
                player.SendHelp("Usage: /vanish");
            }
        }
    }

    internal class StarCommand : ICommand
    {
        public string Command
        {
            get { return "stars"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /stars <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Player.Stars = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class LevelCommand : ICommand
    {
        public string Command
        {
            get { return "level"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /level <amount>");
                }
                else if (args.Length == 1)
                {
                    player.Client.Character.Level = int.Parse(args[0]);
                    player.Client.Player.Level = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class NameCommand : ICommand
    {
        public string Command
        {
            get { return "name"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Use /name <name>");
            }
            else if (args.Length == 1)
            {
                using (var db = new Database())
                {
                    var db1 = db.CreateQuery();
                    db1.CommandText = "SELECT COUNT(name) FROM accounts WHERE name=@name;";
                    db1.Parameters.AddWithValue("@name", args[0]);
                    if ((int) (long) db1.ExecuteScalar() > 0)
                    {
                        player.SendError("Name Already In Use.");
                    }
                    else
                    {
                        db1 = db.CreateQuery();
                        db1.CommandText = "UPDATE accounts SET name=@name WHERE id=@accId";
                        db1.Parameters.AddWithValue("@name", args[0]);
                        db1.Parameters.AddWithValue("@accId", player.Client.Account.AccountId.ToString());
                        if (db1.ExecuteNonQuery() > 0)
                        {
                            player.Client.Player.Credits = db.UpdateCredit(player.Client.Account, -0);
                            player.Client.Player.Name = args[0];
                            player.Client.Player.NameChosen = true;
                            player.Client.Player.UpdateCount++;
                            player.SendInfo("Success!");
                        }
                        else
                        {
                            player.SendError("Internal Server Error Occurred.");
                        }
                    }
                    db1.Dispose();
                }
            }
        }
    }

    internal class RenameCommand : ICommand
    {
        public string Command
        {
            get { return "rename"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                player.SendHelp("Use /rename <Old Player Name> <New Player Name>");
            }
            else if (args.Length == 2)
            {
                using (var db = new Database())
                {
                    var db1 = db.CreateQuery();
                    db1.CommandText = "SELECT COUNT(name) FROM accounts WHERE name=@name;";
                    db1.Parameters.AddWithValue("@name", args[1]);
                    if ((int) (long) db1.ExecuteScalar() > 0)
                    {
                        player.SendError("Name Already In Use.");
                    }
                    else
                    {
                        db1 = db.CreateQuery();
                        db1.CommandText = "SELECT COUNT(name) FROM accounts WHERE name=@name";
                        db1.Parameters.AddWithValue("@name", args[0]);
                        if ((int) (long) db1.ExecuteScalar() < 1)
                        {
                            player.SendError("Name Not Found.");
                        }
                        else
                        {
                            db1 = db.CreateQuery();
                            db1.CommandText = "UPDATE accounts SET name=@newName, namechosen=TRUE WHERE name=@oldName;";
                            db1.Parameters.AddWithValue("@newName", args[1]);
                            db1.Parameters.AddWithValue("@oldName", args[0]);
                            if (db1.ExecuteNonQuery() > 0)
                            {
                                foreach (var playerX in RealmManager.Worlds)
                                {
                                    if (playerX.Key != 0)
                                    {
                                        var world = playerX.Value;
                                        foreach (var p in world.Players)
                                        {
                                            var Client = p.Value;
                                            if ((player.Name.ToLower() == args[0].ToLower()) && player.NameChosen)
                                            {
                                                player.Name = args[1];
                                                player.NameChosen = true;
                                                player.UpdateCount++;
                                                break;
                                            }
                                        }
                                    }
                                }
                                player.SendInfo("Success!");
                                //
                            }
                            else
                            {
                                player.SendError("Internal Server Error Occurred.");
                            }
                        }
                    }
                    db.Dispose();
                }
            }
        }
    }

    /*
    class giveEffCommand : ICommand
    {
        public string Command { get { return "giveeff"; } }
        public int RequiredRank { get { return 4; } }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /giveeff <Effectname or Effectnumber> <Playername without prefixes>");
            }
            else
            {
                try
                {
                    var n = "";
                    foreach (var i in RealmManager.Clients.Values)
                    {
                        if (i.Account.Name.ToUpper().StartsWith("[gm]"))
                        {
                            n = i.Account.Name.Substring(4);
                        }
                        if (i.Account.Name.ToUpper().StartsWith("[dev]"))
                        {
                            n = i.Account.Name.Substring(5);
                        }
                        if (i.Account.Name.ToUpper().StartsWith("[hdev]"))
                        {
                            n = i.Account.Name.Substring(6);
                        }
                        if (i.Account.Name.ToUpper().StartsWith("[admin]"))
                        {
                            n = i.Account.Name.Substring(7);
                        }

                        if (n.ToUpper() == args[0].ToUpper())
                        {
                            ConditionEffectIndex cond = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[1].Trim());
                            //check if the player already has the condition effect
                            i.Player.ApplyConditionEffect(new ConditionEffect()
                            {
                                Effect = cond,
                                DurationMS = -1
                            });
                            {
                                player.SendInfo("Success!");
                            }
                        }
                    }
                }
                catch
                {
                    player.SendError("Invalid effect!");
                }
            }
        }
    }
    */

    internal class messageCommand : ICommand
    {
        public string Command
        {
            get { return "message"; }
        }

        public int RequiredRank
        {
            get { return 5; }
        }

        public void Execute(Player player, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /message <title> <message>");
            }
            else
            {
                var title = string.Join(" ", args);
                var message = string.Join(" ", args.Skip(1).ToArray());
                foreach (var i in RealmManager.Clients.Values)
                    i.SendPacket(new TextBoxPacket
                    {
                        Title = args[0],
                        Message = message,
                        Button1 = "Ok",
                        Type = "GlobalMsg"
                    });
            }
        }
    }

    internal class FameCommand : ICommand
    {
        public string Command
        {
            get { return "fame"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /fame <amount>");
                }
                else
                {
                    using (var db = new Database())
                    {
                        player.CurrentFame = db.UpdateFame(player.Client.Account, int.Parse(args[0]));
                        player.UpdateCount++;
                        db.Dispose();
                    }
                }
            }
            catch
            {
                player.SendError("Error!");
            }
        }
    }

    internal class CloseRealmCommand : ICommand
    {
        public string Command
        {
            get { return "closerealm"; }
        }

        public int RequiredRank
        {
            get { return 3; }
        }

        public void Execute(Player player, string[] args)
        {
            if (player.Owner is GameWorld)
            {
                var gw = player.Owner as GameWorld;
                gw.Overseer.InitCloseRealm();
            }
        }
    }

    internal class LuciferTestingCommand : ICommand
    {
        public string Command
        {
            get { return "lucitest"; }
        }

        public int RequiredRank
        {
            get { return 9; }
        }

        public void Execute(Player player, string[] args)
        {
            player.Client.Reconnect(new ReconnectPacket
            {
                Host = "",
                Port = 2050,
                GameId = World.ADM_ID,
                Name = "Admin Room",
                Key = Empty<byte>.Array,
            });
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.logic.loot;
using terrain;

namespace wServer.realm.setpieces
{
    class TempLava6 : ISetPiece
    {
        public int Size { get { return 5; } }

        static readonly byte Lava = (byte)XmlDatas.IdToType["Lava Blend"];
        static readonly byte Floor = (byte)XmlDatas.IdToType["Red Quad"];

        Random rand = new Random();
        public void RenderSetPiece(World world, IntPoint pos)
        {
            int[,] t = new int[5, 5];


            t[0, 2] = 2;
            t[1, 2] = 1;
            t[2, 2] = 1;
            t[3, 2] = 1;
            t[4, 2] = 2;
            t[2, 0] = 2;
            t[2, 1] = 1;
            t[2, 3] = 1;
            t[2, 4] = 2;
            t[1, 1] = 2;
            t[1, 3] = 2;
            t[3, 3] = 2;
            t[3, 1] = 2;


            for (int x = 0; x < 5; x++)                    //Rendering
                for (int y = 0; y < 5; y++)
                {
                    if (t[x, y] == 1)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.TileId = Lava; tile.ObjType = 0;
                        if (world.Obstacles[x + pos.X, y + pos.Y] == 0)
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                    if (t[x, y] == 2)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.TileId = Floor; tile.ObjType = 0;
                        if (world.Obstacles[x + pos.X, y + pos.Y] == 0)
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                }
        }
    }
}
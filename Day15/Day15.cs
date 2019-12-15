using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day15
    {        
        static void Main(string[] args)
        {
            System.Console.WriteLine("Day 15");

            string filename = "input";
            IntCodeMachine icm;
            using (StreamReader sr = new StreamReader(filename))
            {
                int[] data = sr.ReadLine().Trim().Split(",").Select(s => Int32.Parse(s)).ToArray();
                icm = new IntCodeMachine("arcade", data);
            }
            
            // Part 1
            Room currentRoom = new Room(0, 0);
            icm.Reset();
            while (icm.active)
            {
                int dir = currentRoom.NextDirection();
                if (dir < 5)
                {
                    (int x, int y) = currentRoom.Neighbour(dir);
                    if (Room.GetRoom(x, y) == null)
                    {
                        int response;
                        icm.Next(dir, out response);
                        if (response == Room.WALL)
                        {
                            new Room(x, y, Room.opposite[dir], Room.WALL);
                        }
                        else if (response == Room.EMPTY)
                        {
                            Room room = new Room(x, y, Room.opposite[dir], Room.EMPTY);
                            currentRoom = room;
                        }
                        else if (response == Room.SYSTEM)
                        {
                            Room room = new Room(x, y, Room.opposite[dir], Room.SYSTEM);
                            currentRoom = room;
                        }
                        else
                        {
                            throw new Exception($"Unknown response: {response}");
                        }
                    }
                }
                else
                {
                    // All option have been explored: backtrack to previous room
                    dir = currentRoom.PreviousDir;
                    if (dir > 0)
                    {
                        int response;
                        icm.Next(dir, out response);
                        if (response == Room.EMPTY || response == Room.SYSTEM)
                        {
                            (int x, int y) = currentRoom.Neighbour(dir);
                            currentRoom = Room.GetRoom(x, y);
                        }
                        else
                        {
                            throw new Exception($"Unknown or unexpected response for previous room: {response}");
                        }
                    }
                    else
                    {
                        // No more options
                        // TODO: pathfinding
                        System.Console.WriteLine("Found all rooms?");

                        // Draw the maze
                        int xMin = Int32.MaxValue;
                        int xMax = Int32.MinValue;
                        int yMin = Int32.MaxValue;
                        int yMax = Int32.MinValue;
                        foreach ((int x, int y) p in Room.AllRooms.Keys)
                        {
                            if (p.x < xMin) xMin = p.x;
                            if (p.x > xMax) xMax = p.x;
                            if (p.y < yMin) yMin = p.y;
                            if (p.y > yMax) yMax = p.y;
                        }
                        System.Console.WriteLine($"Canvas: ({xMin},{yMin}) to ({xMax},{yMax}).");
                        for (int y = yMin; y <= yMax; y++)
                        {
                            string line = "";
                            for (int x = xMin; x <= xMax; x++)
                            {
                                if (Room.AllRooms.ContainsKey((x,y)))
                                    line += Room.AllRooms[(x,y)].GetImage();
                                else
                                    line += "@";
                            }
                            System.Console.WriteLine(line);
                        }

                        currentRoom = Room.OxygenSystem;
                        int distance = 0;
                        while (currentRoom.PreviousDir != 0)
                        {
                            distance++;
                            currentRoom = Room.GetRoom(currentRoom.PreviousRoom());
                        }
                        System.Console.WriteLine($"1. {distance}");

                        bool changed = true;
                        while (changed)
                        {
                            changed = false;
                            foreach(Room room in Room.AllRooms.Values)
                            {
                                changed = room.UpdateDistanceToSystem() || changed;
                            }
                        }
                        int time = Room.AllRooms.Values.Where(x => !x.IsWall).Select(x => x.DistanceToSystem).Max();
                        System.Console.WriteLine($"2. {time}");
                        break;
                    }
                }
            }
        }
    }

    class Room
    {
        public const int WALL = 0;
        public const int EMPTY = 1;
        public const int SYSTEM = 2;

        // Collection of all rooms found so far
        public static Dictionary<(int x, int y), Room> AllRooms = new Dictionary<(int x, int y), Room>();

        // Remember the room with the ogygen system (should be 1?)
        public static Room OxygenSystem { get; private set; }

        // Movement commands are: north (1), south (2), west (3), and east (4); hence the leading 0.
        static int[] DX = new int[] { 0, 0, 0, 1, -1 };
        static int[] DY = new int[] { 0, 1, -1, 0, 0 };

        // Index / command for moving in the opposite direction, again 1-based indexed.
        public static int[] opposite = new int[] { 0, 2, 1, 4, 3 };
        
        int searchPointer = 1;          // Direction to search
        public int PreviousDir { get; } // Direction of the previous room
        private int type;               // Type: 0 = wall, 1 = empty, 2 = system
        public int X { get; }
        public int Y { get; }
        public int DistanceToSystem = Int32.MaxValue - 1;

        public bool HasSystem { get => type == SYSTEM; }
        public bool IsWall { get => type == WALL; }

        public Room (int x, int y, int prev = 0, int type = EMPTY)
        {
            AllRooms.Add((x,y), this);
            this.PreviousDir = prev;
            this.type = type;
            this.X = x;
            this.Y = y;
            // System.Console.WriteLine($"New room[{AllRooms.Count}]: ({X}, {Y}), type: {type}, previous dir: {prev}.");
            if (type == SYSTEM)
            {
                OxygenSystem = this;
                DistanceToSystem = 0;
                System.Console.WriteLine($"Found SYSTEM at ({X}, {Y}).");
            }
        }

        public string GetImage()
        {
            if (type == WALL) return "\u2587";
            if (type == EMPTY) return (X == 0 && Y == 0) ? "S" : " ";
            if (type == SYSTEM) return "O";
            return "?";
        }

        public bool UpdateDistanceToSystem ()
        {
            bool changed = false;
            if (type == EMPTY)
            {
                for (int dir = 1; dir < 5; dir++)
                {
                    int dist = GetRoom(Neighbour(dir))?.DistanceToSystem ?? Int32.MaxValue;
                    if (dist + 1 < this.DistanceToSystem)
                    {
                        changed = true;
                        this.DistanceToSystem = dist + 1;
                    }
                }
            }
            return changed;
        }

        public (int, int) Neighbour (int dir) => (X + DX[dir], Y + DY[dir]);

        public (int, int) PreviousRoom () => Neighbour(PreviousDir);

        public int NextDirection () => searchPointer++;

        public static Room GetRoom (int x, int y)
        {
            if (AllRooms.ContainsKey((x,y)))
                return AllRooms[(x, y)];
            else
                return null;
        }

        public static Room GetRoom ((int x, int y) dir) => GetRoom(dir.x, dir.y);
    }
}
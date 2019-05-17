﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Smod2.API;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;
using System.IO;

namespace RogerFKspawner
{
	[PluginDetails(
		author = "RogerFK",
		name = "Spawner de items",
		description = "who reads this lmao haha tbhfam",
		id = "rogerfk.spawner",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 0
		)]
	public class Plugin : Smod2.Plugin
	{
		public override void OnDisable()
		{
			this.Info("Thank god you disabled me. Your CPU will surely thank you tbh");
		}

		public override void OnEnable()
		{
			this.Info("Stuff spawner enabled");
		}

		public override void Register()
		{
			this.AddEventHandlers(new SpawnParser(this), Priority.Low);
			Spawner.Init(this);
			AddCommand("coinfetcher", new Spawner());
		}
	}
	public class SpawnParser : IEventHandlerWaitingForPlayers
	{
		private readonly Plugin plugin;
		private System.Random rand = new System.Random();

		public SpawnParser(Plugin plugin)
		{
			this.plugin = plugin;
		}
		private Vector VectorParser(string vectorData, int i)
		{
			string[] vector = vectorData.Split(',');
			if (vector.Length != 3)
			{
				plugin.Info("Bad format for a vector (" + vectorData + ") in line " + i);
				return null;
			}
			if (!float.TryParse(vector[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) || !float.TryParse(vector[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) || !float.TryParse(vector[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
			{
				plugin.Info("Error parsing vector: (" + vectorData + ") in line " + i);
				return null;
			}
			return new Vector(x, y, z);
		}
		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (!FileManager.FileExists("./items.txt"))
			{
				plugin.Info("Created items.txt file with a microhid (or a coin) in the Intercom room and one at the Silo warhead as an example.");
				File.WriteAllText("./", "NUKE:MICROHID:100:-0.05,402.46,3.52:1,0,0\nINTERCOM:MICROHID,COIN:100:-9.212725,-6.839905,-3.935197:0.5,0,0");
			}
			List<SpawnInfo> spawnlist = new List<SpawnInfo>();
			string[] items = FileManager.ReadAllLines("./items.txt").Where(x => string.IsNullOrWhiteSpace(x) == false).Where(x => x[0] != '#').ToArray();
			if (items.Length < 0)
			{
				plugin.Error("Couldn't get any item from that file, homie");
				return;
			}
			else
			{
				int i = 0;
				foreach (string item in items)
				{
					i++;
					//if (string.IsNullOrWhiteSpace(item)) continue;
					try
					{
						// RoomType:ItemType, ItemType2...:Probability:Vector:Rotation
						string[] data = item.Split(':');
						if (data.Length == 0)
						{
							continue;
						}
						if (data.Length != 5)
						{
							plugin.Info("Error reading line " + i);
							continue;
						}
						if (!Enum.TryParse<RoomType>(data[0], out RoomType room))
						{
							plugin.Info("Error using RoomType " + data[0] + " in line " + i);
							continue;
						}
						string[] itemData = data[1].Split(',');
						List<ItemType> itemTypes = new List<ItemType>();
						if (itemData.Length == 0)
						{
							plugin.Info("Error fetching ItemTypes " + data[1] + " in line " + i);
							continue;
						}
						foreach (string itemDataValue in itemData)
						{
							if (!Enum.TryParse<ItemType>(itemDataValue, out ItemType itemType))
							{
								plugin.Info("Error using ItemType " + itemDataValue + " in line " + i);
								continue;
							}
							itemTypes.Add(itemType);
						}
						if (!float.TryParse(data[2], out float probability))
						{
							plugin.Info("Error using probability " + data[2] + " in line " + i);
							continue;
						}
						Vector position = VectorParser(data[3], i);
						if (position == null)
						{
							continue;
						}

						Vector rotation = VectorParser(data[4], i);
						if (rotation == null)
						{
							continue;
						}

						// If it worked until here means everything went to plan uwu
						spawnlist.Add(new SpawnInfo(room, itemTypes.ToArray(), probability, position, rotation));
					}
					catch (Exception e)
					{
						plugin.Info("Somewhere it fucked up: " + e.ToString());
					}
				}
			}
			if (spawnlist.Count != 0)
			{
				foreach (Room room in Spawner.rooms)
				{
					foreach (SpawnInfo spawn in spawnlist.Where(x => x.RoomType == room.RoomType))
					{
						if (rand.Next(0, 10000) <= spawn.probability * 100)
						{
							Spawner.AddItem(room, spawn.itemType[rand.Next(0, spawn.itemType.Length - 1)], spawn.position, spawn.rotation);
						}
						spawnlist.Remove(spawn); // If this ever gives an exception, I'm fucking done
					}
				}
			}
		}
	}
	public struct SpawnInfo
	{
		public readonly RoomType RoomType;
		public readonly ItemType[] itemType;
		public readonly float probability;
		public readonly Vector position;
		public readonly Vector rotation;

		public SpawnInfo(RoomType roomType, ItemType[] itemType, float probability, Vector position, Vector rotation)
		{
			RoomType = roomType;
			this.itemType = itemType;
			this.probability = probability;
			this.position = position;
			this.rotation = rotation;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
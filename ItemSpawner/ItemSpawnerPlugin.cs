﻿using System.Collections.Generic;
using EXILED;
using UnityEngine;

namespace ItemSpawner
{
	public class ItemSpawnerPlugin : Plugin
	{
		public static ItemSpawnerPlugin Instance { private set; get; };

		public override string getName => "ItemSpawner";

		public override void OnDisable()
		{
			Info("Thank god you disabled me. Your CPU will surely thank you tbh");
		}

		public override void OnEnable()
		{
			Info("Stuff spawner enabled.");
		}

		public string[] allowedranks = new string[] { "owner", "admin" };

		public bool enable = true;

		public bool verbose = true;

		public bool useGlobalItems = true;

		public void Register()
		{
			instance = this;
			AddEventHandlers(new ItemsFileManager(this), Priority.Low);
			AddEventHandlers(new ItemSpawnerCommand(this), Priority.Low);
			Spawner.Init(this);
			AddCommands(new string[] { "itemspawner", "is", "items", "its" }, new ItemSpawnerCommand(this));
		}

		public override void OnReload()
		{
			// this should be used by the plugin, btw, so it saves everything into a file *just in case*
		}
	}
	public struct SpawnInfo
	{
		public readonly RoomType RoomType;
		public readonly int line; // This saves the line to later modify it

		public ItemType[] items;
		public int[] CustomItems;
		public float probability;
		public Vector3 position;
		public Vector3 rotation;

		public SpawnInfo(RoomType roomType, int line, ItemType[] itemType, int[] CustomItems, float probability, Vector3 position, Vector3 rotation)
		{
			RoomType = roomType;
			items = itemType;
			this.CustomItems = CustomItems;
			this.probability = probability;
			this.line = line;
			this.position = position;
			this.rotation = rotation;
		}
	}
	public struct PosVector3Pair
	{
		public readonly Vector3 position;
		public readonly Vector3 rotation;
		public PosVector3Pair(Vector3 position, Vector3 rotation)
		{
			this.position = position;
			this.rotation = rotation;
		}
	}
}

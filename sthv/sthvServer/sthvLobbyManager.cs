﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;
using CitizenFX;



namespace sthvServer
{

	class sthvLobbyManager : BaseScript
	{
		/// <summary>
		/// reset and used in server.cs to check if joining player had died before so people cant rejoin and get a second life.
		/// </summary>
		public static List<Player> DeadPlayers = new List<Player>();
		//public PlayerList PlayersHunters { get; set; }
		//public PlayerList PlayersRunners { get; set; }
		Dictionary<string, bool> PlayerPing = new Dictionary<string, bool >();
		List<Player> AlivePlayers = new List<Player>();
		public sthvLobbyManager()
		{
			EventHandlers["sthv:playerJustAlive"] += new Action<Player>(SyncJustAlive);
			EventHandlers["sthv:playerJustDead"] += new Action<Player>(SyncJustDead);
			EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
			EventHandlers["playerConnecting"] += new Action(() =>
			{ 
			});


			API.RegisterCommand("testaliveplayers", new Action<int, List<object>, string>((src, args, raw) =>
			{
				foreach(Player p in Players)
				{
					if(AlivePlayers.Contains(p)){

						Debug.WriteLine($"player {p.Name} is alive, ping is {p.Ping}");
					}
					else
					{
						Debug.WriteLine($"player {p.Name} is dead, ping is {p.Ping}");
					}
				}
			}), false);

			EventHandlers["test:logIdentifiers"] += new Action<Player>(logIdentifiers);
			
		}
		void logIdentifiers([FromSource]Player player)
		{
			Debug.WriteLine(player.Identifiers["discord"]);
		}
		void OnPlayerDropped([FromSource]Player source, string reason)
		{
			if (source != null)
			{
				string _leftHandle = source.Name;
				if (AlivePlayers.Contains(source))
				{
					AlivePlayers.Remove(source);
				}
				else
				{
					Debug.WriteLine($"player {source.Name} not in alive list anyways :(");
				}
				if (server.hasHuntStarted && _leftHandle == server.runner.Handle)
				{
					Debug.WriteLine("^1Runner left :( ^7");
					server.isHuntOver = true;
				}
			}
			else
			{
				Debug.WriteLine("source returned null onplayerdropped");
			}
			Debug.WriteLine($"dropped {source.Name}");
			CheckAlivePlayers();
			TriggerClientEvent("sthv:refreshsb");
		}
		void SyncJustAlive([FromSource] Player source)
		{
			TriggerClientEvent("sthv:updateAlive", source.Handle, true);
			Debug.WriteLine($"^4player {source.Name} just alive^7");

			if (AlivePlayers.Contains(source))//declares val inline 
			{
				Debug.WriteLine($"player {source.Name} was already in alive list");
			}
			else
			{
				AlivePlayers.Add(source);
			}
			CheckAlivePlayers();

		}

		void SyncJustDead([FromSource] Player source)
		{
			TriggerClientEvent("sthv:updateAlive", source.Handle, false);
			Debug.WriteLine($"^4player {source.Name} just dead^7");
			DeadPlayers.Add(source);
			if(AlivePlayers.Contains(source))
			{
				AlivePlayers.Remove(source);
			}
			else
			{
				Debug.WriteLine($"player {source.Name} wasnt in alivelist :(");
			}
			CheckAlivePlayers();
		}
		private void CheckAlivePlayers()
		{
			Debug.WriteLine($"{AlivePlayers.Count} alive players remaining");

			if(AlivePlayers.Count < 2 && server.hasHuntStarted)
			{
				server.isHuntOver = true;
				server.SendChatMessage("^4Hunt", "All hunters dead, hunt over.");
			}

		}
	}
}





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
	class sthvPlayer : BaseScript
	{
		
	}
	class sthvLobbyManager : BaseScript
	{
		public PlayerList PlayersDead { get; set; }
		public PlayerList PlayersHunters { get; set; }
		public PlayerList PlayersRunners { get; set; }
		Dictionary<string, bool> PlayerPing = new Dictionary<string, bool >();
		Dictionary<string, bool> AlivePlayers = new Dictionary<string, bool>();

		public sthvLobbyManager()
		{
			EventHandlers["sthv:playerJustAlive"] += new Action<Player>(SyncJustAlive);
			EventHandlers["sthv:playerJustDead"] += new Action<Player>(SyncJustDead);
			EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
			EventHandlers["playerConnecting"] += new Action( async() =>
			{ 
			});

			API.RegisterCommand("testaliveplayers", new Action<int, List<object>, string>((src, args, raw) =>
			{
				foreach(Player p in Players)
				{
					if(AlivePlayers.TryGetValue(p.Handle, out bool val) && val){

						Debug.WriteLine($"player {p.Name} is alive, ping is {p.Ping}");
						
					}
					else
					{
						Debug.WriteLine($"player {p.Name} is dead, ping is {p.Ping}");
					}
				}
			}), false);


		}
		void OnPlayerDropped([FromSource]Player source, string reason)
		{
			if (source != null)
			{
				Debug.WriteLine("1");

				string _leftHandle = source.Name;
				Debug.WriteLine("1");
				if (AlivePlayers.ContainsKey(_leftHandle))
				{
					AlivePlayers.Remove(_leftHandle);
				}
				Debug.WriteLine("1");
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

			if (AlivePlayers.TryGetValue(source.Handle, out bool val)) //declares val inline 
			{
				if (val != true)
				{
					AlivePlayers[source.Handle] = true;
				}
			}
			else
			{
				AlivePlayers.Add(source.Handle, true);
			}
			CheckAlivePlayers();

		}

		void SyncJustDead([FromSource] Player source)
		{
			TriggerClientEvent("sthv:updateAlive", source.Handle, false);
			Debug.WriteLine($"^4player {source.Name} just dead^7");

			if(AlivePlayers.TryGetValue(source.Handle, out bool val)){
				if(val != false)
				{
					AlivePlayers[source.Handle] = false;
				}
			}
			else
			{
				AlivePlayers.Add(source.Handle, false);
			}
			CheckAlivePlayers();

		}
		private void CheckAlivePlayers()
		{
			int numberOfAlivePlayers = 0;
			foreach(var i in AlivePlayers)
			{
				if( i.Value == true)
				{
					numberOfAlivePlayers++;
				}
			}
			Debug.WriteLine($"{numberOfAlivePlayers} alive players remaining, {AlivePlayers.Count()} total");
			if(numberOfAlivePlayers < 2 && server.hasHuntStarted)
			{
				server.isHuntOver = true;
				server.SendChatMessage("^4Hunt", "All hunters dead, hunt over.");
			}

		}
	}
}





﻿using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;

namespace sthvServer
{
	public static class sthvPlayerExtensionMethods
	{
		public static string getDiscordId(this Player player)
		{
			return player.Identifiers["discord"];
		}
		public static string getLicense(this Player player)
		{
			return player.Identifiers["license"]; 
		}
	}
}

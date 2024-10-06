﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zork
{
    [CommandClassAttributes]
    public static class RestartCommand
    {
        [Command("RESTART", "RESTART")]
        public static void Restart(Game game, CommandContext commandContext)
        {
            if (game.ConfirmAction("Are you sure you want to restart? "))
            {
                game.Restart();
            }
        }
    }
}

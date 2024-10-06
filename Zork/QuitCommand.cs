using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zork
{
    [CommandClassAttributes]
    public static class QuitCommand
    {
        [Command("QUIT", new string[] {"QUIT", "Q", "GOODBYE", "BYE"})]
        public static void Quit(Game game, CommandContext commandContext)
        {
            if (game.ConfirmAction("Are your sure you want to quit? "))
            {
                game.Quit();
            }
        }
    }
}

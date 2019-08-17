using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace HungerGamesTelegram
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("Hunger Games");

            var telegramGameHost = new TelegramGameHost();
            telegramGameHost.Start();

            Thread.Sleep(1000_000);

            // Game game = new Game();
            // await game.StartGame(new ConsolePlayer());
            
        }
        
    }    
    
}

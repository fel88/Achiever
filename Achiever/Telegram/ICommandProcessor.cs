using System.Threading.Tasks;

namespace Achiever.Telegram
{
    public interface ICommandProcessor
    {
        Task<bool> Process(string message);
    }
}
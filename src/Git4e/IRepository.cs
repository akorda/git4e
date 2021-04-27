using System.Threading.Tasks;

namespace Git4e
{
    public interface IRepository
    {
        Task<Commit> CheckoutAsync(byte[] commitHash);
    }
}

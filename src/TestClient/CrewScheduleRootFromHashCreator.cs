using CrewSchedule;
using Git4e;

namespace TestClient
{
    public class CrewScheduleRootFromHashCreator : IRootFromHashCreator
    {
        public LazyHashableObjectBase CreateRootFromHash(IRepository repository, string rootHash, string rootContentType)
        {
            if (rootHash.IndexOf("|") != -1)
                return new LazyPlan(repository, rootHash);
            else
                return new LazyHashableObject(repository, rootHash, rootContentType);
        }
    }
}

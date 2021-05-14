using System.Threading.Tasks;
using System.Threading;
using ProtoBuf;

namespace Git4e.Tests
{
    public class TestRootData : HashableObject
    {
        public const string TestRootDataContentType = "TestRootData";

        [ProtoContract]
        public class TestRootDataContent : IContent
        {
            [ProtoMember(1)]
            public string Id { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var data = new TestRootData(repository, hash)
                {
                    Id = this.Id,
                    Name = this.Name
                };
                return Task.FromResult(data as IHashableObject);
            }
        }

        string _Id;
        public string Id
        {
            get => _Id;
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    this.MarkAsDirty();
                }
            }
        }
        string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    this.MarkAsDirty();
                }
            }
        }

        public TestRootData()
            : this(null)
        {
        }

        public TestRootData(IRepository repository, string hash = null)
            : base(repository, TestRootDataContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var content = new TestRootDataContent
            {
                Id = this.Id,
                Name = this.Name
            };
            return content;
        }
    }
}

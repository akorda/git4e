using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class Vessel : HashableObject
    {
        public const string VesselContentType = "Vessel";

        [ProtoContract]
        public class VesselContent : IContent
        {
            [ProtoMember(1)]
            public string VesselCode { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public string[] PositionHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var positions = this.PositionHashes
                    .Select(posHash => new LazyHashableObject<VesselPosition>(posHash, VesselPosition.VesselPositionContentType))
                    .ToArray();
                var vessel = new Vessel(hash)
                {
                    VesselCode = this.VesselCode,
                    Name = this.Name,
                    Positions = positions
                };
                return Task.FromResult(vessel as IHashableObject);
            }
        }

        string _VesselCode;
        public string VesselCode
        {
            get => _VesselCode;
            set
            {
                if (_VesselCode != value)
                {
                    _VesselCode = value;
                    this.MarkContentAsDirty();
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
                    this.MarkContentAsDirty();
                }
            }
        }

        IEnumerable<LazyHashableObject<VesselPosition>> _Positions;
        public IEnumerable<LazyHashableObject<VesselPosition>> Positions
        {
            get => _Positions;
            set
            {
                if (_Positions != value)
                {
                    _Positions = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        public Vessel(string hash = null)
            : base(VesselContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var positionHashes = this.Positions?
                //.OrderBy(pos => pos.DutyRankCode)
                //.ThenBy(pos => pos.PositionNo)
                .OrderBy(pos => pos.Hash)
                .Select(pos => pos.Hash)
                .ToArray();
            var content = new VesselContent
            {
                VesselCode = this.VesselCode,
                Name = this.Name,
                PositionHashes = positionHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var positions = this.Positions ?? new LazyHashableObject<VesselPosition>[0];
            foreach (var position in positions)
            {
                yield return await Task.FromResult(position);
            }
        }
    }
}

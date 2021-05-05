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
            public string[] PositionFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var positions =
                    this.PositionFullHashes
                    .Select(posHash => new LazyVesselPosition(posHash))
                    .ToList();
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

        List<LazyVesselPosition> _Positions;
        public List<LazyVesselPosition> Positions
        {
            get => _Positions;
            set
            {
                if (_Positions != value)
                {
                    _Positions = value;
                    this.MarkAsDirty();
                }
            }
        }

        public Vessel(string hash = null)
            : base(VesselContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var positionFullHashes = this.Positions?
                .OrderBy(pos => pos.HashIncludeProperty1)
                .ThenBy(pos => pos.HashIncludeProperty2)
                .Select(pos => pos.FullHash)
                .ToArray();
            var content = new VesselContent
            {
                VesselCode = this.VesselCode,
                Name = this.Name,
                PositionFullHashes = positionFullHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var positions = this.Positions ?? new List<LazyVesselPosition>();
            foreach (var position in positions)
            {
                yield return await Task.FromResult(position);
            }
        }
    }

    /// <summary>
    /// Vessel Hash with the following included properties:
    /// 1. VesselCode
    /// 2. Vessel Name
    /// </summary>
    public class LazyVessel : LazyHashableObject<string, string>
    {
        public LazyVessel(string fullHash)
            : base(fullHash, Vessel.VesselContentType)
        {
        }

        public LazyVessel(Vessel vessel)
            : base(vessel, v => (v as Vessel).VesselCode, v => (v as Vessel).Name)
        {
        }
    }
}
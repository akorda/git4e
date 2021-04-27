using System.Collections.Generic;
using System.IO;
using System.Linq;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class Vessel : HashableObject
    {
        [ProtoContract]
        public class VesselContent : IContent
        {
            [ProtoMember(1)]
            public string VesselCode { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public Hash[] PositionHashes { get; set; }

            public IHashableObject ToHashableObject(IContentSerializer contentSerializer, IObjectLoader objectLoader, IHashCalculator hashCalculator)
            {
                var positions =
                    this.PositionHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHash(hash).Result)
                    .Cast<VesselPosition>()
                    .ToList();
                return new Vessel(contentSerializer, hashCalculator)
                {
                    VesselCode = this.VesselCode,
                    Name = this.Name,
                    Positions = positions
                };
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

        IEnumerable<VesselPosition> _Positions;
        public IEnumerable<VesselPosition> Positions
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

        public Vessel(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base("Vessel", contentSerializer, hashCalculator)
        {
        }

        public override void SerializeContent(Stream stream)
        {
            var positionHashes = this.Positions?
                .OrderBy(pos => pos.DutyRankCode)
                .ThenBy(pos => pos.PositionNo)
                .Select(pos => pos.Hash)
                .ToArray();
            var content = new VesselContent
            {
                VesselCode = this.VesselCode,
                Name = this.Name,
                PositionHashes = positionHashes
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }

        public override IEnumerable<IHashableObject> ChildObjects
        {
            get
            {
                if (this.Positions != null)
                    return this.Positions;
                return new IHashableObject[0];
            }
        }
    }
}

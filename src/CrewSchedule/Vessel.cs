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
            public byte[][] PositionHashes { get; set; }

            public object ToObject(IContentSerializer contentSerializer, IObjectLoader objectLoader)
            {
                var positions =
                    this.PositionHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHash(hash).Result)
                    .Cast<VesselPosition>()
                    .ToList();
                return new Vessel(contentSerializer)
                {
                    VesselCode = this.VesselCode,
                    Name = this.Name,
                    Positions = positions
                };
            }
        }

        public string VesselCode { get; set; }
        public string Name { get; set; }
        public List<VesselPosition> Positions { get; set; } = new List<VesselPosition>();

        public Vessel(IContentSerializer contentSerializer)
            : base("Vessel", contentSerializer)
        {
        }

        public override void SerializeContent(Stream stream, IHashCalculator hashCalculator)
        {
            var positionHashes = this.Positions
                .OrderBy(pos => pos.DutyRankCode)
                .ThenBy(pos => pos.PositionNo)
                .Select(pos => pos.ComputeHash(hashCalculator))
                .ToArray();
            var content = new VesselContent
            {
                VesselCode = this.VesselCode,
                Name = this.Name,
                PositionHashes = positionHashes
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }
    }
}

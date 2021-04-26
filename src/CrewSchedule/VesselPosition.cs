using System.Collections.Generic;
using System.IO;
using System.Linq;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class VesselPosition : HashableObject
    {
        [ProtoContract]
        public class VesselPositionContent : IContent
        {
            [ProtoMember(1)]
            public string VesselPositionId { get; set; }

            [ProtoMember(2)]
            public string VesselCode { get; set; }

            [ProtoMember(3)]
            public string DutyRankCode { get; set; }

            [ProtoMember(4)]
            public int PositionNo { get; set; }

            [ProtoMember(5)]
            public byte[][] SeamanAssignmentHashes { get; set; }

            public object ToObject(IContentSerializer contentSerializer, IObjectLoader objectLoader)
            {
                var seamanAssignments =
                    this.SeamanAssignmentHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHash(hash).Result)
                    .Cast<SeamanAssignment>()
                    .ToList();
                return new VesselPosition(contentSerializer)
                {
                    VesselPositionId = this.VesselPositionId,
                    VesselCode = this.VesselCode,
                    DutyRankCode = this.DutyRankCode,
                    PositionNo = this.PositionNo,
                    SeamanAssignments = seamanAssignments
                };
            }
        }

        public string VesselPositionId { get; set; }
        public string VesselCode { get; set; }
        public string DutyRankCode { get; set; }
        public int PositionNo { get; set; }
        public List<SeamanAssignment> SeamanAssignments { get; set; } = new List<SeamanAssignment>();

        public VesselPosition(IContentSerializer contentSerializer)
            : base("VesselPosition", contentSerializer)
        {
        }

        public override void SerializeContent(Stream stream, IHashCalculator hashCalculator)
        {
            var seamanAssignmentHashes = this
                .SeamanAssignments
                .OrderBy(asn => asn.StartOverlap)
                .Select(asn => asn.ComputeHash(hashCalculator))
                .ToArray();
            var content = new VesselPositionContent
            {
                VesselPositionId = this.VesselPositionId,
                VesselCode = this.VesselCode,
                DutyRankCode = this.DutyRankCode,
                PositionNo = this.PositionNo,
                SeamanAssignmentHashes = seamanAssignmentHashes
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }
    }
}

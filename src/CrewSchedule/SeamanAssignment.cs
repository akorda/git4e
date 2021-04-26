using System.IO;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class SeamanAssignment : HashableObject
    {
        [ProtoContract]
        public class SeamanAssignmentContent : IContent
        {
            [ProtoMember(1)]
            public string SeamanAssignmentId { get; set; }
            [ProtoMember(2)]
            public int StartOverlap { get; set; }
            [ProtoMember(3)]
            public int? StartDuties { get; set; }
            [ProtoMember(4)]
            public int? EndDuties { get; set; }
            [ProtoMember(5)]
            public int EndOverlap { get; set; }
            [ProtoMember(6)]
            public byte[] SeamanHash { get; set; }

            public IHashableObject ToHashableObject(IContentSerializer contentSerializer, IObjectLoader objectLoader, IHashCalculator hashCalculator)
            {
                var seaman = objectLoader.GetObjectByHash(this.SeamanHash).Result as Seaman;
                return new SeamanAssignment(contentSerializer, hashCalculator)
                {
                    SeamanAssignmentId = this.SeamanAssignmentId,
                    StartOverlap = this.StartOverlap,
                    StartDuties = this.StartDuties,
                    EndDuties = this.EndDuties,
                    EndOverlap = this.EndOverlap,
                    Seaman = seaman
                };
            }
        }

        public string SeamanAssignmentId { get; set; }
        public int StartOverlap { get; set; }
        public int? StartDuties { get; set; }
        public int? EndDuties { get; set; }
        public int EndOverlap { get; set; }

        //do not serialize
        public string SeamanCode { get; set; }
        public Seaman Seaman { get; set; }

        //do not serialize
        public string VesselCode { get; set; }
        public string DutyRankCode { get; set; }
        public int PositionNo { get; set; }

        public SeamanAssignment(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base("SeamanAssignment", contentSerializer, hashCalculator)
        {
        }

        public override void SerializeContent(Stream stream)
        {
            var content = new SeamanAssignmentContent
            {
                SeamanAssignmentId = this.SeamanAssignmentId,
                StartOverlap = this.StartOverlap,
                StartDuties = this.StartDuties,
                EndDuties = this.EndDuties,
                EndOverlap = this.EndOverlap,
                SeamanHash = this.Seaman?.Hash
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }
    }
}

﻿using System.IO;
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

            public object ToObject(IContentSerializer contentSerializer, IObjectLoader objectLoader)
            {
                var seaman = objectLoader.GetObjectByHash(this.SeamanHash).Result as Seaman;
                return new SeamanAssignment(contentSerializer)
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

        public SeamanAssignment(IContentSerializer contentSerializer)
            : base("SeamanAssignment", contentSerializer)
        {
        }

        public override void SerializeContent(Stream stream, IHashCalculator hashCalculator)
        {
            var content = new SeamanAssignmentContent
            {
                SeamanAssignmentId = this.SeamanAssignmentId,
                StartOverlap = this.StartOverlap,
                StartDuties = this.StartDuties,
                EndDuties = this.EndDuties,
                EndOverlap = this.EndOverlap,
                SeamanHash = this.Seaman?.ComputeHash(hashCalculator)
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }
    }
}

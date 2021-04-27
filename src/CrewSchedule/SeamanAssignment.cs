using System.Collections.Generic;
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
            public Hash SeamanHash { get; set; }

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

        string _SeamanAssignmentId;
        public string SeamanAssignmentId
        {
            get => _SeamanAssignmentId;
            set
            {
                if (_SeamanAssignmentId != value)
                {
                    _SeamanAssignmentId = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        int _StartOverlap;
        public int StartOverlap
        {
            get => _StartOverlap;
            set
            {
                if (_StartOverlap != value)
                {
                    _StartOverlap = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        int? _StartDuties;
        public int? StartDuties
        {
            get => _StartDuties;
            set
            {
                if (_StartDuties != value)
                {
                    _StartDuties = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        int? _EndDuties;
        public int? EndDuties
        {
            get => _EndDuties;
            set
            {
                if (_EndDuties != value)
                {
                    _EndDuties = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        int _EndOverlap;
        public int EndOverlap
        {
            get => _EndOverlap;
            set
            {
                if (_EndOverlap != value)
                {
                    _EndOverlap = value;
                    this.MarkContentAsDirty();
                }
            }
        }

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

        public override IEnumerable<IHashableObject> ChildObjects
        {
            get
            {
                if (this.Seaman != null)
                    yield return this.Seaman;
            }
        }
    }
}

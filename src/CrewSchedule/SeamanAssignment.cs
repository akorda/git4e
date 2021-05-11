using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class SeamanAssignment : HashableObject
    {
        public const string SeamanAssignmentContentType = "SeamanAssignment";

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
            public string SeamanFullHash { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var assignment = new SeamanAssignment(repository, hash)
                {
                    SeamanAssignmentId = this.SeamanAssignmentId,
                    StartOverlap = this.StartOverlap,
                    StartDuties = this.StartDuties,
                    EndDuties = this.EndDuties,
                    EndOverlap = this.EndOverlap,
                    Seaman = new LazyHashableObject(repository, this.SeamanFullHash, CrewSchedule.Seaman.SeamanContentType)
                };
                return Task.FromResult(assignment as IHashableObject);
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
                    this.MarkAsDirty();
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
                    this.MarkAsDirty();
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
                    this.MarkAsDirty();
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
                    this.MarkAsDirty();
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
                    this.MarkAsDirty();
                }
            }
        }

        //do not serialize
        internal string SeamanCode { get; set; }

        public LazyHashableObject Seaman { get; set; }

        //do not serialize
        internal string VesselCode { get; set; }
        internal string DutyRankCode { get; set; }
        internal int PositionNo { get; set; }

        public SeamanAssignment(IRepository repository, string hash = null)
            : base(repository, SeamanAssignmentContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var content = new SeamanAssignmentContent
            {
                SeamanAssignmentId = this.SeamanAssignmentId,
                StartOverlap = this.StartOverlap,
                StartDuties = this.StartDuties,
                EndDuties = this.EndDuties,
                EndOverlap = this.EndOverlap,
                SeamanFullHash = this.Seaman?.FullHash
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            if (this.Seaman != null)
            {
                yield return await Task.FromResult(this.Seaman);
            }
        }
    }

    /// <summary>
    /// VesselPosition Hash with the following included properties:
    /// 1. SeamanAssignmentId
    /// 2. Seaman->SeamanCode
    /// </summary>
    public class LazySeamanAssignment : LazyHashableObject<string, string>
    {
        public LazySeamanAssignment(IRepository repository, string fullHash)
            : base(repository, fullHash, SeamanAssignment.SeamanAssignmentContentType)
        {
        }

        public LazySeamanAssignment(SeamanAssignment asn)
            : base(asn, a => (a as SeamanAssignment).SeamanAssignmentId, a => (a as SeamanAssignment).Seaman?.GetValue<Seaman>().SeamanCode)
        {
        }
    }
}

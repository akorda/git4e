using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class VesselPosition : HashableObject
    {
        public const string VesselPositionContentType = "VesselPosition";

        [ProtoContract]
        public class VesselPositionContent : IContent
        {
            [ProtoMember(1)]
            public string VesselCode { get; set; }
            [ProtoMember(2)]
            public string DutyRankCode { get; set; }
            [ProtoMember(3)]
            public int PositionNo { get; set; }
            [ProtoMember(4)]
            public string SeamanAssignmentsHash { get; set; }
            [ProtoMember(5)]
            public string[] SeamanAssignmentFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var seamanAssignments = this.SeamanAssignmentFullHashes?
                    .Select(asnHash => new LazySeamanAssignment(repository, asnHash))
                    ?? new LazySeamanAssignment[0];
                var position = new VesselPosition(repository, hash)
                {
                    VesselCode = this.VesselCode,
                    DutyRankCode = this.DutyRankCode,
                    PositionNo = this.PositionNo,
                    SeamanAssignments = new HashableList<LazySeamanAssignment>(repository, seamanAssignments, this.SeamanAssignmentsHash)
                };
                return Task.FromResult(position as IHashableObject);
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

        string _DutyRankCode;
        public string DutyRankCode
        {
            get => _DutyRankCode;
            set
            {
                if (_DutyRankCode != value)
                {
                    _DutyRankCode = value;
                    this.MarkAsDirty();
                }
            }
        }

        int _PositionNo;
        public int PositionNo
        {
            get => _PositionNo;
            set
            {
                if (_PositionNo != value)
                {
                    _PositionNo = value;
                    this.MarkAsDirty();
                }
            }
        }

        HashableList<LazySeamanAssignment> _SeamanAssignments;
        public HashableList<LazySeamanAssignment> SeamanAssignments
        {
            get => _SeamanAssignments;
            set
            {
                if (_SeamanAssignments != value)
                {
                    _SeamanAssignments = value;
                    this.MarkAsDirty();
                }
            }
        }

        public VesselPosition(IRepository repository, string hash = null)
            : base(repository, VesselPositionContentType, hash)
        {
        }

        protected override object GetContent()
        {
            if (this.SeamanAssignments == null)
                this.SeamanAssignments = new HashableList<LazySeamanAssignment>(this.Repository);

            var content = new VesselPositionContent
            {
                VesselCode = this.VesselCode,
                DutyRankCode = this.DutyRankCode,
                PositionNo = this.PositionNo,
                SeamanAssignmentsHash = this.SeamanAssignments.Hash,
                SeamanAssignmentFullHashes = this.SeamanAssignments.FullHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var assignments = this.SeamanAssignments ?? new HashableList<LazySeamanAssignment>(this.Repository);
            foreach (var assignment in assignments)
            {
                yield return await Task.FromResult(assignment);
            }
        }

        public override string UniqueId => $"{VesselCode}.{DutyRankCode}.{PositionNo}";
    }

    /// <summary>
    /// VesselPosition Hash with the following included properties:
    /// 1. DutyRankCode
    /// 2. PositionNo
    /// </summary>
    public class LazyVesselPosition : LazyHashableObject
    {
        public LazyVesselPosition(IRepository repository, string fullHash)
            : base(repository, fullHash, VesselPosition.VesselPositionContentType)
        {
        }

        public LazyVesselPosition(VesselPosition vesselPosition)
            : base(vesselPosition, vesselPosition.DutyRankCode, vesselPosition.PositionNo.ToString())
        {
        }

        public string DutyRankCode => this.IncludedProperties[0];

        private int? _PositionNo;
        public int PositionNo
        {
            get
            {
                if (!_PositionNo.HasValue)
                    _PositionNo = int.Parse(this.IncludedProperties[1]);
                return _PositionNo.Value;
            }
        }
    }
}
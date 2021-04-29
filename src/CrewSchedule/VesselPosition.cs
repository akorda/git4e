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
            public string VesselPositionId { get; set; }

            [ProtoMember(2)]
            public string VesselCode { get; set; }

            [ProtoMember(3)]
            public string DutyRankCode { get; set; }

            [ProtoMember(4)]
            public int PositionNo { get; set; }

            [ProtoMember(5)]
            public string[] SeamanAssignmentHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var seamanAssignments = this.SeamanAssignmentHashes
                    .Select(asnHash => new LazyHashableObject<SeamanAssignment>(asnHash, SeamanAssignment.SeamanAssignmentContentType))
                    .ToArray();
                var position = new VesselPosition(hash)
                {
                    VesselPositionId = this.VesselPositionId,
                    VesselCode = this.VesselCode,
                    DutyRankCode = this.DutyRankCode,
                    PositionNo = this.PositionNo,
                    SeamanAssignments = seamanAssignments
                };
                return Task.FromResult(position as IHashableObject);
            }
        }

        string _VesselPositionId;
        public string VesselPositionId
        {
            get => _VesselPositionId;
            set
            {
                if (_VesselPositionId != value)
                {
                    _VesselPositionId = value;
                    this.MarkContentAsDirty();
                }
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

        string _DutyRankCode;
        public string DutyRankCode
        {
            get => _DutyRankCode;
            set
            {
                if (_DutyRankCode != value)
                {
                    _DutyRankCode = value;
                    this.MarkContentAsDirty();
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
                    this.MarkContentAsDirty();
                }
            }
        }

        IEnumerable<LazyHashableObject<SeamanAssignment>> _SeamanAssignments;
        public IEnumerable<LazyHashableObject<SeamanAssignment>> SeamanAssignments
        {
            get => _SeamanAssignments;
            set
            {
                if (_SeamanAssignments != value)
                {
                    _SeamanAssignments = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        public VesselPosition(string hash = null)
            : base(VesselPositionContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var seamanAssignmentHashes = this.SeamanAssignments?
                //.OrderBy(asn => asn.StartOverlap)
                .OrderBy(asn => asn.Hash)
                .Select(asn => asn.Hash)
                .ToArray();
            var content = new VesselPositionContent
            {
                VesselPositionId = this.VesselPositionId,
                VesselCode = this.VesselCode,
                DutyRankCode = this.DutyRankCode,
                PositionNo = this.PositionNo,
                SeamanAssignmentHashes = seamanAssignmentHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var assignments = this.SeamanAssignments ?? new LazyHashableObject<SeamanAssignment>[0];
            foreach (var assignment in assignments)
            {
                yield return await Task.FromResult(assignment);
            }
        }
    }
}

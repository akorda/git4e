using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using Microsoft.Extensions.DependencyInjection;
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
            public string[] SeamanAssignmentHashes { get; set; }

            public async Task<IHashableObject> ToHashableObjectAsync(IServiceProvider serviceProvider, IObjectLoader objectLoader, CancellationToken cancellationToken = default)
            {
                var loadAssignmentTasks = this.SeamanAssignmentHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHashAsync(hash, cancellationToken));
                await Task.WhenAll(loadAssignmentTasks);

                var seamanAssignments = loadAssignmentTasks
                    .Select(task => task.Result)
                    .Cast<SeamanAssignment>()
                    .ToList();
                var position = ActivatorUtilities.CreateInstance<VesselPosition>(serviceProvider);
                position.VesselPositionId = this.VesselPositionId;
                position.VesselCode = this.VesselCode;
                position.DutyRankCode = this.DutyRankCode;
                position.PositionNo = this.PositionNo;
                position.SeamanAssignments = seamanAssignments;
                return position;
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

        IEnumerable<SeamanAssignment> _SeamanAssignments;
        public IEnumerable<SeamanAssignment> SeamanAssignments
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

        public VesselPosition(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base("VesselPosition", contentSerializer, hashCalculator)
        {
        }

        protected override object GetContent()
        {
            var seamanAssignmentHashes = this.SeamanAssignments?
                .OrderBy(asn => asn.StartOverlap)
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

        public override IEnumerable<IHashableObject> ChildObjects
        {
            get
            {
                if (this.SeamanAssignments != null)
                    return this.SeamanAssignments;
                return new IHashableObject[0];
            }
        }
    }
}

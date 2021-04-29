﻿using System;
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
            public string SeamanHash { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var assignment = new SeamanAssignment(hash)
                {
                    SeamanAssignmentId = this.SeamanAssignmentId,
                    StartOverlap = this.StartOverlap,
                    StartDuties = this.StartDuties,
                    EndDuties = this.EndDuties,
                    EndOverlap = this.EndOverlap,
                    Seaman = new LazyHashableObject<Seaman>(this.SeamanHash, CrewSchedule.Seaman.SeamanContentType)
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

        public LazyHashableObject<Seaman> Seaman { get; set; }

        //do not serialize
        public string VesselCode { get; set; }
        public string DutyRankCode { get; set; }
        public int PositionNo { get; set; }

        public SeamanAssignment(string hash = null)
            : base(SeamanAssignmentContentType, hash)
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
                SeamanHash = this.Seaman?.Hash
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
}

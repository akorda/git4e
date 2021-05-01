using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;
using ProtoBuf;

namespace CrewSchedule
{
    public class Plan : HashableObject
    {
        public const string PlanContentType = "Plan";

        [ProtoContract]
        public class PlanContent : IContent
        {
            [ProtoMember(1)]
            public string PlanVersionId { get; set; }
            [ProtoMember(2)]
            public string[] VesselHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var vessels =
                    this.VesselHashes
                    .Select(vesselHash => new LazyHashableObject<Vessel>(vesselHash, Vessel.VesselContentType))
                    .ToLazyHashableObjectList();
                var plan = new Plan(hash)
                {
                    PlanVersionId = this.PlanVersionId,
                    Vessels = vessels
                };
                return Task.FromResult(plan as IHashableObject);
            }
        }

        string _PlanVersionId;
        public string PlanVersionId
        {
            get => _PlanVersionId;
            set
            {
                if (_PlanVersionId != value)
                {
                    _PlanVersionId = value;
                    this.MarkAsDirty();
                }
            }
        }

        LazyHashableObjectList<Vessel> _Vessels;
        public LazyHashableObjectList<Vessel> Vessels
        {
            get => _Vessels;
            set
            {
                if (_Vessels != value)
                {
                    _Vessels = value;
                    if (value != null)
                        value.Parent = this;
                    this.MarkAsDirty();
                }
            }
        }

        public Plan(string hash = null)
            : base(PlanContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var vesselHashes = this.Vessels?
                .OrderBy(vessel => vessel.Hash)
                .Select(vessel => vessel.Hash)
                .ToArray();
            var content = new PlanContent
            {
                PlanVersionId = this.PlanVersionId,
                VesselHashes = vesselHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var vessels = this.Vessels.AsEnumerable() ?? new LazyHashableObject<Vessel>[0];
            foreach (var vessel in vessels)
            {
                yield return await Task.FromResult(vessel);
            }
        }
    }
}

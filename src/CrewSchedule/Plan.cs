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
            public string[] VesselFullHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                var vessels =
                    this.VesselFullHashes
                    .Select(vesselHash => new LazyVessel(repository, vesselHash))
                    .ToList();
                var plan = new Plan(repository, hash)
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

        List<LazyVessel> _Vessels;
        public List<LazyVessel> Vessels
        {
            get => _Vessels;
            set
            {
                if (_Vessels != value)
                {
                    _Vessels = value;
                    this.MarkAsDirty();
                }
            }
        }

        public Plan(IRepository repository, string hash = null)
            : base(repository, PlanContentType, hash)
        {
        }

        protected override object GetContent()
        {
            var vesselFullHashes = this.Vessels?
                .OrderBy(vessel => vessel.FullHash)
                .Select(vessel => vessel.FullHash)
                .ToArray();
            var content = new PlanContent
            {
                PlanVersionId = this.PlanVersionId,
                VesselFullHashes = vesselFullHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            var vessels = this.Vessels.AsEnumerable() ?? new List<LazyVessel>();
            foreach (var vessel in vessels)
            {
                yield return await Task.FromResult(vessel);
            }
        }
    }

    /// <summary>
    /// Plan Hash with the following included properties:
    /// 1. PlanVersionId
    /// </summary>
    public class LazyPlan : LazyHashableObject<string>
    {
        public LazyPlan(IRepository repository, string fullHash)
            : base(repository, fullHash, Plan.PlanContentType)
        {
        }

        public LazyPlan(Plan plan)
            : base(plan, p => (p as Plan).PlanVersionId)
        {
        }
    }
}

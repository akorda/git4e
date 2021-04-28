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
    public class Plan : HashableObject
    {
        [ProtoContract]
        public class PlanContent : IContent
        {
            [ProtoMember(1)]
            public string PlanVersionId { get; set; }
            [ProtoMember(2)]
            public string[] VesselHashes { get; set; }

            public async Task<IHashableObject> ToHashableObjectAsync(IServiceProvider serviceProvider, IObjectLoader objectLoader, CancellationToken cancellationToken = default)
            {
                var loadVesselTasks = this.VesselHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHashAsync(hash, cancellationToken));
                await Task.WhenAll(loadVesselTasks);

                var vessels = loadVesselTasks
                    .Select(task => task.Result)
                    .Cast<Vessel>()
                    .ToArray();
                var plan = ActivatorUtilities.CreateInstance<Plan>(serviceProvider);
                plan.PlanVersionId = this.PlanVersionId;
                plan.Vessels = vessels;
                return plan;
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
                    this.MarkContentAsDirty();
                }
            }
        }

        IEnumerable<Vessel> _Vessels;
        public IEnumerable<Vessel> Vessels
        {
            get => _Vessels;
            set
            {
                if (_Vessels != value)
                {
                    _Vessels = value;
                    this.MarkContentAsDirty();
                }
            }
        }

        public Plan(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base("Plan", contentSerializer, hashCalculator)
        {
        }

        public override async Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var vesselHashes = this.Vessels?
                .OrderBy(vessel => vessel.VesselCode)
                .Select(vessel => vessel.Hash)
                .ToArray();
            var content = new PlanContent
            {
                PlanVersionId = this.PlanVersionId,
                VesselHashes = vesselHashes
            };
            await this.ContentSerializer.SerializeContentAsync(stream, this.Type, content, cancellationToken);
        }

        public override IEnumerable<IHashableObject> ChildObjects
        {
            get
            {
                if (this.Vessels != null) return this.Vessels;
                return new IHashableObject[0];
            }
        }
    }
}

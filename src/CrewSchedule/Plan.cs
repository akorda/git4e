using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            public Hash[] VesselHashes { get; set; }

            public IHashableObject ToHashableObject(IServiceProvider serviceProvider, IObjectLoader objectLoader)
            {
                var vessels =
                    this.VesselHashes
                    .Where(hash => hash != null)
                    .Select(hash => objectLoader.GetObjectByHash(hash).Result)
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

        public override void SerializeContent(Stream stream)
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
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
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

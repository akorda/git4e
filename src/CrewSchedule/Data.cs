using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Git4e;
using Microsoft.Extensions.DependencyInjection;

namespace CrewSchedule
{
    public class Data
    {
        public IEnumerable<Seaman> Seamen { get; private set; }
        public IEnumerable<SeamanAssignment> Assignments { get; private set; }
        public IEnumerable<VesselPosition> Positions { get; private set; }
        public IEnumerable<Vessel> Vessels { get; private set; }
        public Plan Plan { get; private set; }

        public async Task LoadAsync(
            string connectionString,
            string planVersionId,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var planVersionParameter = new { PlanVersionId = planVersionId };
                var sql = $@"
                    WITH SA AS
                    (
	                    SELECT DISTINCT(VesselCode)
	                    FROM cs.SeamanAssignments
	                    WHERE PlanVersionId = @PlanVersionId
                    )
                    SELECT V.VesselCode, V.Name
                    FROM md.Vessels V
                    WHERE V.VesselCode IN (SELECT VesselCode FROM SA)
                    ";

                var command = new CommandDefinition(sql, parameters: planVersionParameter, cancellationToken: cancellationToken);

                this.Vessels = (await connection.QueryAsync(command))
                    .Select(row =>
                    {
                        var vessel = ActivatorUtilities.CreateInstance<Vessel>(serviceProvider);
                        vessel.VesselCode = row.VesselCode;
                        vessel.Name = row.Name;
                        return vessel;
                    })
                    .ToArray();

                var vesselsMap = new Dictionary<string, List<VesselPosition>>();
                foreach (var vessel in Vessels)
                    vesselsMap[vessel.VesselCode] = new List<VesselPosition>();

                sql = $@"
                    WITH SA AS
                    (
	                    SELECT VesselCode, DutyRankCode, PositionNo
	                    FROM cs.SeamanAssignments
	                    WHERE PlanVersionId = @PlanVersionId
	                    GROUP BY VesselCode, DutyRankCode, PositionNo
                    )
                    SELECT VP.VesselPositionId, VP.VesselCode, VP.DutyRankCode, VP.PositionNo
                    FROM cs.VesselPositions VP
                    RIGHT JOIN SA ON SA.VesselCode = VP.VesselCode AND SA.DutyRankCode = VP.DutyRankCode AND SA.PositionNo = VP.PositionNo
                    ";

                command = new CommandDefinition(sql, parameters: planVersionParameter, cancellationToken: cancellationToken);
                this.Positions = (await connection.QueryAsync(command))
                    .Select(row =>
                    {
                        var position = ActivatorUtilities.CreateInstance<VesselPosition>(serviceProvider);
                        position.VesselPositionId = row.VesselPositionId;
                        position.VesselCode = row.VesselCode;
                        position.DutyRankCode = row.DutyRankCode;
                        position.PositionNo = row.PositionNo;
                        return position;
                    })
                    .ToArray();

                var positionMap = new Dictionary<string, List<SeamanAssignment>>();
                foreach (var position in Positions)
                    positionMap[$"{position.VesselCode}#{position.DutyRankCode}#{position.PositionNo}"] = new List<SeamanAssignment>();

                sql = $@"
                    SELECT SeamanAssignmentId, VesselCode, DutyRankCode, PositionNo, StartOverlappingSlot, StartSlot, EndSlot, EndOverlappingSlot, SeamanCode, VesselCode, DutyRankCode, PositionNo
                    FROM cs.SeamanAssignments
                    WHERE PlanVersionId = @PlanVersionId";
                command = new CommandDefinition(sql, parameters: planVersionParameter, cancellationToken: cancellationToken);
                this.Assignments = (await connection.QueryAsync(command))
                    .Select(row =>
                    {
                        var assignment = ActivatorUtilities.CreateInstance<SeamanAssignment>(serviceProvider);
                        assignment.SeamanAssignmentId = row.SeamanAssignmentId;
                        assignment.StartOverlap = row.StartOverlappingSlot;
                        assignment.StartDuties = row.StartSlot;
                        assignment.EndDuties = row.EndSlot;
                        assignment.EndOverlap = row.EndOverlappingSlot;
                        //In memory only
                        assignment.SeamanCode = row.SeamanCode;
                        assignment.VesselCode = row.VesselCode;
                        assignment.DutyRankCode = row.DutyRankCode;
                        assignment.PositionNo = row.PositionNo;
                        return assignment;
                    })
                    .ToArray();
                var asnsMap = Assignments.ToDictionary(asn => asn.SeamanAssignmentId);

                sql = $@"
                    SELECT PersonCode, LastName, FirstName
                    FROM cs.Persons WHERE PersonCode IN
                    (SELECT DISTINCT(SeamanCode) FROM cs.SeamanAssignments WHERE PlanVersionId = @PlanVersionId)";
                command = new CommandDefinition(sql, parameters: planVersionParameter, cancellationToken: cancellationToken);
                this.Seamen = (await connection.QueryAsync(command))
                    .Select(row =>
                    {
                        var seaman = ActivatorUtilities.CreateInstance<Seaman>(serviceProvider);
                        seaman.SeamanCode = row.PersonCode;
                        seaman.LastName = row.LastName;
                        seaman.FirstName = row.FirstName;
                        return seaman;
                    })
                    .ToArray();

                var seamenMap = Seamen.ToDictionary(s => s.SeamanCode);

                foreach (var asn in Assignments)
                {
                    if (positionMap.TryGetValue($"{asn.VesselCode}#{asn.DutyRankCode}#{asn.PositionNo}", out var seamanAssignments))
                        seamanAssignments.Add(asn);

                    if (seamenMap.TryGetValue(asn.SeamanCode, out var seaman))
                        asn.Seaman = new LazyHashableObject<Seaman>(seaman);
                }

                foreach (var pos in Positions)
                {
                    if (vesselsMap.TryGetValue(pos.VesselCode, out var vesselPositions))
                        vesselPositions.Add(pos);
                    else
                    {
                        var a = 1;
                        a++;
                    }
                }

                foreach (var position in Positions)
                    position.SeamanAssignments = positionMap[$"{position.VesselCode}#{position.DutyRankCode}#{position.PositionNo}"].Select(asn => new LazyHashableObject<SeamanAssignment>(asn)).ToArray();

                foreach (var vessel in Vessels)
                    vessel.Positions = vesselsMap[vessel.VesselCode].Select(vp => new LazyHashableObject<VesselPosition>(vp)).ToArray();

                this.Plan = ActivatorUtilities.CreateInstance<Plan>(serviceProvider);
                this.Plan.PlanVersionId = planVersionId;
                this.Plan.Vessels = Vessels.Select(vessel => new LazyHashableObject<Vessel>(vessel)).ToArray();
            }
        }
    }
}

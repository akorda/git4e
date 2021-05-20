using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;

namespace CrewSchedule
{
    public class CommitsComparer : ICommitsComparer
    {
        public async Task CompareCommits(ICommit commit, ICommit prevCommit, ICommitsComparerVisitor visitor, CancellationToken cancellationToken = default)
        {
            if (commit.Root.Hash == prevCommit.Root.Hash)
            {
                Console.WriteLine($"Commit '{commit.Hash}' and commit '{prevCommit.Hash}' have the same Root");
                return;
            }

            var plan1 = commit.Root.LoadValue<Plan>();
            var plan2 = prevCommit.Root.LoadValue<Plan>();

            //compare properties
            if (plan1.PlanVersionId != plan2.PlanVersionId)
            {
                Console.WriteLine($"{nameof(Plan.PlanVersionId)}: '{plan1.PlanVersionId}' <> '{plan2.PlanVersionId}'");
            }

            //first compare collections hashes
            if (plan1.Vessels.Hash != plan2.Vessels.Hash)
            {
                foreach (var lv1 in plan1.Vessels)
                {
                    //find Vessels in lib1 but not in lib2 and updated Vessels
                    var lv2 = plan2.Vessels.FirstOrDefault(a => a.VesselCode == lv1.VesselCode);
                    if (lv2 == null)
                    {
                        Console.WriteLine($"Vessel '{lv1.Name}' ({lv1.VesselCode}) created at commit '{commit.Hash}'");
                        continue;
                    }

                    if (lv1.Hash != lv2.Hash)
                    {
                        Console.WriteLine($"Vessel '{lv1.Name}' ({lv1.VesselCode}) updated at commit '{commit.Hash}'");
                        await CompareVessels(commit, lv1.LoadValue<Vessel>(), lv2.LoadValue<Vessel>(), cancellationToken);
                        continue;
                    }
                }

                foreach (var la2 in plan2.Vessels)
                {
                    var la1 = plan1.Vessels.FirstOrDefault(a => a.VesselCode == la2.VesselCode);
                    if (la1 == null)
                    {
                        Console.WriteLine($"Vessel '{la2.Name}' ({la2.VesselCode}) deleted at commit '{commit.Hash}'");
                        continue;
                    }
                }
            }
        }

        private static async Task CompareVessels(ICommit commit, Vessel vessel1, Vessel vessel2, CancellationToken cancellationToken)
        {
            //compare simple properties. Their Ids are the same

            if (!string.Equals(vessel1.Name, vessel2.Name))
            {
                Console.WriteLine($"{nameof(Vessel.Name)}: '{vessel1.Name}' <> '{vessel2.Name}'");
            }

            //first compare collections hashes
            if (vessel1.Positions.Hash != vessel2.Positions.Hash)
            {
                //find Positions in lib1 but not in lib2 and updated Positions
                foreach (var al1 in vessel1.Positions)
                {
                    var al2 = vessel2.Positions.FirstOrDefault(a => a.UniqueId == al1.UniqueId);
                    if (al2 == null)
                    {
                        Console.WriteLine($"Position '{al1.UniqueId}' ({al1.UniqueId}) created at commit '{commit.Hash}'");
                        continue;
                    }

                    if (al1.Hash != al2.Hash)
                    {
                        Console.WriteLine($"Position '{al1.UniqueId}' ({al1.UniqueId}) updated at commit '{commit.Hash}'");
                        await ComparePositions(commit, al1.LoadValue<VesselPosition>(), al2.LoadValue<VesselPosition>(), cancellationToken);
                        continue;
                    }
                }

                foreach (var al2 in vessel2.Positions)
                {
                    var al1 = vessel1.Positions.FirstOrDefault(a => a.UniqueId == al2.UniqueId);
                    if (al1 == null)
                    {
                        Console.WriteLine($"Position '{al2.UniqueId}' deleted at commit '{commit.Hash}'");
                        continue;
                    }
                }
            }
        }

        private static async Task ComparePositions(ICommit commit, VesselPosition vesselPosition1, VesselPosition vesselPosition2, CancellationToken cancellationToken)
        {
            //there are no simple properties except those in primary key. No need to compare

            //first compare collections hashes
            if (vesselPosition1.SeamanAssignments.Hash != vesselPosition2.SeamanAssignments.Hash)
            {
                //find SeamanAssignments in lib1 but not in lib2 and updated SeamanAssignments
                foreach (var al1 in vesselPosition1.SeamanAssignments)
                {
                    var al2 = vesselPosition2.SeamanAssignments.FirstOrDefault(a => a.UniqueId == al1.UniqueId);
                    if (al2 == null)
                    {
                        Console.WriteLine($"SeamanAssignment '{al1.UniqueId}' ({al1.UniqueId}) created at commit '{commit.Hash}'");
                        continue;
                    }

                    if (al1.Hash != al2.Hash)
                    {
                        Console.WriteLine($"SeamanAssignment '{al1.UniqueId}' ({al1.UniqueId}) updated at commit '{commit.Hash}'");
                        await CompareSeamanAssignments(commit, al1.LoadValue<SeamanAssignment>(), al2.LoadValue<SeamanAssignment>(), cancellationToken);
                        continue;
                    }
                }

                foreach (var al2 in vesselPosition2.SeamanAssignments)
                {
                    var al1 = vesselPosition1.SeamanAssignments.FirstOrDefault(a => a.UniqueId == al2.UniqueId);
                    if (al1 == null)
                    {
                        Console.WriteLine($"SeamanAssignment '{al2.UniqueId}' deleted at commit '{commit.Hash}'");
                        continue;
                    }
                }
            }
        }

        private static Task CompareSeamanAssignments(ICommit commit, SeamanAssignment asn1, SeamanAssignment asn2, CancellationToken cancellationToken)
        {
            //compare simple properties. Their Ids are the same

            if (!string.Equals(asn1.StartOverlap, asn2.StartOverlap))
            {
                Console.WriteLine($"{nameof(SeamanAssignment.StartOverlap)}: '{asn1.StartOverlap}' <> '{asn2.StartOverlap}'");
            }

            if (!string.Equals(asn1.StartDuties, asn2.StartDuties))
            {
                Console.WriteLine($"{nameof(SeamanAssignment.StartDuties)}: '{asn1.StartDuties}' <> '{asn2.StartDuties}'");
            }

            if (!string.Equals(asn1.EndDuties, asn2.EndDuties))
            {
                Console.WriteLine($"{nameof(SeamanAssignment.EndDuties)}: '{asn1.EndDuties}' <> '{asn2.EndDuties}'");
            }

            if (!string.Equals(asn1.EndOverlap, asn2.EndOverlap))
            {
                Console.WriteLine($"{nameof(SeamanAssignment.EndOverlap)}: '{asn1.EndOverlap}' <> '{asn2.EndOverlap}'");
            }

            return Task.CompletedTask;
        }
    }
}

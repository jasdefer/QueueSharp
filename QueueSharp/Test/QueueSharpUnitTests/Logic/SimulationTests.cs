using QueueSharp.Factories;
using QueueSharp.Logic;
using QueueSharp.Logic.Validation;
using QueueSharp.Model.Components;
using QueueSharp.Model.DurationDistribution;
using QueueSharp.Model.Routing;
using QueueSharp.Model.ServerSelector;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace QueueSharpUnitTests.Logic;
public class SimulationTests
{
    [Fact]
    public void SingleNode()
    {
        Cohort[] cohorts = CohortFactory.GetSingleNode("Node01",
            serverCount: 2,
            arrivalDistribution: new ConstantDuration(10),
            serviceDistribution: new ConstantDuration(30),
            arrivalEnd: 1000,
            serviceEnd: 2000);
        Simulation simulation = new(cohorts);
        ImmutableArray<NodeVisitRecord> result = simulation.Start().ToImmutableArray();
        ImmutableArray<NodeServiceRecord> nodeServiceRecords = result.OfType<NodeServiceRecord>().ToImmutableArray();
        nodeServiceRecords.Should().HaveCount(100);
        nodeServiceRecords.Should().AllSatisfy(x => x.ServiceDuration.Should().Be(30));
        nodeServiceRecords.Should().AllSatisfy(x => x.Destination.Should().BeNull());
        for (int i = 1; i < nodeServiceRecords.Length; i++)
        {
            nodeServiceRecords[i].ArrivalTime.Should().Be(nodeServiceRecords[i - 1].ArrivalTime + 10);
            nodeServiceRecords[i].QueueSizeAtArrival.Should().BeGreaterThanOrEqualTo(nodeServiceRecords[i - 1].QueueSizeAtArrival);
        }
        NodeVisitRecordsValidation.Validate(result);
    }

    [Fact]
    public void ThreeNode()
    {
        Cohort[] cohorts = CohortFactory.GetEventEntrance(null, 0.1, 1);
        Simulation simulation = new(cohorts);
        ImmutableArray<NodeVisitRecord> result = simulation.Start().ToImmutableArray();
        NodeVisitRecordsValidation.Validate(result);
    }

    [Fact]
    public void CiwComparison_SingleNode_SameArrivalDistribution()
    {
        double[] empiracalInterarrivalTimes = [94.00781327103127, 72.14844626733313, 14.723185844713157, 34.202938982980015, 29.845604593399855, 52.719193203294026, 77.729343611498, 4.928050173107749, 1.4378512284424687, 90.32287913699474, 28.349262231674345, 71.8331057676881, 0.10541371000840627, 29.474252782508188, 63.924048632128915, 12.987927497189048, 145.26779913653945, 115.84812654581708, 1.5533811231534855, 1.2887602950697783, 38.98020532358987, 139.96648504338737, 23.99900042711556, 12.205554409785236, 27.419155925543237, 1.4735408657222706, 12.531625873584971, 28.8026718895062, 34.240327167232635, 13.26892941575693, 13.124538823653893, 12.344990325646904, 30.77260430994602, 17.10913855787726, 1.086198563358721, 90.87786213042637, 40.64772448219924, 51.40224368182726, 10.283988357281942, 244.93286828297096, 98.28654960578842, 6.4422600599039015, 20.225417199657386, 63.914061702416575, 62.09961870422467, 137.78900847846307, 27.41832742436418, 88.60834122069355, 55.479450615841415, 18.074935906417522, 44.28572505675311, 107.05691225205828, 93.60427183999627, 35.188552788199104, 44.458377918790575, 1.7567965302960147, 13.902429427291736, 79.82713263054666, 26.748573400012447, 9.497976695635316, 39.792091682584214, 60.70801978053805, 56.11746433990584, 23.476428745227167, 28.898299007791138, 35.50718922273336, 75.3536824279754, 36.79630626764856, 24.98234157749539, 33.63718967429895, 1.5010560990381236, 2.2230601266787744, 60.76552310191937, 204.28227735930386, 44.96968107290786, 25.010746373364782, 9.337519291091667, 34.88171748647028, 201.08251258392283, 73.59765408571957, 38.784875065498, 98.40924254520996, 13.209745277538786, 36.05384682923159, 152.31696203723368, 43.11319228416687, 30.728976342405986, 15.686210720868075, 39.703246714278066, 157.46315231509925, 0.2862744412423126, 76.54409992424462, 85.8750794519392, 108.65666715170937, 67.45058620910368, 82.81072923101055, 36.56096916159004, 41.2035690037892, 27.76419368466395, 2.8879866573770414, 102.01494741606075, 42.19842605855138, 11.14714233390805, 35.1316481769245, 33.172148786991784, 22.06419794262638, 21.238353842626566, 38.66136398489289, 48.84046091582604, 47.39583830919128, 30.638008156058277, 1.4186869156810644, 13.042597462647791, 9.752790249253849, 43.908924765113625, 98.66725475147996, 80.08314595341926, 79.75150097916321, 84.75996824361937, 14.738291159601431, 92.17732815175441, 55.90711704793375, 4.3451584769718465, 0.8415744111180175, 0.7333505798860642, 70.44474710346276, 14.354727297525642, 5.79797025939115, 49.01508083214412, 21.111965389576653, 3.602486510275412, 8.695384127496254, 37.47322201854149, 9.204853377381369, 15.935555743229088, 62.16859738196035, 30.32110831592945, 19.430529816809212, 32.10094133489292, 1.195917723916864, 24.43340497296049, 27.315618042764072, 10.415167242907955, 5.757171324651608, 115.0385870834998, 35.679330583357114, 11.72861763315359, 46.52564952586363, 84.92429583651847, 1.0518930281123176, 0.9013008823230848, 7.9182455519012365, 63.44076374022279, 8.731218373171032, 60.97219803490225, 56.68749152933469, 39.34017443189896, 12.461528139052461, 185.64737461623554, 79.9275835845956, 36.34549057180175, 12.628346489677824, 52.27819135354821, 25.117912820624042, 42.88292972073759, 19.374811683794178, 49.840867377775794, 3.0291904309351594, 17.734271190424806, 171.95011870643975, 104.18623275046775, 18.292028176243548, 97.77786893945722, 18.57954082330434, 140.08105123480618, 68.09806503210439, 26.90746550771655, 14.541558100334441, 0.4258212238382839, 105.48180135830262, 1.9327032775436237, 85.57743864527583, 163.77379717439726, 42.2311386100173, 9.40795381091084, 101.16480637965469, 182.05255649381616, 60.87370056310465, 35.55270239719539, 23.738254097064782, 21.30361561852078, 11.518590447856695, 56.066368832944136, 28.365400464139384, 10.790937476283943, 5.514422055948671, 54.82435666237143, 17.55400784819176, 34.63735525372067, 19.677740028289918, 102.63862025276103, 114.96864768945852, 0.9129331567892223, 11.210519223966912, 19.855557868528194, 217.33188547270947, 76.32390604013017, 20.707307567097814, 11.97824459970252, 56.11273961082952, 90.91576990807334, 134.55041603877908, 21.06827876866737, 107.02042215160873, 58.095208503784306, 33.130774878494776, 211.7087182439882, 13.370476790194516, 64.63385999975799, 4.424089990205175, 9.298057110509035, 120.94908273577676, 11.974330920489592, 71.17202738463129, 45.84064717667752, 91.9841421058245, 22.95183922866636, 20.7973855810651, 17.21017239709181, 101.02838557767791, 46.31484748702678, 154.2910085515541, 109.13581380106007, 7.271291296436175, 40.055606916090255, 5.506091527151511, 1.99621355664749, 3.800519274567705, 100.55863336096081, 77.58592208314622, 88.1603416022117, 20.843808171834098, 47.749763146292025, 76.14090597934955, 23.743945011574397, 42.28946126736264, 12.661718185605423, 4.263912841104684, 15.511631314866463, 110.71411944000101, 41.556920401446405, 129.55817976159778, 30.60318234049919, 16.229943865524547, 77.32659793243693, 87.94568916638673, 0.6229518479012768, 55.495539707146236, 4.80809886795214, 6.114172905712621, 108.16728191096263, 2.0423256207377563, 13.697727368913547, 221.8072425321334, 27.323813437442368, 6.139927274532965, 9.159102593363059, 13.81536951306407, 68.1301449707189, 5.425726772449707, 120.82377103605722, 23.763052910411716, 175.76990372025102, 119.96731673687646, 17.408672424480756, 14.611964443052784, 32.409655953289985, 5.275200961612427, 52.78485305096365, 2.021323086313714, 0.5280865376935253, 202.51722561901806, 17.516886190785954, 45.38769413276714, 29.877718819436268, 18.79149464335569, 3.2517210062978847, 122.31816435651308];
        Cohort[] cohorts = CohortFactory.GetSingleNode("Node01",
                serverCount: 3,
                new EmpiricalDuration(empiracalInterarrivalTimes.Select(x => (int)Math.Round(x, 0)), false, 1),
                new ConstantDuration(120),
                arrivalEnd: 20000,
                serviceEnd: 20000);
        SimulationSettings simulationSettings = new(14400);
        Simulation simulation = new(cohorts, simulationSettings);
        ImmutableArray<NodeVisitRecord> nodeVisitRecords = simulation.Start().ToImmutableArray();
        SimulationReport report = SimulationAnalysis.GetSimulationReport(nodeVisitRecords);
        NodeVisitRecordsValidation.Validate(nodeVisitRecords);
        report.NodeReportsByNodeId.Should().HaveCount(1);
        SimulationNodeReport nodeReport = report.NodeReportsByNodeId.Single().Value;
        nodeReport.WaitingTimeMetrics.Mean.Should().BeApproximately(79, 1);
        nodeReport.WaitingTimeMetrics.Variance.Should().BeApproximately(12099, 500);
        nodeReport.ServiceDurationMetrics.Mean.Should().Be(120);
    }

    [Fact]
    public void CiwComparison_SingleNode()
    {
        Cohort[] cohorts = CohortFactory.GetSingleNode("Node01",
        serverCount: 3,
                arrivalDistribution: new ExponentialDistribution(0.002, 1),
                serviceDistribution: new ExponentialDistribution(0.001, 2),
                arrivalEnd: 100000000,
                serviceEnd: 100000000);
        SimulationSettings simulationSettings = new(50000000);
        Simulation simulation = new(cohorts, simulationSettings);
        ImmutableArray<NodeVisitRecord> nodeVisitRecords = simulation.Start().ToImmutableArray();
        SimulationReport report = SimulationAnalysis.GetSimulationReport(nodeVisitRecords);
        NodeVisitRecordsValidation.Validate(nodeVisitRecords);
        report.NodeReportsByNodeId.Should().HaveCount(1);
        SimulationNodeReport nodeReport = report.NodeReportsByNodeId.Single().Value;
        nodeReport.ServiceDurationMetrics.Mean.Should().BeApproximately(1000, 50);
        nodeReport.WaitingTimeMetrics.Mean.Should().BeApproximately(436, 30);
    }

    [Fact]
    public void CiwComparison_RandomDestination()
    {
        int simulationRunTime = 20000;
        int randomSeed = 0;
        Node coldFood = new Node("Cold Food", 1);
        Node hotFood = new Node("Hot Food", 2);
        Node till = new Node("Till", 2);

        WeightedArc coldToHot = new WeightedArc(coldFood, hotFood, 0.3);
        WeightedArc coldToTill = new WeightedArc(coldFood, till, 0.7);
        WeightedArc hotToTill = new WeightedArc(hotFood, till, 1);

        IRouting routing = new RandomRouteSelection([coldToHot, coldToTill, hotToTill], null, ++randomSeed);

        IServerSelector serverSelector = new FirstServerSelector();
        Dictionary<Node, NodeProperties> propertiesByNode = new Dictionary<Node, NodeProperties>
        {
            {
                coldFood, new NodeProperties(DistributionFactory.CreateExponential(start: 0, end: simulationRunTime * 2, 0.003, ++randomSeed),
                    DistributionFactory.CreateExponential(start: 0, end: simulationRunTime * 2, 0.01, ++randomSeed),
                    serverSelector)
            },
            {
                hotFood, new NodeProperties(DistributionFactory.CreateExponential(start: 0, end: simulationRunTime * 2, 0.002, ++randomSeed),
                    DistributionFactory.CreateExponential(start: 0, end: simulationRunTime * 2, 0.004, ++randomSeed),
                    serverSelector)
            },
            {
                till, new NodeProperties(DurationDistributionSelector.None,
                    DistributionFactory.CreateExponential(start: 0, end: simulationRunTime * 2, 0.005, ++randomSeed),
                    serverSelector)
            }
        };

        Cohort[] cohorts = [
            new Cohort("Cohort01", propertiesByNode.ToFrozenDictionary(), routing)
            ];
        SimulationSettings simulationSettings = new(simulationRunTime);
        SimulationReport[] reports = new SimulationReport[1000];
        for (int i = 0; i < reports.Length; i++)
        {
            Simulation simulation = new(cohorts, simulationSettings);
            ImmutableArray<NodeVisitRecord> nodeVisitRecords = simulation.Start().ToImmutableArray();
            NodeVisitRecordsValidation.Validate(nodeVisitRecords);
            reports[i] = SimulationAnalysis.GetSimulationReport(nodeVisitRecords);
            reports[i].NodeReportsByNodeId.Should().HaveCount(3);
            simulation.ClearState();
        }

        FrozenDictionary<string, SimulationAggregationNodeReport> mergedReport = SimulationAnalysis.Merge(reports);

        mergedReport[coldFood.Id].WaitingTimeMetrics.Mean.Mean.Should().BeApproximately(41, 5);
        mergedReport[hotFood.Id].WaitingTimeMetrics.Mean.Mean.Should().BeApproximately(34, 4);
        mergedReport[till.Id].WaitingTimeMetrics.Mean.Mean.Should().BeApproximately(62, 6);
        mergedReport[coldFood.Id].ServiceDurationMetrics.Count.Mean.Should().BeApproximately(60, 6);
        mergedReport[hotFood.Id].ServiceDurationMetrics.Count.Mean.Should().BeApproximately(57, 6);
        mergedReport[till.Id].ServiceDurationMetrics.Count.Mean.Should().BeApproximately(98, 10);
    }

    [Fact]
    public void CiwComparison_RestrictedNetworks()
    {
        int randomSeed = 0;
        Node[] nodes = [
            new Node("Node01", serverCount: 1, queueCapacity: 1),
            new Node("Node02", serverCount: 1, queueCapacity: 1),
            new Node("Node03", serverCount: 1, queueCapacity: 1),
            ];

        WeightedArc[] arcs = [
            new WeightedArc(nodes[0], nodes[1]),
            new WeightedArc(nodes[1], nodes[2]),
            ];
        Dictionary<Node, QueueIsFullBehavior> queueFullBehaviorByNode = new()
        {
            { nodes[0], QueueIsFullBehavior.RejectIndividual},
            { nodes[1], QueueIsFullBehavior.WaitAndBlockCurrentServer},
            { nodes[2], QueueIsFullBehavior.WaitAndBlockCurrentServer},
        };
        IRouting routing = new RandomRouteSelection(arcs, queueFullBehaviorByNode.ToFrozenDictionary(), ++randomSeed);
        IServerSelector serverSelector = new FirstServerSelector();

        int simulationDuration = 40000;
        Dictionary<Node, NodeProperties> propertiesByNode = new()
        {
            {
                nodes[0],
                new NodeProperties(ArrivalDistributionSelector: DistributionFactory.CreateConstant(start: 0, end: simulationDuration * 2, duration: 330, ++randomSeed, arrivalFraction: 1),
                    ServiceDurationSelector: DistributionFactory.CreateConstant(start: 0, end: simulationDuration * 2, duration: 350, ++randomSeed),
                    ServerSelector: serverSelector)
            },
            {
                nodes[1],
                new NodeProperties(ArrivalDistributionSelector: DurationDistributionSelector.Empty,
                    ServiceDurationSelector: DistributionFactory.CreateConstant(start: 0, end: simulationDuration * 2, duration: 400, ++randomSeed),
                    ServerSelector: serverSelector)
            },
            {
                nodes[2],
                new NodeProperties(ArrivalDistributionSelector: DurationDistributionSelector.Empty,
                    ServiceDurationSelector: DistributionFactory.CreateConstant(start: 0, end: simulationDuration * 2, duration: 450, ++randomSeed),
                    ServerSelector: serverSelector)
            },
        };
        Cohort cohort = new Cohort("Stools", propertiesByNode.ToFrozenDictionary(), routing);
        SimulationSettings simulationSettings = new SimulationSettings(MaxTime: simulationDuration);
        Simulation simulation = new([cohort], simulationSettings);
        ImmutableArray<NodeVisitRecord> nodeVisitRecords = simulation.Start().ToImmutableArray();
        NodeVisitRecordsValidation.Validate(nodeVisitRecords);
        SimulationReport report = SimulationAnalysis.GetSimulationReport(nodeVisitRecords);
        File.WriteAllText("VisitRecords.csv", SimulationAnalysis.ToCsv(nodeVisitRecords));

        report.NodeReportsByNodeId[nodes[0].Id].RejectedIndividuals.Should().Be(29);
        report.NodeReportsByNodeId[nodes[0].Id].ServiceDurationMetrics.Sum.Should().Be(31500);
        report.NodeReportsByNodeId[nodes[0].Id].ServiceDurationMetrics.Count.Should().Be(90);
        report.NodeReportsByNodeId[nodes[0].Id].BlockDurationMetrics.Sum.Should().Be(7950);

        report.NodeReportsByNodeId[nodes[1].Id].ServiceDurationMetrics.Count.Should().Be(88);
        report.NodeReportsByNodeId[nodes[1].Id].BlockDurationMetrics.Sum.Should().Be(3900);

        report.NodeReportsByNodeId[nodes[2].Id].ServiceDurationMetrics.Count.Should().Be(86);
        report.NodeReportsByNodeId[nodes[2].Id].BlockDurationMetrics.Sum.Should().Be(0);

        nodeVisitRecords.Where(x => x.Node.Id == nodes[0].Id).Sum(x => x.ArrivalTime).Should().Be(2356200);
        nodeVisitRecords.Where(x => x.Node.Id == nodes[1].Id).Sum(x => x.ArrivalTime).Should().Be(1703940);
        nodeVisitRecords.Where(x => x.Node.Id == nodes[2].Id).Sum(x => x.ArrivalTime).Should().Be(1701180);
    }
}

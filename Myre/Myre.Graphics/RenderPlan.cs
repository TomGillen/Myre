using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Ninject;

namespace Myre.Graphics
{
    public class RenderPlan
    {
        struct ResourceFreePoint
        {
            public string Name;
            public int Index;
        }

        private IKernel kernel;
        private List<RendererComponent> components;
        private Dictionary<string, RendererComponent.Resource> resources;
        private Dictionary<string, int> resourcesLastUsed;
        private List<ResourceFreePoint> freePoints;
        private RenderTargetInfo? output;

        internal RenderPlan(IKernel kernel, RenderPlan previous, RendererComponent component)
        {
            this.kernel = kernel;
            this.components = new List<RendererComponent>();
            this.resources = new Dictionary<string, RendererComponent.Resource>();
            this.resourcesLastUsed = new Dictionary<string, int>();

            if (previous != null)
            {
                components.AddRange(previous.components);

                foreach (var item in previous.resources)
                    resources.Add(item.Key, item.Value);

                foreach (var item in previous.resourcesLastUsed)
                    resourcesLastUsed.Add(item.Key, item.Value);
            }

            component.SpecifyResources();

            components.Add(component);
            if (!component.ValidateInput(previous == null ? null : previous.output))
                throw new InvalidOperationException("Invalid input for renderer component " + component.GetType().Name);

            foreach (var resource in component.InputResources)
            {
                var resourceAvailable = resources.ContainsKey(resource.Name);

                if (resourceAvailable)
                    resourcesLastUsed[resource.Name] = components.Count - 1;
                else if (!resource.Optional)
                    throw new InvalidOperationException(string.Format("The resource {0} is not available to component {1}. Please insert an earlier component which outputs this resource.", resource, component.GetType().Name));
            }

            foreach (var resource in component.OutputResources)
            {
                resources[resource.Name] = resource;
                resourcesLastUsed[resource.Name] = components.Count - 1;
            }

            output = component.OutputTarget ?? previous.output;
        }

        public static RenderPlan StartWith(IKernel kernel, RendererComponent component)
        {
            return new RenderPlan(kernel, null, component);
        }

        public static RenderPlan StartWith<T>(IKernel kernel)
            where T : RendererComponent
        {
            var component = kernel.Get<T>();
            return StartWith(kernel, component);
        }

        public RenderPlan Then(RendererComponent component)
        {
            return new RenderPlan(kernel, this, component);
        }

        public RenderPlan Then<T>()
            where T : RendererComponent
        {
            var component = kernel.Get<T>();
            return Then(component);
        }

        public RenderTarget2D Execute(Renderer renderer)
        {
            if (freePoints == null)
                Initialise();

            RenderTarget2D activeTarget = null;
            RendererComponent.Resource activeResource = null;
            bool activeResourceUnneeded = false;

            int resourceIndex = 0;
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];

                if (!component.Initialised)
                    component.Initialise(renderer);

                var newOutput = component.Draw(renderer);
                if (newOutput != null)
                {
                    if (activeResourceUnneeded)
                        activeResource.Finalise(renderer);

                    activeResource = FindSetOutput(component);
                    activeTarget = newOutput;
                }

                while (resourceIndex < freePoints.Count && freePoints[resourceIndex].Index <= i)
                {
                    var point = freePoints[resourceIndex];
                    if (activeResource != null && point.Name == activeResource.Name)
                        activeResourceUnneeded = true;
                    else
                        resources[point.Name].Finalise(renderer);

                    resourceIndex++;
                }
            }

            return activeTarget;
        }

        private void Initialise()
        {
            freePoints = new List<ResourceFreePoint>(from resource in resourcesLastUsed
                                                     orderby resource.Value ascending
                                                     select new ResourceFreePoint() { Name = resource.Key, Index = resource.Value });
        }

        private RendererComponent.Resource FindSetOutput(RendererComponent component)
        {
            for (int i = 0; i < component.OutputResources.Count; i++)
            {
                var output = component.OutputResources[i];
                if (output.IsLeftSet)
                    return output;
            }

            return null;
        }
    }

    /*
    public class RenderPlan
    {
        struct ResourceFreePoint
        {
            public string ResourceName;
            public int Index;
        }

        class Node
        {
            public RendererComponent[] Phases;
            public List<string> Inputs;
            public List<RendererComponent.Output> Outputs;
            public List<Node> Dependancies = new List<Node>();
            public bool Added;
        }

        private List<RendererComponent[]> phases;
        private List<RendererComponent> implicitPhases;

        private ResourceFreePoint[] freePoints;
        private Dictionary<string, RendererComponent.Output> outputs;
        private RendererComponent[] sortedPhases;
        private string defaultFinalOutput;
        private string userSpecifiedFinalOutput;

        public ReadOnlyCollection<RendererComponent> Phases { get; private set; }
        
        public string FinalOutput
        {
            get { return userSpecifiedFinalOutput ?? defaultFinalOutput; }
            set { userSpecifiedFinalOutput = value; }
        }

        public RenderPlan()
        {
            phases = new List<RendererComponent[]>();
            implicitPhases = new List<RendererComponent>();
            outputs = new Dictionary<string, RendererComponent.Output>();
        }

        public void Add(params RendererComponent[] phaseChain)
        {
            bool isImplicit = true;
            for (int i = 0; i < phaseChain.Length; i++)
            {
                if (phaseChain[i].Outputs.Count > 0)
                {
                    isImplicit = false;
                    break;
                }
            }

            if (isImplicit)
                implicitPhases.AddRange(phaseChain);
            else
            {
                phases.Add(phaseChain);
                for (int i = 1; i < phaseChain.Length; i++)
                {
                    var current = phaseChain[i];
                    var previous = phaseChain[i - 1];

                    current.IsChainedFrom.UnionWith(from o in previous.Outputs
                                                    where o.IsLeftSet
                                                    select o.Name);
                }
            }
        }

        public void Apply()
        {
            var graph = CreateDependancyGraph();
            sortedPhases = SortDependancyGraph(graph);

            if (sortedPhases.Length > 0)
                defaultFinalOutput = sortedPhases[sortedPhases.Length - 1].Outputs.FirstOrDefault().Name;

            var list = new List<RendererComponent>();
            list.AddRange(sortedPhases);
            list.AddRange(implicitPhases);
            Phases = new ReadOnlyCollection<RendererComponent>(list);

            freePoints = FindResourceFreePoints(list);
        }

        private Node[] CreateDependancyGraph()
        {
            var nodes = new Node[phases.Count];
            for (int i = 0; i < phases.Count; i++)
            {
                var node = new Node() { Phases = phases[i] };

                var outputs = new List<RendererComponent.Output>();
                var inputs = new List<string>();

                foreach (var phase in node.Phases)
                {
                    foreach (var item in phase.Inputs)
                    {
                        if (!inputs.Contains(item) && !outputs.Any(o => o.Name == item))
                            inputs.Add(item);
                    }

                    foreach (var item in phase.Outputs)
                    {
                        if (!outputs.Contains(item))
                            outputs.Add(item);
                    }
                }

                node.Inputs = inputs;
                node.Outputs = outputs;

                nodes[i] = node;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];

                var dependancies = from n in nodes
                                   where n != node
                                   from input in node.Inputs
                                   where n.Outputs.Any(o => o.Name == input) && !n.Inputs.Contains(input)
                                   select n;

                node.Dependancies.AddRange(dependancies);
            }

            return nodes;
        }

        private RendererComponent[] SortDependancyGraph(Node[] graph)
        {
            var sorted = new List<RendererComponent>();
            for (int i = 0; i < graph.Length; i++)
                AddToSorted(graph[i], sorted);

            return sorted.ToArray();
        }

        private void AddToSorted(Node node, List<RendererComponent> sorted)
        {
            if (!node.Added)
            {
                foreach (var dependancy in node.Dependancies)
                    AddToSorted(dependancy, sorted);

                sorted.AddRange(node.Phases);
                node.Added = true;
            }
        }

        private ResourceFreePoint[] FindResourceFreePoints(IList<RendererComponent> sorted)
        {
            var resourcesLastUsed = new Dictionary<string, int>();

            for (int i = 0; i < sorted.Count; i++)
            {
                var phase = sorted[i];
                foreach (var output in phase.Outputs)
                {
                    resourcesLastUsed[output.Name] = i;
                    outputs[output.Name] = output;
                }

                foreach (var input in phase.Inputs.Union(phase.IsChainedFrom))
                    resourcesLastUsed[input] = i;
            }

            var freePoints = from item in resourcesLastUsed
                             orderby item.Value
                             select new ResourceFreePoint() { Index = item.Value, ResourceName = item.Key };

            return freePoints.ToArray();
        }

        public RenderTarget2D Execute(Renderer renderer, RendererMetadata metadata)
        {
            int resourceIndex = 0;
            for (int i = 0; i < sortedPhases.Length; i++)
            {
                if (!sortedPhases[i].Initialised)
                    sortedPhases[i].Initialise(renderer);

                sortedPhases[i].Draw(renderer);

                while (resourceIndex < freePoints.Length && freePoints[resourceIndex].Index <= i)
                {
                    var point = freePoints[resourceIndex];
                    if (point.ResourceName != FinalOutput)
                        outputs[point.ResourceName].Finalise(renderer);

                    resourceIndex++;
                }
            }

            RenderTarget2D implicitTarget = null;
            if (defaultFinalOutput == null)
            {
                var resolution = metadata.Get<Vector2>("resolution").Value;
                implicitTarget = RenderTargetManager.GetTarget(renderer.Device, (int)resolution.X, (int)resolution.Y, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                renderer.Device.SetRenderTarget(implicitTarget);
                renderer.Device.Clear(Color.Black);
            }
            else if (userSpecifiedFinalOutput != null)
            {
                var target = renderer.GetResource(userSpecifiedFinalOutput);
                renderer.Device.SetRenderTarget(target);
            }

            for (int i = 0; i < implicitPhases.Count; i++)
            {
                if (!implicitPhases[i].Initialised)
                    implicitPhases[i].Initialise(renderer);

                implicitPhases[i].Draw(renderer);

                while (resourceIndex < freePoints.Length && freePoints[resourceIndex].Index <= i + sortedPhases.Length)
                {
                    var point = freePoints[resourceIndex];
                    if (point.ResourceName != FinalOutput)
                    {
                        RendererComponent.Output output;
                        if (outputs.TryGetValue(point.ResourceName, out output))
                            output.Finalise(renderer);
                    }

                    resourceIndex++;
                }
            }

            if (FinalOutput != null)
                return renderer.GetResource(FinalOutput);
            else
                return implicitTarget;
        }
    }
     */
}

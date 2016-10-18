using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flow
{
    /// <summary>
    /// Fluent API for building a behaviour tree.
    /// </summary>
    public class FlowBuilder
    {
        /// <summary>
        /// Last node created.
        /// </summary>
        private IFlowNode curNode = null;

        /// <summary>
        /// Stack node nodes that we are build via the fluent API.
        /// </summary>
        private Stack<IFlowCompositeNode> parentNodeStack = new Stack<IFlowCompositeNode>();

        /// <summary>
        /// Create an action node.
        /// </summary>
        public FlowBuilder Do(string name, Func<TimeData, FlowStatus> fn)
        {
            if (parentNodeStack.Count <= 0)
            {
                throw new ApplicationException("Can't create an unnested ActionNode, it must be a leaf node.");
            }

            var actionNode = new ActionNode(name, fn);
            parentNodeStack.Peek().AddChild(actionNode);
            return this;
        }

        /// <summary>
        /// Like an action node... but the function can return true/false and is mapped to success/failure.
        /// </summary>
        public FlowBuilder Condition(string name, Func<TimeData, bool> fn)
        {
            return Do(name, t => fn(t) ? FlowStatus.Success : FlowStatus.Failure);
        }

        /// <summary>
        /// Create an inverter node that inverts the success/failure of its children.
        /// </summary>
        public FlowBuilder Inverter(string name)
        {
            var inverterNode = new InverterNode(name);

            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(inverterNode);
            }

            parentNodeStack.Push(inverterNode);
            return this;
        }

        /// <summary>
        /// Create a sequence node.
        /// </summary>
        public FlowBuilder Sequence(string name)
        {
            var sequenceNode = new SequenceNode(name);

            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(sequenceNode);
            }

            parentNodeStack.Push(sequenceNode);
            return this;
        }

        /// <summary>
        /// Create a parallel node.
        /// </summary>
        public FlowBuilder Parallel(string name, int numRequiredToFail, int numRequiredToSucceed)
        {
            var parallelNode = new ParallelNode(name, numRequiredToFail, numRequiredToSucceed);

            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(parallelNode);
            }

            parentNodeStack.Push(parallelNode);
            return this;
        }

        /// <summary>
        /// Create a selector node.
        /// </summary>
        public FlowBuilder Selector(string name)
        {
            var selectorNode = new SelectorNode(name);

            if (parentNodeStack.Count > 0)
            {
                parentNodeStack.Peek().AddChild(selectorNode);
            }

            parentNodeStack.Push(selectorNode);
            return this;
        }

        /// <summary>
        /// Splice a sub tree into the parent tree.
        /// </summary>
        public FlowBuilder Splice(IFlowNode subTree)
        {
            if (subTree == null)
            {
                throw new ArgumentNullException("subTree");
            }

            if (parentNodeStack.Count <= 0)
            {
                throw new ApplicationException("Can't splice an unnested sub-tree, there must be a parent-tree.");
            }

            parentNodeStack.Peek().AddChild(subTree);
            return this;
        }

        /// <summary>
        /// Build the actual tree.
        /// </summary>
        public IFlowNode Build()
        {
            if (curNode == null)
            {
                throw new ApplicationException("Can't create a behaviour tree with zero nodes");
            }
            return curNode;
        }

        /// <summary>
        /// Ends a sequence of children.
        /// </summary>
        public FlowBuilder End()
        {
            curNode = parentNodeStack.Pop();
            return this;
        }
    }
}

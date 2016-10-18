using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Action
{
    /// <summary>
    /// Runs child nodes in sequence, until one fails.
    /// </summary>
    public class SequenceNode : IActionCompositeNode
    {
        /// <summary>
        /// Name of the node.
        /// </summary>
        private string name;

        /// <summary>
        /// List of child nodes.
        /// </summary>
        private List<IActionNode> children = new List<IActionNode>(); //todo: this could be optimized as a baked array.

        public SequenceNode(string name)
        {
            this.name = name;
        }

        public ActionStatus Tick(TimeData time)
        {
            foreach (var child in children)
            {
                var childStatus = child.Tick(time);
                if (childStatus != ActionStatus.Success)
                {
                    return childStatus;
                }
            }

            return ActionStatus.Success;
        }

        /// <summary>
        /// Add a child to the sequence.
        /// </summary>
        public void AddChild(IActionNode child)
        {
            children.Add(child);
        }
    }
}

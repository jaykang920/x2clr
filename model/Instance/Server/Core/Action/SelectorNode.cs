using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Action
{
    /// <summary>
    /// Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
    /// </summary>
    public class SelectorNode : IActionCompositeNode
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        private string name;

        /// <summary>
        /// List of child nodes.
        /// </summary>
        private List<IActionNode> children = new List<IActionNode>(); //todo: optimization, bake this to an array.

        public SelectorNode(string name)
        {
            this.name = name;
        }

        public ActionStatus Tick(TimeData time)
        {
            foreach (var child in children)
            {
                var childStatus = child.Tick(time);
                if (childStatus != ActionStatus.Failure)
                {
                    return childStatus;
                }
            }

            return ActionStatus.Failure;
        }

        /// <summary>
        /// Add a child node to the selector.
        /// </summary>
        public void AddChild(IActionNode child)
        {
            children.Add(child);
        }
    }
}

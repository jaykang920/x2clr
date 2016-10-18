using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Action
{
    /// <summary>
    /// Decorator node that inverts the success/failure of its child.
    /// </summary>
    public class InverterNode : IActionCompositeNode
    {
        /// <summary>
        /// Name of the node.
        /// </summary>
        private string name;

        /// <summary>
        /// The child to be inverted.
        /// </summary>
        private IActionNode childNode;

        public InverterNode(string name)
        {
            this.name = name;
        }

        public ActionStatus Tick(TimeData time)
        {
            if (childNode == null)
            {
                throw new ApplicationException("InverterNode must have a child node!");
            }

            var result = childNode.Tick(time);
            if (result == ActionStatus.Failure)
            {
                return ActionStatus.Success;
            }
            else if (result == ActionStatus.Success)
            {
                return ActionStatus.Failure;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Add a child to the parent node.
        /// </summary>
        public void AddChild(IActionNode child)
        {
            if (this.childNode != null)
            {
                throw new ApplicationException("Can't add more than a single child to InverterNode!");
            }

            this.childNode = child;
        }
    }
}

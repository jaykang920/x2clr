using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Action 
{
    /// <summary>
    /// Interface for behaviour tree nodes.
    /// </summary>
    public interface IActionCompositeNode : IActionNode
    {
        /// <summary>
        /// Add a child to the parent node.
        /// </summary>
        void AddChild(IActionNode child);
    }
}

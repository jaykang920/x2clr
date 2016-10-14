using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Net Flow which has Feed function to Send directly to Link.
    /// This is to avoid queueing delay in Flow queue competing with other events.
    /// </summary>
    public class NetFlow : SingleThreadFlow 
    {
        private SessionBasedLink link;

        public NetFlow()
            : base()
        {
        }

        public override void Feed(Event e)
        {
            if (e._Handle > 0)
            {
                if (this.link == null)
                {
                    throw new InvalidOperationException(
                        "NetFlow needs to have a SessionBasedLink setup"
                    );
                }

                link.Send(e);
            }
            else
            {
                base.Feed(e);
            }
        }

        protected override void OnCaseAdded(ICase c)
        {
            var slink = c as SessionBasedLink;
            
            if ( slink == null )
            {
                return;
            }

            if ( this.link != null )
            {
                throw new InvalidOperationException(
                    "NetFlow needs to have a single SessionBasedLink only"
                );
            }

            this.link = slink;
        } 

        protected override void OnCaseRemoved(ICase c)
        {
            if ( Object.ReferenceEquals(c, link))
            {
                link = null;
            }
        } 
    }
}
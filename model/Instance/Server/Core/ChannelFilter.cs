using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Core
{
    /// <summary>
    /// Used in Preprocess or before Post() to setup TypeId based Channel setting.
    /// TypeId is searched for upto the root of Event.Tag.
    /// </summary>
    public class ChannelFilter
    {
        Dictionary<int, string> channels;


        public ChannelFilter()
        {
            channels = new Dictionary<int, string>();
        }

        public void Add(int typeId, string channel)
        {
            channels[typeId] = channel;
        }

        public void Remove(int typeId)
        {
            channels.Remove(typeId);
        }

        public void Process(Event e)
        {
            // from child to parent traversal   

            string channel;

            Event.Tag tag = (Event.Tag)e.GetTypeTag();

            while (tag != null)
            {
                int typeId = tag.TypeId;

                if (channels.TryGetValue(typeId, out channel))
                {
                    e._Channel = channel;

                    return; // found
                }

                tag = (Event.Tag)tag.Base;
            }
        }
    }
}

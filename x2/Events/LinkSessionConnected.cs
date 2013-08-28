// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.Events
{
    public sealed class LinkSessionConnected : Event
    {
        new private static readonly Tag tag;

        public bool Result { get; set; }
        public object Context { get; set; }

        static LinkSessionConnected()
        {
            tag = new Tag(Event.tag, typeof(LinkSessionConnected), 0,
                          (int)BuiltinType.LinkSessionConnected);
        }

        public LinkSessionConnected()
            : base(tag.NumProps)
        {
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            return Hash.Update(base.GetHashCode(fingerprint), tag.TypeId);
        }

        public override int GetTypeId()
        {
            return tag.TypeId;
        }

        public override Cell.Tag GetTypeTag()
        {
            return tag;
        }
    }
}

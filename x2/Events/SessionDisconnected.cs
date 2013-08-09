// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.Events
{
    public class SessionDisconnected : Event
    {
        new private static readonly Tag tag;

        public object Context;

        static SessionDisconnected()
        {
            tag = new Tag(Event.tag, typeof(SessionDisconnected), 0,
                          (int)BuiltinType.SessionDisconnected);
        }

        public SessionDisconnected() : base(tag.NumProps) { }

        public override int GetHashCode()
        {
            return Hash.Update(base.GetHashCode(), tag.TypeId);
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

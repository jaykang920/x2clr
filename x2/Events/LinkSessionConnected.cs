// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Text;

namespace x2.Events
{
    public sealed class LinkSessionConnected : Event
    {
        new private static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private string linkName;
        private bool result;
        private object context;

        public string LinkName
        {
            get { return linkName; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                linkName = value;
            }
        }

        public bool Result
        {
            get { return result; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                result = value;
            }
        }

        public object Context
        {
            get { return context; }
            set
            {
                fingerprint.Touch(tag.Offset + 2);
                context = value;
            }
        }

        static LinkSessionConnected()
        {
            tag = new Tag(Event.tag, typeof(LinkSessionConnected), 3,
                          (int)BuiltinType.LinkSessionConnected);
        }

        public LinkSessionConnected()
            : base(tag.NumProps)
        {
        }

        public override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            LinkSessionConnected o = (LinkSessionConnected)other;
            if (linkName != o.linkName)
            {
                return false;
            }
            if (result != o.result)
            {
                return false;
            }
            if (context != o.context)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            var hash = new Hash(base.GetHashCode(fingerprint));
            if (fingerprint.Length <= tag.Offset)
            {
                return hash.Code;
            }
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                hash.Update(linkName);
            }
            if (touched[1])
            {
                hash.Update(result);
            }
            if (touched[2])
            {
                hash.Update(context);
            }
            return hash.Code;
        }

        public override int GetTypeId()
        {
            return tag.TypeId;
        }

        public override Cell.Tag GetTypeTag()
        {
            return tag;
        }

        public override bool IsEquivalent(Cell other)
        {
            if (!base.IsEquivalent(other))
            {
                return false;
            }
            LinkSessionConnected o = (LinkSessionConnected)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (linkName != o.linkName)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (result != o.result)
                {
                    return false;
                }
            }
            if (touched[2])
            {
                if (context != o.context)
                {
                    return false;
                }
            }
            return true;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                stringBuilder.AppendFormat(" LinkName={0}", linkName);
            }
            if (touched[1])
            {
                stringBuilder.AppendFormat(" Result={0}", result);
            }
            if (touched[2])
            {
                stringBuilder.AppendFormat(" Context={0}", context);
            }
        }
    }
}

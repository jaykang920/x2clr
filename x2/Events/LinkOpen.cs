// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Text;

namespace x2.Events
{
    public sealed class LinkOpen : Event
    {
        new private static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                name = value;
            }
        }

        static LinkOpen()
        {
            tag = new Tag(Event.tag, typeof(LinkOpen), 1,
                          (int)BuiltinType.LinkOpen);
        }

        public LinkOpen() : base(tag.NumProps) { }

        public override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            LinkOpen o = (LinkOpen)other;
            if (name != o.name)
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
                hash.Update(name);
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
            LinkOpen o = (LinkOpen)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (name != o.name)
                {
                    return false;
                }
            }
            return true;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Name={0}", name);
        }
    }
}

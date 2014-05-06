// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Text;

using x2;

namespace x2.Links.SocketLink
{
    public class HeartbeatEvent : Event
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private long timestamp_;

        public long Timestamp
        {
            get { return timestamp_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                timestamp_ = value;
            }
        }

        static HeartbeatEvent()
        {
            tag = new Tag(Event.tag, typeof(HeartbeatEvent), 1,
                    0);
        }

        new public static HeartbeatEvent New()
        {
            return new HeartbeatEvent();
        }

        public HeartbeatEvent()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected HeartbeatEvent(int length)
            : base(length + tag.NumProps)
        {
            Initialize();
        }

        public override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            HeartbeatEvent o = (HeartbeatEvent)other;
            if (timestamp_ != o.timestamp_)
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
                hash.Update(timestamp_);
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
            HeartbeatEvent o = (HeartbeatEvent)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (timestamp_ != o.timestamp_)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Load(x2.Buffer buffer)
        {
            base.Load(buffer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                buffer.Read(out timestamp_);
            }
        }

        public override void Serialize(x2.Buffer buffer)
        {
            buffer.WriteUInt29(tag.TypeId);
            this.Dump(buffer);
        }

        protected override void Dump(x2.Buffer buffer)
        {
            base.Dump(buffer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                buffer.Write(timestamp_);
            }
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Timestamp={0}", timestamp_);
        }

        private void Initialize()
        {
            timestamp_ = 0;
        }
    }
}

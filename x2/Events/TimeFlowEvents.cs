// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Text;

namespace x2.Events
{
    public class TimeoutEvent : Event
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private int key;

        public int Key
        {
            get { return key; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                key = value;
            }
        }

        static TimeoutEvent()
        {
            tag = new Tag(Event.tag, typeof(TimeoutEvent), 1,
                    (int)(int)BuiltinType.TimeoutEvent);
        }

        new public static TimeoutEvent New()
        {
            return new TimeoutEvent();
        }

        public TimeoutEvent()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected TimeoutEvent(int length)
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
            TimeoutEvent o = (TimeoutEvent)other;
            if (key != o.key)
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
                hash.Update(key);
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
            TimeoutEvent o = (TimeoutEvent)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (key != o.key)
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
                buffer.Read(out key);
            }
        }

        public TimeoutEvent Run(Action<TimeoutEvent> action)
        {
            action(this);
            return this;
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
                buffer.Write(key);
            }
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Key={0}", key);
        }

        private void Initialize()
        {
            key = 0;
        }
    }

    public class PeriodicEvent : Event
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private int key;

        public int Key
        {
            get { return key; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                key = value;
            }
        }

        static PeriodicEvent()
        {
            tag = new Tag(Event.tag, typeof(PeriodicEvent), 1,
                    (int)(int)BuiltinType.PeriodicEvent);
        }

        new public static PeriodicEvent New()
        {
            return new PeriodicEvent();
        }

        public PeriodicEvent()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected PeriodicEvent(int length)
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
            PeriodicEvent o = (PeriodicEvent)other;
            if (key != o.key)
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
                hash.Update(key);
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
            PeriodicEvent o = (PeriodicEvent)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (key != o.key)
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
                buffer.Read(out key);
            }
        }

        public PeriodicEvent Run(Action<PeriodicEvent> action)
        {
            action(this);
            return this;
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
                buffer.Write(key);
            }
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Key={0}", key);
        }

        private void Initialize()
        {
            key = 0;
        }
    }
}

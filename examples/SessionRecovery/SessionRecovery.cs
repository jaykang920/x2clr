﻿// auto-generated by xpiler

using System;
using System.Collections.Generic;
using System.Text;

using x2;

namespace x2.Examples.SessionRecovery
{
    public class TestReq : Event
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private long serial_;

        public long Serial
        {
            get { return serial_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                serial_ = value;
            }
        }

        static TestReq()
        {
            tag = new Tag(Event.tag, typeof(TestReq), 1,
                    1);
        }

        new public static TestReq New()
        {
            return new TestReq();
        }

        public TestReq()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected TestReq(int length)
            : base(length + tag.NumProps)
        {
            Initialize();
        }

        protected override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            TestReq o = (TestReq)other;
            if (serial_ != o.serial_)
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
                hash.Update(tag.Offset + 0);
                hash.Update(serial_);
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

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            TestReq o = (TestReq)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (serial_ != o.serial_)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                deserializer.Read(out serial_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Serial", out serial_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(serial_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Serial", serial_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(serial_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Serial={0}", serial_);
        }

        private void Initialize()
        {
            serial_ = 0;
        }
    }

    public class TestResp : Event
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private long serial_;

        public long Serial
        {
            get { return serial_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                serial_ = value;
            }
        }

        static TestResp()
        {
            tag = new Tag(Event.tag, typeof(TestResp), 1,
                    2);
        }

        new public static TestResp New()
        {
            return new TestResp();
        }

        public TestResp()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected TestResp(int length)
            : base(length + tag.NumProps)
        {
            Initialize();
        }

        protected override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            TestResp o = (TestResp)other;
            if (serial_ != o.serial_)
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
                hash.Update(tag.Offset + 0);
                hash.Update(serial_);
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

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            TestResp o = (TestResp)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (serial_ != o.serial_)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                deserializer.Read(out serial_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Serial", out serial_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(serial_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Serial", serial_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(serial_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Serial={0}", serial_);
        }

        private void Initialize()
        {
            serial_ = 0;
        }
    }
}

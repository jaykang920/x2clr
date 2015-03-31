﻿// auto-generated by xpiler

using System;
using System.Collections.Generic;
using System.Text;

using x2;

namespace x2.Samples.HeadFirst
{
    public class CapitalizeReq : Event
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private string message_;

        public string Message
        {
            get { return message_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                message_ = value;
            }
        }

        static CapitalizeReq()
        {
            tag = new Tag(Event.tag, typeof(CapitalizeReq), 1,
                    1);
        }

        new public static CapitalizeReq New()
        {
            return new CapitalizeReq();
        }

        public CapitalizeReq()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected CapitalizeReq(int length)
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
            CapitalizeReq o = (CapitalizeReq)other;
            if (message_ != o.message_)
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
                hash.Update(message_);
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
            CapitalizeReq o = (CapitalizeReq)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (message_ != o.message_)
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
                deserializer.Read(out message_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Message", out message_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(message_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Message", message_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(message_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Message=\"{0}\"", message_.Replace("\"", "\\\""));
        }

        private void Initialize()
        {
            message_ = "";
        }
    }

    public class CapitalizeResp : Event
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private string result_;

        public string Result
        {
            get { return result_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                result_ = value;
            }
        }

        static CapitalizeResp()
        {
            tag = new Tag(Event.tag, typeof(CapitalizeResp), 1,
                    2);
        }

        new public static CapitalizeResp New()
        {
            return new CapitalizeResp();
        }

        public CapitalizeResp()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected CapitalizeResp(int length)
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
            CapitalizeResp o = (CapitalizeResp)other;
            if (result_ != o.result_)
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
                hash.Update(result_);
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
            CapitalizeResp o = (CapitalizeResp)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (result_ != o.result_)
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
                deserializer.Read(out result_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Result", out result_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(result_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Result", result_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(result_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Result=\"{0}\"", result_.Replace("\"", "\\\""));
        }

        private void Initialize()
        {
            result_ = "";
        }
    }
}
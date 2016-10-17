﻿// auto-generated by x2clr xpiler

using System;
using System.Collections.Generic;
using System.Text;

using x2;

namespace Events.Login
{
    public class EventLoginReq : Event
    {
        protected new static readonly Tag tag;

        public new static int TypeId { get { return tag.TypeId; } }

        private string account_;
        private string password_;

        public string Account
        {
            get { return account_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                account_ = value;
            }
        }

        public string Password
        {
            get { return password_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                password_ = value;
            }
        }

        static EventLoginReq()
        {
            tag = new Tag(Event.tag, typeof(EventLoginReq), 2,
                    (int)EventLoginTypes.LoginReq);
        }

        public new static EventLoginReq New()
        {
            return new EventLoginReq();
        }

        public EventLoginReq()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected EventLoginReq(int length)
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
            EventLoginReq o = (EventLoginReq)other;
            if (account_ != o.account_)
            {
                return false;
            }
            if (password_ != o.password_)
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
                hash.Update(account_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(password_);
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

        public override Func<Event> GetFactoryMethod()
        {
            return EventLoginReq.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            EventLoginReq o = (EventLoginReq)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (account_ != o.account_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (password_ != o.password_)
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
                deserializer.Read(out account_);
            }
            if (touched[1])
            {
                deserializer.Read(out password_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Account", out account_);
            deserializer.Read("Password", out password_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(account_);
            }
            if (touched[1])
            {
                serializer.Write(password_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Account", account_);
            serializer.Write("Password", password_);
        }

        public override int GetLength()
        {
            int length = base.GetLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetLength(account_);
            }
            if (touched[1])
            {
                length += Serializer.GetLength(password_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Account=\"{0}\"", account_.Replace("\"", "\\\""));
            stringBuilder.AppendFormat(" Password=\"{0}\"", password_.Replace("\"", "\\\""));
        }

        private void Initialize()
        {
            account_ = "";
            password_ = "";
        }
    }

    public class EventLoginResp : Event
    {
        protected new static readonly Tag tag;

        public new static int TypeId { get { return tag.TypeId; } }

        private string account_;
        private int result_;
        private int guid_;

        public string Account
        {
            get { return account_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                account_ = value;
            }
        }

        public int Result
        {
            get { return result_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                result_ = value;
            }
        }

        public int Guid
        {
            get { return guid_; }
            set
            {
                fingerprint.Touch(tag.Offset + 2);
                guid_ = value;
            }
        }

        static EventLoginResp()
        {
            tag = new Tag(Event.tag, typeof(EventLoginResp), 3,
                    (int)EventLoginTypes.LoginResp);
        }

        public new static EventLoginResp New()
        {
            return new EventLoginResp();
        }

        public EventLoginResp()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected EventLoginResp(int length)
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
            EventLoginResp o = (EventLoginResp)other;
            if (account_ != o.account_)
            {
                return false;
            }
            if (result_ != o.result_)
            {
                return false;
            }
            if (guid_ != o.guid_)
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
                hash.Update(account_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(result_);
            }
            if (touched[2])
            {
                hash.Update(tag.Offset + 2);
                hash.Update(guid_);
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

        public override Func<Event> GetFactoryMethod()
        {
            return EventLoginResp.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            EventLoginResp o = (EventLoginResp)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (account_ != o.account_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (result_ != o.result_)
                {
                    return false;
                }
            }
            if (touched[2])
            {
                if (guid_ != o.guid_)
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
                deserializer.Read(out account_);
            }
            if (touched[1])
            {
                deserializer.Read(out result_);
            }
            if (touched[2])
            {
                deserializer.Read(out guid_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Account", out account_);
            deserializer.Read("Result", out result_);
            deserializer.Read("Guid", out guid_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(account_);
            }
            if (touched[1])
            {
                serializer.Write(result_);
            }
            if (touched[2])
            {
                serializer.Write(guid_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Account", account_);
            serializer.Write("Result", result_);
            serializer.Write("Guid", guid_);
        }

        public override int GetLength()
        {
            int length = base.GetLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetLength(account_);
            }
            if (touched[1])
            {
                length += Serializer.GetLength(result_);
            }
            if (touched[2])
            {
                length += Serializer.GetLength(guid_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Account=\"{0}\"", account_.Replace("\"", "\\\""));
            stringBuilder.AppendFormat(" Result={0}", result_);
            stringBuilder.AppendFormat(" Guid={0}", guid_);
        }

        private void Initialize()
        {
            account_ = "";
            result_ = 0;
            guid_ = 0;
        }
    }

    public class EventLogout : Event
    {
        protected new static readonly Tag tag;

        public new static int TypeId { get { return tag.TypeId; } }

        private string account_;
        private int guid_;

        public string Account
        {
            get { return account_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                account_ = value;
            }
        }

        public int Guid
        {
            get { return guid_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                guid_ = value;
            }
        }

        static EventLogout()
        {
            tag = new Tag(Event.tag, typeof(EventLogout), 2,
                    (int)EventLoginTypes.Logout);
        }

        public new static EventLogout New()
        {
            return new EventLogout();
        }

        public EventLogout()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected EventLogout(int length)
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
            EventLogout o = (EventLogout)other;
            if (account_ != o.account_)
            {
                return false;
            }
            if (guid_ != o.guid_)
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
                hash.Update(account_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(guid_);
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

        public override Func<Event> GetFactoryMethod()
        {
            return EventLogout.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            EventLogout o = (EventLogout)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (account_ != o.account_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (guid_ != o.guid_)
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
                deserializer.Read(out account_);
            }
            if (touched[1])
            {
                deserializer.Read(out guid_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Account", out account_);
            deserializer.Read("Guid", out guid_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(account_);
            }
            if (touched[1])
            {
                serializer.Write(guid_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Account", account_);
            serializer.Write("Guid", guid_);
        }

        public override int GetLength()
        {
            int length = base.GetLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetLength(account_);
            }
            if (touched[1])
            {
                length += Serializer.GetLength(guid_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Account=\"{0}\"", account_.Replace("\"", "\\\""));
            stringBuilder.AppendFormat(" Guid={0}", guid_);
        }

        private void Initialize()
        {
            account_ = "";
            guid_ = 0;
        }
    }

    public class EventMasterLoginReq : Event
    {
        protected new static readonly Tag tag;

        public new static int TypeId { get { return tag.TypeId; } }

        private string account_;
        private string password_;

        public string Account
        {
            get { return account_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                account_ = value;
            }
        }

        public string Password
        {
            get { return password_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                password_ = value;
            }
        }

        static EventMasterLoginReq()
        {
            tag = new Tag(Event.tag, typeof(EventMasterLoginReq), 2,
                    (int)EventLoginTypes.MasterLoginReq);
        }

        public new static EventMasterLoginReq New()
        {
            return new EventMasterLoginReq();
        }

        public EventMasterLoginReq()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected EventMasterLoginReq(int length)
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
            EventMasterLoginReq o = (EventMasterLoginReq)other;
            if (account_ != o.account_)
            {
                return false;
            }
            if (password_ != o.password_)
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
                hash.Update(account_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(password_);
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

        public override Func<Event> GetFactoryMethod()
        {
            return EventMasterLoginReq.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            EventMasterLoginReq o = (EventMasterLoginReq)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (account_ != o.account_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (password_ != o.password_)
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
                deserializer.Read(out account_);
            }
            if (touched[1])
            {
                deserializer.Read(out password_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Account", out account_);
            deserializer.Read("Password", out password_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(account_);
            }
            if (touched[1])
            {
                serializer.Write(password_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Account", account_);
            serializer.Write("Password", password_);
        }

        public override int GetLength()
        {
            int length = base.GetLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetLength(account_);
            }
            if (touched[1])
            {
                length += Serializer.GetLength(password_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Account=\"{0}\"", account_.Replace("\"", "\\\""));
            stringBuilder.AppendFormat(" Password=\"{0}\"", password_.Replace("\"", "\\\""));
        }

        private void Initialize()
        {
            account_ = "";
            password_ = "";
        }
    }

    public class EventMasterLoginResp : Event
    {
        protected new static readonly Tag tag;

        public new static int TypeId { get { return tag.TypeId; } }

        private string account_;
        private int result_;
        private int guid_;

        public string Account
        {
            get { return account_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                account_ = value;
            }
        }

        public int Result
        {
            get { return result_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                result_ = value;
            }
        }

        public int Guid
        {
            get { return guid_; }
            set
            {
                fingerprint.Touch(tag.Offset + 2);
                guid_ = value;
            }
        }

        static EventMasterLoginResp()
        {
            tag = new Tag(Event.tag, typeof(EventMasterLoginResp), 3,
                    (int)EventLoginTypes.MasterLoginResp);
        }

        public new static EventMasterLoginResp New()
        {
            return new EventMasterLoginResp();
        }

        public EventMasterLoginResp()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected EventMasterLoginResp(int length)
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
            EventMasterLoginResp o = (EventMasterLoginResp)other;
            if (account_ != o.account_)
            {
                return false;
            }
            if (result_ != o.result_)
            {
                return false;
            }
            if (guid_ != o.guid_)
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
                hash.Update(account_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(result_);
            }
            if (touched[2])
            {
                hash.Update(tag.Offset + 2);
                hash.Update(guid_);
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

        public override Func<Event> GetFactoryMethod()
        {
            return EventMasterLoginResp.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            EventMasterLoginResp o = (EventMasterLoginResp)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (account_ != o.account_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (result_ != o.result_)
                {
                    return false;
                }
            }
            if (touched[2])
            {
                if (guid_ != o.guid_)
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
                deserializer.Read(out account_);
            }
            if (touched[1])
            {
                deserializer.Read(out result_);
            }
            if (touched[2])
            {
                deserializer.Read(out guid_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Account", out account_);
            deserializer.Read("Result", out result_);
            deserializer.Read("Guid", out guid_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(account_);
            }
            if (touched[1])
            {
                serializer.Write(result_);
            }
            if (touched[2])
            {
                serializer.Write(guid_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Account", account_);
            serializer.Write("Result", result_);
            serializer.Write("Guid", guid_);
        }

        public override int GetLength()
        {
            int length = base.GetLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetLength(account_);
            }
            if (touched[1])
            {
                length += Serializer.GetLength(result_);
            }
            if (touched[2])
            {
                length += Serializer.GetLength(guid_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Account=\"{0}\"", account_.Replace("\"", "\\\""));
            stringBuilder.AppendFormat(" Result={0}", result_);
            stringBuilder.AppendFormat(" Guid={0}", guid_);
        }

        private void Initialize()
        {
            account_ = "";
            result_ = 0;
            guid_ = 0;
        }
    }

    public class EventMasterLogout : Event
    {
        protected new static readonly Tag tag;

        public new static int TypeId { get { return tag.TypeId; } }

        private string account_;
        private int guid_;

        public string Account
        {
            get { return account_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                account_ = value;
            }
        }

        public int Guid
        {
            get { return guid_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                guid_ = value;
            }
        }

        static EventMasterLogout()
        {
            tag = new Tag(Event.tag, typeof(EventMasterLogout), 2,
                    (int)EventLoginTypes.MasterLogout);
        }

        public new static EventMasterLogout New()
        {
            return new EventMasterLogout();
        }

        public EventMasterLogout()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected EventMasterLogout(int length)
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
            EventMasterLogout o = (EventMasterLogout)other;
            if (account_ != o.account_)
            {
                return false;
            }
            if (guid_ != o.guid_)
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
                hash.Update(account_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(guid_);
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

        public override Func<Event> GetFactoryMethod()
        {
            return EventMasterLogout.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            EventMasterLogout o = (EventMasterLogout)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (account_ != o.account_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (guid_ != o.guid_)
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
                deserializer.Read(out account_);
            }
            if (touched[1])
            {
                deserializer.Read(out guid_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Account", out account_);
            deserializer.Read("Guid", out guid_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(account_);
            }
            if (touched[1])
            {
                serializer.Write(guid_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Account", account_);
            serializer.Write("Guid", guid_);
        }

        public override int GetLength()
        {
            int length = base.GetLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetLength(account_);
            }
            if (touched[1])
            {
                length += Serializer.GetLength(guid_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Account=\"{0}\"", account_.Replace("\"", "\\\""));
            stringBuilder.AppendFormat(" Guid={0}", guid_);
        }

        private void Initialize()
        {
            account_ = "";
            guid_ = 0;
        }
    }
}
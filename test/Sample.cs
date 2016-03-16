﻿// auto-generated by xpiler

using System;
using System.Collections.Generic;
using System.Text;

using x2;

namespace x2.Tests
{
    public static class SampleConsts
    {
        public const int Const1 = 1;
        public const int Const2 = 2;

        private static ConstsInfo<int> info;

        static SampleConsts()
        {
            info = new ConstsInfo<int>();
            info.Add("Const1", 1);
            info.Add("Const2", 2);
        }

        public static bool ContainsName(string name)
        {
            return info.ContainsName(name);
        }

        public static bool ContainsValue(int value)
        {
            return info.ContainsValue(value);
        }

        public static string GetName(int value)
        {
            return info.GetName(value);
        }

        public static int Parse(string name)
        {
            return info.Parse(name);
        }

        public static bool TryParse(string name, out int result)
        {
            return info.TryParse(name, out result);
        }
    }

    public class SampleCell1 : Cell
    {
        new protected static readonly Tag tag;

        private int foo_;
        private string bar_;

        public int Foo
        {
            get { return foo_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                foo_ = value;
            }
        }

        public string Bar
        {
            get { return bar_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                bar_ = value;
            }
        }

        static SampleCell1()
        {
            tag = new Tag(null, typeof(SampleCell1), 2);
        }

        public SampleCell1()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected SampleCell1(int length)
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
            SampleCell1 o = (SampleCell1)other;
            if (foo_ != o.foo_)
            {
                return false;
            }
            if (bar_ != o.bar_)
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
                hash.Update(foo_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(bar_);
            }
            return hash.Code;
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
            SampleCell1 o = (SampleCell1)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (foo_ != o.foo_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (bar_ != o.bar_)
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
                deserializer.Read(out foo_);
            }
            if (touched[1])
            {
                deserializer.Read(out bar_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Foo", out foo_);
            deserializer.Read("Bar", out bar_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(foo_);
            }
            if (touched[1])
            {
                serializer.Write(bar_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Foo", foo_);
            serializer.Write("Bar", bar_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(foo_);
            }
            if (touched[1])
            {
                length += Serializer.GetEncodedLength(bar_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Foo={0}", foo_);
            stringBuilder.AppendFormat(" Bar=\"{0}\"", bar_.Replace("\"", "\\\""));
        }

        private void Initialize()
        {
            foo_ = 0;
            bar_ = "";
        }
    }

    public class SampleCell2 : SampleCell1
    {
        new protected static readonly Tag tag;

        private bool baz_;

        public bool Baz
        {
            get { return baz_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                baz_ = value;
            }
        }

        static SampleCell2()
        {
            tag = new Tag(SampleCell1.tag, typeof(SampleCell2), 1);
        }

        public SampleCell2()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected SampleCell2(int length)
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
            SampleCell2 o = (SampleCell2)other;
            if (baz_ != o.baz_)
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
                hash.Update(baz_);
            }
            return hash.Code;
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
            SampleCell2 o = (SampleCell2)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (baz_ != o.baz_)
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
                deserializer.Read(out baz_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Baz", out baz_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(baz_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Baz", baz_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(baz_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Baz={0}", baz_);
        }

        private void Initialize()
        {
            baz_ = false;
        }
    }

    public class SampleCell3 : SampleCell1
    {
        new protected static readonly Tag tag;

        private bool qux_;

        public bool Qux
        {
            get { return qux_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                qux_ = value;
            }
        }

        static SampleCell3()
        {
            tag = new Tag(SampleCell1.tag, typeof(SampleCell3), 1);
        }

        public SampleCell3()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected SampleCell3(int length)
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
            SampleCell3 o = (SampleCell3)other;
            if (qux_ != o.qux_)
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
                hash.Update(qux_);
            }
            return hash.Code;
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
            SampleCell3 o = (SampleCell3)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (qux_ != o.qux_)
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
                deserializer.Read(out qux_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Qux", out qux_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(qux_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Qux", qux_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(qux_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Qux={0}", qux_);
        }

        private void Initialize()
        {
            qux_ = false;
        }
    }

    public class SampleCell4 : SampleCell2
    {
        new protected static readonly Tag tag;

        private bool quux_;

        public bool Quux
        {
            get { return quux_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                quux_ = value;
            }
        }

        static SampleCell4()
        {
            tag = new Tag(SampleCell2.tag, typeof(SampleCell4), 1);
        }

        public SampleCell4()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected SampleCell4(int length)
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
            SampleCell4 o = (SampleCell4)other;
            if (quux_ != o.quux_)
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
                hash.Update(quux_);
            }
            return hash.Code;
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
            SampleCell4 o = (SampleCell4)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (quux_ != o.quux_)
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
                deserializer.Read(out quux_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Quux", out quux_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(quux_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Quux", quux_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(quux_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Quux={0}", quux_);
        }

        private void Initialize()
        {
            quux_ = false;
        }
    }

    public class SampleEvent1 : Event
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private int foo_;
        private string bar_;

        public int Foo
        {
            get { return foo_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                foo_ = value;
            }
        }

        public string Bar
        {
            get { return bar_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                bar_ = value;
            }
        }

        static SampleEvent1()
        {
            tag = new Tag(Event.tag, typeof(SampleEvent1), 2,
                    1);
        }

        new public static SampleEvent1 New()
        {
            return new SampleEvent1();
        }

        public SampleEvent1()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected SampleEvent1(int length)
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
            SampleEvent1 o = (SampleEvent1)other;
            if (foo_ != o.foo_)
            {
                return false;
            }
            if (bar_ != o.bar_)
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
                hash.Update(foo_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(bar_);
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
            SampleEvent1 o = (SampleEvent1)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (foo_ != o.foo_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (bar_ != o.bar_)
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
                deserializer.Read(out foo_);
            }
            if (touched[1])
            {
                deserializer.Read(out bar_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Foo", out foo_);
            deserializer.Read("Bar", out bar_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(foo_);
            }
            if (touched[1])
            {
                serializer.Write(bar_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Foo", foo_);
            serializer.Write("Bar", bar_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(foo_);
            }
            if (touched[1])
            {
                length += Serializer.GetEncodedLength(bar_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Foo={0}", foo_);
            stringBuilder.AppendFormat(" Bar=\"{0}\"", bar_.Replace("\"", "\\\""));
        }

        private void Initialize()
        {
            foo_ = 0;
            bar_ = "";
        }
    }

    public class SampleEvent2 : SampleEvent1
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private bool baz_;

        public bool Baz
        {
            get { return baz_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                baz_ = value;
            }
        }

        static SampleEvent2()
        {
            tag = new Tag(SampleEvent1.tag, typeof(SampleEvent2), 1,
                    2);
        }

        new public static SampleEvent2 New()
        {
            return new SampleEvent2();
        }

        public SampleEvent2()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected SampleEvent2(int length)
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
            SampleEvent2 o = (SampleEvent2)other;
            if (baz_ != o.baz_)
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
                hash.Update(baz_);
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
            SampleEvent2 o = (SampleEvent2)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (baz_ != o.baz_)
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
                deserializer.Read(out baz_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Baz", out baz_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(baz_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Baz", baz_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(baz_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Baz={0}", baz_);
        }

        private void Initialize()
        {
            baz_ = false;
        }
    }

    public class SampleEvent3 : SampleEvent1
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private bool qux_;

        public bool Qux
        {
            get { return qux_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                qux_ = value;
            }
        }

        static SampleEvent3()
        {
            tag = new Tag(SampleEvent1.tag, typeof(SampleEvent3), 1,
                    3);
        }

        new public static SampleEvent3 New()
        {
            return new SampleEvent3();
        }

        public SampleEvent3()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected SampleEvent3(int length)
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
            SampleEvent3 o = (SampleEvent3)other;
            if (qux_ != o.qux_)
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
                hash.Update(qux_);
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
            SampleEvent3 o = (SampleEvent3)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (qux_ != o.qux_)
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
                deserializer.Read(out qux_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Qux", out qux_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(qux_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Qux", qux_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(qux_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Qux={0}", qux_);
        }

        private void Initialize()
        {
            qux_ = false;
        }
    }

    public class SampleEvent4 : SampleEvent2
    {
        new protected static readonly Tag tag;

        new public static int TypeId { get { return tag.TypeId; } }

        private bool quux_;

        public bool Quux
        {
            get { return quux_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                quux_ = value;
            }
        }

        static SampleEvent4()
        {
            tag = new Tag(SampleEvent2.tag, typeof(SampleEvent4), 1,
                    4);
        }

        new public static SampleEvent4 New()
        {
            return new SampleEvent4();
        }

        public SampleEvent4()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected SampleEvent4(int length)
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
            SampleEvent4 o = (SampleEvent4)other;
            if (quux_ != o.quux_)
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
                hash.Update(quux_);
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
            SampleEvent4 o = (SampleEvent4)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (quux_ != o.quux_)
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
                deserializer.Read(out quux_);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("Quux", out quux_);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(quux_);
            }
        }

        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("Quux", quux_);
        }

        public override int GetEncodedLength()
        {
            int length = base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetEncodedLength(quux_);
            }
            return length;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Quux={0}", quux_);
        }

        private void Initialize()
        {
            quux_ = false;
        }
    }
}
